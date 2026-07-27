using System.Security.Claims;
using GmbhCmsPortalApp.Components;
using GmbhCmsPortalApp.Components.Service;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7123/") 
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<AuthenticationStateProvider, CustomJwtAuthenticationStateProvider>();

builder.Services.AddAuthorizationCore();

var app = builder.Build();

// app.Use(async (context, next) =>
// {
//     context.Response.Headers.Add(
//         "Content-Security-Policy", 
//         "default-src 'self'; " +
//         "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://cdn.tailwindcss.com; " +
//         "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
//         "font-src 'self' https://fonts.gstatic.com; " +
//         "connect-src 'self' ws: wss:;"
//     );
//     await next();
// });

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAntiforgery();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();