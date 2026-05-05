namespace ClickUp.Client.Infrastructure;

internal static class HttpContentCompatibilityExtensions
{
    public static Task<byte[]> ReadAsByteArrayCompatAsync(
        this HttpContent content,
        CancellationToken cancellationToken)
    {
#if NET5_0_OR_GREATER
        return content.ReadAsByteArrayAsync(cancellationToken);
#else
        cancellationToken.ThrowIfCancellationRequested();
        return content.ReadAsByteArrayAsync();
#endif
    }

    public static Task<string> ReadAsStringCompatAsync(
        this HttpContent content,
        CancellationToken cancellationToken)
    {
#if NET5_0_OR_GREATER
        return content.ReadAsStringAsync(cancellationToken);
#else
        cancellationToken.ThrowIfCancellationRequested();
        return content.ReadAsStringAsync();
#endif
    }
}
