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
}
