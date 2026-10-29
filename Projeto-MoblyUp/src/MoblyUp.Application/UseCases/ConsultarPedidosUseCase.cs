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
        private readonly IMensagemChatRepository _mensagemRepository;

        public ConsultarPedidosUseCase(IPedidoMontagemRepository pedidoRepository, IUsuarioRepository usuarioRepository, IMensagemChatRepository mensagemRepository)
        {
            _pedidoRepository = pedidoRepository;
            _usuarioRepository = usuarioRepository;
            _mensagemRepository = mensagemRepository;
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

            var resposta = PedidoMontagemResponse.DeEntidade(pedido);
            await PreencherReputacoesAsync(new[] { resposta });
            await PreencherMensagensNaoLidasAsync(new[] { resposta }, usuarioId);
            return resposta;
        }

        public async Task<IEnumerable<PedidoMontagemResponse>> ObterDoClienteAsync(string clienteId)
        {
            var pedidos = await _pedidoRepository.ObterPorClienteIdAsync(clienteId);
            var lista = await MontarListaAsync(pedidos.Select(PedidoMontagemResponse.DeEntidade));
            await PreencherMensagensNaoLidasAsync(lista, clienteId);
            return lista;
        }

        public async Task<IEnumerable<PedidoMontagemResponse>> ObterDoMontadorAsync(string montadorId)
        {
            var pedidos = await _pedidoRepository.ObterPorMontadorIdAsync(montadorId);
            var lista = await MontarListaAsync(pedidos.Select(PedidoMontagemResponse.DeEntidade));
            await PreencherMensagensNaoLidasAsync(lista, montadorId);
            return lista;
        }

        // Contador de mensagens novas do chat (uma consulta agrupada para a lista inteira)
        private async Task PreencherMensagensNaoLidasAsync(IReadOnlyCollection<PedidoMontagemResponse> pedidos, string usuarioId)
        {
            var comChat = pedidos.Where(p => p.ChatDisponivel).Select(p => p.Id).ToList();
            var naoLidas = await _mensagemRepository.ContarNaoLidasAsync(usuarioId, comChat);
            foreach (var pedido in pedidos)
                pedido.MensagensNaoLidas = naoLidas.GetValueOrDefault(pedido.Id);
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

            return await MontarListaAsync(pedidos.Select(PedidoMontagemResponse.DeEntidade));
        }

        private async Task<List<PedidoMontagemResponse>> MontarListaAsync(IEnumerable<PedidoMontagemResponse> pedidos)
        {
            var lista = pedidos.OrderByDescending(p => p.DataCriacao).ToList();
            await PreencherReputacoesAsync(lista);
            return lista;
        }

        // Reputação de cliente e montador de cada pedido, com duas consultas agrupadas no total
        private async Task PreencherReputacoesAsync(IReadOnlyCollection<PedidoMontagemResponse> pedidos)
        {
            var clientes = await _pedidoRepository.ObterResumosReputacaoAsync(
                pedidos.Select(p => p.ClienteId), TipoPerfil.Cliente);
            var montadores = await _pedidoRepository.ObterResumosReputacaoAsync(
                pedidos.Where(p => p.MontadorId != null).Select(p => p.MontadorId!), TipoPerfil.Montador);

            foreach (var pedido in pedidos)
            {
                if (clientes.TryGetValue(pedido.ClienteId, out var cliente))
                {
                    pedido.MediaAvaliacaoCliente = Math.Round(cliente.Media, 1);
                    pedido.TotalAvaliacoesCliente = cliente.Total;
                }

                if (pedido.MontadorId != null && montadores.TryGetValue(pedido.MontadorId, out var montador))
                {
                    pedido.MediaAvaliacaoMontador = Math.Round(montador.Media, 1);
                    pedido.TotalAvaliacoesMontador = montador.Total;
                }
            }
        }

        private async Task<bool> EhMontadorAsync(string usuarioId)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
            return usuario?.Perfil == TipoPerfil.Montador;
        }
    }
}
