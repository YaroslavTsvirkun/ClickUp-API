using System.Text.Json.Serialization;

namespace ClickUp.Client.Models;

public sealed class ClickUpOAuthTokenRequest
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; init; } = string.Empty;

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; init; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; init; } = string.Empty;
}
