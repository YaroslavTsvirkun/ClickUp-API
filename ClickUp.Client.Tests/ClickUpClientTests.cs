using System.Net;
using System.Text;
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

    [Fact]
    public async Task ExchangesOAuthCodeWithoutAuthorizationHeader()
    {
        using var handler = new RecordingHandler(_ =>
            TestHttp.JsonResponse("""{"access_token":"pk_oauth"}"""));
        using var client = TestClient.CreateOAuth(handler);

        await client.GetAccessTokenJsonAsync(
            "client_id",
            "client_secret",
            "code_123",
            TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);
        Assert.Equal("https://api.clickup.com/api/v2/oauth/token", request.Uri.AbsoluteUri);
        Assert.Equal("POST", request.Method);
        Assert.False(request.Headers.ContainsKey("Authorization"));
        Assert.Equal(
            """{"client_id":"client_id","client_secret":"client_secret","code":"code_123"}""",
            request.Body);
    }

    [Fact]
    public async Task UsesV3BaseUrlForChatRequests()
    {
        using var handler = new RecordingHandler(_ => TestHttp.JsonResponse("""{"data":[]}"""));
        using var client = TestClient.CreateV3(handler);

        await client.GetChatChannelsJsonAsync(
            "workspace 1",
            new Dictionary<string, string?>
            {
                ["limit"] = "25",
                ["cursor"] = "next_page",
            },
            TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(
            "https://api.clickup.com/api/v3/workspaces/workspace%201/chat/channels?limit=25&cursor=next_page",
            request.Uri.AbsoluteUri);
        Assert.Equal("GET", request.Method);
        Assert.Equal("pk_test", request.Headers["Authorization"]);
    }

    [Fact]
    public async Task SupportsPatchRequestsForV3Endpoints()
    {
        using var handler = new RecordingHandler(_ => TestHttp.JsonResponse("""{"id":"channel_1"}"""));
        using var client = TestClient.CreateV3(handler);
        const string body = """{"name":"Renamed channel"}""";

        await client.UpdateChatChannelJsonAsync(
            "workspace_1",
            "channel_1",
            body,
            TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);
        Assert.Equal("https://api.clickup.com/api/v3/workspaces/workspace_1/chat/channels/channel_1", request.Uri.AbsoluteUri);
        Assert.Equal("PATCH", request.Method);
        Assert.Equal(body, request.Body);
        Assert.Equal("application/json; charset=utf-8", request.ContentType);
    }

    [Fact]
    public async Task UploadsV3AttachmentsAsMultipartFormData()
    {
        using var handler = new RecordingHandler(_ => TestHttp.JsonResponse("""{"id":"attachment_1"}"""));
        using var client = TestClient.CreateV3(handler);
        using var content = new MemoryStream(Encoding.UTF8.GetBytes("hello from codex"));

        await client.CreateAttachmentJsonAsync(
            "workspace 1",
            "tasks",
            "task 1",
            content,
            "note.txt",
            overrideFileName: "custom-name.txt",
            cancellationToken: TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(
            "https://api.clickup.com/api/v3/workspaces/workspace%201/tasks/task%201/attachments",
            request.Uri.AbsoluteUri);
        Assert.Equal("POST", request.Method);
        Assert.StartsWith("multipart/form-data;", request.ContentType, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(request.Body);
        Assert.Contains("note.txt", request.Body, StringComparison.Ordinal);
        Assert.Contains("custom-name.txt", request.Body, StringComparison.Ordinal);
        Assert.Contains("hello from codex", request.Body, StringComparison.Ordinal);
    }
}
