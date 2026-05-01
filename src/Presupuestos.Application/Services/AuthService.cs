using Microsoft.Extensions.Options;

using Presupuestos.Application.Abstractions;

using Presupuestos.Application.Common.Exceptions;

using Presupuestos.Application.Dto.Auth;

using Presupuestos.Application.Options;

using Presupuestos.Domain.Entities;



namespace Presupuestos.Application.Services;



public class AuthService : IAuthService

{

    private readonly IUserRepository _users;

    private readonly ITenantRepository _tenants;

    private readonly IRefreshTokenRepository _refreshTokens;

    private readonly IUnitOfWork _uow;

    private readonly IPasswordHasher _passwordHasher;

    private readonly ITokenService _tokenService;

    private readonly IDeviceService _devices;

    private readonly JwtOptions _jwt;



    public AuthService(

        IUserRepository users,

        ITenantRepository tenants,

        IRefreshTokenRepository refreshTokens,

        IUnitOfWork uow,

        IPasswordHasher passwordHasher,

        ITokenService tokenService,

        IDeviceService devices,

        IOptions<JwtOptions> jwtOptions)

    {

        _users = users;

        _tenants = tenants;

        _refreshTokens = refreshTokens;

        _uow = uow;

        _passwordHasher = passwordHasher;

        _tokenService = tokenService;

        _devices = devices;

        _jwt = jwtOptions.Value;

    }



    public async Task<TokenResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken cancellationToken = default)

    {

        var existing = await _users.GetByEmailAsync(dto.Email.Trim(), cancellationToken);

        if (existing != null)

            throw new AuthException("Ya existe un usuario con ese correo.");



        var tenantId = Guid.NewGuid();

        var userId = Guid.NewGuid();



        var tenant = new Tenant

        {

            Id = tenantId,

            Name = dto.TenantName.Trim()

        };

        _tenants.Add(tenant);



        var user = new User

        {

            Id = userId,

            TenantId = tenantId,

            Email = dto.Email.Trim().ToLowerInvariant(),

            PasswordHash = _passwordHasher.Hash(dto.Password),

            IsActive = true,

            ExpirationDate = null,

            Role = UserRole.Admin,

            GlobalMarginPercent = null,

            CreatedAt = DateTimeOffset.UtcNow,

            IsSuperAdmin = false

        };

        _users.Add(user);



        await _uow.SaveChangesAsync(cancellationToken);



        var deviceRecordId = await _devices.TryRegisterOnSignupAsync(user, dto.DeviceId, dto.DeviceName, cancellationToken);

        return await IssueTokensAsync(user, deviceRecordId, cancellationToken);

    }



    public async Task<TokenResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)

    {

        var user = await _users.GetByEmailAsync(dto.Email.Trim().ToLowerInvariant(), cancellationToken);

        if (user == null || string.IsNullOrEmpty(user.PasswordHash))

            throw new UnauthorizedAppException("Credenciales inválidas.");



        if (!_passwordHasher.Verify(dto.Password, user.PasswordHash))

            throw new UnauthorizedAppException("Credenciales inválidas.");



        EnsureAccountUsable(user);



        var deviceRecordId = await _devices.RegisterOrValidateForLoginAsync(user, dto.DeviceId, dto.DeviceName, cancellationToken);



        return await IssueTokensAsync(user, deviceRecordId, cancellationToken);

    }



    public async Task<TokenResponseDto> RefreshAsync(RefreshRequestDto dto, CancellationToken cancellationToken = default)

    {

        var hash = _tokenService.HashRefreshToken(dto.RefreshToken);

        var stored = await _refreshTokens.FindByTokenHashAsync(hash, cancellationToken);

        if (stored == null || stored.Expiration < DateTimeOffset.UtcNow)

            throw new UnauthorizedAppException("Refresh token inválido o expirado.");



        var user = await _users.GetByIdGlobalAsync(stored.UserId, cancellationToken);

        if (user == null)

            throw new UnauthorizedAppException("Usuario no encontrado.");



        EnsureAccountUsable(user);



        _refreshTokens.Remove(stored);

        await _uow.SaveChangesAsync(cancellationToken);



        return await IssueTokensAsync(user, stored.DeviceRecordId, cancellationToken);

    }



    public async Task LogoutAsync(RefreshRequestDto dto, CancellationToken cancellationToken = default)

    {

        var hash = _tokenService.HashRefreshToken(dto.RefreshToken);

        var stored = await _refreshTokens.FindByTokenHashAsync(hash, cancellationToken);

        if (stored == null)

            return;



        _refreshTokens.Remove(stored);

        await _uow.SaveChangesAsync(cancellationToken);

    }



    public async Task LogoutAllAsync(Guid userId, CancellationToken cancellationToken = default)

    {

        await _refreshTokens.RevokeAllForUserAsync(userId, cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);

    }



    private async Task<TokenResponseDto> IssueTokensAsync(User user, Guid? deviceRecordId, CancellationToken cancellationToken)

    {

        var (access, accessExpires) = _tokenService.CreateAccessToken(user);

        var refreshPlain = _tokenService.GenerateRefreshToken();

        var refreshHash = _tokenService.HashRefreshToken(refreshPlain);

        var refreshExpires = DateTimeOffset.UtcNow.AddDays(_jwt.RefreshTokenDays);



        var rt = new RefreshToken

        {

            Id = Guid.NewGuid(),

            UserId = user.Id,

            Token = refreshHash,

            Expiration = refreshExpires,

            DeviceRecordId = deviceRecordId

        };

        _refreshTokens.Add(rt);

        await _uow.SaveChangesAsync(cancellationToken);

        await _users.UpdateLastLoginAsync(user.Id, cancellationToken);

        string? tenantName = null;
        if (user.TenantId.HasValue)
        {
            var tenant = await _tenants.GetByIdAsync(user.TenantId.Value, cancellationToken);
            tenantName = tenant?.Name;
        }

        return new TokenResponseDto

        {

            AccessToken = access,

            RefreshToken = refreshPlain,

            AccessTokenExpiresAt = accessExpires,

            RefreshTokenExpiresAt = refreshExpires,

            UserId = user.Id,

            TenantId = user.TenantId,

            TenantName = tenantName

        };

    }



    private static void EnsureAccountUsable(User user)

    {

        if (!user.IsActive)

            throw new ForbiddenAppException("La cuenta está desactivada.");



        if (user.ExpirationDate.HasValue && user.ExpirationDate.Value < DateTimeOffset.UtcNow)

            throw new ForbiddenAppException("La cuenta ha expirado.");

    }

}

