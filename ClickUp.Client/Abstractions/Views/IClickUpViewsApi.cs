using System.Text.Json;
using Refit;

namespace ClickUp.Client.Abstractions.Views;

[Headers("Accept: application/json")]
public interface IClickUpViewsApi
{
    [Get("/folder/{folderId}/view")]
    Task<string> GetFolderViewsAsync(
        string folderId,
        CancellationToken cancellationToken = default);

    [Post("/folder/{folderId}/view")]
    Task<string> CreateFolderViewAsync(
        string folderId,
        [Body] JsonElement view,
        CancellationToken cancellationToken = default);

    [Get("/view/{viewId}/task")]
    Task<string> GetViewTasksAsync(
        string viewId,
        [AliasAs("page")] int page,
        CancellationToken cancellationToken = default);
}
