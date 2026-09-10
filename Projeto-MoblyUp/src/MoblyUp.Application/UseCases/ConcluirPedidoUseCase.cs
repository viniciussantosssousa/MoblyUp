using System;
using System.Threading.Tasks;
using MoblyUp.Domain.Interfaces;

namespace MoblyUp.Application.UseCases
{
    public class ConcluirPedidoUseCase
    {
        private readonly IPedidoMontagemRepository _pedidoRepository;

        public ConcluirPedidoUseCase(IPedidoMontagemRepository pedidoRepository)
        {
            _pedidoRepository = pedidoRepository;
        }

        public async Task ExecutarAsync(Guid pedidoId)
        {
            var pedido = await _pedidoRepository.ObterPorIdAsync(pedidoId);
            
            if (pedido == null)
                throw new Exception("Pedido não encontrado.");

            pedido.ConcluirPedido();

            await _pedidoRepository.AtualizarAsync(pedido);
        }
    }
}