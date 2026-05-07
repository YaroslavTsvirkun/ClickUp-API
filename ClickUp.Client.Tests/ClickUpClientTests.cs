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
    public async Task SupportsSpaceCrudEndpoints()
    {
        var responses = new Queue<HttpResponseMessage>(new[]
        {
            TestHttp.JsonResponse("""{"id":"space_1"}"""),
            TestHttp.JsonResponse("""{"id":"space_2"}"""),
            TestHttp.JsonResponse("""{"id":"space_2"}"""),
            TestHttp.JsonResponse("""{}"""),
        });
        using var handler = new RecordingHandler(_ => responses.Dequeue());
        using var client = TestClient.Create(handler);

        await client.Spaces.GetSpaceJsonAsync("space 1", TestContext.Current.CancellationToken);
        await client.Spaces.CreateSpaceJsonAsync(
            "team_1",
            """{"name":"Engineering","multiple_assignees":true}""",
            TestContext.Current.CancellationToken);
        await client.Spaces.UpdateSpaceJsonAsync(
            "space_2",
            """{"name":"Platform"}""",
            TestContext.Current.CancellationToken);
        await client.Spaces.DeleteSpaceJsonAsync("space_3", TestContext.Current.CancellationToken);

        Assert.Equal(4, handler.Requests.Count);
        Assert.Equal("https://api.clickup.com/api/v2/space/space%201", handler.Requests[0].Uri.AbsoluteUri);
        Assert.Equal("GET", handler.Requests[0].Method);
        Assert.Equal("https://api.clickup.com/api/v2/team/team_1/space", handler.Requests[1].Uri.AbsoluteUri);
        Assert.Equal("POST", handler.Requests[1].Method);
        Assert.Equal("""{"name":"Engineering","multiple_assignees":true}""", handler.Requests[1].Body);
        Assert.Equal("https://api.clickup.com/api/v2/space/space_2", handler.Requests[2].Uri.AbsoluteUri);
        Assert.Equal("PUT", handler.Requests[2].Method);
        Assert.Equal("""{"name":"Platform"}""", handler.Requests[2].Body);
        Assert.Equal("https://api.clickup.com/api/v2/space/space_3", handler.Requests[3].Uri.AbsoluteUri);
        Assert.Equal("DELETE", handler.Requests[3].Method);
    }

    [Fact]
    public async Task SupportsFolderCrudEndpoints()
    {
        var responses = new Queue<HttpResponseMessage>(new[]
        {
            TestHttp.JsonResponse("""{"id":"folder_1"}"""),
            TestHttp.JsonResponse("""{"id":"folder_2"}"""),
            TestHttp.JsonResponse("""{"id":"folder_2"}"""),
            TestHttp.JsonResponse("""{}"""),
        });
        using var handler = new RecordingHandler(_ => responses.Dequeue());
        using var client = TestClient.Create(handler);

        await client.Folders.GetFolderJsonAsync("folder 1", TestContext.Current.CancellationToken);
        await client.Folders.CreateFolderJsonAsync(
            "space_1",
            """{"name":"Backend"}""",
            TestContext.Current.CancellationToken);
        await client.Folders.UpdateFolderJsonAsync(
            "folder_2",
            """{"name":"Platform"}""",
            TestContext.Current.CancellationToken);
        await client.Folders.DeleteFolderJsonAsync("folder_3", TestContext.Current.CancellationToken);

        Assert.Equal(4, handler.Requests.Count);
        Assert.Equal("https://api.clickup.com/api/v2/folder/folder%201", handler.Requests[0].Uri.AbsoluteUri);
        Assert.Equal("GET", handler.Requests[0].Method);
        Assert.Equal("https://api.clickup.com/api/v2/space/space_1/folder", handler.Requests[1].Uri.AbsoluteUri);
        Assert.Equal("POST", handler.Requests[1].Method);
        Assert.Equal("""{"name":"Backend"}""", handler.Requests[1].Body);
        Assert.Equal("https://api.clickup.com/api/v2/folder/folder_2", handler.Requests[2].Uri.AbsoluteUri);
        Assert.Equal("PUT", handler.Requests[2].Method);
        Assert.Equal("""{"name":"Platform"}""", handler.Requests[2].Body);
        Assert.Equal("https://api.clickup.com/api/v2/folder/folder_3", handler.Requests[3].Uri.AbsoluteUri);
        Assert.Equal("DELETE", handler.Requests[3].Method);
    }

    [Fact]
    public async Task SupportsListCrudAndTaskMembershipEndpoints()
    {
        var responses = new Queue<HttpResponseMessage>(new[]
        {
            TestHttp.JsonResponse("""{"id":"list_1"}"""),
            TestHttp.JsonResponse("""{"id":"list_2"}"""),
            TestHttp.JsonResponse("""{"id":"list_3"}"""),
            TestHttp.JsonResponse("""{"id":"list_3"}"""),
            TestHttp.JsonResponse("""{}"""),
            TestHttp.JsonResponse("""{}"""),
            TestHttp.JsonResponse("""{}"""),
        });
        using var handler = new RecordingHandler(_ => responses.Dequeue());
        using var client = TestClient.Create(handler);

        await client.Lists.GetListJsonAsync("list 1", TestContext.Current.CancellationToken);
        await client.Lists.CreateListJsonAsync(
            "folder_1",
            """{"name":"Sprint"}""",
            TestContext.Current.CancellationToken);
        await client.Lists.CreateFolderlessListJsonAsync(
            "space_1",
            """{"name":"Inbox"}""",
            TestContext.Current.CancellationToken);
        await client.Lists.UpdateListJsonAsync(
            "list_3",
            """{"name":"Backlog"}""",
            TestContext.Current.CancellationToken);
        await client.Lists.DeleteListJsonAsync("list_4", TestContext.Current.CancellationToken);
        await client.Lists.AddTaskToListJsonAsync("list_5", "task_1", TestContext.Current.CancellationToken);
        await client.Lists.RemoveTaskFromListJsonAsync("list_5", "task_1", TestContext.Current.CancellationToken);

        Assert.Equal(7, handler.Requests.Count);
        Assert.Equal("https://api.clickup.com/api/v2/list/list%201", handler.Requests[0].Uri.AbsoluteUri);
        Assert.Equal("GET", handler.Requests[0].Method);
        Assert.Equal("https://api.clickup.com/api/v2/folder/folder_1/list", handler.Requests[1].Uri.AbsoluteUri);
        Assert.Equal("POST", handler.Requests[1].Method);
        Assert.Equal("""{"name":"Sprint"}""", handler.Requests[1].Body);
        Assert.Equal("https://api.clickup.com/api/v2/space/space_1/list", handler.Requests[2].Uri.AbsoluteUri);
        Assert.Equal("POST", handler.Requests[2].Method);
        Assert.Equal("""{"name":"Inbox"}""", handler.Requests[2].Body);
        Assert.Equal("https://api.clickup.com/api/v2/list/list_3", handler.Requests[3].Uri.AbsoluteUri);
        Assert.Equal("PUT", handler.Requests[3].Method);
        Assert.Equal("""{"name":"Backlog"}""", handler.Requests[3].Body);
        Assert.Equal("https://api.clickup.com/api/v2/list/list_4", handler.Requests[4].Uri.AbsoluteUri);
        Assert.Equal("DELETE", handler.Requests[4].Method);
        Assert.Equal("https://api.clickup.com/api/v2/list/list_5/task/task_1", handler.Requests[5].Uri.AbsoluteUri);
        Assert.Equal("POST", handler.Requests[5].Method);
        Assert.Equal("https://api.clickup.com/api/v2/list/list_5/task/task_1", handler.Requests[6].Uri.AbsoluteUri);
        Assert.Equal("DELETE", handler.Requests[6].Method);
    }

    [Fact]
    public async Task SupportsTaskMergeAndTimeInStatusEndpoints()
    {
        var responses = new Queue<HttpResponseMessage>(new[]
        {
            TestHttp.JsonResponse("""{"id":"task_1"}"""),
            TestHttp.JsonResponse("""{"current_status":{"status":"in progress"}}"""),
            TestHttp.JsonResponse("""{"tasks":[{"task_id":"task_1"},{"task_id":"task_2"}]}"""),
        });
        using var handler = new RecordingHandler(_ => responses.Dequeue());
        using var client = TestClient.Create(handler);

        await client.Tasks.MergeTasksJsonAsync(
            "task_1",
            ["task_2", "task_3"],
            TestContext.Current.CancellationToken);
        await client.Tasks.GetTaskTimeInStatusJsonAsync(
            "task 1",
            customTaskIds: true,
            teamId: "321",
            cancellationToken: TestContext.Current.CancellationToken);
        await client.Tasks.GetBulkTaskTimeInStatusJsonAsync(
            ["task_1", "task_2"],
            customTaskIds: true,
            teamId: "321",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(3, handler.Requests.Count);
        Assert.Equal("https://api.clickup.com/api/v2/task/task_1/merge", handler.Requests[0].Uri.AbsoluteUri);
        Assert.Equal("POST", handler.Requests[0].Method);
        Assert.Equal("""{"source_task_ids":["task_2","task_3"]}""", handler.Requests[0].Body);
        Assert.Equal(
            "https://api.clickup.com/api/v2/task/task%201/time_in_status?custom_task_ids=true&team_id=321",
            handler.Requests[1].Uri.AbsoluteUri);
        Assert.Equal("GET", handler.Requests[1].Method);
        Assert.Equal(
            "https://api.clickup.com/api/v2/task/bulk_time_in_status/task_ids?task_ids=task_1%2Ctask_2&custom_task_ids=true&team_id=321",
            handler.Requests[2].Uri.AbsoluteUri);
        Assert.Equal("GET", handler.Requests[2].Method);
    }

    [Fact]
    public async Task SupportsTaskDependencyEndpoints()
    {
        var responses = new Queue<HttpResponseMessage>(new[]
        {
            TestHttp.JsonResponse("""{}"""),
            TestHttp.JsonResponse("""{}"""),
            TestHttp.JsonResponse("""{}"""),
            TestHttp.JsonResponse("""{}"""),
        });
        using var handler = new RecordingHandler(_ => responses.Dequeue());
        using var client = TestClient.Create(handler);

        await client.Tasks.AddTaskDependencyJsonAsync(
            "task 1",
            dependsOnTaskId: "task_2",
            customTaskIds: true,
            teamId: "321",
            cancellationToken: TestContext.Current.CancellationToken);
        await client.Tasks.AddTaskDependencyJsonAsync(
            "task_1",
            dependencyOfTaskId: "task 3",
            cancellationToken: TestContext.Current.CancellationToken);
        await client.Tasks.DeleteTaskDependencyJsonAsync(
            "task_1",
            dependsOnTaskId: "task_2",
            customTaskIds: true,
            teamId: "321",
            cancellationToken: TestContext.Current.CancellationToken);
        await client.Tasks.DeleteTaskDependencyJsonAsync(
            "task_1",
            dependencyOfTaskId: "task 3",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(4, handler.Requests.Count);
        Assert.Equal(
            "https://api.clickup.com/api/v2/task/task%201/dependency?custom_task_ids=true&team_id=321",
            handler.Requests[0].Uri.AbsoluteUri);
        Assert.Equal("POST", handler.Requests[0].Method);
        Assert.Equal("""{"depends_on":"task_2","dependency_of":null}""", handler.Requests[0].Body);
        Assert.Equal("https://api.clickup.com/api/v2/task/task_1/dependency", handler.Requests[1].Uri.AbsoluteUri);
        Assert.Equal("POST", handler.Requests[1].Method);
        Assert.Equal("""{"depends_on":null,"dependency_of":"task 3"}""", handler.Requests[1].Body);
        Assert.Equal(
            "https://api.clickup.com/api/v2/task/task_1/dependency?depends_on=task_2&custom_task_ids=true&team_id=321",
            handler.Requests[2].Uri.AbsoluteUri);
        Assert.Equal("DELETE", handler.Requests[2].Method);
        Assert.Equal(
            "https://api.clickup.com/api/v2/task/task_1/dependency?dependency_of=task%203",
            handler.Requests[3].Uri.AbsoluteUri);
        Assert.Equal("DELETE", handler.Requests[3].Method);
    }

    [Fact]
    public async Task SupportsTaskLinkEndpoints()
    {
        var responses = new Queue<HttpResponseMessage>(new[]
        {
            TestHttp.JsonResponse("""{}"""),
            TestHttp.JsonResponse("""{}"""),
        });
        using var handler = new RecordingHandler(_ => responses.Dequeue());
        using var client = TestClient.Create(handler);

        await client.Tasks.AddTaskLinkJsonAsync(
            "task 1",
            "task 2",
            customTaskIds: true,
            teamId: "321",
            cancellationToken: TestContext.Current.CancellationToken);
        await client.Tasks.DeleteTaskLinkJsonAsync(
            "task_1",
            "task 2",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal(
            "https://api.clickup.com/api/v2/task/task%201/link/task%202?custom_task_ids=true&team_id=321",
            handler.Requests[0].Uri.AbsoluteUri);
        Assert.Equal("POST", handler.Requests[0].Method);
        Assert.Equal("https://api.clickup.com/api/v2/task/task_1/link/task%202", handler.Requests[1].Uri.AbsoluteUri);
        Assert.Equal("DELETE", handler.Requests[1].Method);
    }

    [Fact]
    public async Task SupportsFolderViewsAndViewTasksEndpoints()
    {
        var responses = new Queue<HttpResponseMessage>(new[]
        {
            TestHttp.JsonResponse("""{"views":[],"required_views":[]}"""),
            TestHttp.JsonResponse("""{"id":"view_1"}"""),
            TestHttp.JsonResponse("""{"tasks":[]}"""),
        });
        using var handler = new RecordingHandler(_ => responses.Dequeue());
        using var client = TestClient.Create(handler);

        const string viewJson = """{"name":"Backlog View","type":"list","grouping":{"field":"status","dir":1},"divide":{"field":"none","dir":1},"sorting":{"fields":[{"field":"dateCreated","dir":-1}]},"filters":{"op":"AND","fields":[],"search":"","show_closed":false},"columns":{"fields":[{"field":"dateCreated"}]},"team_sidebar":{"assignees":[],"assigned_comments":false,"unassigned_tasks":false},"settings":{"show_task_locations":true,"show_subtasks":3,"show_subtask_parent_names":true,"show_closed_subtasks":false}}""";

        await client.Views.GetFolderViewsJsonAsync("folder 1", TestContext.Current.CancellationToken);
        await client.Views.CreateFolderViewJsonAsync(
            "folder_1",
            viewJson,
            TestContext.Current.CancellationToken);
        await client.Views.GetViewTasksJsonAsync(
            "view 1",
            page: 2,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(3, handler.Requests.Count);
        Assert.Equal("https://api.clickup.com/api/v2/folder/folder%201/view", handler.Requests[0].Uri.AbsoluteUri);
        Assert.Equal("GET", handler.Requests[0].Method);
        Assert.Equal("https://api.clickup.com/api/v2/folder/folder_1/view", handler.Requests[1].Uri.AbsoluteUri);
        Assert.Equal("POST", handler.Requests[1].Method);
        Assert.Equal(viewJson, handler.Requests[1].Body);
        Assert.Equal("https://api.clickup.com/api/v2/view/view%201/task?page=2", handler.Requests[2].Uri.AbsoluteUri);
        Assert.Equal("GET", handler.Requests[2].Method);
    }

    [Fact]
    public async Task ValidatesTaskDependencyArguments()
    {
        using var handler = new RecordingHandler(_ => TestHttp.JsonResponse("""{}"""));
        using var client = TestClient.Create(handler);

        await Assert.ThrowsAsync<ArgumentException>(() => client.Tasks.AddTaskDependencyJsonAsync(
            "task_1",
            dependsOnTaskId: "task_2",
            dependencyOfTaskId: "task_3",
            cancellationToken: TestContext.Current.CancellationToken));

        await Assert.ThrowsAsync<ArgumentException>(() => client.Tasks.DeleteTaskDependencyJsonAsync(
            "task_1",
            dependsOnTaskId: "task_2",
            customTaskIds: true,
            cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SupportsChecklistEndpoints()
    {
        var responses = new Queue<HttpResponseMessage>(new[]
        {
            TestHttp.JsonResponse("""{"id":"checklist_1"}"""),
            TestHttp.JsonResponse("""{"id":"checklist_1"}"""),
            TestHttp.JsonResponse("""{}"""),
            TestHttp.JsonResponse("""{"id":"item_1"}"""),
            TestHttp.JsonResponse("""{"id":"item_1"}"""),
            TestHttp.JsonResponse("""{}"""),
        });
        using var handler = new RecordingHandler(_ => responses.Dequeue());
        using var client = TestClient.Create(handler);

        await client.Checklists.CreateChecklistJsonAsync(
            "task 1",
            """{"name":"Release"}""",
            customTaskIds: true,
            teamId: "321",
            cancellationToken: TestContext.Current.CancellationToken);
        await client.Checklists.UpdateChecklistJsonAsync(
            "checklist_1",
            """{"name":"Release v2","position":0}""",
            TestContext.Current.CancellationToken);
        await client.Checklists.DeleteChecklistJsonAsync(
            "checklist_1",
            TestContext.Current.CancellationToken);
        await client.Checklists.CreateChecklistItemJsonAsync(
            "checklist_1",
            """{"name":"Publish package","assignee":183}""",
            TestContext.Current.CancellationToken);
        await client.Checklists.UpdateChecklistItemJsonAsync(
            "checklist_1",
            "item_1",
            """{"name":"Publish package","resolved":true}""",
            TestContext.Current.CancellationToken);
        await client.Checklists.DeleteChecklistItemJsonAsync(
            "checklist_1",
            "item_1",
            TestContext.Current.CancellationToken);

        Assert.Equal(6, handler.Requests.Count);
        Assert.Equal(
            "https://api.clickup.com/api/v2/task/task%201/checklist?custom_task_ids=true&team_id=321",
            handler.Requests[0].Uri.AbsoluteUri);
        Assert.Equal("POST", handler.Requests[0].Method);
        Assert.Equal("""{"name":"Release"}""", handler.Requests[0].Body);
        Assert.Equal("https://api.clickup.com/api/v2/checklist/checklist_1", handler.Requests[1].Uri.AbsoluteUri);
        Assert.Equal("PUT", handler.Requests[1].Method);
        Assert.Equal("""{"name":"Release v2","position":0}""", handler.Requests[1].Body);
        Assert.Equal("https://api.clickup.com/api/v2/checklist/checklist_1", handler.Requests[2].Uri.AbsoluteUri);
        Assert.Equal("DELETE", handler.Requests[2].Method);
        Assert.Equal(
            "https://api.clickup.com/api/v2/checklist/checklist_1/checklist_item",
            handler.Requests[3].Uri.AbsoluteUri);
        Assert.Equal("POST", handler.Requests[3].Method);
        Assert.Equal("""{"name":"Publish package","assignee":183}""", handler.Requests[3].Body);
        Assert.Equal(
            "https://api.clickup.com/api/v2/checklist/checklist_1/checklist_item/item_1",
            handler.Requests[4].Uri.AbsoluteUri);
        Assert.Equal("PUT", handler.Requests[4].Method);
        Assert.Equal("""{"name":"Publish package","resolved":true}""", handler.Requests[4].Body);
        Assert.Equal(
            "https://api.clickup.com/api/v2/checklist/checklist_1/checklist_item/item_1",
            handler.Requests[5].Uri.AbsoluteUri);
        Assert.Equal("DELETE", handler.Requests[5].Method);
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
