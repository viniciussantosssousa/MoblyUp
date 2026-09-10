using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Enums;

namespace MoblyUp.Domain.Interfaces
{
    public interface IPedidoMontagemRepository
    {
        Task<PedidoMontagem> ObterPorIdAsync(Guid id);
        Task<IEnumerable<PedidoMontagem>> ObterPorClienteIdAsync(Guid clienteId);
        Task<IEnumerable<PedidoMontagem>> ObterPorMontadorIdAsync(Guid montadorId);
        Task<IEnumerable<PedidoMontagem>> ObterPorStatusAsync(StatusPedido status);
        Task AdicionarAsync(PedidoMontagem pedido);
        Task AtualizarAsync(PedidoMontagem pedido);
    }
}