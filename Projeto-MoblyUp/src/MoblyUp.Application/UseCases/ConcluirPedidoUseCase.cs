using System;
using System.Threading.Tasks;
using MoblyUp.Application.Interfaces;
using MoblyUp.Domain.Interfaces;

namespace MoblyUp.Application.UseCases
{
    public class ConcluirPedidoUseCase
    {
        private readonly IPedidoMontagemRepository _pedidoRepository;
        private readonly INotificacaoService _notificacaoService;

        public ConcluirPedidoUseCase(IPedidoMontagemRepository pedidoRepository, INotificacaoService notificacaoService)
        {
            _pedidoRepository = pedidoRepository;
            _notificacaoService = notificacaoService;
        }

        public async Task ExecutarAsync(Guid pedidoId, string montadorId)
        {
            var pedido = await _pedidoRepository.ObterPorIdAsync(pedidoId);

            if (pedido == null)
                throw new Exception("Pedido não encontrado.");

            if (pedido.MontadorId != montadorId)
                throw new UnauthorizedAccessException("Apenas o montador responsável pode concluir o pedido.");

            pedido.ConcluirPedido();

            await _pedidoRepository.AtualizarAsync(pedido);

            await _notificacaoService.NotificarUsuarioAsync(
                pedido.ClienteId,
                "Montagem concluída! 🛋️",
                $"Seu {pedido.TipoMovel} está pronto. Toque para ver a foto do serviço.",
                pedido.Id);
        }
    }
}
