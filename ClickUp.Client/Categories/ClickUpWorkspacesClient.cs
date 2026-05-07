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

    public string GetSharedHierarchyJson(string teamId)
    {
        return GetSharedHierarchyJsonAsync(teamId).GetAwaiter().GetResult();
    }

    public Task<string> GetSharedHierarchyJsonAsync(
        string teamId,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.GetSharedHierarchyAsync(teamId, ct),
            "GET",
            $"team/{teamId}/shared",
            cancellationToken);
    }

    public string GetCustomRolesJson(string teamId, bool? includeMembers = null)
    {
        return GetCustomRolesJsonAsync(teamId, includeMembers).GetAwaiter().GetResult();
    }

    public Task<string> GetCustomRolesJsonAsync(
        string teamId,
        bool? includeMembers = null,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.GetCustomRolesAsync(
                teamId,
                includeMembers.HasValue ? BoolString(includeMembers.Value) : null,
                ct),
            "GET",
            $"team/{teamId}/customroles",
            cancellationToken);
    }
}
