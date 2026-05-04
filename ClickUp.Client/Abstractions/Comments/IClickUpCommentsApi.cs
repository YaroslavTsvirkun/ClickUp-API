using ClickUp.Client.Models;
using Refit;

namespace ClickUp.Client.Abstractions.Comments;

[Headers("Accept: application/json")]
public interface IClickUpCommentsApi
{
    [Post("/task/{taskId}/comment")]
    Task<string> AddTaskCommentAsync(
        string taskId,
        [Body] ClickUpCommentRequest comment,
        CancellationToken cancellationToken = default);
}
