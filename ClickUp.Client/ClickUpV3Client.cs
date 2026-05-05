using System.Net.Http.Headers;
using ClickUp.Client.Abstractions.Raw;
using ClickUp.Client.Categories;
using ClickUp.Client.Infrastructure;

namespace ClickUp.Client;

public sealed class ClickUpV3Client : ClickUpEndpointClient, IDisposable
{
    private const string DefaultV2BaseUrl = "https://api.clickup.com/api/v2";
    private const string DefaultV3BaseUrl = "https://api.clickup.com/api/v3";

    private readonly HttpClient? _httpClient;
    private readonly ClickUpRawClient _raw;

    public ClickUpV3Client(string apiToken)
        : this(apiToken, options: null)
    {
    }

    public ClickUpV3Client(string apiToken, ClickUpClientOptions? options)
        : this(CreateConfiguredHttpClient(apiToken, options))
    {
    }

    public ClickUpV3Client(
        string apiToken,
        HttpMessageHandler primaryHandler,
        ClickUpClientOptions? options = null)
        : this(CreateConfiguredHttpClient(apiToken, options, primaryHandler))
    {
    }

    internal ClickUpV3Client(HttpClient httpClient)
        : this(
            ClickUpHttpClientFactory.CreateApi<IClickUpRawApi>(httpClient),
            httpClient.BaseAddress ?? new Uri(DefaultV3BaseUrl),
            httpClient)
    {
    }

    internal ClickUpV3Client(
        IClickUpRawApi rawApi,
        Uri baseUri,
        HttpClient? httpClient = null)
        : base(baseUri)
    {
        _raw = new ClickUpRawClient(rawApi, baseUri);
        _httpClient = httpClient;
    }

    public static ClickUpV3Client FromEnvironment()
    {
        return FromEnvironment("CLICKUP_API_TOKEN");
    }

    public static ClickUpV3Client FromEnvironment(string variableName)
    {
        var token = Environment.GetEnvironmentVariable(variableName);

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException($"Environment variable '{variableName}' is not set.");
        }

