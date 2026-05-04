using System.Text.Json;
using ClickUp.Client.Abstractions.Tasks;
using ClickUp.Client.Models;

namespace ClickUp.Client.Categories;

public sealed class ClickUpTasksClient : ClickUpEndpointClient
{
    private readonly IClickUpTasksApi _api;

    internal ClickUpTasksClient(IClickUpTasksApi api, Uri baseUri)
        : base(baseUri)
    {
        _api = api;
    }

    public string GetTaskJson(string taskId)
    {
        return GetTaskJsonAsync(taskId).GetAwaiter().GetResult();
    }

    public Task<string> GetTaskJsonAsync(
        string taskId,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.GetTaskAsync(taskId, ct),
            "GET",
            $"task/{taskId}",
            cancellationToken);
    }

    public string GetTasksJson(
        string listId,
        int page = 0,
        bool archived = false,
        bool includeClosed = true,
        bool subtasks = true)
    {
        return GetTasksJsonAsync(listId, page, archived, includeClosed, subtasks)
            .GetAwaiter()
            .GetResult();
    }

    public Task<string> GetTasksJsonAsync(
        string listId,
        int page = 0,
        bool archived = false,
        bool includeClosed = true,
        bool subtasks = true,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.GetTasksAsync(
                listId,
                BoolString(archived),
                BoolString(includeClosed),
                BoolString(subtasks),
                page,
                ct),
            "GET",
            $"list/{listId}/task",
            cancellationToken);
    }

    public string GetAllTasksJson(
        string listId,
        bool archived = false,
        bool includeClosed = true,
        bool subtasks = true,
        int maxPages = 100)
    {
        return GetAllTasksJsonAsync(listId, archived, includeClosed, subtasks, maxPages)
            .GetAwaiter()
            .GetResult();
    }

    public async Task<string> GetAllTasksJsonAsync(
        string listId,
        bool archived = false,
        bool includeClosed = true,
        bool subtasks = true,
        int maxPages = 100,
        CancellationToken cancellationToken = default)
    {
        if (maxPages <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPages), "maxPages must be greater than zero.");
        }

        var tasks = new List<JsonElement>();

        for (var page = 0; page < maxPages; page++)
        {
            var pageJson = await GetTasksJsonAsync(
                listId,
                page,
                archived,
                includeClosed,
                subtasks,
                cancellationToken).ConfigureAwait(false);

            using var document = JsonDocument.Parse(pageJson);
            var root = document.RootElement;
            var pageCount = 0;

            if (root.TryGetProperty("tasks", out var tasksElement) &&
                tasksElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var task in tasksElement.EnumerateArray())
                {
                    tasks.Add(task.Clone());
                    pageCount++;
                }
            }

            var isLastPage =
                root.TryGetProperty("last_page", out var lastPageElement) &&
                lastPageElement.ValueKind == JsonValueKind.True;

            if (isLastPage || pageCount == 0)
            {
                return JsonSerializer.Serialize(tasks, ClickUpClient.JsonOptions);
            }
        }

        throw new InvalidOperationException($"Stopped after {maxPages} task pages for list '{listId}'.");
    }

    public string CreateTaskJson(string listId, string taskJson)
    {
        return CreateTaskJsonAsync(listId, taskJson).GetAwaiter().GetResult();
    }

    public Task<string> CreateTaskJsonAsync(
        string listId,
        string taskJson,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.CreateTaskAsync(listId, ParseJson(taskJson), ct),
            "POST",
            $"list/{listId}/task",
            cancellationToken);
    }

    public string UpdateTaskJson(string taskId, string taskJson)
    {
        return UpdateTaskJsonAsync(taskId, taskJson).GetAwaiter().GetResult();
    }

    public Task<string> UpdateTaskJsonAsync(
        string taskId,
        string taskJson,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.UpdateTaskAsync(taskId, ParseJson(taskJson), ct),
            "PUT",
            $"task/{taskId}",
            cancellationToken);
    }

    public string SetTaskStatusJson(string taskId, string status)
    {
        return SetTaskStatusJsonAsync(taskId, status).GetAwaiter().GetResult();
    }

    public Task<string> SetTaskStatusJsonAsync(
        string taskId,
        string status,
        CancellationToken cancellationToken = default)
    {
        var update = new ClickUpTaskUpdateRequest { Status = status };
        return UpdateTaskAsync(taskId, update, cancellationToken);
    }

    public string CompleteTaskJson(string taskId)
    {
        return CompleteTaskJson(taskId, "complete");
    }

    public string CompleteTaskJson(string taskId, string status)
    {
        return SetTaskStatusJson(taskId, status);
    }

    public string AssignTaskJson(string taskId, long[] userIds)
    {
        return AssignTaskJsonAsync(taskId, userIds).GetAwaiter().GetResult();
    }

    public Task<string> AssignTaskJsonAsync(
        string taskId,
        long[] userIds,
        CancellationToken cancellationToken = default)
    {
        var update = new ClickUpTaskUpdateRequest
        {
            Assignees = new ClickUpAssigneeUpdate(userIds),
        };

        return UpdateTaskAsync(taskId, update, cancellationToken);
    }

    public string AssignTaskAndSetStatusJson(string taskId, string status, long[] userIds)
    {
        return AssignTaskAndSetStatusJsonAsync(taskId, status, userIds).GetAwaiter().GetResult();
    }

    public Task<string> AssignTaskAndSetStatusJsonAsync(
        string taskId,
        string status,
        long[] userIds,
        CancellationToken cancellationToken = default)
    {
        var update = new ClickUpTaskUpdateRequest
        {
            Assignees = new ClickUpAssigneeUpdate(userIds),
            Status = status,
        };

        return UpdateTaskAsync(taskId, update, cancellationToken);
    }

    public string DeleteTaskJson(
        string taskId,
        bool customTaskIds = false,
        string? teamId = null)
    {
        return DeleteTaskJsonAsync(taskId, customTaskIds, teamId).GetAwaiter().GetResult();
    }

    public Task<string> DeleteTaskJsonAsync(
        string taskId,
        bool customTaskIds = false,
        string? teamId = null,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.DeleteTaskAsync(taskId, OptionalBool(customTaskIds), teamId, ct),
            "DELETE",
            $"task/{taskId}",
            cancellationToken);
    }

    private Task<string> UpdateTaskAsync(
        string taskId,
        ClickUpTaskUpdateRequest update,
        CancellationToken cancellationToken)
    {
        return SendAsync(
            ct => _api.UpdateTaskAsync(taskId, ToJsonElement(update), ct),
            "PUT",
            $"task/{taskId}",
            cancellationToken);
    }
}
