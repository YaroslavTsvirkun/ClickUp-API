using ClickUp.Client.Abstractions.Spaces;

namespace ClickUp.Client.Categories;

public sealed class ClickUpSpacesClient : ClickUpEndpointClient
{
    private readonly IClickUpSpacesApi _api;

    internal ClickUpSpacesClient(IClickUpSpacesApi api, Uri baseUri)
        : base(baseUri)
    {
        _api = api;
    }

    public string GetSpacesJson(string teamId, bool archived = false)
    {
        return GetSpacesJsonAsync(teamId, archived).GetAwaiter().GetResult();
    }

    public Task<string> GetSpacesJsonAsync(
        string teamId,
        bool archived = false,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.GetSpacesAsync(teamId, BoolString(archived), ct),
            "GET",
            $"team/{teamId}/space",
            cancellationToken);
    }

    public string GetSpaceJson(string spaceId)
    {
        return GetSpaceJsonAsync(spaceId).GetAwaiter().GetResult();
    }

    public Task<string> GetSpaceJsonAsync(
        string spaceId,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.GetSpaceAsync(spaceId, ct),
            "GET",
            $"space/{spaceId}",
            cancellationToken);
    }

    public string CreateSpaceJson(string teamId, string spaceJson)
    {
        return CreateSpaceJsonAsync(teamId, spaceJson).GetAwaiter().GetResult();
    }

    public Task<string> CreateSpaceJsonAsync(
        string teamId,
        string spaceJson,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.CreateSpaceAsync(teamId, ParseJson(spaceJson), ct),
            "POST",
            $"team/{teamId}/space",
            cancellationToken);
    }

    public string UpdateSpaceJson(string spaceId, string spaceJson)
    {
        return UpdateSpaceJsonAsync(spaceId, spaceJson).GetAwaiter().GetResult();
    }

    public Task<string> UpdateSpaceJsonAsync(
        string spaceId,
        string spaceJson,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.UpdateSpaceAsync(spaceId, ParseJson(spaceJson), ct),
            "PUT",
            $"space/{spaceId}",
            cancellationToken);
    }

    public string DeleteSpaceJson(string spaceId)
    {
        return DeleteSpaceJsonAsync(spaceId).GetAwaiter().GetResult();
    }

    public Task<string> DeleteSpaceJsonAsync(
        string spaceId,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.DeleteSpaceAsync(spaceId, ct),
            "DELETE",
            $"space/{spaceId}",
            cancellationToken);
    }
}