        return new ClickUpV3Client(token);
    }

    public string RequestJson(string method, string endpoint)
    {
        return RequestJsonAsync(method, endpoint).GetAwaiter().GetResult();
    }

    public string RequestJson(string method, string endpoint, string? jsonBody)
    {
        return RequestJsonAsync(method, endpoint, jsonBody).GetAwaiter().GetResult();
    }

    public Task<string> RequestJsonAsync(
        string method,
        string endpoint,
        string? jsonBody = null,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default)
    {
        return _raw.RequestJsonAsync(method, endpoint, jsonBody, query, cancellationToken);
    }

    public string GetChatChannelsJson(
        string workspaceId,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        return GetChatChannelsJsonAsync(workspaceId, query).GetAwaiter().GetResult();
    }

    public Task<string> GetChatChannelsJsonAsync(
        string workspaceId,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "GET",
            $"workspaces/{PathSegment(workspaceId)}/chat/channels",
            query: query,
            cancellationToken: cancellationToken);
    }

    public string CreateChatChannelJson(string workspaceId, string channelJson)
    {
        return CreateChatChannelJsonAsync(workspaceId, channelJson).GetAwaiter().GetResult();
    }

    public Task<string> CreateChatChannelJsonAsync(
        string workspaceId,
        string channelJson,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "POST",
            $"workspaces/{PathSegment(workspaceId)}/chat/channels",
            channelJson,
            cancellationToken: cancellationToken);
    }

    public string CreateLocationChatChannelJson(string workspaceId, string channelJson)
    {
        return CreateLocationChatChannelJsonAsync(workspaceId, channelJson).GetAwaiter().GetResult();
    }

    public Task<string> CreateLocationChatChannelJsonAsync(
        string workspaceId,
        string channelJson,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "POST",
            $"workspaces/{PathSegment(workspaceId)}/chat/channels/location",
            channelJson,
            cancellationToken: cancellationToken);
    }

    public string CreateDirectMessageChatChannelJson(string workspaceId, string channelJson)
    {
        return CreateDirectMessageChatChannelJsonAsync(workspaceId, channelJson)
            .GetAwaiter()
            .GetResult();
    }

    public Task<string> CreateDirectMessageChatChannelJsonAsync(
        string workspaceId,
        string channelJson,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "POST",
            $"workspaces/{PathSegment(workspaceId)}/chat/channels/direct_message",
            channelJson,
            cancellationToken: cancellationToken);
    }

    public string GetChatChannelJson(
        string workspaceId,
        string channelId,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        return GetChatChannelJsonAsync(workspaceId, channelId, query).GetAwaiter().GetResult();
    }

    public Task<string> GetChatChannelJsonAsync(
        string workspaceId,
        string channelId,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "GET",
            $"workspaces/{PathSegment(workspaceId)}/chat/channels/{PathSegment(channelId)}",
            query: query,
            cancellationToken: cancellationToken);
    }

    public string UpdateChatChannelJson(string workspaceId, string channelId, string channelJson)
    {
        return UpdateChatChannelJsonAsync(workspaceId, channelId, channelJson)
            .GetAwaiter()
            .GetResult();
    }

    public Task<string> UpdateChatChannelJsonAsync(
        string workspaceId,
        string channelId,
        string channelJson,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "PATCH",
            $"workspaces/{PathSegment(workspaceId)}/chat/channels/{PathSegment(channelId)}",
            channelJson,
            cancellationToken: cancellationToken);
    }

    public string DeleteChatChannelJson(string workspaceId, string channelId)
    {
        return DeleteChatChannelJsonAsync(workspaceId, channelId).GetAwaiter().GetResult();
    }

    public Task<string> DeleteChatChannelJsonAsync(
        string workspaceId,
        string channelId,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "DELETE",
            $"workspaces/{PathSegment(workspaceId)}/chat/channels/{PathSegment(channelId)}",
            cancellationToken: cancellationToken);
    }

    public string GetChatChannelFollowersJson(
        string workspaceId,
        string channelId,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        return GetChatChannelFollowersJsonAsync(workspaceId, channelId, query)
            .GetAwaiter()
            .GetResult();
    }

    public Task<string> GetChatChannelFollowersJsonAsync(
        string workspaceId,
        string channelId,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "GET",
            $"workspaces/{PathSegment(workspaceId)}/chat/channels/{PathSegment(channelId)}/followers",
            query: query,
            cancellationToken: cancellationToken);
    }

    public string GetChatChannelMembersJson(
        string workspaceId,
        string channelId,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        return GetChatChannelMembersJsonAsync(workspaceId, channelId, query)
            .GetAwaiter()
            .GetResult();
    }

    public Task<string> GetChatChannelMembersJsonAsync(
        string workspaceId,
        string channelId,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "GET",
            $"workspaces/{PathSegment(workspaceId)}/chat/channels/{PathSegment(channelId)}/members",
            query: query,
            cancellationToken: cancellationToken);
    }

    public string GetChatMessagesJson(
        string workspaceId,
        string channelId,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        return GetChatMessagesJsonAsync(workspaceId, channelId, query).GetAwaiter().GetResult();
    }

    public Task<string> GetChatMessagesJsonAsync(
        string workspaceId,
        string channelId,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "GET",
            $"workspaces/{PathSegment(workspaceId)}/chat/channels/{PathSegment(channelId)}/messages",
            query: query,
            cancellationToken: cancellationToken);
    }

    public string CreateChatMessageJson(string workspaceId, string channelId, string messageJson)
    {
        return CreateChatMessageJsonAsync(workspaceId, channelId, messageJson)
            .GetAwaiter()
            .GetResult();
    }

    public Task<string> CreateChatMessageJsonAsync(
        string workspaceId,
        string channelId,
        string messageJson,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "POST",
            $"workspaces/{PathSegment(workspaceId)}/chat/channels/{PathSegment(channelId)}/messages",
            messageJson,
            cancellationToken: cancellationToken);
    }

    public string UpdateChatMessageJson(string workspaceId, string messageId, string messageJson)
    {
        return UpdateChatMessageJsonAsync(workspaceId, messageId, messageJson)
            .GetAwaiter()
            .GetResult();
    }

    public Task<string> UpdateChatMessageJsonAsync(
        string workspaceId,
        string messageId,
        string messageJson,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "PATCH",
            $"workspaces/{PathSegment(workspaceId)}/chat/messages/{PathSegment(messageId)}",
            messageJson,
            cancellationToken: cancellationToken);
    }

    public string DeleteChatMessageJson(string workspaceId, string messageId)
    {
        return DeleteChatMessageJsonAsync(workspaceId, messageId).GetAwaiter().GetResult();
    }

    public Task<string> DeleteChatMessageJsonAsync(
        string workspaceId,
        string messageId,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "DELETE",
            $"workspaces/{PathSegment(workspaceId)}/chat/messages/{PathSegment(messageId)}",
            cancellationToken: cancellationToken);
    }

    public string GetChatMessageReactionsJson(
        string workspaceId,
        string messageId,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        return GetChatMessageReactionsJsonAsync(workspaceId, messageId, query)
            .GetAwaiter()
            .GetResult();
    }

    public Task<string> GetChatMessageReactionsJsonAsync(
        string workspaceId,
        string messageId,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "GET",
            $"workspaces/{PathSegment(workspaceId)}/chat/messages/{PathSegment(messageId)}/reactions",
            query: query,
            cancellationToken: cancellationToken);
    }

    public string CreateChatReactionJson(string workspaceId, string messageId, string reactionJson)
    {
        return CreateChatReactionJsonAsync(workspaceId, messageId, reactionJson)
            .GetAwaiter()
            .GetResult();
    }

    public Task<string> CreateChatReactionJsonAsync(
        string workspaceId,
        string messageId,
        string reactionJson,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "POST",
            $"workspaces/{PathSegment(workspaceId)}/chat/messages/{PathSegment(messageId)}/reactions",
            reactionJson,
            cancellationToken: cancellationToken);
    }

    public string DeleteChatReactionJson(string workspaceId, string messageId, string reaction)
    {
        return DeleteChatReactionJsonAsync(workspaceId, messageId, reaction)
            .GetAwaiter()
            .GetResult();
    }

    public Task<string> DeleteChatReactionJsonAsync(
        string workspaceId,
        string messageId,
        string reaction,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "DELETE",
            $"workspaces/{PathSegment(workspaceId)}/chat/messages/{PathSegment(messageId)}/reactions/{PathSegment(reaction)}",
            cancellationToken: cancellationToken);
    }

    public string GetChatMessageRepliesJson(
        string workspaceId,
        string messageId,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        return GetChatMessageRepliesJsonAsync(workspaceId, messageId, query)
            .GetAwaiter()
            .GetResult();
    }

    public Task<string> GetChatMessageRepliesJsonAsync(
        string workspaceId,
        string messageId,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "GET",
            $"workspaces/{PathSegment(workspaceId)}/chat/messages/{PathSegment(messageId)}/replies",
            query: query,
            cancellationToken: cancellationToken);
    }

    public string CreateReplyMessageJson(string workspaceId, string messageId, string replyJson)
    {
        return CreateReplyMessageJsonAsync(workspaceId, messageId, replyJson)
            .GetAwaiter()
            .GetResult();
    }

    public Task<string> CreateReplyMessageJsonAsync(
        string workspaceId,
        string messageId,
        string replyJson,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "POST",
            $"workspaces/{PathSegment(workspaceId)}/chat/messages/{PathSegment(messageId)}/replies",
            replyJson,
            cancellationToken: cancellationToken);
    }

    public string GetChatMessageTaggedUsersJson(
        string workspaceId,
        string messageId,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        return GetChatMessageTaggedUsersJsonAsync(workspaceId, messageId, query)
            .GetAwaiter()
            .GetResult();
    }

    public Task<string> GetChatMessageTaggedUsersJsonAsync(
        string workspaceId,
        string messageId,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "GET",
            $"workspaces/{PathSegment(workspaceId)}/chat/messages/{PathSegment(messageId)}/tagged_users",
            query: query,
            cancellationToken: cancellationToken);
    }

    public string GetAttachmentsJson(
        string workspaceId,
        string entityType,
        string entityId,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        return GetAttachmentsJsonAsync(workspaceId, entityType, entityId, query)
            .GetAwaiter()
            .GetResult();
    }

    public Task<string> GetAttachmentsJsonAsync(
        string workspaceId,
        string entityType,
        string entityId,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "GET",
            $"workspaces/{PathSegment(workspaceId)}/{PathSegment(entityType)}/{PathSegment(entityId)}/attachments",
            query: query,
            cancellationToken: cancellationToken);
    }

    public string CreateAttachmentJson(
        string workspaceId,
        string entityType,
        string entityId,
        string filePath,
        string? overrideFileName = null,
        string formFieldName = "attachment")
    {
        return CreateAttachmentJsonAsync(
                workspaceId,
                entityType,
                entityId,
                filePath,
                overrideFileName,
                formFieldName)
            .GetAwaiter()
            .GetResult();
    }

    public async Task<string> CreateAttachmentJsonAsync(
        string workspaceId,
        string entityType,
        string entityId,
        string filePath,
        string? overrideFileName = null,
        string formFieldName = "attachment",
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        await using var content = File.OpenRead(filePath);
        return await CreateAttachmentJsonAsync(
            workspaceId,
            entityType,
            entityId,
            content,
            Path.GetFileName(filePath),
            overrideFileName,
            formFieldName,
            cancellationToken).ConfigureAwait(false);
    }

    public string CreateAttachmentJson(
        string workspaceId,
        string entityType,
        string entityId,
        Stream content,
        string fileName,
        string? overrideFileName = null,
        string formFieldName = "attachment")
    {
        return CreateAttachmentJsonAsync(
                workspaceId,
                entityType,
                entityId,
                content,
                fileName,
                overrideFileName,
                formFieldName)
            .GetAwaiter()
            .GetResult();
    }

    public async Task<string> CreateAttachmentJsonAsync(
        string workspaceId,
        string entityType,
        string entityId,
        Stream content,
        string fileName,
        string? overrideFileName = null,
        string formFieldName = "attachment",
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(formFieldName);

        using var multipart = new MultipartFormDataContent();
        using var streamContent = new StreamContent(content);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        multipart.Add(streamContent, formFieldName, fileName);

        if (!string.IsNullOrWhiteSpace(overrideFileName))
        {
            multipart.Add(new StringContent(overrideFileName), "filename");
        }

        return await SendMultipartAsync(
            $"workspaces/{PathSegment(workspaceId)}/{PathSegment(entityType)}/{PathSegment(entityId)}/attachments",
            multipart,
            cancellationToken).ConfigureAwait(false);
    }

    public string PatchAclJson(string workspaceId, string objectType, string objectId, string aclJson)
    {
        return PatchAclJsonAsync(workspaceId, objectType, objectId, aclJson).GetAwaiter().GetResult();
    }

    public Task<string> PatchAclJsonAsync(
        string workspaceId,
        string objectType,
        string objectId,
        string aclJson,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "PATCH",
            $"workspaces/{PathSegment(workspaceId)}/{PathSegment(objectType)}/{PathSegment(objectId)}/acls",
            aclJson,
            cancellationToken: cancellationToken);
    }

    public string QueryAuditLogJson(string workspaceId, string queryJson)
    {
        return QueryAuditLogJsonAsync(workspaceId, queryJson).GetAwaiter().GetResult();
    }

    public Task<string> QueryAuditLogJsonAsync(
        string workspaceId,
        string queryJson,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "POST",
            $"workspaces/{PathSegment(workspaceId)}/auditlogs",
            queryJson,
            cancellationToken: cancellationToken);
    }

    public string GetCommentSubtypesJson(string workspaceId, string commentType)
    {
        return GetCommentSubtypesJsonAsync(workspaceId, commentType).GetAwaiter().GetResult();
    }

    public Task<string> GetCommentSubtypesJsonAsync(
        string workspaceId,
        string commentType,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "GET",
            $"workspaces/{PathSegment(workspaceId)}/comments/types/{PathSegment(commentType)}/subtypes",
            cancellationToken: cancellationToken);
    }

    public string SearchDocsJson(
        string workspaceId,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        return SearchDocsJsonAsync(workspaceId, query).GetAwaiter().GetResult();
    }

    public Task<string> SearchDocsJsonAsync(
        string workspaceId,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "GET",
            $"workspaces/{PathSegment(workspaceId)}/docs",
            query: query,
            cancellationToken: cancellationToken);
    }

    public string CreateDocJson(string workspaceId, string docJson)
    {
        return CreateDocJsonAsync(workspaceId, docJson).GetAwaiter().GetResult();
    }

    public Task<string> CreateDocJsonAsync(
        string workspaceId,
        string docJson,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "POST",
            $"workspaces/{PathSegment(workspaceId)}/docs",
            docJson,
            cancellationToken: cancellationToken);
    }

    public string GetDocJson(string workspaceId, string docId)
    {
        return GetDocJsonAsync(workspaceId, docId).GetAwaiter().GetResult();
    }

    public Task<string> GetDocJsonAsync(
        string workspaceId,
        string docId,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "GET",
            $"workspaces/{PathSegment(workspaceId)}/docs/{PathSegment(docId)}",
            cancellationToken: cancellationToken);
    }

    public string GetDocPageListingJson(
        string workspaceId,
        string docId,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        return GetDocPageListingJsonAsync(workspaceId, docId, query).GetAwaiter().GetResult();
    }

    public Task<string> GetDocPageListingJsonAsync(
        string workspaceId,
        string docId,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "GET",
            $"workspaces/{PathSegment(workspaceId)}/docs/{PathSegment(docId)}/page_listing",
            query: query,
            cancellationToken: cancellationToken);
    }

    public string GetDocPagesJson(
        string workspaceId,
        string docId,
        IReadOnlyDictionary<string, string?>? query = null)
    {
        return GetDocPagesJsonAsync(workspaceId, docId, query).GetAwaiter().GetResult();
    }

    public Task<string> GetDocPagesJsonAsync(
        string workspaceId,
        string docId,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "GET",
            $"workspaces/{PathSegment(workspaceId)}/docs/{PathSegment(docId)}/pages",
            query: query,
            cancellationToken: cancellationToken);
    }

    public string CreateDocPageJson(string workspaceId, string docId, string pageJson)
    {
        return CreateDocPageJsonAsync(workspaceId, docId, pageJson).GetAwaiter().GetResult();
    }

    public Task<string> CreateDocPageJsonAsync(
        string workspaceId,
        string docId,
        string pageJson,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "POST",
            $"workspaces/{PathSegment(workspaceId)}/docs/{PathSegment(docId)}/pages",
            pageJson,
            cancellationToken: cancellationToken);
    }

    public string GetDocPageJson(string workspaceId, string docId, string pageId)
    {
        return GetDocPageJsonAsync(workspaceId, docId, pageId).GetAwaiter().GetResult();
    }

    public Task<string> GetDocPageJsonAsync(
        string workspaceId,
        string docId,
        string pageId,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "GET",
            $"workspaces/{PathSegment(workspaceId)}/docs/{PathSegment(docId)}/pages/{PathSegment(pageId)}",
            cancellationToken: cancellationToken);
    }

    public string EditDocPageJson(string workspaceId, string docId, string pageId, string pageJson)
    {
        return EditDocPageJsonAsync(workspaceId, docId, pageId, pageJson).GetAwaiter().GetResult();
    }

    public Task<string> EditDocPageJsonAsync(
        string workspaceId,
        string docId,
        string pageId,
        string pageJson,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "PUT",
            $"workspaces/{PathSegment(workspaceId)}/docs/{PathSegment(docId)}/pages/{PathSegment(pageId)}",
            pageJson,
            cancellationToken: cancellationToken);
    }

    public string MoveTaskToHomeListJson(
        string workspaceId,
        string taskId,
        string listId,
        string moveTaskJson)
    {
        return MoveTaskToHomeListJsonAsync(workspaceId, taskId, listId, moveTaskJson)
            .GetAwaiter()
            .GetResult();
    }

    public Task<string> MoveTaskToHomeListJsonAsync(
        string workspaceId,
        string taskId,
        string listId,
        string moveTaskJson,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "PUT",
            $"workspaces/{PathSegment(workspaceId)}/tasks/{PathSegment(taskId)}/home_list/{PathSegment(listId)}",
            moveTaskJson,
            cancellationToken: cancellationToken);
    }

    public string UpdateTimeEstimatesByUserJson(
        string workspaceId,
        string taskId,
        string estimatesJson)
    {
        return UpdateTimeEstimatesByUserJsonAsync(workspaceId, taskId, estimatesJson)
            .GetAwaiter()
            .GetResult();
    }

    public Task<string> UpdateTimeEstimatesByUserJsonAsync(
        string workspaceId,
        string taskId,
        string estimatesJson,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "PATCH",
            $"workspaces/{PathSegment(workspaceId)}/tasks/{PathSegment(taskId)}/time_estimates_by_user",
            estimatesJson,
            cancellationToken: cancellationToken);
    }

    public string ReplaceTimeEstimatesByUserJson(
        string workspaceId,
        string taskId,
        string estimatesJson)
    {
        return ReplaceTimeEstimatesByUserJsonAsync(workspaceId, taskId, estimatesJson)
            .GetAwaiter()
            .GetResult();
    }

    public Task<string> ReplaceTimeEstimatesByUserJsonAsync(
        string workspaceId,
        string taskId,
        string estimatesJson,
        CancellationToken cancellationToken = default)
    {
        return RequestJsonAsync(
            "PUT",
            $"workspaces/{PathSegment(workspaceId)}/tasks/{PathSegment(taskId)}/time_estimates_by_user",
            estimatesJson,
            cancellationToken: cancellationToken);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    private static HttpClient CreateConfiguredHttpClient(
        string apiToken,
        ClickUpClientOptions? options,
        HttpMessageHandler? primaryHandler = null)
    {
        var configuredOptions = options is null
            ? new ClickUpClientOptions()
            : CloneOptions(options);

        if (string.Equals(
            configuredOptions.BaseUrl.TrimEnd('/'),
            DefaultV2BaseUrl,
            StringComparison.OrdinalIgnoreCase))
        {
            configuredOptions.BaseUrl = DefaultV3BaseUrl;
        }

        return ClickUpHttpClientFactory.Create(apiToken, configuredOptions, primaryHandler);
    }

    private static ClickUpClientOptions CloneOptions(ClickUpClientOptions options)
    {
        return new ClickUpClientOptions
        {
            BaseUrl = options.BaseUrl,
            Timeout = options.Timeout,
            MaxRetries = options.MaxRetries,
            BaseRetryDelay = options.BaseRetryDelay,
            MaxRetryDelay = options.MaxRetryDelay,
        };
    }

    private async Task<string> SendMultipartAsync(
        string endpoint,
        HttpContent content,
        CancellationToken cancellationToken)
    {
        if (_httpClient is null)
        {
            throw new InvalidOperationException("Attachment uploads require an HttpClient-backed ClickUpV3Client.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, BuildUri(endpoint))
        {
            Content = content,
        };

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var responseBody = response.Content is null
                ? string.Empty
                : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ClickUpApiException(
                    $"ClickUp POST {BuildUri(endpoint)} failed with status {(int)response.StatusCode} ({response.StatusCode}).",
                    "POST",
                    BuildUri(endpoint),
                    response.StatusCode,
                    responseBody);
            }

            return responseBody;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            throw new ClickUpApiException(
                $"ClickUp POST {BuildUri(endpoint)} failed before receiving a response: {exception.Message}",
                "POST",
                BuildUri(endpoint),
                innerException: exception);
        }
    }

    private static string PathSegment(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value;
    }
}
