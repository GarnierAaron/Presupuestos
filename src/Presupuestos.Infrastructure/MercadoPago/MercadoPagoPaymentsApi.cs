using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Presupuestos.Application.Abstractions;
using Presupuestos.Application.Dto.MercadoPago;
using Presupuestos.Application.Options;

namespace Presupuestos.Infrastructure.MercadoPago;

public class MercadoPagoPaymentsApi : IMercadoPagoPaymentsApi
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient _http;
    private readonly MercadoPagoOptions _opt;

    public MercadoPagoPaymentsApi(HttpClient http, IOptions<MercadoPagoOptions> opt)
    {
        _http = http;
        _opt = opt.Value;
        var token = _opt.AccessToken?.Trim();
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<MercadoPagoPreferenceResultDto> CreateCheckoutPreferenceAsync(
        MercadoPagoPreferenceRequestDto body,
        CancellationToken cancellationToken = default)
    {
        var baseUri = NormalizeBase(_opt.BaseUrl);
        using var req = new HttpRequestMessage(HttpMethod.Post, new Uri(baseUri, "checkout/preferences"))
        {
            Content = JsonContent.Create(body, options: SerializerOptions),
        };

        using var resp = await _http.SendAsync(req, cancellationToken);
        var text = await resp.Content.ReadAsStringAsync(cancellationToken);
        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"HTTP {(int)resp.StatusCode}: {text}");

        using var doc = JsonDocument.Parse(text);
        var root = doc.RootElement;
        var id = root.GetProperty("id").GetString() ?? string.Empty;
        var init = root.TryGetProperty("init_point", out var ip) ? ip.GetString() ?? string.Empty : string.Empty;
        var sandbox = root.TryGetProperty("sandbox_init_point", out var sp) ? sp.GetString() : null;
        return new MercadoPagoPreferenceResultDto
        {
            PreferenceId = id,
            InitPoint = init,
            SandboxInitPoint = sandbox,
        };
    }

    public async Task<MercadoPagoPaymentDto?> GetPaymentAsync(long paymentId, CancellationToken cancellationToken = default)
    {
        var baseUri = NormalizeBase(_opt.BaseUrl);
        using var req = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUri, $"v1/payments/{paymentId}"));
        using var resp = await _http.SendAsync(req, cancellationToken);
        var text = await resp.Content.ReadAsStringAsync(cancellationToken);
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"HTTP {(int)resp.StatusCode}: {text}");

        using var doc = JsonDocument.Parse(text);
        var root = doc.RootElement;
        var id = ReadLong(root, "id");
        var status = root.TryGetProperty("status", out var st) ? st.GetString() ?? string.Empty : string.Empty;
        var ext = root.TryGetProperty("external_reference", out var er) ? er.GetString() : null;
        var pref = root.TryGetProperty("preference_id", out var pr) ? pr.GetString() : null;
        return new MercadoPagoPaymentDto
        {
            Id = id ?? paymentId,
            Status = status,
            ExternalReference = ext,
            PreferenceId = pref,
        };
    }

    private static long? ReadLong(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var el))
            return null;
        return el.ValueKind switch
        {
            JsonValueKind.Number => el.TryGetInt64(out var n) ? n : null,
            JsonValueKind.String => long.TryParse(el.GetString(), out var p) ? p : null,
            _ => null,
        };
    }

    private static Uri NormalizeBase(string baseUrl)
    {
        var s = string.IsNullOrWhiteSpace(baseUrl) ? "https://api.mercadopago.com" : baseUrl.Trim();
        if (!s.EndsWith('/'))
            s += "/";
        return new Uri(s);
    }
}
