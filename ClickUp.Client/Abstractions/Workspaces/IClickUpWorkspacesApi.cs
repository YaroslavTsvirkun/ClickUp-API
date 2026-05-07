using Refit;

namespace ClickUp.Client.Abstractions.Workspaces;

[Headers("Accept: application/json")]
public interface IClickUpWorkspacesApi
{
    [Get("/team")]
    Task<string> GetWorkspacesAsync(CancellationToken cancellationToken = default);

    [Get("/team/{teamId}/shared")]
    Task<string> GetSharedHierarchyAsync(
        string teamId,
        CancellationToken cancellationToken = default);

    [Get("/team/{teamId}/customroles")]
    Task<string> GetCustomRolesAsync(
        string teamId,
        [AliasAs("include_members")] string? includeMembers,
        CancellationToken cancellationToken = default);
}
