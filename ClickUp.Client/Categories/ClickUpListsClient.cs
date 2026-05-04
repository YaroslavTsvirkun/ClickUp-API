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
}
