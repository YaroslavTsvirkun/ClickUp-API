using System.Net.Http.Headers;

namespace ClickUp.Client.Infrastructure;

internal sealed class ClickUpAuthHandler : DelegatingHandler
{
    private readonly string _apiToken;

    public ClickUpAuthHandler(string apiToken)
    {
        _apiToken = string.IsNullOrWhiteSpace(apiToken)
            ? throw new ArgumentException("ClickUp API token is required.", nameof(apiToken))
            : apiToken;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains("Authorization"))
        {
            request.Headers.TryAddWithoutValidation("Authorization", _apiToken);
        }

        if (request.Headers.Accept.Count == 0)
        {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        return base.SendAsync(request, cancellationToken);
    }
}
