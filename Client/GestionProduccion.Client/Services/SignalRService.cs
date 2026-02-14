using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace GestionProduccion.Client.Services
{
    public class SignalRService
    {
        private HubConnection? _hubConnection;

        public event Action<int, string, string>? OnUpdateReceived;

        public async Task StartConnection(string hubUrl)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .Build();

            _hubConnection.On<int, string, string>("ReceiveUpdate", (opId, novaEtapa, novoStatus) =>
            {
                OnUpdateReceived?.Invoke(opId, novaEtapa, novoStatus);
            });

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
                catch (ObjectDisposedException) { /* Already gone */ }
                finally
                {
                    await _hubConnection.DisposeAsync();
                    _hubConnection = null;
                }
            }
        }
    }
}
