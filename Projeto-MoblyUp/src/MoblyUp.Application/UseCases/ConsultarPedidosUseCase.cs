using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoblyUp.Application.DTOs;
using MoblyUp.Domain.Enums;
using MoblyUp.Domain.Interfaces;

namespace MoblyUp.Application.UseCases
{
    public class ConsultarPedidosUseCase
    {
        private readonly IPedidoMontagemRepository _pedidoRepository;

        public ConsultarPedidosUseCase(IPedidoMontagemRepository pedidoRepository)
        {
            _pedidoRepository = pedidoRepository;
        }

        public async Task<PedidoMontagemResponse?> ObterPorIdAsync(Guid id)
        {
            var pedido = await _pedidoRepository.ObterPorIdAsync(id);
            return pedido == null ? null : PedidoMontagemResponse.DeEntidade(pedido);
        }

        public async Task<IEnumerable<PedidoMontagemResponse>> ObterDoClienteAsync(string clienteId)
        {
            var pedidos = await _pedidoRepository.ObterPorClienteIdAsync(clienteId);
            return Ordenar(pedidos.Select(PedidoMontagemResponse.DeEntidade));
        }

        public async Task<IEnumerable<PedidoMontagemResponse>> ObterDoMontadorAsync(string montadorId)
        {
            var pedidos = await _pedidoRepository.ObterPorMontadorIdAsync(montadorId);
            return Ordenar(pedidos.Select(PedidoMontagemResponse.DeEntidade));
        }

        // Pedidos abertos para propostas. A região vem no formato "Cidade - UF"
        // e o endereço do pedido termina com ela (ver SolicitarServicoScreen).
        public async Task<IEnumerable<PedidoMontagemResponse>> ObterDisponiveisAsync(string? regiao)
        {
            var pedidos = await _pedidoRepository.ObterPorStatusAsync(StatusPedido.Disponivel);

            if (!string.IsNullOrWhiteSpace(regiao))
            {
                var regiaoNormalizada = regiao.Trim();
                pedidos = pedidos.Where(p =>
                    p.EnderecoServico.EndsWith(regiaoNormalizada, StringComparison.OrdinalIgnoreCase));
            }

            return Ordenar(pedidos.Select(PedidoMontagemResponse.DeEntidade));
        }

        private static IEnumerable<PedidoMontagemResponse> Ordenar(IEnumerable<PedidoMontagemResponse> pedidos)
        {
            return pedidos.OrderByDescending(p => p.DataCriacao).ToList();
        }
    }
}
