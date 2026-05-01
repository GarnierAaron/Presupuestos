using System.Net;
using System.Net.Sockets;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Presupuestos.Api.Middleware;
using Presupuestos.Api.Swagger;
using Presupuestos.Application;
using Presupuestos.Application.Auth;
using Presupuestos.Application.Options;
using Presupuestos.Application.Validation;
using Presupuestos.Infrastructure;
using Presupuestos.Infrastructure.Persistence;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddEndpointsApiExplorer();

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
          ?? throw new InvalidOperationException($"Configura la sección {JwtOptions.SectionName}.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Coincidir con los tipos de claim del token emitido (sub, tenant_id, URI de rol).
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AuthPolicies.SuperAdmin,
        p => p.RequireAuthenticatedUser().RequireClaim(AuthConstants.SuperAdminClaim, "true"));
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 40;
        o.QueueLimit = 0;
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Presupuestos API",
        Version = "v1",
        Description = "Rutas de negocio requieren JWT (Bearer). Claims habituales: tenant_id, sub (user id), email, role. Super admin: claim super_admin=true sin tenant_id. Panel: GET/PATCH /api/admin/users, GET /api/admin/stats. Suscripciones: POST /api/Subscriptions/create, GET /api/Subscriptions/me; webhook MP: POST /api/webhooks/mercadopago (sin JWT). Con SubscriptionAccess:Enforce=true hace falta suscripción activa del tenant (excepciones: Auth, admin, webhooks, create/me). Tras login/registro, envía Authorization: Bearer {accessToken}. Refresh: POST /api/Auth/refresh. Sin token, el middleware de tenant acepta X-Tenant-Id / X-User-Id para integraciones; [Authorize] exige JWT."
    });
    options.OperationFilter<TenantHeadersOperationFilter>();
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod();

        if (builder.Environment.IsDevelopment())
        {           
            policy.SetIsOriginAllowed(static origin =>
            {
                if (string.IsNullOrEmpty(origin)) return false;
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)) return false;
                if (uri.Scheme is not ("http" or "https")) return false;
                if (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                    || uri.Host == "127.0.0.1")
                    return true;
                // PWA / móvil en la misma Wi‑Fi: origen http://192.168.x.x:PUERTO (ej. 5999)
                return IsPrivateLanIpv4(uri.Host);
            });
        }
        else
        {
            policy.SetIsOriginAllowed(static origin =>
            {
                if (string.IsNullOrEmpty(origin)) return false;
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)) return false;
                if (uri.Scheme is not ("http" or "https")) return false;
                return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                       || uri.Host == "127.0.0.1";
            });
        }
    });
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// En desarrollo, redirigir HTTP→HTTPS puede hacer que el navegador reintente sin Authorization.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseUserAccountValidation();
app.UseDeviceValidation();
app.UseTenantResolution();
app.UseSubscriptionAccess();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();

static bool IsPrivateLanIpv4(string host)
{
    if (!IPAddress.TryParse(host, out var ip)) return false;
    if (ip.AddressFamily != AddressFamily.InterNetwork) return false;
    var b = ip.GetAddressBytes();
    if (b[0] == 10) return true;
    if (b[0] == 172 && b[1] >= 16 && b[1] <= 31) return true;
    if (b[0] == 192 && b[1] == 168) return true;
    return false;
}
