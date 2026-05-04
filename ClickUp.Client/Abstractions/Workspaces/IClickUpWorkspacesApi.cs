using Refit;

namespace ClickUp.Client.Abstractions.Workspaces;

[Headers("Accept: application/json")]
public interface IClickUpWorkspacesApi
{
    [Get("/team")]
    Task<string> GetWorkspacesAsync(CancellationToken cancellationToken = default);
}
