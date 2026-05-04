using System.Text.Json;
using ClickUp.Client.Abstractions.Comments;
using ClickUp.Client.Abstractions.Folders;
using ClickUp.Client.Abstractions.Lists;
using ClickUp.Client.Abstractions.Raw;
using ClickUp.Client.Abstractions.Spaces;
using ClickUp.Client.Abstractions.Tags;
using ClickUp.Client.Abstractions.Tasks;
using ClickUp.Client.Abstractions.Users;
using ClickUp.Client.Abstractions.Workspaces;
using ClickUp.Client.Categories;
using ClickUp.Client.Infrastructure;

namespace ClickUp.Client;

public sealed class ClickUpClient : IDisposable
{
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
    };

    private readonly HttpClient? _httpClient;

    public ClickUpClient(string apiToken)
        : this(apiToken, options: null)
    {
    }

    public ClickUpClient(string apiToken, ClickUpClientOptions? options)
        : this(ClickUpHttpClientFactory.Create(apiToken, options ?? new ClickUpClientOptions()))
    {
    }

    public ClickUpClient(
        string apiToken,
        HttpMessageHandler primaryHandler,
        ClickUpClientOptions? options = null)
        : this(ClickUpHttpClientFactory.Create(
            apiToken,
            options ?? new ClickUpClientOptions(),
            primaryHandler))
    {
    }

    internal ClickUpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        var baseUri = httpClient.BaseAddress ?? new Uri("https://api.clickup.com/api/v2");

        Users = new ClickUpUsersClient(
            ClickUpHttpClientFactory.CreateApi<IClickUpUsersApi>(httpClient),
            baseUri);
        Workspaces = new ClickUpWorkspacesClient(
            ClickUpHttpClientFactory.CreateApi<IClickUpWorkspacesApi>(httpClient),
            baseUri);
        Spaces = new ClickUpSpacesClient(
            ClickUpHttpClientFactory.CreateApi<IClickUpSpacesApi>(httpClient),
            baseUri);
        Folders = new ClickUpFoldersClient(
            ClickUpHttpClientFactory.CreateApi<IClickUpFoldersApi>(httpClient),
            baseUri);
        Lists = new ClickUpListsClient(
            ClickUpHttpClientFactory.CreateApi<IClickUpListsApi>(httpClient),
            baseUri);
        Tasks = new ClickUpTasksClient(
            ClickUpHttpClientFactory.CreateApi<IClickUpTasksApi>(httpClient),
            baseUri);
        Tags = new ClickUpTagsClient(
            ClickUpHttpClientFactory.CreateApi<IClickUpTagsApi>(httpClient),
            baseUri);
        Comments = new ClickUpCommentsClient(
            ClickUpHttpClientFactory.CreateApi<IClickUpCommentsApi>(httpClient),
            baseUri);
        Raw = new ClickUpRawClient(
            ClickUpHttpClientFactory.CreateApi<IClickUpRawApi>(httpClient),
            baseUri);
    }

    internal ClickUpClient(
        IClickUpUsersApi usersApi,
        IClickUpWorkspacesApi workspacesApi,
        IClickUpSpacesApi spacesApi,
        IClickUpFoldersApi foldersApi,
        IClickUpListsApi listsApi,
        IClickUpTasksApi tasksApi,
        IClickUpTagsApi tagsApi,
        IClickUpCommentsApi commentsApi,
        IClickUpRawApi rawApi,
        Uri baseUri)
    {
        Users = new ClickUpUsersClient(usersApi, baseUri);
        Workspaces = new ClickUpWorkspacesClient(workspacesApi, baseUri);
        Spaces = new ClickUpSpacesClient(spacesApi, baseUri);
        Folders = new ClickUpFoldersClient(foldersApi, baseUri);
        Lists = new ClickUpListsClient(listsApi, baseUri);
        Tasks = new ClickUpTasksClient(tasksApi, baseUri);
        Tags = new ClickUpTagsClient(tagsApi, baseUri);
        Comments = new ClickUpCommentsClient(commentsApi, baseUri);
        Raw = new ClickUpRawClient(rawApi, baseUri);
    }

    public ClickUpUsersClient Users { get; }

    public ClickUpWorkspacesClient Workspaces { get; }

    public ClickUpSpacesClient Spaces { get; }

    public ClickUpFoldersClient Folders { get; }

    public ClickUpListsClient Lists { get; }

    public ClickUpTasksClient Tasks { get; }

    public ClickUpTagsClient Tags { get; }

    public ClickUpCommentsClient Comments { get; }

    public ClickUpRawClient Raw { get; }

    public static ClickUpClient FromEnvironment()
    {
        return FromEnvironment("CLICKUP_API_TOKEN");
    }

    public static ClickUpClient FromEnvironment(string variableName)
    {
        var token = Environment.GetEnvironmentVariable(variableName);

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException($"Environment variable '{variableName}' is not set.");
        }

        return new ClickUpClient(token);
    }

    public string RequestJson(string method, string endpoint)
    {
        return Raw.RequestJson(method, endpoint);
    }

    public string RequestJson(string method, string endpoint, string? jsonBody)
    {
        return Raw.RequestJson(method, endpoint, jsonBody);
    }

    public Task<string> RequestJsonAsync(
        string method,
        string endpoint,
        string? jsonBody = null,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default)
    {
        return Raw.RequestJsonAsync(method, endpoint, jsonBody, query, cancellationToken);
    }

    public string GetAuthorizedUserJson()
    {
        return Users.GetAuthorizedUserJson();
    }

    public Task<string> GetAuthorizedUserJsonAsync(CancellationToken cancellationToken = default)
    {
        return Users.GetAuthorizedUserJsonAsync(cancellationToken);
    }

    public string GetTeamsJson()
    {
        return Workspaces.GetTeamsJson();
    }

    public Task<string> GetTeamsJsonAsync(CancellationToken cancellationToken = default)
    {
        return Workspaces.GetTeamsJsonAsync(cancellationToken);
    }

    public string GetSpacesJson(string teamId, bool archived = false)
    {
        return Spaces.GetSpacesJson(teamId, archived);
    }

    public Task<string> GetSpacesJsonAsync(
        string teamId,
        bool archived = false,
        CancellationToken cancellationToken = default)
    {
        return Spaces.GetSpacesJsonAsync(teamId, archived, cancellationToken);
    }

    public string GetFolderlessListsJson(string spaceId, bool archived = false)
    {
        return Lists.GetFolderlessListsJson(spaceId, archived);
    }

    public Task<string> GetFolderlessListsJsonAsync(
        string spaceId,
        bool archived = false,
        CancellationToken cancellationToken = default)
    {
        return Lists.GetFolderlessListsJsonAsync(spaceId, archived, cancellationToken);
    }

    public string GetFoldersJson(string spaceId, bool archived = false)
    {
        return Folders.GetFoldersJson(spaceId, archived);
    }

    public Task<string> GetFoldersJsonAsync(
        string spaceId,
        bool archived = false,
        CancellationToken cancellationToken = default)
    {
        return Folders.GetFoldersJsonAsync(spaceId, archived, cancellationToken);
    }

    public string GetFolderListsJson(string folderId, bool archived = false)
    {
        return Lists.GetFolderListsJson(folderId, archived);
    }

    public Task<string> GetFolderListsJsonAsync(
        string folderId,
        bool archived = false,
        CancellationToken cancellationToken = default)
    {
        return Lists.GetFolderListsJsonAsync(folderId, archived, cancellationToken);
    }

    public string GetTaskJson(string taskId)
    {
        return Tasks.GetTaskJson(taskId);
    }

    public Task<string> GetTaskJsonAsync(
        string taskId,
        CancellationToken cancellationToken = default)
    {
        return Tasks.GetTaskJsonAsync(taskId, cancellationToken);
    }

    public string GetTasksJson(
        string listId,
        int page = 0,
        bool archived = false,
        bool includeClosed = true,
        bool subtasks = true)
    {
        return Tasks.GetTasksJson(listId, page, archived, includeClosed, subtasks);
    }

    public Task<string> GetTasksJsonAsync(
        string listId,
        int page = 0,
        bool archived = false,
        bool includeClosed = true,
        bool subtasks = true,
        CancellationToken cancellationToken = default)
    {
        return Tasks.GetTasksJsonAsync(listId, page, archived, includeClosed, subtasks, cancellationToken);
    }

    public string GetAllTasksJson(
        string listId,
        bool archived = false,
        bool includeClosed = true,
        bool subtasks = true,
        int maxPages = 100)
    {
        return Tasks.GetAllTasksJson(listId, archived, includeClosed, subtasks, maxPages);
    }

    public Task<string> GetAllTasksJsonAsync(
        string listId,
        bool archived = false,
        bool includeClosed = true,
        bool subtasks = true,
        int maxPages = 100,
        CancellationToken cancellationToken = default)
    {
        return Tasks.GetAllTasksJsonAsync(
            listId,
            archived,
            includeClosed,
            subtasks,
            maxPages,
            cancellationToken);
    }

    public string CreateTaskJson(string listId, string taskJson)
    {
        return Tasks.CreateTaskJson(listId, taskJson);
    }

    public Task<string> CreateTaskJsonAsync(
        string listId,
        string taskJson,
        CancellationToken cancellationToken = default)
    {
        return Tasks.CreateTaskJsonAsync(listId, taskJson, cancellationToken);
    }

    public string UpdateTaskJson(string taskId, string taskJson)
    {
        return Tasks.UpdateTaskJson(taskId, taskJson);
    }

    public Task<string> UpdateTaskJsonAsync(
        string taskId,
        string taskJson,
        CancellationToken cancellationToken = default)
    {
        return Tasks.UpdateTaskJsonAsync(taskId, taskJson, cancellationToken);
    }

    public string SetTaskStatusJson(string taskId, string status)
    {
        return Tasks.SetTaskStatusJson(taskId, status);
    }

    public Task<string> SetTaskStatusJsonAsync(
        string taskId,
        string status,
        CancellationToken cancellationToken = default)
    {
        return Tasks.SetTaskStatusJsonAsync(taskId, status, cancellationToken);
    }

    public string CompleteTaskJson(string taskId)
    {
        return Tasks.CompleteTaskJson(taskId);
    }

    public string CompleteTaskJson(string taskId, string status)
    {
        return Tasks.CompleteTaskJson(taskId, status);
    }

    public string AssignTaskJson(string taskId, long[] userIds)
    {
        return Tasks.AssignTaskJson(taskId, userIds);
    }

    public Task<string> AssignTaskJsonAsync(
        string taskId,
        long[] userIds,
        CancellationToken cancellationToken = default)
    {
        return Tasks.AssignTaskJsonAsync(taskId, userIds, cancellationToken);
    }

    public string AssignTaskAndSetStatusJson(string taskId, string status, long[] userIds)
    {
        return Tasks.AssignTaskAndSetStatusJson(taskId, status, userIds);
    }

    public Task<string> AssignTaskAndSetStatusJsonAsync(
        string taskId,
        string status,
        long[] userIds,
        CancellationToken cancellationToken = default)
    {
        return Tasks.AssignTaskAndSetStatusJsonAsync(taskId, status, userIds, cancellationToken);
    }

    public string AddTaskCommentJson(string taskId, string commentText, bool notifyAll = false)
    {
        return Comments.AddTaskCommentJson(taskId, commentText, notifyAll);
    }

    public Task<string> AddTaskCommentJsonAsync(
        string taskId,
        string commentText,
        bool notifyAll = false,
        CancellationToken cancellationToken = default)
    {
        return Comments.AddTaskCommentJsonAsync(taskId, commentText, notifyAll, cancellationToken);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
