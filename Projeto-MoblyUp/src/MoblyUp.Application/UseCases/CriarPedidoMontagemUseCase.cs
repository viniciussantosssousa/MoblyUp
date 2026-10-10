using System;
using System.Threading.Tasks;
using MoblyUp.Application.DTOs;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Enums;
using MoblyUp.Domain.Interfaces;

namespace MoblyUp.Application.UseCases
{
    public class CriarPedidoMontagemUseCase
    {
        private readonly IPedidoMontagemRepository _pedidoRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public CriarPedidoMontagemUseCase(IPedidoMontagemRepository pedidoRepository, IUsuarioRepository usuarioRepository)
        {
            _pedidoRepository = pedidoRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task ExecutarAsync(CriarPedidoMontagemRequest request)
        {
            var cliente = await _usuarioRepository.ObterPorIdAsync(request.ClienteId);
            if (cliente == null || cliente.Perfil != TipoPerfil.Cliente)
                throw new UnauthorizedAccessException("Apenas clientes cadastrados podem solicitar montagens.");

            if (string.IsNullOrWhiteSpace(request.TipoMovel) || string.IsNullOrWhiteSpace(request.EnderecoServico))
                throw new Exception("Informe o tipo do móvel e o endereço do serviço.");

            var pedido = new PedidoMontagem(
                request.ClienteId,
                request.TipoMovel,
                request.TamanhoMovel,
                request.UrlFotoMovelTemporaria,
                request.EnderecoServico
            );

            await _pedidoRepository.AdicionarAsync(pedido);
        }
    }
}
