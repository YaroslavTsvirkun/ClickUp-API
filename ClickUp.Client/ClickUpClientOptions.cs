namespace ClickUp.Client;

public sealed class ClickUpClientOptions
{
    public string BaseUrl { get; set; } = "https://api.clickup.com/api/v2";

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

    public int MaxRetries { get; set; } = 3;

    public TimeSpan BaseRetryDelay { get; set; } = TimeSpan.FromMilliseconds(500);

    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(5);
}
