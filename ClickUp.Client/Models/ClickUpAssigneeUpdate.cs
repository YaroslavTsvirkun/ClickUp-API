using System.Text.Json.Serialization;

namespace ClickUp.Client.Models;

public sealed record ClickUpAssigneeUpdate(
    [property: JsonPropertyName("add")] IReadOnlyCollection<long> Add);
