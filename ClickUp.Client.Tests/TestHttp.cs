using System.Net;
using System.Text;

namespace ClickUp.Client.Tests;

internal static class TestHttp
{
    public static HttpResponseMessage JsonResponse(
        string json,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
    }
}
