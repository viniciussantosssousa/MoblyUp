using System;
using System.Threading.Tasks;
using MoblyUp.Application.DTOs;
using MoblyUp.Application.Interfaces;
using MoblyUp.Application.Utils;
using MoblyUp.Domain.Enums;
using MoblyUp.Domain.Interfaces;

namespace MoblyUp.Application.UseCases
{
    // Montador envia uma proposta de valor para um pedido disponível
    public class AceitarPedidoUseCase
    {
        private readonly IPedidoMontagemRepository _pedidoRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly INotificacaoService _notificacaoService;

        public AceitarPedidoUseCase(IPedidoMontagemRepository pedidoRepository, IUsuarioRepository usuarioRepository, INotificacaoService notificacaoService)
        {
            _pedidoRepository = pedidoRepository;
            _usuarioRepository = usuarioRepository;
            _notificacaoService = notificacaoService;
        }

        public async Task ExecutarAsync(Guid pedidoId, string montadorId, AceitarPedidoRequest request)
        {
            var montador = await _usuarioRepository.ObterPorIdAsync(montadorId);
            if (montador == null || montador.Perfil != TipoPerfil.Montador)
                throw new UnauthorizedAccessException("Apenas montadores cadastrados podem enviar propostas.");

            if (string.IsNullOrWhiteSpace(montador.ChavePix))
                throw new Exception("Cadastre sua chave Pix no Perfil antes de enviar propostas.");

            var pedido = await _pedidoRepository.ObterPorIdAsync(pedidoId);

            if (pedido == null)
                throw new Exception("Pedido não encontrado.");

            pedido.ReceberProposta(montadorId, request.ValorProposto);

            await _pedidoRepository.AtualizarAsync(pedido);

            await _notificacaoService.NotificarUsuarioAsync(
                pedido.ClienteId,
                "Nova proposta recebida! 💰",
                $"{montador.Nome} quer montar seu {pedido.TipoMovel} por {Moeda.Formatar(pedido.ValorProposto)}.",
                pedido.Id);
        }
    }
}
