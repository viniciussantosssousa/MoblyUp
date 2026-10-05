using System;
using System.Threading.Tasks;
using MoblyUp.Application.DTOs;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Interfaces;

namespace MoblyUp.Application.UseCases
{
    public class CriarPedidoMontagemUseCase
    {
        private readonly IPedidoMontagemRepository _pedidoRepository;

        public CriarPedidoMontagemUseCase(IPedidoMontagemRepository pedidoRepository)
        {
            _pedidoRepository = pedidoRepository;
        }

        public async Task ExecutarAsync(CriarPedidoMontagemRequest request)
        {
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