using Microsoft.AspNetCore.SignalR;

namespace GestionProduccion.Hubs;

public class ProducaoHub : Hub
{
    public async Task NotificarAtualizacao(int opId, string novaEtapa, string novoStatus)
    {
        if (Clients != null)
        {
            await Clients.All.SendAsync("ReceiveUpdate", opId, novaEtapa, novoStatus);
        }
    }
}
