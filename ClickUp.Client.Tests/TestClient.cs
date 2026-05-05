using ClickUp.Client;

namespace ClickUp.Client.Tests;

internal static class TestClient
{
    public static ClickUpClient Create(RecordingHandler handler)
    {
        var options = new ClickUpClientOptions
        {
            BaseRetryDelay = TimeSpan.FromMilliseconds(1),
            MaxRetryDelay = TimeSpan.FromMilliseconds(1),
        };

        return new ClickUpClient("pk_test", handler, options);
    }

    public static ClickUpV3Client CreateV3(RecordingHandler handler)
    {
        var options = new ClickUpClientOptions
        {
            BaseRetryDelay = TimeSpan.FromMilliseconds(1),
            MaxRetryDelay = TimeSpan.FromMilliseconds(1),
        };

        return new ClickUpV3Client("pk_test", handler, options);
    }

    public static ClickUpOAuthClient CreateOAuth(RecordingHandler handler)
    {
        var options = new ClickUpClientOptions
        {
            BaseRetryDelay = TimeSpan.FromMilliseconds(1),
            MaxRetryDelay = TimeSpan.FromMilliseconds(1),
        };

        return new ClickUpOAuthClient(handler, options);
    }
}
