using ClickUp.Client.Abstractions.Raw;

namespace ClickUp.Client.Categories;

public sealed class ClickUpRawClient : ClickUpEndpointClient
{
    private readonly IClickUpRawApi _api;

    internal ClickUpRawClient(IClickUpRawApi api, Uri baseUri)
        : base(baseUri)
    {
        _api = api;
    }

    public string RequestJson(string method, string endpoint)
    {
        return RequestJsonAsync(method, endpoint).GetAwaiter().GetResult();
    }

    public string RequestJson(string method, string endpoint, string? jsonBody)
    {
        return RequestJsonAsync(method, endpoint, jsonBody).GetAwaiter().GetResult();
    }

    public async Task<string> RequestJsonAsync(
        string method,
        string endpoint,
        string? jsonBody = null,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedEndpoint = NormalizeEndpoint(endpoint);
        var upperMethod = method.ToUpperInvariant();

        return upperMethod switch
        {
            "GET" => await SendAsync(
                ct => _api.GetRawAsync(normalizedEndpoint, ToDictionary(query), ct),
                "GET",
                normalizedEndpoint,
                cancellationToken).ConfigureAwait(false),
            "POST" => await SendAsync(
                ct => _api.PostRawAsync(normalizedEndpoint, ParseJson(jsonBody), ct),
                "POST",
                normalizedEndpoint,
                cancellationToken).ConfigureAwait(false),
            "PUT" => await SendAsync(
                ct => _api.PutRawAsync(normalizedEndpoint, ParseJson(jsonBody), ct),
                "PUT",
                normalizedEndpoint,
                cancellationToken).ConfigureAwait(false),
            "PATCH" => await SendAsync(
                ct => _api.PatchRawAsync(normalizedEndpoint, ParseJson(jsonBody), ct),
                "PATCH",
                normalizedEndpoint,
                cancellationToken).ConfigureAwait(false),
            "DELETE" => await SendAsync(
                ct => _api.DeleteRawAsync(normalizedEndpoint, ct),
                "DELETE",
                normalizedEndpoint,
                cancellationToken).ConfigureAwait(false),
            _ => throw new NotSupportedException($"HTTP method '{method}' is not supported by RequestJson."),
        };
    }

    private static Dictionary<string, string?> ToDictionary(
        IReadOnlyDictionary<string, string?>? query)
    {
        return query is null
            ? new Dictionary<string, string?>(StringComparer.Ordinal)
            : query
                .Where(item => item.Value is not null)
                .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
    }
}
