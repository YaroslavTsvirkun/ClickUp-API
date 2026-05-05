using ClickUp.Client.Abstractions.Lists;

namespace ClickUp.Client.Categories;

public sealed class ClickUpListsClient : ClickUpEndpointClient
{
    private readonly IClickUpListsApi _api;

    internal ClickUpListsClient(IClickUpListsApi api, Uri baseUri)
        : base(baseUri)
    {
        _api = api;
    }

    public string GetFolderlessListsJson(string spaceId, bool archived = false)
    {
        return GetFolderlessListsJsonAsync(spaceId, archived).GetAwaiter().GetResult();
    }

    public Task<string> GetFolderlessListsJsonAsync(
        string spaceId,
        bool archived = false,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.GetFolderlessListsAsync(spaceId, BoolString(archived), ct),
            "GET",
            $"space/{spaceId}/list",
            cancellationToken);
    }

    public string GetFolderListsJson(string folderId, bool archived = false)
    {
        return GetFolderListsJsonAsync(folderId, archived).GetAwaiter().GetResult();
    }

    public Task<string> GetFolderListsJsonAsync(
        string folderId,
        bool archived = false,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.GetFolderListsAsync(folderId, BoolString(archived), ct),
            "GET",
            $"folder/{folderId}/list",
            cancellationToken);
    }

    public string GetListJson(string listId)
    {
        return GetListJsonAsync(listId).GetAwaiter().GetResult();
    }

    public Task<string> GetListJsonAsync(
        string listId,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.GetListAsync(listId, ct),
            "GET",
            $"list/{listId}",
            cancellationToken);
    }

    public string CreateListJson(string folderId, string listJson)
    {
        return CreateListJsonAsync(folderId, listJson).GetAwaiter().GetResult();
    }

    public Task<string> CreateListJsonAsync(
        string folderId,
        string listJson,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.CreateListAsync(folderId, ParseJson(listJson), ct),
            "POST",
            $"folder/{folderId}/list",
            cancellationToken);
    }

    public string CreateFolderlessListJson(string spaceId, string listJson)
    {
        return CreateFolderlessListJsonAsync(spaceId, listJson).GetAwaiter().GetResult();
    }

    public Task<string> CreateFolderlessListJsonAsync(
        string spaceId,
        string listJson,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.CreateFolderlessListAsync(spaceId, ParseJson(listJson), ct),
            "POST",
            $"space/{spaceId}/list",
            cancellationToken);
    }

    public string UpdateListJson(string listId, string listJson)
    {
        return UpdateListJsonAsync(listId, listJson).GetAwaiter().GetResult();
    }

    public Task<string> UpdateListJsonAsync(
        string listId,
        string listJson,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.UpdateListAsync(listId, ParseJson(listJson), ct),
            "PUT",
            $"list/{listId}",
            cancellationToken);
    }

    public string DeleteListJson(string listId)
    {
        return DeleteListJsonAsync(listId).GetAwaiter().GetResult();
    }

    public Task<string> DeleteListJsonAsync(
        string listId,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.DeleteListAsync(listId, ct),
            "DELETE",
            $"list/{listId}",
            cancellationToken);
    }

    public string AddTaskToListJson(string listId, string taskId)
    {
        return AddTaskToListJsonAsync(listId, taskId).GetAwaiter().GetResult();
    }

    public Task<string> AddTaskToListJsonAsync(
        string listId,
        string taskId,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.AddTaskToListAsync(listId, taskId, ct),
            "POST",
            $"list/{listId}/task/{taskId}",
            cancellationToken);
    }

    public string RemoveTaskFromListJson(string listId, string taskId)
    {
        return RemoveTaskFromListJsonAsync(listId, taskId).GetAwaiter().GetResult();
    }

    public Task<string> RemoveTaskFromListJsonAsync(
        string listId,
        string taskId,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.RemoveTaskFromListAsync(listId, taskId, ct),
            "DELETE",
            $"list/{listId}/task/{taskId}",
            cancellationToken);
    }
}
