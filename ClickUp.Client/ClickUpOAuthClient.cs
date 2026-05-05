using ClickUp.Client.Abstractions.OAuth;
using ClickUp.Client.Categories;
using ClickUp.Client.Infrastructure;
using ClickUp.Client.Models;

namespace ClickUp.Client;

public sealed class ClickUpOAuthClient : ClickUpEndpointClient, IDisposable
{
    private readonly HttpClient? _httpClient;
    private readonly IClickUpOAuthApi _api;

    public ClickUpOAuthClient()
        : this(options: null)
    {
    }

    public ClickUpOAuthClient(ClickUpClientOptions? options)
        : this(CreateConfiguredHttpClient(options))
    {
    }

    public ClickUpOAuthClient(
        HttpMessageHandler primaryHandler,
        ClickUpClientOptions? options = null)
        : this(CreateConfiguredHttpClient(options, primaryHandler))
    {
    }

    internal ClickUpOAuthClient(HttpClient httpClient)
        : this(
            ClickUpHttpClientFactory.CreateApi<IClickUpOAuthApi>(httpClient),
            httpClient.BaseAddress ?? new Uri("https://api.clickup.com/api/v2"),
            httpClient)
    {
    }

    internal ClickUpOAuthClient(
        IClickUpOAuthApi api,
        Uri baseUri,
        HttpClient? httpClient = null)
        : base(baseUri)
    {
        _api = api;
        _httpClient = httpClient;
    }

    public string GetAccessTokenJson(string clientId, string clientSecret, string code)
    {
        return GetAccessTokenJsonAsync(clientId, clientSecret, code).GetAwaiter().GetResult();
    }

    public Task<string> GetAccessTokenJsonAsync(
        string clientId,
        string clientSecret,
        string code,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientSecret);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var request = new ClickUpOAuthTokenRequest
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            Code = code,
        };

        return SendAsync(
            ct => _api.GetAccessTokenAsync(request, ct),
            "POST",
            "oauth/token",
            cancellationToken);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    private static HttpClient CreateConfiguredHttpClient(
        ClickUpClientOptions? options,
        HttpMessageHandler? primaryHandler = null)
    {
        var configuredOptions = options is null
            ? new ClickUpClientOptions()
            : CloneOptions(options);

        return ClickUpHttpClientFactory.CreateAnonymous(configuredOptions, primaryHandler);
    }

    private static ClickUpClientOptions CloneOptions(ClickUpClientOptions options)
    {
        return new ClickUpClientOptions
        {
            BaseUrl = options.BaseUrl,
            Timeout = options.Timeout,
            MaxRetries = options.MaxRetries,
            BaseRetryDelay = options.BaseRetryDelay,
            MaxRetryDelay = options.MaxRetryDelay,
        };
    }
}
