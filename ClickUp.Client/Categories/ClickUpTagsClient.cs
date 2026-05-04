using ClickUp.Client.Abstractions.Tags;

namespace ClickUp.Client.Categories;

public sealed class ClickUpTagsClient : ClickUpEndpointClient
{
    private readonly IClickUpTagsApi _api;

    internal ClickUpTagsClient(IClickUpTagsApi api, Uri baseUri)
        : base(baseUri)
    {
        _api = api;
    }

    public string GetSpaceTagsJson(string spaceId)
    {
        return GetSpaceTagsJsonAsync(spaceId).GetAwaiter().GetResult();
    }

    public Task<string> GetSpaceTagsJsonAsync(
        string spaceId,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.GetSpaceTagsAsync(spaceId, ct),
            "GET",
            $"space/{spaceId}/tag",
            cancellationToken);
    }

    public string CreateSpaceTagJson(string spaceId, string tagJson)
    {
        return CreateSpaceTagJsonAsync(spaceId, tagJson).GetAwaiter().GetResult();
    }

    public Task<string> CreateSpaceTagJsonAsync(
        string spaceId,
        string tagJson,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.CreateSpaceTagAsync(spaceId, ParseJson(tagJson), ct),
            "POST",
            $"space/{spaceId}/tag",
            cancellationToken);
    }

    public string EditSpaceTagJson(string spaceId, string tagName, string tagJson)
    {
        return EditSpaceTagJsonAsync(spaceId, tagName, tagJson).GetAwaiter().GetResult();
    }

    public Task<string> EditSpaceTagJsonAsync(
        string spaceId,
        string tagName,
        string tagJson,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.EditSpaceTagAsync(spaceId, tagName, ParseJson(tagJson), ct),
            "PUT",
            $"space/{spaceId}/tag/{tagName}",
            cancellationToken);
    }

    public string DeleteSpaceTagJson(string spaceId, string tagName, string tagJson = "{}")
    {
        return DeleteSpaceTagJsonAsync(spaceId, tagName, tagJson).GetAwaiter().GetResult();
    }

    public Task<string> DeleteSpaceTagJsonAsync(
        string spaceId,
        string tagName,
        string tagJson = "{}",
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.DeleteSpaceTagAsync(spaceId, tagName, ParseJson(tagJson), ct),
            "DELETE",
            $"space/{spaceId}/tag/{tagName}",
            cancellationToken);
    }

    public string AddTagToTaskJson(
        string taskId,
        string tagName,
        bool customTaskIds = false,
        string? teamId = null)
    {
        return AddTagToTaskJsonAsync(taskId, tagName, customTaskIds, teamId)
            .GetAwaiter()
            .GetResult();
    }

    public Task<string> AddTagToTaskJsonAsync(
        string taskId,
        string tagName,
        bool customTaskIds = false,
        string? teamId = null,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.AddTagToTaskAsync(taskId, tagName, OptionalBool(customTaskIds), teamId, ct),
            "POST",
            $"task/{taskId}/tag/{tagName}",
            cancellationToken);
    }

    public string RemoveTagFromTaskJson(
        string taskId,
        string tagName,
        bool customTaskIds = false,
        string? teamId = null)
    {
        return RemoveTagFromTaskJsonAsync(taskId, tagName, customTaskIds, teamId)
            .GetAwaiter()
            .GetResult();
    }

    public Task<string> RemoveTagFromTaskJsonAsync(
        string taskId,
        string tagName,
        bool customTaskIds = false,
        string? teamId = null,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            ct => _api.RemoveTagFromTaskAsync(taskId, tagName, OptionalBool(customTaskIds), teamId, ct),
            "DELETE",
            $"task/{taskId}/tag/{tagName}",
            cancellationToken);
    }
}
