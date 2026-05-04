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
}
