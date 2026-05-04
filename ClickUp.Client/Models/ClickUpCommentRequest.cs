using System.Text.Json.Serialization;

namespace ClickUp.Client.Models;

public sealed record ClickUpCommentRequest(
    [property: JsonPropertyName("comment_text")] string CommentText,
    [property: JsonPropertyName("notify_all")] bool NotifyAll);
