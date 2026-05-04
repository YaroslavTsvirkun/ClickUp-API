using Refit;

namespace ClickUp.Client.Abstractions.Lists;

[Headers("Accept: application/json")]
public interface IClickUpListsApi
{
    [Get("/space/{spaceId}/list")]
    Task<string> GetFolderlessListsAsync(
        string spaceId,
        [AliasAs("archived")] string archived,
        CancellationToken cancellationToken = default);

    [Get("/folder/{folderId}/list")]
    Task<string> GetFolderListsAsync(
        string folderId,
        [AliasAs("archived")] string archived,
        CancellationToken cancellationToken = default);
}
