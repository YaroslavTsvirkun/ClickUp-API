using System.Text.Json.Serialization;

namespace ClickUp.Client.Models;

public sealed record ClickUpTaskUpdateRequest
{
    [JsonPropertyName("assignees")]
    public ClickUpAssigneeUpdate? Assignees { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }
}
