using Refit;

namespace ClickUp.Client.Abstractions.Spaces;

[Headers("Accept: application/json")]
public interface IClickUpSpacesApi
{
    [Get("/team/{teamId}/space")]
    Task<string> GetSpacesAsync(
        string teamId,
        [AliasAs("archived")] string archived,
        CancellationToken cancellationToken = default);
}
