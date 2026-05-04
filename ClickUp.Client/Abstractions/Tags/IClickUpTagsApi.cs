using System.Text.Json;
using Refit;

namespace ClickUp.Client.Abstractions.Tags;

[Headers("Accept: application/json")]
public interface IClickUpTagsApi
{
    [Get("/space/{spaceId}/tag")]
    Task<string> GetSpaceTagsAsync(string spaceId, CancellationToken cancellationToken = default);

    [Post("/space/{spaceId}/tag")]
    Task<string> CreateSpaceTagAsync(
        string spaceId,
        [Body] JsonElement tag,
        CancellationToken cancellationToken = default);

    [Put("/space/{spaceId}/tag/{tagName}")]
    Task<string> EditSpaceTagAsync(
        string spaceId,
        string tagName,
        [Body] JsonElement tag,
        CancellationToken cancellationToken = default);

    [Delete("/space/{spaceId}/tag/{tagName}")]
    Task<string> DeleteSpaceTagAsync(
        string spaceId,
        string tagName,
        [Body] JsonElement tag,
        CancellationToken cancellationToken = default);

    [Post("/task/{taskId}/tag/{tagName}")]
    Task<string> AddTagToTaskAsync(
        string taskId,
        string tagName,
        [AliasAs("custom_task_ids")] string? customTaskIds,
        [AliasAs("team_id")] string? teamId,
        CancellationToken cancellationToken = default);

    [Delete("/task/{taskId}/tag/{tagName}")]
    Task<string> RemoveTagFromTaskAsync(
        string taskId,
        string tagName,
        [AliasAs("custom_task_ids")] string? customTaskIds,
        [AliasAs("team_id")] string? teamId,
        CancellationToken cancellationToken = default);
}
