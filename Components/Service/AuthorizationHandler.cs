using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace GmbhCmsPortalApp.Components.Service;

public class AuthorizationHandler : DelegatingHandler
{
    private readonly IJSRuntime _jsRuntime;

    public AuthorizationHandler(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch
        {
            // Fallback for pre-rendering environments where JS Interop isn't ready
        }

        return await base.SendAsync(request, cancellationToken);
    }
}