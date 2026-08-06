using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7123/")
});

builder.Services.AddHttpContextAccessor();

// builder.Services.AddTransient<CustomAuthorizationMessageHandler>();
builder.Services.AddTransient<AuthorizationHandler>();

// 2. Register a named HttpClient ("AuthorizedClient") using the handler and base URL from configuration
builder.Services.AddHttpClient("AuthorizedClient", client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7123/";
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<AuthorizationHandler>();
// .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

builder.Services.AddScoped<AuthenticationStateProvider, CustomJwtAuthenticationStateProvider>();

builder.Services.AddScoped<UserProfileState>();

builder.Services.AddTransient<AuthorizationHandler>();

// Make it the default HttpClient for injection
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("AuthorizedClient"));

builder.Services.AddAuthorizationCore();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 20 MB
});

// 2. Form Multipart Limit (20MB)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 20 MB
});

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

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();