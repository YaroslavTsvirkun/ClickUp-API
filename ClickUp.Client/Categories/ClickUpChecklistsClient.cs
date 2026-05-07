using ClickUp.Client.Abstractions.Checklists;
using ClickUp.Client.Infrastructure;

namespace ClickUp.Client.Categories;

public sealed class ClickUpChecklistsClient : ClickUpEndpointClient
{
    private readonly IClickUpChecklistsApi _api;

    internal ClickUpChecklistsClient(IClickUpChecklistsApi api, Uri baseUri)
        : base(baseUri)
    {
        _api = api;
    }

    public string CreateChecklistJson(
        string taskId,
        string checklistJson,
        bool customTaskIds = false,
        string? teamId = null)
    {
        return CreateChecklistJsonAsync(taskId, checklistJson, customTaskIds, teamId)
            .GetAwaiter()
            .GetResult();
    }

    public Task<string> CreateChecklistJsonAsync(
        string taskId,
        string checklistJson,
        bool customTaskIds = false,
        string? teamId = null,
        CancellationToken cancellationToken = default)
    {
        var customTaskIdsValue = GetCustomTaskIdsValue(customTaskIds, teamId);
        return SendAsync(
            ct => _api.CreateChecklistAsync(
                taskId,
                customTaskIdsValue,
                teamId,
                ParseJson(checklistJson),
                ct),
            "POST",
            $"task/{taskId}/checklist",
            cancellationToken);
    }

    public string UpdateChecklistJson(string checklistId, string checklistJson)
    {
        return UpdateChecklistJsonAsync(checklistId, checklistJson).GetAwaiter().GetResult();
    }

    public Task<string> UpdateChecklistJsonAsync(
        string checklistId,
        string checklistJson,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.UpdateChecklistAsync(checklistId, ParseJson(checklistJson), ct),
            "PUT",
            $"checklist/{checklistId}",
            cancellationToken);
    }

    public string DeleteChecklistJson(string checklistId)
    {
        return DeleteChecklistJsonAsync(checklistId).GetAwaiter().GetResult();
    }

    public Task<string> DeleteChecklistJsonAsync(
        string checklistId,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.DeleteChecklistAsync(checklistId, ct),
            "DELETE",
            $"checklist/{checklistId}",
            cancellationToken);
    }

    public string CreateChecklistItemJson(string checklistId, string checklistItemJson)
    {
        return CreateChecklistItemJsonAsync(checklistId, checklistItemJson).GetAwaiter().GetResult();
    }

    public Task<string> CreateChecklistItemJsonAsync(
        string checklistId,
        string checklistItemJson,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.CreateChecklistItemAsync(checklistId, ParseJson(checklistItemJson), ct),
            "POST",
            $"checklist/{checklistId}/checklist_item",
            cancellationToken);
    }

    public string UpdateChecklistItemJson(
        string checklistId,
        string checklistItemId,
        string checklistItemJson)
    {
        return UpdateChecklistItemJsonAsync(checklistId, checklistItemId, checklistItemJson)
            .GetAwaiter()
            .GetResult();
    }

    public Task<string> UpdateChecklistItemJsonAsync(
        string checklistId,
        string checklistItemId,
        string checklistItemJson,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.UpdateChecklistItemAsync(
                checklistId,
                checklistItemId,
                ParseJson(checklistItemJson),
                ct),
            "PUT",
            $"checklist/{checklistId}/checklist_item/{checklistItemId}",
            cancellationToken);
    }

    public string DeleteChecklistItemJson(string checklistId, string checklistItemId)
    {
        return DeleteChecklistItemJsonAsync(checklistId, checklistItemId).GetAwaiter().GetResult();
    }

    public Task<string> DeleteChecklistItemJsonAsync(
        string checklistId,
        string checklistItemId,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.DeleteChecklistItemAsync(checklistId, checklistItemId, ct),
            "DELETE",
            $"checklist/{checklistId}/checklist_item/{checklistItemId}",
            cancellationToken);
    }

    private static string? GetCustomTaskIdsValue(bool customTaskIds, string? teamId)
    {
        if (!customTaskIds)
        {
            return null;
        }

        ArgumentGuard.ThrowIfNullOrWhiteSpace(teamId, nameof(teamId));
        return "true";
    }
}
