using System;
using System.Threading.Tasks;
using MoblyUp.Application.DTOs;
using MoblyUp.Domain.Enums;
using MoblyUp.Domain.Interfaces;

namespace MoblyUp.Application.UseCases
{
    // Montador envia uma proposta de valor para um pedido disponível
    public class AceitarPedidoUseCase
    {
        private readonly IPedidoMontagemRepository _pedidoRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public AceitarPedidoUseCase(IPedidoMontagemRepository pedidoRepository, IUsuarioRepository usuarioRepository)
        {
            _pedidoRepository = pedidoRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task ExecutarAsync(Guid pedidoId, AceitarPedidoRequest request)
        {
            var montador = await _usuarioRepository.ObterPorIdAsync(request.MontadorId);
            if (montador == null || montador.Perfil != TipoPerfil.Montador)
                throw new UnauthorizedAccessException("Apenas montadores cadastrados podem enviar propostas.");

            if (string.IsNullOrWhiteSpace(montador.ChavePix))
                throw new Exception("Cadastre sua chave Pix no Perfil antes de enviar propostas.");

            var pedido = await _pedidoRepository.ObterPorIdAsync(pedidoId);

            if (pedido == null)
                throw new Exception("Pedido não encontrado.");

            pedido.ReceberProposta(request.MontadorId, request.ValorProposto);

            await _pedidoRepository.AtualizarAsync(pedido);
        }
    }
}
