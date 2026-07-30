using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace GmbhCmsPortalApp.Components.Service;

public class CustomAuthorizationMessageHandler : DelegatingHandler
{
    private readonly IJSRuntime _jsRuntime;

    public CustomAuthorizationMessageHandler(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Retrieve the token from localStorage
        var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}