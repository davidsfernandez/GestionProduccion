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
    }

    public class SignalRService : ISignalRService
    {
        private HubConnection? _hubConnection;

        public event Action<int, string, string>? OnUpdateReceived;
        public event Action<string, string>? OnMessageReceived;
        public event Action<int, string, string>? OnNotificationReceived;

        private Task? _startTask;

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

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrl)
                    .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
                    .Build();

                _hubConnection.Closed += async (error) =>
                {
                    Console.WriteLine($"SignalR Connection Closed: {error?.Message}");
                    await Task.CompletedTask;
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

                Console.WriteLine($"Starting SignalR connection to: {hubUrl}");
                _startTask = _hubConnection.StartAsync();
                await _startTask;
                Console.WriteLine("SignalR Connected successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR Connection Error: {ex.Message}");
            }
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
