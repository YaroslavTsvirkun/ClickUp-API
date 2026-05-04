namespace ClickUp.Client.Tests;

internal sealed class RecordingHandler : HttpMessageHandler, IDisposable
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

    public RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        _responseFactory = responseFactory;
    }

    public List<RequestRecord> Requests { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var headers = request.Headers.ToDictionary(
            header => header.Key,
            header => string.Join(",", header.Value),
            StringComparer.OrdinalIgnoreCase);
        var body = request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken);

        Requests.Add(new RequestRecord(
            request.Method.Method,
            request.RequestUri ?? throw new InvalidOperationException("RequestUri is null."),
            headers,
            body,
            request.Content?.Headers.ContentType?.ToString()));

        var response = _responseFactory(request);
        response.RequestMessage ??= request;
        return response;
    }
}
