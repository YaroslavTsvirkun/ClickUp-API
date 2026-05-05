using System.Text.Json;
using Refit;

namespace ClickUp.Client.Abstractions.Tasks;

[Headers("Accept: application/json")]
public interface IClickUpTasksApi
{
    [Get("/task/{taskId}")]
    Task<string> GetTaskAsync(string taskId, CancellationToken cancellationToken = default);

    [Get("/list/{listId}/task")]
    Task<string> GetTasksAsync(
        string listId,
        [AliasAs("archived")] string archived,
        [AliasAs("include_closed")] string includeClosed,
        [AliasAs("subtasks")] string subtasks,
        [AliasAs("page")] int page,
        CancellationToken cancellationToken = default);

    [Post("/list/{listId}/task")]
    Task<string> CreateTaskAsync(
        string listId,
        [Body] JsonElement task,
        CancellationToken cancellationToken = default);

    [Put("/task/{taskId}")]
    Task<string> UpdateTaskAsync(
        string taskId,
        [Body] JsonElement taskUpdate,
        CancellationToken cancellationToken = default);

    [Delete("/task/{taskId}")]
    Task<string> DeleteTaskAsync(
        string taskId,
        [AliasAs("custom_task_ids")] string? customTaskIds,
        [AliasAs("team_id")] string? teamId,
        CancellationToken cancellationToken = default);

    [Post("/task/{taskId}/merge")]
    Task<string> MergeTasksAsync(
        string taskId,
        [Body] JsonElement mergeRequest,
        CancellationToken cancellationToken = default);

    [Headers("Content-Type: application/json")]
    [Get("/task/{taskId}/time_in_status")]
    Task<string> GetTaskTimeInStatusAsync(
        string taskId,
        [AliasAs("custom_task_ids")] string? customTaskIds,
        [AliasAs("team_id")] string? teamId,
        CancellationToken cancellationToken = default);

    [Headers("Content-Type: application/json")]
    [Get("/task/bulk_time_in_status/task_ids")]
    Task<string> GetBulkTaskTimeInStatusAsync(
        [AliasAs("task_ids")] string taskIds,
        [AliasAs("custom_task_ids")] string? customTaskIds,
        [AliasAs("team_id")] string? teamId,
        CancellationToken cancellationToken = default);
}
