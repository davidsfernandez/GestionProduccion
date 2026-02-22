using Microsoft.JSInterop;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using GestionProduccion.Client.Models.DTOs;

namespace GestionProduccion.Client.Auth
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IJSRuntime _jsRuntime;

        public AuthHeaderHandler(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            // Check if unauthorized and not a refresh-token call itself to avoid infinite loop
            if (response.StatusCode == HttpStatusCode.Unauthorized && 
                !request.RequestUri!.ToString().Contains("api/Auth/refresh-token"))
            {
                var refreshToken = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "refreshToken");

                if (!string.IsNullOrWhiteSpace(refreshToken))
                {
                    // Attempt to refresh the token
                    var refreshResponse = await base.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/Auth/refresh-token")
                    {
                        Content = JsonContent.Create(new RefreshTokenRequest { RefreshToken = refreshToken })
                    }, cancellationToken);

                    if (refreshResponse.IsSuccessStatusCode)
                    {
                        var loginResponse = await refreshResponse.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);
                        
                        if (loginResponse != null)
                        {
                            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", loginResponse.Token);
                            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "refreshToken", loginResponse.RefreshToken);

                            // Retry original request with new token
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
                            return await base.SendAsync(request, cancellationToken);
                        }
                    }
                }
            }

            return response;
        }
    }
}
