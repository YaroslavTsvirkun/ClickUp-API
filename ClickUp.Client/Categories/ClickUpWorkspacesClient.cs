using ClickUp.Client.Abstractions.Workspaces;

namespace ClickUp.Client.Categories;

public sealed class ClickUpWorkspacesClient : ClickUpEndpointClient
{
    private readonly IClickUpWorkspacesApi _api;

    internal ClickUpWorkspacesClient(IClickUpWorkspacesApi api, Uri baseUri)
        : base(baseUri)
    {
        _api = api;
    }

    public string GetWorkspacesJson()
    {
        return GetWorkspacesJsonAsync().GetAwaiter().GetResult();
    }

    public Task<string> GetWorkspacesJsonAsync(CancellationToken cancellationToken = default)
    {
        return SendAsync(_api.GetWorkspacesAsync, "GET", "team", cancellationToken);
    }

    public string GetTeamsJson()
    {
        return GetWorkspacesJson();
    }

    public Task<string> GetTeamsJsonAsync(CancellationToken cancellationToken = default)
    {
        return GetWorkspacesJsonAsync(cancellationToken);
    }
}
