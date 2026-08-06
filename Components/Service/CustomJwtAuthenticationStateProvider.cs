using System.Text.Json;

namespace GmbhCmsPortalApp.Components.Service;

public class CustomJwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ClaimsPrincipal _cachedUser = new(new ClaimsIdentity());

    public CustomJwtAuthenticationStateProvider(IJSRuntime jsRuntime, IHttpContextAccessor httpContextAccessor)
    {
        _jsRuntime = jsRuntime;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // 1. First check if we have an HttpContext available (useful during Server-side initial load/prerender if token is in cookies, etc.)
        // If using localStorage, we handle it carefully. During prerendering, JS is not available.
        try
        {
            var token = string.Empty;

            // Check if JS Interop is available by trying to fetch from localStorage
            // In Blazor Server prerendering, calling JS will throw an exception.
            token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

            if (!string.IsNullOrEmpty(token))
            {
                var claims = ParseClaimsFromJwt(token);
                var identity = new ClaimsIdentity(claims, "jwt");
                _cachedUser = new ClaimsPrincipal(identity);
            }
            else
            {
                _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
            }
        }
        catch
        {
            // During prerendering, JS interop fails, so we fallback to an anonymous user safely
            _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
        }

        return new AuthenticationState(_cachedUser);
    }

    public void NotifyUserAuthentication(string token)
    {
        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        _cachedUser = new ClaimsPrincipal(identity);

        var authState = Task.FromResult(new AuthenticationState(_cachedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
        _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(_cachedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        var claims = new List<Claim>();
        if (keyValuePairs != null)
        {
            foreach (var kvp in keyValuePairs)
            {
                if (kvp.Value is JsonElement element && element.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in element.EnumerateArray())
                    {
                        claims.Add(new Claim(kvp.Key, item.ToString() ?? ""));
                    }
                }
                else
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value?.ToString() ?? ""));
                }
            }
        }

        return claims;
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        return Convert.FromBase64String(base64);
    }
}