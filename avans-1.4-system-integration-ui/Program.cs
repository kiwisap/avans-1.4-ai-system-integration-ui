using Microsoft.AspNetCore.Components.Authorization;
using avans_1._4_system_integration_ui.Components;
using avans_1._4_system_integration_ui.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

// Configure HttpClient for API calls with bearer token
var apiBaseUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7000";

builder.Services.AddScoped<TokenStorageService>();
builder.Services.AddScoped<AuthorizationMessageHandler>();

builder.Services.AddHttpClient<AuthService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<AuthorizationMessageHandler>();

// Register custom authentication services
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
