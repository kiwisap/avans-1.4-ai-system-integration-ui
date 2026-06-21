using Microsoft.JSInterop;

namespace avans_1._4_system_integration_ui.Services;

public class TokenStorageService(IJSRuntime jsRuntime)
{
    private const string TokenKey = "authToken";

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task SetTokenAsync(string token)
    {
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
    }

    public async Task RemoveTokenAsync()
    {
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
    }
}