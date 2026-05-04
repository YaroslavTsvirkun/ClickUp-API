namespace ClickUp.Client.Tests;

internal sealed record RequestRecord(
    string Method,
    Uri Uri,
    IReadOnlyDictionary<string, string> Headers,
    string? Body,
    string? ContentType);
