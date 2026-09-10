using System;
using System.Threading.Tasks;
using MoblyUp.Application.DTOs;
using MoblyUp.Domain.Interfaces;

namespace MoblyUp.Application.UseCases
{
    public class AceitarPedidoUseCase
    {
        private readonly IPedidoMontagemRepository _pedidoRepository;

        public AceitarPedidoUseCase(IPedidoMontagemRepository pedidoRepository)
        {
            _pedidoRepository = pedidoRepository;
        }

        public async Task ExecutarAsync(Guid pedidoId, AceitarPedidoRequest request)
        {
            var pedido = await _pedidoRepository.ObterPorIdAsync(pedidoId);

            if (pedido == null)
                throw new Exception("Pedido não encontrado.");

            pedido.ReceberProposta(request.MontadorId, request.ValorProposto);

            await _pedidoRepository.AtualizarAsync(pedido);
        }
    }
}