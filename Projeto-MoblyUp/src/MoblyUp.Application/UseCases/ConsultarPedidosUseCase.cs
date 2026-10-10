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
        private readonly IUsuarioRepository _usuarioRepository;

        public ConsultarPedidosUseCase(IPedidoMontagemRepository pedidoRepository, IUsuarioRepository usuarioRepository)
        {
            _pedidoRepository = pedidoRepository;
            _usuarioRepository = usuarioRepository;
        }

        // Pode ver o pedido: o cliente dono, o montador vinculado ou
        // qualquer montador enquanto o pedido estiver aberto para propostas.
        public async Task<PedidoMontagemResponse?> ObterPorIdAsync(Guid id, string usuarioId)
        {
            var pedido = await _pedidoRepository.ObterPorIdAsync(id);
            if (pedido == null)
                return null;

            var participa = pedido.ClienteId == usuarioId || pedido.MontadorId == usuarioId;
            if (!participa)
            {
                var aberto = pedido.Status == StatusPedido.Disponivel && await EhMontadorAsync(usuarioId);
                if (!aberto)
                    throw new UnauthorizedAccessException("Você não tem acesso a este pedido.");
            }

            return PedidoMontagemResponse.DeEntidade(pedido);
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

        // Pedidos abertos para propostas (somente montadores). A região vem no formato
        // "Cidade - UF" e o endereço do pedido termina com ela (ver SolicitarServicoScreen).
        public async Task<IEnumerable<PedidoMontagemResponse>> ObterDisponiveisAsync(string usuarioId, string? regiao)
        {
            if (!await EhMontadorAsync(usuarioId))
                throw new UnauthorizedAccessException("Apenas montadores podem ver as solicitações disponíveis.");

            var pedidos = await _pedidoRepository.ObterPorStatusAsync(StatusPedido.Disponivel);

            // Esconde os pedidos que este montador já recusou ou dos quais desistiu
            var recusados = await _pedidoRepository.ObterIdsRecusadosAsync(usuarioId);
            pedidos = pedidos.Where(p => !recusados.Contains(p.Id));

            if (!string.IsNullOrWhiteSpace(regiao))
            {
                var regiaoNormalizada = regiao.Trim();
                pedidos = pedidos.Where(p =>
                    p.EnderecoServico.EndsWith(regiaoNormalizada, StringComparison.OrdinalIgnoreCase));
            }

            return Ordenar(pedidos.Select(PedidoMontagemResponse.DeEntidade));
        }

        private async Task<bool> EhMontadorAsync(string usuarioId)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
            return usuario?.Perfil == TipoPerfil.Montador;
        }

        private static IEnumerable<PedidoMontagemResponse> Ordenar(IEnumerable<PedidoMontagemResponse> pedidos)
        {
            return pedidos.OrderByDescending(p => p.DataCriacao).ToList();
        }
    }
}
