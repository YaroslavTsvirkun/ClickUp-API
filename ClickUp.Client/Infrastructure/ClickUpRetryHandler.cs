using System.Net;

namespace ClickUp.Client.Infrastructure;

internal sealed class ClickUpRetryHandler : DelegatingHandler
{
    private readonly ClickUpClientOptions _options;

    public ClickUpRetryHandler(ClickUpClientOptions options)
    {
        _options = options;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var attempts = Math.Max(1, _options.MaxRetries);
        var bufferedRequest = await BufferedRequest.CreateAsync(request, cancellationToken)
            .ConfigureAwait(false);

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            using var requestAttempt = bufferedRequest.CreateRequest();
            HttpResponseMessage response;

            try
            {
                response = await base.SendAsync(requestAttempt, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception exception) when (attempt < attempts && IsTransient(exception, cancellationToken))
            {
                await DelayBeforeRetry(attempt, response: null, cancellationToken)
                    .ConfigureAwait(false);
                continue;
            }

            if (ShouldRetry(response.StatusCode) && attempt < attempts)
            {
                var delay = GetRetryAfter(response) ?? GetExponentialDelay(attempt);
                response.Dispose();
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                continue;
            }

            return response;
        }

        throw new InvalidOperationException("Retry loop exited unexpectedly.");
    }

    private Task DelayBeforeRetry(
        int attempt,
        HttpResponseMessage? response,
        CancellationToken cancellationToken)
    {
        var delay = GetRetryAfter(response) ?? GetExponentialDelay(attempt);
        return Task.Delay(delay, cancellationToken);
    }

    private TimeSpan GetExponentialDelay(int attempt)
    {
        var delay = TimeSpan.FromMilliseconds(
            _options.BaseRetryDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));

        if (delay > _options.MaxRetryDelay)
        {
            return _options.MaxRetryDelay;
        }

        return delay < TimeSpan.Zero ? TimeSpan.Zero : delay;
    }

    private static bool ShouldRetry(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.TooManyRequests || (int)statusCode >= 500;
    }

    private static bool IsTransient(Exception exception, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        return exception is HttpRequestException or TaskCanceledException;
    }

    private static TimeSpan? GetRetryAfter(HttpResponseMessage? response)
    {
        if (response?.Headers.RetryAfter is null)
        {
            return null;
        }

        if (response.Headers.RetryAfter.Delta.HasValue)
        {
            return response.Headers.RetryAfter.Delta.Value;
        }

        if (response.Headers.RetryAfter.Date.HasValue)
        {
            return response.Headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow;
        }

        return null;
    }

    private sealed class BufferedRequest
    {
        private BufferedRequest(
            HttpMethod method,
            Uri? requestUri,
            Version version,
            HttpVersionPolicy versionPolicy,
            IReadOnlyDictionary<string, string[]> headers,
            byte[]? content,
            IReadOnlyDictionary<string, string[]> contentHeaders)
        {
            Method = method;
            RequestUri = requestUri;
            Version = version;
            VersionPolicy = versionPolicy;
            Headers = headers;
            Content = content;
            ContentHeaders = contentHeaders;
        }

        private HttpMethod Method { get; }

        private Uri? RequestUri { get; }

        private Version Version { get; }

        private HttpVersionPolicy VersionPolicy { get; }

        private IReadOnlyDictionary<string, string[]> Headers { get; }

        private byte[]? Content { get; }

        private IReadOnlyDictionary<string, string[]> ContentHeaders { get; }

        public static async Task<BufferedRequest> CreateAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var headers = request.Headers.ToDictionary(
                header => header.Key,
                header => header.Value.ToArray(),
                StringComparer.OrdinalIgnoreCase);
            byte[]? content = null;
            var contentHeaders = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

            if (request.Content is not null)
            {
                content = await request.Content.ReadAsByteArrayAsync(cancellationToken)
                    .ConfigureAwait(false);
                contentHeaders = request.Content.Headers.ToDictionary(
                    header => header.Key,
                    header => header.Value.ToArray(),
                    StringComparer.OrdinalIgnoreCase);
            }

            return new BufferedRequest(
                request.Method,
                request.RequestUri,
                request.Version,
                request.VersionPolicy,
                headers,
                content,
                contentHeaders);
        }

        public HttpRequestMessage CreateRequest()
        {
            var request = new HttpRequestMessage(Method, RequestUri)
            {
                Version = Version,
                VersionPolicy = VersionPolicy,
            };

            foreach (var header in Headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (Content is not null)
            {
                request.Content = new ByteArrayContent(Content);

                foreach (var header in ContentHeaders)
                {
                    request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return request;
        }
    }
}
