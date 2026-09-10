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

        // Injetamos apenas o repositório do Pedido agora
        public CriarPedidoMontagemUseCase(IPedidoMontagemRepository pedidoRepository)
        {
            _pedidoRepository = pedidoRepository;
        }

        public async Task ExecutarAsync(CriarPedidoMontagemRequest request)
        {
            // Criamos o pedido diretamente com os dados dinâmicos do cliente
            var pedido = new PedidoMontagem(
                request.ClienteId,
                request.TipoMovel,
                request.TamanhoMovel,
                request.UrlFotoMovelTemporaria,
                request.EnderecoServico
            );

            // Salvamos no banco de dados
            await _pedidoRepository.AdicionarAsync(pedido);
        }
    }
}