using Refit;
using System.Text.Json;

namespace ClickUp.Client.Abstractions.Folders;

[Headers("Accept: application/json")]
public interface IClickUpFoldersApi
{
    [Get("/space/{spaceId}/folder")]
    Task<string> GetFoldersAsync(
        string spaceId,
        [AliasAs("archived")] string archived,
        CancellationToken cancellationToken = default);

    [Get("/folder/{folderId}")]
    Task<string> GetFolderAsync(
        string folderId,
        CancellationToken cancellationToken = default);

    [Post("/space/{spaceId}/folder")]
    Task<string> CreateFolderAsync(
        string spaceId,
        [Body] JsonElement folder,
        CancellationToken cancellationToken = default);

    [Put("/folder/{folderId}")]
    Task<string> UpdateFolderAsync(
        string folderId,
        [Body] JsonElement folderUpdate,
        CancellationToken cancellationToken = default);

    [Delete("/folder/{folderId}")]
    Task<string> DeleteFolderAsync(
        string folderId,
        CancellationToken cancellationToken = default);
}
