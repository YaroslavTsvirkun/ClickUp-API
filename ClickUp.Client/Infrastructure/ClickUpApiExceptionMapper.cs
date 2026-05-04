using System.Text.Json;
using Refit;

namespace ClickUp.Client.Infrastructure;

internal static class ClickUpApiExceptionMapper
{
    public static ClickUpApiException Map(ApiException exception)
    {
        var body = exception.Content;
        var details = TryGetErrorSummary(body);
        var method = exception.HttpMethod?.Method ?? "HTTP";
        var uri = exception.Uri ?? new Uri("about:blank");
        var message = details is null
            ? $"ClickUp {method} {uri} failed with {(int)exception.StatusCode} {exception.ReasonPhrase}."
            : $"ClickUp {method} {uri} failed with {(int)exception.StatusCode}: {details}";

        return new ClickUpApiException(
            message,
            method,
            uri,
            exception.StatusCode,
            body,
            exception);
    }

    private static string? TryGetErrorSummary(string? responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            foreach (var propertyName in new[] { "err", "error", "message" })
            {
                if (root.TryGetProperty(propertyName, out var property) &&
                    property.ValueKind == JsonValueKind.String)
                {
                    return property.GetString();
                }
            }
        }
        catch (JsonException)
        {
            return responseBody;
        }

        return null;
    }
}
