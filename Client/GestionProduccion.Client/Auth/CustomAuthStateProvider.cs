using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;

namespace GestionProduccion.Client.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                if (string.IsNullOrWhiteSpace(token)) return new AuthenticationState(_anonymous);

                var claims = ParseClaimsFromJwt(token);
                // Use default ClaimTypes for compatibility
                var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch
            {
                return new AuthenticationState(_anonymous);
            }
        }

        public async Task MarkUserAsAuthenticated(string token, string? refreshToken = null)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "refreshToken", refreshToken);
                }

                var claims = ParseClaimsFromJwt(token);
                var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
            }
            catch (Exception)
            {
                // If token is invalid, clear storage and notify as anonymous
                await MarkUserAsLoggedOut();
            }
        }

        public async Task MarkUserAsLoggedOut()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "refreshToken");
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);

            using var jsonDoc = JsonDocument.Parse(jsonBytes);
            foreach (var element in jsonDoc.RootElement.EnumerateObject())
            {
                var key = element.Name;
                var value = element.Value;

                // Robust mapping to standard .NET ClaimTypes
                if (key == "role" || key == "roles" || key == ClaimTypes.Role || key == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                {
                    if (value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in value.EnumerateArray())
                        {
                            claims.Add(new Claim(ClaimTypes.Role, item.ToString()));
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(ClaimTypes.Role, value.ToString()));
                    }
                }
                else if (key == "unique_name" || key == "name" || key == ClaimTypes.Name || key == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                {
                    claims.Add(new Claim(ClaimTypes.Name, value.ToString()));
                }
                else if (key == "sub" || key == "nameid" || key == ClaimTypes.NameIdentifier || key == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, value.ToString()));
                }
                else if (key == "email" || key == ClaimTypes.Email || key == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
                {
                    claims.Add(new Claim(ClaimTypes.Email, value.ToString()));
                }
                else
                {
                    claims.Add(new Claim(key, value.ToString()));
                }
            }
            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
