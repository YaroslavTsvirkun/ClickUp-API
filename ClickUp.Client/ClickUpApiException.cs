using System.Net;

namespace ClickUp.Client;

public sealed class ClickUpApiException : Exception
{
    public ClickUpApiException(
        string message,
        string method,
        Uri requestUri,
        HttpStatusCode? statusCode = null,
        string? responseBody = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Method = method;
        RequestUri = requestUri;
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public string Method { get; }

    public Uri RequestUri { get; }

    public HttpStatusCode? StatusCode { get; }

    public string? ResponseBody { get; }
}
