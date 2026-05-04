using Refit;

namespace ClickUp.Client.Abstractions.Users;

[Headers("Accept: application/json")]
public interface IClickUpUsersApi
{
    [Get("/user")]
    Task<string> GetAuthorizedUserAsync(CancellationToken cancellationToken = default);

    [Get("/team/{teamId}/user/{userId}")]
    Task<string> GetUserAsync(
        string teamId,
        string userId,
        [AliasAs("include_shared")] string includeShared,
        CancellationToken cancellationToken = default);
}
