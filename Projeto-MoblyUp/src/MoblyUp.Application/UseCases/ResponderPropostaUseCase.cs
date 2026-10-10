using System;
using System.Threading.Tasks;
using MoblyUp.Application.Interfaces;
using MoblyUp.Application.Utils;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Interfaces;

namespace MoblyUp.Application.UseCases
{
    // Ações do cliente sobre o próprio pedido: aceitar/recusar a proposta e confirmar o pagamento
    public class ResponderPropostaUseCase
    {
        private readonly IPedidoMontagemRepository _pedidoRepository;
        private readonly INotificacaoService _notificacaoService;

        public ResponderPropostaUseCase(IPedidoMontagemRepository pedidoRepository, INotificacaoService notificacaoService)
        {
            _pedidoRepository = pedidoRepository;
            _notificacaoService = notificacaoService;
        }

        public Task AceitarAsync(Guid pedidoId, string clienteId) =>
            ExecutarAsync(pedidoId, clienteId, pedido => pedido.AceitarProposta(),
                "Proposta aceita! 🎉",
                pedido => $"O cliente aceitou {Moeda.Formatar(pedido.ValorProposto)} pelo {pedido.TipoMovel}. Aguarde a confirmação do pagamento.");

        public Task RecusarAsync(Guid pedidoId, string clienteId) =>
            ExecutarAsync(pedidoId, clienteId, pedido => pedido.RecusarProposta(),
                "Proposta recusada",
                pedido => $"O cliente recusou sua proposta para o {pedido.TipoMovel}. O pedido continua aberto para uma nova oferta.");

        // Pagamento simulado: ainda não há gateway integrado, o cliente confirma o PIX no app
        public Task ConfirmarPagamentoAsync(Guid pedidoId, string clienteId) =>
            ExecutarAsync(pedidoId, clienteId, pedido => pedido.ConfirmarPagamento(),
                "Pagamento confirmado! ✅",
                pedido => $"Você está liberado para montar o {pedido.TipoMovel} em {pedido.EnderecoServico}.");

        // Cliente cancela o pedido antes do pagamento; o montador com proposta (se houver) é avisado
        public Task CancelarAsync(Guid pedidoId, string clienteId) =>
            ExecutarAsync(pedidoId, clienteId, pedido => pedido.Cancelar(),
                "Pedido cancelado",
                pedido => $"O cliente cancelou o pedido de {pedido.TipoMovel}.");

        private async Task ExecutarAsync(Guid pedidoId, string clienteId, Action<PedidoMontagem> acao,
            string tituloMontador, Func<PedidoMontagem, string> mensagemMontador)
        {
            var pedido = await _pedidoRepository.ObterPorIdAsync(pedidoId);

            if (pedido == null)
                throw new Exception("Pedido não encontrado.");

            if (pedido.ClienteId != clienteId)
                throw new UnauthorizedAccessException("Apenas o cliente dono do pedido pode realizar esta ação.");

            // Guarda o montador antes da ação: ao recusar, o pedido deixa de ter montador
            var montadorId = pedido.MontadorId;
            var mensagem = mensagemMontador(pedido);

            acao(pedido);

            await _pedidoRepository.AtualizarAsync(pedido);

            await _notificacaoService.NotificarUsuarioAsync(montadorId, tituloMontador, mensagem, pedido.Id);
        }
    }
}
