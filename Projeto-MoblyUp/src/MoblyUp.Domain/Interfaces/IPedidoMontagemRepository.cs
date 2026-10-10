using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Enums;

namespace MoblyUp.Domain.Interfaces
{
    public interface IPedidoMontagemRepository
    {
        Task<PedidoMontagem?> ObterPorIdAsync(Guid id);
        Task<IEnumerable<PedidoMontagem>> ObterPorClienteIdAsync(string clienteId);
        Task<IEnumerable<PedidoMontagem>> ObterPorMontadorIdAsync(string montadorId);
        Task<IEnumerable<PedidoMontagem>> ObterPorStatusAsync(StatusPedido status);
        Task AdicionarAsync(PedidoMontagem pedido);
        Task AtualizarAsync(PedidoMontagem pedido);

        // Pedidos que o montador recusou/desistiu e não devem voltar ao feed dele
        Task RegistrarRecusaAsync(Guid pedidoId, string montadorId);
        Task<HashSet<Guid>> ObterIdsRecusadosAsync(string montadorId);
    }
}
