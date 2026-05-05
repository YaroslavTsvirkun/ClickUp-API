using System.Text.Json;
using ClickUp.Client.Infrastructure;
using Refit;

namespace ClickUp.Client.Categories;

public abstract class ClickUpEndpointClient
{
    protected ClickUpEndpointClient(Uri baseUri)
    {
        BaseUri = baseUri;
    }

    protected Uri BaseUri { get; }

    protected async Task<string> SendAsync(
        Func<CancellationToken, Task<string>> call,
        string method,
        string endpoint,
        CancellationToken cancellationToken)
    {
        try
        {
            return await call(cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException exception)
        {
            throw ClickUpApiExceptionMapper.Map(exception);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            throw new ClickUpApiException(
                $"ClickUp {method} {BuildUri(endpoint)} failed before receiving a response: {exception.Message}",
                method,
                BuildUri(endpoint),
                innerException: exception);
        }
    }

    protected Uri BuildUri(string endpoint)
    {
        var baseUri = BaseUri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal)
            ? BaseUri.AbsoluteUri
            : $"{BaseUri.AbsoluteUri}/";

        return new Uri(baseUri + NormalizeEndpoint(endpoint), UriKind.Absolute);
    }

    protected static JsonElement ParseJson(string? json)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        return document.RootElement.Clone();
    }

    protected static JsonElement ToJsonElement<T>(T value)
    {
        return JsonSerializer.SerializeToElement(value, ClickUpClient.JsonOptions);
    }

    protected static string NormalizeEndpoint(string endpoint)
    {
        return endpoint.TrimStart('/');
    }

    protected static string BoolString(bool value)
    {
        return value ? "true" : "false";
    }

    protected static string? OptionalBool(bool value)
    {
        return value ? "true" : null;
    }
}
