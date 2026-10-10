using System;
using System.Threading.Tasks;
using MoblyUp.Application.Interfaces;
using MoblyUp.Domain.Enums;
using MoblyUp.Domain.Interfaces;

namespace MoblyUp.Application.UseCases
{
    // O montador pode negar um serviço de duas formas:
    // - Recusar um pedido do feed: o pedido some só para ele e continua aberto para os outros.
    // - Desistir de uma proposta antes do pagamento: o pedido volta a ficar aberto e o cliente é avisado.
    public class AcoesMontadorUseCase
    {
        private readonly IPedidoMontagemRepository _pedidoRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly INotificacaoService _notificacaoService;

        public AcoesMontadorUseCase(IPedidoMontagemRepository pedidoRepository, IUsuarioRepository usuarioRepository, INotificacaoService notificacaoService)
        {
            _pedidoRepository = pedidoRepository;
            _usuarioRepository = usuarioRepository;
            _notificacaoService = notificacaoService;
        }

        public async Task RecusarPedidoAsync(Guid pedidoId, string montadorId)
        {
            var montador = await _usuarioRepository.ObterPorIdAsync(montadorId);
            if (montador == null || montador.Perfil != TipoPerfil.Montador)
                throw new UnauthorizedAccessException("Apenas montadores podem recusar pedidos.");

            var pedido = await _pedidoRepository.ObterPorIdAsync(pedidoId);
            if (pedido == null)
                throw new Exception("Pedido não encontrado.");

            if (pedido.Status != StatusPedido.Disponivel)
                throw new Exception("Este pedido não está mais aberto para propostas.");

            await _pedidoRepository.RegistrarRecusaAsync(pedidoId, montadorId);
        }

        public async Task DesistirAsync(Guid pedidoId, string montadorId)
        {
            var pedido = await _pedidoRepository.ObterPorIdAsync(pedidoId);
            if (pedido == null)
                throw new Exception("Pedido não encontrado.");

            pedido.DesistirProposta(montadorId);
            await _pedidoRepository.AtualizarAsync(pedido);

            // Não volta para o feed de quem desistiu
            await _pedidoRepository.RegistrarRecusaAsync(pedidoId, montadorId);

            await _notificacaoService.NotificarUsuarioAsync(
                pedido.ClienteId,
                "O montador desistiu do serviço",
                $"Seu pedido de {pedido.TipoMovel} voltou a ficar disponível para outros montadores.",
                pedido.Id);
        }
    }
}
