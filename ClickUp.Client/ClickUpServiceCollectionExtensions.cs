using ClickUp.Client.Abstractions.Comments;
using ClickUp.Client.Abstractions.Folders;
using ClickUp.Client.Abstractions.Lists;
using ClickUp.Client.Abstractions.Raw;
using ClickUp.Client.Abstractions.Spaces;
using ClickUp.Client.Abstractions.Tags;
using ClickUp.Client.Abstractions.Tasks;
using ClickUp.Client.Abstractions.Users;
using ClickUp.Client.Abstractions.Workspaces;
using ClickUp.Client.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ClickUp.Client;

public static class ClickUpServiceCollectionExtensions
{
    private const string HttpClientName = "ClickUp.Client";

    public static IHttpClientBuilder AddClickUpApiClient(
        this IServiceCollection services,
        string apiToken,
        ClickUpClientOptions? options = null)
    {
        options ??= new ClickUpClientOptions();
        var baseUri = new Uri(options.BaseUrl.TrimEnd('/'));

        var builder = services
            .AddHttpClient(HttpClientName, client =>
            {
                client.BaseAddress = baseUri;
                client.Timeout = options.Timeout;
            })
            .AddHttpMessageHandler(() => new ClickUpRetryHandler(options))
            .AddHttpMessageHandler(() => new ClickUpAuthHandler(apiToken));

        AddRefitApi<IClickUpUsersApi>(services);
        AddRefitApi<IClickUpWorkspacesApi>(services);
        AddRefitApi<IClickUpSpacesApi>(services);
        AddRefitApi<IClickUpFoldersApi>(services);
        AddRefitApi<IClickUpListsApi>(services);
        AddRefitApi<IClickUpTasksApi>(services);
        AddRefitApi<IClickUpTagsApi>(services);
        AddRefitApi<IClickUpCommentsApi>(services);
        AddRefitApi<IClickUpRawApi>(services);

        services.AddTransient(provider =>
            new ClickUpClient(
                provider.GetRequiredService<IClickUpUsersApi>(),
                provider.GetRequiredService<IClickUpWorkspacesApi>(),
                provider.GetRequiredService<IClickUpSpacesApi>(),
                provider.GetRequiredService<IClickUpFoldersApi>(),
                provider.GetRequiredService<IClickUpListsApi>(),
                provider.GetRequiredService<IClickUpTasksApi>(),
                provider.GetRequiredService<IClickUpTagsApi>(),
                provider.GetRequiredService<IClickUpCommentsApi>(),
                provider.GetRequiredService<IClickUpRawApi>(),
                baseUri));

        return builder;
    }

    private static void AddRefitApi<TApi>(IServiceCollection services)
        where TApi : class
    {
        services.AddTransient(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(HttpClientName);
            return ClickUpHttpClientFactory.CreateApi<TApi>(httpClient);
        });
    }
}
