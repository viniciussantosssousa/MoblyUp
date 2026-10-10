using System;
using System.Threading.Tasks;
using MoblyUp.Application.DTOs;
using MoblyUp.Application.Interfaces;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Enums;
using MoblyUp.Domain.Interfaces;

namespace MoblyUp.Application.UseCases
{
    public class CriarPedidoMontagemUseCase
    {
        private readonly IPedidoMontagemRepository _pedidoRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly INotificacaoService _notificacaoService;

        public CriarPedidoMontagemUseCase(IPedidoMontagemRepository pedidoRepository, IUsuarioRepository usuarioRepository, INotificacaoService notificacaoService)
        {
            _pedidoRepository = pedidoRepository;
            _usuarioRepository = usuarioRepository;
            _notificacaoService = notificacaoService;
        }

        public async Task ExecutarAsync(string clienteId, CriarPedidoMontagemRequest request)
        {
            var cliente = await _usuarioRepository.ObterPorIdAsync(clienteId);
            if (cliente == null || cliente.Perfil != TipoPerfil.Cliente)
                throw new UnauthorizedAccessException("Apenas clientes cadastrados podem solicitar montagens.");

            if (string.IsNullOrWhiteSpace(request.TipoMovel) || string.IsNullOrWhiteSpace(request.EnderecoServico))
                throw new Exception("Informe o tipo do móvel e o endereço do serviço.");

            var pedido = new PedidoMontagem(
                clienteId,
                request.TipoMovel,
                request.TamanhoMovel,
                request.UrlFotoMovelTemporaria,
                request.EnderecoServico
            );

            await _pedidoRepository.AdicionarAsync(pedido);

            await _notificacaoService.NotificarMontadoresDaRegiaoAsync(
                pedido.EnderecoServico,
                "Nova montagem na sua região 🔨",
                $"{pedido.TipoMovel} ({pedido.TamanhoMovel}). Envie sua proposta!",
                pedido.Id);
        }
    }
}
