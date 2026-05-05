using ClickUp.Client.Models;
using Refit;

namespace ClickUp.Client.Abstractions.OAuth;

[Headers("Accept: application/json")]
public interface IClickUpOAuthApi
{
    [Post("/oauth/token")]
    Task<string> GetAccessTokenAsync(
        [Body] ClickUpOAuthTokenRequest request,
        CancellationToken cancellationToken = default);
}
