using Microsoft.JSInterop;

namespace avans_1._4_system_integration_ui.Services
{
    public class TokenStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string TokenKey = "authToken";

        public TokenStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string?> GetTokenAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenKey);
            }
            catch
            {
                return null;
            }
        }

        public async Task SetTokenAsync(string token)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
        }

        public async Task RemoveTokenAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        }
    }
}
