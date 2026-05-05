using System.Text.Json;
using Refit;

namespace ClickUp.Client.Abstractions.Raw;

[Headers("Accept: application/json")]
public interface IClickUpRawApi
{
    [Get("/{**endpoint}")]
    Task<string> GetRawAsync(
        string endpoint,
        [Query(CollectionFormat.Multi)] Dictionary<string, string?> query,
        CancellationToken cancellationToken = default);

    [Post("/{**endpoint}")]
    Task<string> PostRawAsync(
        string endpoint,
        [Body] JsonElement body,
        CancellationToken cancellationToken = default);

    [Put("/{**endpoint}")]
    Task<string> PutRawAsync(
        string endpoint,
        [Body] JsonElement body,
        CancellationToken cancellationToken = default);

    [Patch("/{**endpoint}")]
    Task<string> PatchRawAsync(
        string endpoint,
        [Body] JsonElement body,
        CancellationToken cancellationToken = default);

    [Delete("/{**endpoint}")]
    Task<string> DeleteRawAsync(string endpoint, CancellationToken cancellationToken = default);
}
