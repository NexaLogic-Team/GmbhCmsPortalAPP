using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;

namespace GmbhCmsPortalApp.Components.Service;

public class CustomAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly PersistentComponentState _persistentComponentState;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CustomAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        IServiceScopeFactory scopeFactory,
        PersistentComponentState persistentComponentState,
        IHttpContextAccessor httpContextAccessor)
        : base(loggerFactory)
    {
        _scopeFactory = scopeFactory;
        _persistentComponentState = persistentComponentState;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        var principal = authenticationState.User;
        return principal.Identity?.IsAuthenticated ?? false;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.User != null && context.User.Identity != null && context.User.Identity.IsAuthenticated)
        {
            return Task.FromResult(new AuthenticationState(context.User));
        }

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
    }
}