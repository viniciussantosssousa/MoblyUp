using System;
using System.Threading.Tasks;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Interfaces;

namespace MoblyUp.Application.UseCases
{
    // Ações do cliente sobre o próprio pedido: aceitar/recusar a proposta e confirmar o pagamento
    public class ResponderPropostaUseCase
    {
        private readonly IPedidoMontagemRepository _pedidoRepository;

        public ResponderPropostaUseCase(IPedidoMontagemRepository pedidoRepository)
        {
            _pedidoRepository = pedidoRepository;
        }

        public Task AceitarAsync(Guid pedidoId, string clienteId) =>
            ExecutarAsync(pedidoId, clienteId, pedido => pedido.AceitarProposta());

        public Task RecusarAsync(Guid pedidoId, string clienteId) =>
            ExecutarAsync(pedidoId, clienteId, pedido => pedido.RecusarProposta());

        // Pagamento simulado: ainda não há gateway integrado, o cliente confirma o PIX no app
        public Task ConfirmarPagamentoAsync(Guid pedidoId, string clienteId) =>
            ExecutarAsync(pedidoId, clienteId, pedido => pedido.ConfirmarPagamento());

        private async Task ExecutarAsync(Guid pedidoId, string clienteId, Action<PedidoMontagem> acao)
        {
            var pedido = await _pedidoRepository.ObterPorIdAsync(pedidoId);

            if (pedido == null)
                throw new Exception("Pedido não encontrado.");

            if (pedido.ClienteId != clienteId)
                throw new UnauthorizedAccessException("Apenas o cliente dono do pedido pode realizar esta ação.");

            acao(pedido);

            await _pedidoRepository.AtualizarAsync(pedido);
        }
    }
}
