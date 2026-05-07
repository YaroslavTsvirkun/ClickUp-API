using ClickUp.Client.Abstractions.Views;

namespace ClickUp.Client.Categories;

public sealed class ClickUpViewsClient : ClickUpEndpointClient
{
    private readonly IClickUpViewsApi _api;

    internal ClickUpViewsClient(IClickUpViewsApi api, Uri baseUri)
        : base(baseUri)
    {
        _api = api;
    }

    public string GetFolderViewsJson(string folderId)
    {
        return GetFolderViewsJsonAsync(folderId).GetAwaiter().GetResult();
    }

    public Task<string> GetFolderViewsJsonAsync(
        string folderId,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.GetFolderViewsAsync(folderId, ct),
            "GET",
            $"folder/{folderId}/view",
            cancellationToken);
    }

    public string CreateFolderViewJson(string folderId, string viewJson)
    {
        return CreateFolderViewJsonAsync(folderId, viewJson).GetAwaiter().GetResult();
    }

    public Task<string> CreateFolderViewJsonAsync(
        string folderId,
        string viewJson,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.CreateFolderViewAsync(folderId, ParseJson(viewJson), ct),
            "POST",
            $"folder/{folderId}/view",
            cancellationToken);
    }

    public string GetViewTasksJson(string viewId, int page = 0)
    {
        return GetViewTasksJsonAsync(viewId, page).GetAwaiter().GetResult();
    }

    public Task<string> GetViewTasksJsonAsync(
        string viewId,
        int page = 0,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.GetViewTasksAsync(viewId, page, ct),
            "GET",
            $"view/{viewId}/task",
            cancellationToken);
    }
}
