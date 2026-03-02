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
    }

    public class SignalRService : ISignalRService
    {
        private HubConnection? _hubConnection;

        public event Action<int, string, string>? OnUpdateReceived;
        public event Action<string, string>? OnMessageReceived;

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
                    .WithAutomaticReconnect()
                    .Build();

                _hubConnection.On<int, string, string>("ReceiveUpdate", (opId, novaEtapa, novoStatus) =>
                {
                    OnUpdateReceived?.Invoke(opId, novaEtapa, novoStatus);
                });

                _hubConnection.On<object>("ReceiveMessage", (data) =>
                {
                    // Basic parsing for message notifications
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

                _startTask = _hubConnection.StartAsync();
                await _startTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when navigating away
            }
            catch (Exception)
            {
                // Silent connection error
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
                catch (OperationCanceledException)
                {
                    // Safe to ignore
                }
                catch (Exception)
                {
                    // Silent stop error
                }
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
