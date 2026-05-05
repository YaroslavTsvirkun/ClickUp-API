using ClickUp.Client.Abstractions.Folders;

namespace ClickUp.Client.Categories;

public sealed class ClickUpFoldersClient : ClickUpEndpointClient
{
    private readonly IClickUpFoldersApi _api;

    internal ClickUpFoldersClient(IClickUpFoldersApi api, Uri baseUri)
        : base(baseUri)
    {
        _api = api;
    }

    public string GetFoldersJson(string spaceId, bool archived = false)
    {
        return GetFoldersJsonAsync(spaceId, archived).GetAwaiter().GetResult();
    }

    public Task<string> GetFoldersJsonAsync(
        string spaceId,
        bool archived = false,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.GetFoldersAsync(spaceId, BoolString(archived), ct),
            "GET",
            $"space/{spaceId}/folder",
            cancellationToken);
    }

    public string GetFolderJson(string folderId)
    {
        return GetFolderJsonAsync(folderId).GetAwaiter().GetResult();
    }

    public Task<string> GetFolderJsonAsync(
        string folderId,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.GetFolderAsync(folderId, ct),
            "GET",
            $"folder/{folderId}",
            cancellationToken);
    }

    public string CreateFolderJson(string spaceId, string folderJson)
    {
        return CreateFolderJsonAsync(spaceId, folderJson).GetAwaiter().GetResult();
    }

    public Task<string> CreateFolderJsonAsync(
        string spaceId,
        string folderJson,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.CreateFolderAsync(spaceId, ParseJson(folderJson), ct),
            "POST",
            $"space/{spaceId}/folder",
            cancellationToken);
    }

    public string UpdateFolderJson(string folderId, string folderJson)
    {
        return UpdateFolderJsonAsync(folderId, folderJson).GetAwaiter().GetResult();
    }

    public Task<string> UpdateFolderJsonAsync(
        string folderId,
        string folderJson,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.UpdateFolderAsync(folderId, ParseJson(folderJson), ct),
            "PUT",
            $"folder/{folderId}",
            cancellationToken);
    }

    public string DeleteFolderJson(string folderId)
    {
        return DeleteFolderJsonAsync(folderId).GetAwaiter().GetResult();
    }

    public Task<string> DeleteFolderJsonAsync(
        string folderId,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.DeleteFolderAsync(folderId, ct),
            "DELETE",
            $"folder/{folderId}",
            cancellationToken);
    }
}
