using System;
using System.Threading.Tasks;
using MoblyUp.Domain.Interfaces;

namespace MoblyUp.Application.UseCases
{
    public class EnviarFotoConclusaoUseCase
    {
        private readonly IPedidoMontagemRepository _pedidoRepository;

        public EnviarFotoConclusaoUseCase(IPedidoMontagemRepository pedidoRepository)
        {
            _pedidoRepository = pedidoRepository;
        }

        public async Task ExecutarAsync(Guid pedidoId, string urlFoto)
        {
            var pedido = await _pedidoRepository.ObterPorIdAsync(pedidoId);
            
            if (pedido == null)
                throw new Exception("Pedido não encontrado.");

            pedido.EnviarFotoConclusao(urlFoto);

            await _pedidoRepository.AtualizarAsync(pedido);
        }
    }
}