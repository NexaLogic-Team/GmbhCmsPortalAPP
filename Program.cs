using System.Security.Claims;
using GmbhCmsPortalApp.Components;
using GmbhCmsPortalApp.Components.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

builder.Services.AddAuthorizationCore();

var app = builder.Build();

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

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// app.MapPost("/api/login", async (HttpContext context) =>
// {
//     var form = await context.Request.ReadFormAsync();
//     var email = form["email"];
//     var password = form["password"];
//
//     if (!string.IsNullOrEmpty(email))
//     {
//         var claims = new List<Claim>
//         {
//             new Claim(ClaimTypes.Name, email),
//             new Claim(ClaimTypes.Email, email),
//             new Claim(ClaimTypes.Role, "Admin")
//         };
//
//         var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
//         var authProperties = new AuthenticationProperties
//         {
//             IsPersistent = true,
//             AllowRefresh = true
//         };
//
//         await context.SignInAsync(
//             CookieAuthenticationDefaults.AuthenticationScheme, 
//             new ClaimsPrincipal(claimsIdentity), 
//             authProperties);
//         
//         return Results.Redirect("/cms/dashboard", true);
//     }
//     
//     return Results.Redirect("/", true);
// });

app.MapPost("/api/login", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    var email = form["email"];
    var password = form["password"];

    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
    {
        return Results.Redirect("/?error=MissingCredentials", true);
    }

    // TODO: Production တွင် Database မှ Admin အချက်အလက်များကို ယူ၍ စစ်ဆေးရပါမည်။
    bool isValidUser = (email == "gmbh@gmail.com" && password == "123@gmbh.com");

    if (isValidUser)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, email!),
            new Claim(ClaimTypes.Email, email!),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            AllowRefresh = true
        };

        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, 
            new ClaimsPrincipal(claimsIdentity), 
            authProperties);
        
        return Results.Redirect("/cms/dashboard", true);
    }
    
    return Results.Redirect("/?error=InvalidCredentials", true);
});

app.MapGet("/api/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    if (context.Request.Cookies.Count > 0)
    {
        foreach (var cookie in context.Request.Cookies.Keys)
        {
            context.Response.Cookies.Delete(cookie);
        }
    }

    return Results.Redirect("/", true);
});

app.Run();