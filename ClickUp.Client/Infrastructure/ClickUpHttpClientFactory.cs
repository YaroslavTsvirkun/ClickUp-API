using Refit;

namespace ClickUp.Client.Infrastructure;

internal static class ClickUpHttpClientFactory
{
    public static HttpClient Create(
        string apiToken,
        ClickUpClientOptions options,
        HttpMessageHandler? primaryHandler = null)
    {
        var authHandler = new ClickUpAuthHandler(apiToken)
        {
            InnerHandler = primaryHandler ?? new SocketsHttpHandler(),
        };
        var retryHandler = new ClickUpRetryHandler(options)
        {
            InnerHandler = authHandler,
        };

        return new HttpClient(retryHandler, disposeHandler: true)
        {
            BaseAddress = new Uri(options.BaseUrl.TrimEnd('/')),
            Timeout = options.Timeout,
        };
    }

    public static HttpClient CreateAnonymous(
        ClickUpClientOptions options,
        HttpMessageHandler? primaryHandler = null)
    {
        var retryHandler = new ClickUpRetryHandler(options)
        {
            InnerHandler = primaryHandler ?? new SocketsHttpHandler(),
        };

        return new HttpClient(retryHandler, disposeHandler: true)
        {
            BaseAddress = new Uri(options.BaseUrl.TrimEnd('/')),
            Timeout = options.Timeout,
        };
    }

    public static RefitSettings CreateRefitSettings()
    {
        return new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(ClickUpClient.JsonOptions),
            CollectionFormat = CollectionFormat.Multi,
        };
    }

    public static TApi CreateApi<TApi>(HttpClient httpClient)
    {
        return RestService.For<TApi>(httpClient, CreateRefitSettings());
    }
}
