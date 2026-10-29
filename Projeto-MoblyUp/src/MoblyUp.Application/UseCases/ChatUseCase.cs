using System;
using System.Linq;
using System.Threading.Tasks;
using MoblyUp.Application.DTOs;
using MoblyUp.Application.Interfaces;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Interfaces;
using MoblyUp.Domain.ValueObjects;

namespace MoblyUp.Application.UseCases
{
    // Chat entre cliente e montador de um pedido (como no Uber):
    // - abre quando o cliente aceita a proposta e fica aberto enquanto o serviço está em andamento;
    // - depois de concluído/cancelado a conversa fica só para leitura;
    // - só os dois participantes acessam;
    // - mensagens com telefone, e-mail, links ou outros apps são recusadas (ProtecaoContato).
    public class ChatUseCase
    {
        private static readonly TimeSpan JanelaNumeroPicado = TimeSpan.FromMinutes(10);

        private readonly IPedidoMontagemRepository _pedidoRepository;
        private readonly IMensagemChatRepository _mensagemRepository;
        private readonly INotificacaoService _notificacaoService;
        private readonly ITempoReal _tempoReal;

        public ChatUseCase(
            IPedidoMontagemRepository pedidoRepository,
            IMensagemChatRepository mensagemRepository,
            INotificacaoService notificacaoService,
            ITempoReal tempoReal)
        {
            _pedidoRepository = pedidoRepository;
            _mensagemRepository = mensagemRepository;
            _notificacaoService = notificacaoService;
            _tempoReal = tempoReal;
        }

        public async Task<ConversaResponse> ObterConversaAsync(Guid pedidoId, string usuarioId)
        {
            var pedido = await ObterPedidoDoParticipanteAsync(pedidoId, usuarioId);
            if (!pedido.ChatDisponivel)
                throw new InvalidOperationException("O chat é liberado quando o cliente aceita a proposta do montador.");

            // Abrir a conversa marca como lidas as mensagens recebidas
            await MarcarComoLidasAsync(pedido, usuarioId);

            var mensagens = await _mensagemRepository.ObterDoPedidoAsync(pedidoId);
            var ehCliente = pedido.ClienteId == usuarioId;
            var outra = ehCliente ? pedido.Montador : pedido.Cliente;

            return new ConversaResponse
            {
                PedidoId = pedido.Id,
                TipoMovel = pedido.TipoMovel,
                Status = pedido.Status,
                Aberto = pedido.ChatAberto,
                OutraParteId = pedido.OutraParte(usuarioId),
                OutraParteNome = ReputacaoResponse.PrimeiroNome(outra?.Nome),
                OutraParteFoto = outra?.FotoPerfilUrl,
                OutraParteTipo = ehCliente ? "montador" : "cliente",
                Mensagens = mensagens.Select(MensagemChatResponse.DeEntidade).ToList(),
            };
        }

        public async Task<MensagemChatResponse> EnviarAsync(Guid pedidoId, string remetenteId, string texto)
        {
            var pedido = await ObterPedidoDoParticipanteAsync(pedidoId, remetenteId);

            if (!pedido.ChatAberto)
                throw new InvalidOperationException(pedido.ChatDisponivel
                    ? "Este serviço foi encerrado: a conversa agora é só para leitura."
                    : "O chat é liberado quando o cliente aceita a proposta do montador.");

            // Proteção contra negociação por fora: contato na própria mensagem...
            if (ProtecaoContato.Verificar(texto) != null)
                throw new InvalidOperationException(ProtecaoContato.MensagemBloqueio);

            // ...ou um número de telefone picado em várias mensagens seguidas
            var recentes = await _mensagemRepository.ObterTextosRecentesAsync(pedidoId, remetenteId, JanelaNumeroPicado, 5);
            if (ProtecaoContato.CompletaNumeroComAnteriores(texto, recentes))
                throw new InvalidOperationException(ProtecaoContato.MensagemBloqueio);

            var mensagem = new MensagemChat(pedidoId, remetenteId, texto);
            await _mensagemRepository.AdicionarAsync(mensagem);
            var resposta = MensagemChatResponse.DeEntidade(mensagem);

            var destinatarioId = pedido.OutraParte(remetenteId)!;

            // Tempo real para os dois (o remetente pode estar com o app aberto em outro aparelho)
            await IgnorarFalhaAsync(() => _tempoReal.MensagemChatAsync(new[] { remetenteId, destinatarioId }, resposta));

            // Push para quem recebeu (aparece mesmo com o app fechado)
            var nomeRemetente = ReputacaoResponse.PrimeiroNome(
                (pedido.ClienteId == remetenteId ? pedido.Cliente : pedido.Montador)?.Nome);
            await _notificacaoService.NotificarUsuarioAsync(
                destinatarioId,
                $"💬 {nomeRemetente}",
                mensagem.Texto.Length > 120 ? mensagem.Texto[..117] + "..." : mensagem.Texto,
                pedidoId,
                "chat");

            return resposta;
        }

        public async Task MarcarComoLidasAsync(Guid pedidoId, string usuarioId)
        {
            var pedido = await ObterPedidoDoParticipanteAsync(pedidoId, usuarioId);
            await MarcarComoLidasAsync(pedido, usuarioId);
        }

        private async Task MarcarComoLidasAsync(PedidoMontagem pedido, string usuarioId)
        {
            var marcadas = await _mensagemRepository.MarcarComoLidasAsync(pedido.Id, usuarioId);
            var outraParte = pedido.OutraParte(usuarioId);

            // Avisa quem enviou que as mensagens foram vistas ("✓✓")
            if (marcadas > 0 && outraParte != null)
                await IgnorarFalhaAsync(() => _tempoReal.ChatLidoAsync(outraParte, pedido.Id));
        }

        private async Task<PedidoMontagem> ObterPedidoDoParticipanteAsync(Guid pedidoId, string usuarioId)
        {
            var pedido = await _pedidoRepository.ObterPorIdAsync(pedidoId)
                ?? throw new KeyNotFoundException("Pedido não encontrado.");

            if (!pedido.Participa(usuarioId) || pedido.MontadorId == null)
                throw new UnauthorizedAccessException("Só o cliente e o montador do serviço participam desta conversa.");

            return pedido;
        }

        private static async Task IgnorarFalhaAsync(Func<Task> acao)
        {
            try { await acao(); } catch { /* aviso em tempo real é complementar */ }
        }
    }
}
