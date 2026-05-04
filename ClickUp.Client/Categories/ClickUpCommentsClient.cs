using ClickUp.Client.Abstractions.Comments;
using ClickUp.Client.Models;

namespace ClickUp.Client.Categories;

public sealed class ClickUpCommentsClient : ClickUpEndpointClient
{
    private readonly IClickUpCommentsApi _api;

    internal ClickUpCommentsClient(IClickUpCommentsApi api, Uri baseUri)
        : base(baseUri)
    {
        _api = api;
    }

    public string AddTaskCommentJson(string taskId, string commentText, bool notifyAll = false)
    {
        return AddTaskCommentJsonAsync(taskId, commentText, notifyAll).GetAwaiter().GetResult();
    }

    public Task<string> AddTaskCommentJsonAsync(
        string taskId,
        string commentText,
        bool notifyAll = false,
        CancellationToken cancellationToken = default)
    {
        var comment = new ClickUpCommentRequest(commentText, notifyAll);
        return SendAsync(
            ct => _api.AddTaskCommentAsync(taskId, comment, ct),
            "POST",
            $"task/{taskId}/comment",
            cancellationToken);
    }
}
