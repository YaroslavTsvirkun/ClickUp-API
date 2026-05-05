using Refit;
using System.Text.Json;

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

    [Get("/list/{listId}")]
    Task<string> GetListAsync(
        string listId,
        CancellationToken cancellationToken = default);

    [Post("/folder/{folderId}/list")]
    Task<string> CreateListAsync(
        string folderId,
        [Body] JsonElement list,
        CancellationToken cancellationToken = default);

    [Post("/space/{spaceId}/list")]
    Task<string> CreateFolderlessListAsync(
        string spaceId,
        [Body] JsonElement list,
        CancellationToken cancellationToken = default);

    [Put("/list/{listId}")]
    Task<string> UpdateListAsync(
        string listId,
        [Body] JsonElement listUpdate,
        CancellationToken cancellationToken = default);

    [Headers("Content-Type: application/json")]
    [Delete("/list/{listId}")]
    Task<string> DeleteListAsync(
        string listId,
        CancellationToken cancellationToken = default);

    [Post("/list/{listId}/task/{taskId}")]
    Task<string> AddTaskToListAsync(
        string listId,
        string taskId,
        CancellationToken cancellationToken = default);

    [Delete("/list/{listId}/task/{taskId}")]
    Task<string> RemoveTaskFromListAsync(
        string listId,
        string taskId,
        CancellationToken cancellationToken = default);
}
