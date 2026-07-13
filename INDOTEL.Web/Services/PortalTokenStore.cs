using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace INDOTEL.WEB.Services;

public sealed class PortalTokenState
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTimeOffset AccessTokenExpiraEn { get; set; }
    public DateTimeOffset RefreshTokenExpiraEn { get; set; }
}

public sealed class PortalTokenStore
{
    private const string Prefix = "indotel:portal:session:";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IDistributedCache _cache;

    public PortalTokenStore(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task SaveAsync(
        string sessionId,
        PortalTokenState state,
        CancellationToken cancellationToken = default)
    {
        var absoluteExpiration = state.RefreshTokenExpiraEn <= DateTimeOffset.UtcNow
            ? DateTimeOffset.UtcNow.AddMinutes(5)
            : state.RefreshTokenExpiraEn;

        await _cache.SetStringAsync(
            BuildKey(sessionId),
            JsonSerializer.Serialize(state, JsonOptions),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = absoluteExpiration
            },
            cancellationToken);
    }

    public async Task<PortalTokenState?> GetAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        var json = await _cache.GetStringAsync(BuildKey(sessionId), cancellationToken);
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            return JsonSerializer.Deserialize<PortalTokenState>(json, JsonOptions);
        }
        catch (JsonException)
        {
            await RemoveAsync(sessionId, cancellationToken);
            return null;
        }
    }

    public Task RemoveAsync(
        string sessionId,
        CancellationToken cancellationToken = default) =>
        _cache.RemoveAsync(BuildKey(sessionId), cancellationToken);

    private static string BuildKey(string sessionId) => Prefix + sessionId;
}
