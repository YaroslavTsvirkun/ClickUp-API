using System.Net;
using System.Text.Json;
using ClickUp.Client;
using Xunit;

namespace ClickUp.Client.Tests;

public sealed class ClickUpClientTests
{
    [Fact]
    public async Task SendsAuthorizationHeaderAndQueryParameters()
    {
        using var handler = new RecordingHandler(_ => TestHttp.JsonResponse("""{"spaces":[]}"""));
        using var client = TestClient.Create(handler);

        await client.GetSpacesJsonAsync(
            "team 1",
            cancellationToken: TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(
            "https://api.clickup.com/api/v2/team/team%201/space?archived=false",
            request.Uri.AbsoluteUri);
        Assert.Equal("GET", request.Method);
        Assert.Equal("pk_test", request.Headers["Authorization"]);
        Assert.Equal("application/json", request.Headers["Accept"]);
    }

    [Fact]
    public async Task SerializesJsonRequestBodies()
    {
        using var handler = new RecordingHandler(_ => TestHttp.JsonResponse("""{"id":"task_1"}"""));
        using var client = TestClient.Create(handler);
        var body = """{"name":"Review API client","priority":3}""";

        await client.CreateTaskJsonAsync(
            "list_1",
            body,
            TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);
        Assert.Equal("https://api.clickup.com/api/v2/list/list_1/task", request.Uri.AbsoluteUri);
        Assert.Equal("POST", request.Method);
        Assert.Equal("application/json; charset=utf-8", request.ContentType);
        Assert.Equal(body, request.Body);
    }

    [Fact]
    public async Task CollectsAllTaskPages()
    {
        var responses = new Queue<HttpResponseMessage>(new[]
        {
            TestHttp.JsonResponse("""{"tasks":[{"id":"1"}],"last_page":false}"""),
            TestHttp.JsonResponse("""{"tasks":[{"id":"2"}],"last_page":true}"""),
        });
        using var handler = new RecordingHandler(_ => responses.Dequeue());
        using var client = TestClient.Create(handler);

        var tasksJson = await client.GetAllTasksJsonAsync(
            "list_1",
            cancellationToken: TestContext.Current.CancellationToken);
        using var document = JsonDocument.Parse(tasksJson);
        var tasks = document.RootElement.EnumerateArray().ToArray();

        Assert.Equal(2, tasks.Length);
        Assert.Equal("1", tasks[0].GetProperty("id").GetString());
        Assert.Equal("2", tasks[1].GetProperty("id").GetString());
        Assert.Contains("page=0", handler.Requests[0].Uri.AbsoluteUri);
        Assert.Contains("page=1", handler.Requests[1].Uri.AbsoluteUri);
    }

    [Fact]
    public async Task ThrowsApiExceptionWithResponseBody()
    {
        using var handler = new RecordingHandler(_ =>
            TestHttp.JsonResponse("""{"err":"Task not found"}""", HttpStatusCode.NotFound));
        using var client = TestClient.Create(handler);

        var exception = await Assert.ThrowsAsync<ClickUpApiException>(
            () => client.GetTaskJsonAsync("missing", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("""{"err":"Task not found"}""", exception.ResponseBody);
        Assert.Contains("Task not found", exception.Message);
    }

    [Fact]
    public async Task RetriesTransientFailures()
    {
        var responses = new Queue<HttpResponseMessage>(new[]
        {
            TestHttp.JsonResponse("""{"err":"Please retry"}""", HttpStatusCode.InternalServerError),
            TestHttp.JsonResponse("""{"user":{"id":123}}"""),
        });
        using var handler = new RecordingHandler(_ => responses.Dequeue());
        using var client = TestClient.Create(handler);

        var userJson = await client.GetAuthorizedUserJsonAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal("""{"user":{"id":123}}""", userJson);
    }

    [Fact]
    public async Task ExposesCategoryClients()
    {
        using var handler = new RecordingHandler(_ => TestHttp.JsonResponse("""{"user":{"id":456}}"""));
        using var client = TestClient.Create(handler);

        await client.Users.GetUserJsonAsync(
            "123",
            "456",
            includeShared: false,
            cancellationToken: TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);
        Assert.Equal("https://api.clickup.com/api/v2/team/123/user/456?include_shared=false", request.Uri.AbsoluteUri);
        Assert.Equal("GET", request.Method);
    }

    [Fact]
    public async Task SendsTagRequestsThroughTagsCategory()
    {
        using var handler = new RecordingHandler(_ => TestHttp.JsonResponse("""{"tags":[]}"""));
        using var client = TestClient.Create(handler);

        await client.Tags.GetSpaceTagsJsonAsync(
            "space 1",
            TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);
        Assert.Equal("https://api.clickup.com/api/v2/space/space%201/tag", request.Uri.AbsoluteUri);
        Assert.Equal("GET", request.Method);
    }
}
