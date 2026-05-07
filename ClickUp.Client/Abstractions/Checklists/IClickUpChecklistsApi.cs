using System.Text.Json;
using Refit;

namespace ClickUp.Client.Abstractions.Checklists;

[Headers("Accept: application/json")]
public interface IClickUpChecklistsApi
{
    [Post("/task/{taskId}/checklist")]
    Task<string> CreateChecklistAsync(
        string taskId,
        [AliasAs("custom_task_ids")] string? customTaskIds,
        [AliasAs("team_id")] string? teamId,
        [Body] JsonElement checklist,
        CancellationToken cancellationToken = default);

    [Put("/checklist/{checklistId}")]
    Task<string> UpdateChecklistAsync(
        string checklistId,
        [Body] JsonElement checklist,
        CancellationToken cancellationToken = default);

    [Delete("/checklist/{checklistId}")]
    Task<string> DeleteChecklistAsync(
        string checklistId,
        CancellationToken cancellationToken = default);

    [Post("/checklist/{checklistId}/checklist_item")]
    Task<string> CreateChecklistItemAsync(
        string checklistId,
        [Body] JsonElement checklistItem,
        CancellationToken cancellationToken = default);

    [Put("/checklist/{checklistId}/checklist_item/{checklistItemId}")]
    Task<string> UpdateChecklistItemAsync(
        string checklistId,
        string checklistItemId,
        [Body] JsonElement checklistItem,
        CancellationToken cancellationToken = default);

    [Delete("/checklist/{checklistId}/checklist_item/{checklistItemId}")]
    Task<string> DeleteChecklistItemAsync(
        string checklistId,
        string checklistItemId,
        CancellationToken cancellationToken = default);
}
