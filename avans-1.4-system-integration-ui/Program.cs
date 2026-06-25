using Microsoft.AspNetCore.Components.Authorization;
using avans_1._4_system_integration_ui.Components;
using avans_1._4_system_integration_ui.Services;
using avans_1._4_system_integration_ui.Authentication;
using avans_1._4_system_integration_ui.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddBlazorBootstrap();
builder.Services.AddCascadingAuthenticationState();

// Configure HttpClient for API calls with bearer token
var apiBaseUrl = builder.Configuration.GetValue<string>("ApiSettings:BackendBaseUrl") ?? "https://localhost:7000";

builder.Services.AddScoped<TokenStorageService>();

builder.Services.AddHttpClient<TokenRefreshService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<AuthService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<TrashDataService>(c => c.BaseAddress = new Uri(apiBaseUrl));

// Configure HttpClient for PredictionsService with api key
var predictionsApiBaseUrl = builder.Configuration.GetValue<string>("ApiSettings:PredictionsBaseUrl") ?? "https://localhost:8000";
var apiKey = builder.Configuration.GetValue<string>("ai-api-key");
builder.Services.AddHttpClient<PredictionsService>(client =>
{
    client.BaseAddress = new Uri(predictionsApiBaseUrl);
    client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
});

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
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
