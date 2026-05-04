using Refit;

namespace ClickUp.Client.Abstractions.Folders;

[Headers("Accept: application/json")]
public interface IClickUpFoldersApi
{
    [Get("/space/{spaceId}/folder")]
    Task<string> GetFoldersAsync(
        string spaceId,
        [AliasAs("archived")] string archived,
        CancellationToken cancellationToken = default);
}
