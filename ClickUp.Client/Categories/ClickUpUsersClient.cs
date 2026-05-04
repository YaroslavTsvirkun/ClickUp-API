using ClickUp.Client.Abstractions.Users;

namespace ClickUp.Client.Categories;

public sealed class ClickUpUsersClient : ClickUpEndpointClient
{
    private readonly IClickUpUsersApi _api;

    internal ClickUpUsersClient(IClickUpUsersApi api, Uri baseUri)
        : base(baseUri)
    {
        _api = api;
    }

    public string GetAuthorizedUserJson()
    {
        return GetAuthorizedUserJsonAsync().GetAwaiter().GetResult();
    }

    public Task<string> GetAuthorizedUserJsonAsync(CancellationToken cancellationToken = default)
    {
        return SendAsync(_api.GetAuthorizedUserAsync, "GET", "user", cancellationToken);
    }

    public string GetUserJson(string teamId, string userId, bool includeShared = true)
    {
        return GetUserJsonAsync(teamId, userId, includeShared).GetAwaiter().GetResult();
    }

    public Task<string> GetUserJsonAsync(
        string teamId,
        string userId,
        bool includeShared = true,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.GetUserAsync(teamId, userId, BoolString(includeShared), ct),
            "GET",
            $"team/{teamId}/user/{userId}",
            cancellationToken);
    }
}
