/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Http.Connections;
using System;
using System.Threading.Tasks;

namespace GestionProduccion.Client.Services
{
    public interface ISignalRService
    {
        Task StartConnection(string hubUrl);
        Task StopConnection();
        event Action<int, string, string>? OnUpdateReceived;
        event Action<string, string>? OnMessageReceived; // message, type
        event Action<int, string, string>? OnNotificationReceived; // userId, title, message
        event Action<HubConnectionState>? OnStatusChanged;
    }

    public class SignalRService : ISignalRService
    {
        private readonly IJSRuntime _jsRuntime;
        private HubConnection? _hubConnection;

        public event Action<int, string, string>? OnUpdateReceived;
        public event Action<string, string>? OnMessageReceived;
        public event Action<int, string, string>? OnNotificationReceived;
        public event Action<HubConnectionState>? OnStatusChanged;

        private Task? _startTask;

        public SignalRService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task StartConnection(string hubUrl)
        {
            try
            {
                if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected)
                {
                    return;
                }

                if (_startTask != null && !_startTask.IsCompleted)
                {
                    await _startTask;
                    return;
                }

                // SECURITY GUARD: Ensure we have a token before trying to connect to the authenticated hub
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("SignalR: Delaying connection until auth token is available...");
                    return;
                }

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrl, options => {
                        options.Transports = HttpTransportType.WebSockets;
                        options.SkipNegotiation = true;
                        options.AccessTokenProvider = () => Task.FromResult<string?>(token);
                    })
                    .WithAutomaticReconnect(new[] { 
                        TimeSpan.Zero, 
                        TimeSpan.FromSeconds(2), 
                        TimeSpan.FromSeconds(5), 
                        TimeSpan.FromSeconds(15), 
                        TimeSpan.FromSeconds(30) 
                    })
                    .Build();

                // AGGRESSIVE SETTINGS: Critical for factory floor environments with potential network drops
                _hubConnection.ServerTimeout = TimeSpan.FromSeconds(30); // Faster detection of dead server
                _hubConnection.KeepAliveInterval = TimeSpan.FromSeconds(10); // Frequent heartbeat

                _hubConnection.Reconnecting += (error) =>
                {
                    OnStatusChanged?.Invoke(HubConnectionState.Reconnecting);
                    Console.WriteLine("SignalR: Connection lost. Reconnecting...");
                    return Task.CompletedTask;
                };

                _hubConnection.Reconnected += (connectionId) =>
                {
                    OnStatusChanged?.Invoke(HubConnectionState.Connected);
                    Console.WriteLine("SignalR: Connection restored.");
                    return Task.CompletedTask;
                };

                _hubConnection.Closed += async (error) =>
                {
                    OnStatusChanged?.Invoke(HubConnectionState.Disconnected);
                    Console.WriteLine($"SignalR: Connection closed ({error?.Message}). Retrying in 5s...");
                    await Task.Delay(5000);
                    await StartConnection(hubUrl);
                };

                _hubConnection.On<int, string, string>("ReceiveUpdate", (opId, novaEtapa, novoStatus) =>
                {
                    OnUpdateReceived?.Invoke(opId, novaEtapa, novoStatus);
                });

                _hubConnection.On<int, string, string>("ReceiveNotification", (userId, title, message) =>
                {
                    OnNotificationReceived?.Invoke(userId, title, message);
                });

                _hubConnection.On<object>("ReceiveMessage", (data) =>
                {
                    try
                    {
                        var json = System.Text.Json.JsonSerializer.Serialize(data);
                        using var doc = System.Text.Json.JsonDocument.Parse(json);
                        var msg = doc.RootElement.GetProperty("message").GetString() ?? "";
                        var type = doc.RootElement.GetProperty("type").GetString() ?? "info";
                        OnMessageReceived?.Invoke(msg, type);
                    }
                    catch { }
                });

                Console.WriteLine($"Initiating WebSocket connection to: {hubUrl}");
                _startTask = _hubConnection.StartAsync();
                await _startTask;
                Console.WriteLine("SignalR established via WebSockets successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR Connection Error: {ex.Message}");
                
                // Fallback: If forced WebSockets fails due to network strictness, retry with auto-negotiation
                if (ex.Message.Contains("WebSockets"))
                {
                    Console.WriteLine("Retrying with negotiation fallback...");
                    await RetryWithNegotiation(hubUrl);
                }
            }
        }

        private async Task RetryWithNegotiation(string hubUrl)
        {
             _hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrl, options => {
                        options.AccessTokenProvider = async () => await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                    })
                    .WithAutomaticReconnect()
                    .Build();
             await _hubConnection.StartAsync();
        }

        public async Task StopConnection()
        {
            if (_hubConnection != null)
            {
                try
                {
                    if (_hubConnection.State != HubConnectionState.Disconnected)
                    {
                        await _hubConnection.StopAsync();
                    }
                }
                catch (Exception) { }
                finally
                {
                    await _hubConnection.DisposeAsync();
                    _hubConnection = null;
                    _startTask = null;
                }
            }
        }
    }
}
