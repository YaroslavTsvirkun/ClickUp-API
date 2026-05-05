using Refit;
using System.Text.Json;

namespace ClickUp.Client.Abstractions.Spaces;

[Headers("Accept: application/json")]
public interface IClickUpSpacesApi
{
    [Get("/team/{teamId}/space")]
    Task<string> GetSpacesAsync(
        string teamId,
        [AliasAs("archived")] string archived,
        CancellationToken cancellationToken = default);

    [Get("/space/{spaceId}")]
    Task<string> GetSpaceAsync(
        string spaceId,
        CancellationToken cancellationToken = default);

    [Post("/team/{teamId}/space")]
    Task<string> CreateSpaceAsync(
        string teamId,
        [Body] JsonElement space,
        CancellationToken cancellationToken = default);

    [Put("/space/{spaceId}")]
    Task<string> UpdateSpaceAsync(
        string spaceId,
        [Body] JsonElement spaceUpdate,
        CancellationToken cancellationToken = default);

    [Delete("/space/{spaceId}")]
    Task<string> DeleteSpaceAsync(
        string spaceId,
        CancellationToken cancellationToken = default);
}
