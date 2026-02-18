using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace GestionProduccion.Client.Services
{
    public class SignalRService
    {
        private HubConnection? _hubConnection;

        public event Action<int, string, string>? OnUpdateReceived;

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

                _startTask = _hubConnection.StartAsync();
                await _startTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when navigating away
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
                catch (OperationCanceledException)
                {
                    // Safe to ignore
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SignalR Stop Error: {ex.Message}");
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
