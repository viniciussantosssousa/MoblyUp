using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoblyUp.Application.DTOs;

namespace MoblyUp.Application.Interfaces
{
    // Avisos instantâneos para os apps conectados (SignalR). Complementa o push:
    // atualiza as telas abertas sem o usuário recarregar e mostra o aviso dentro do app.
    public interface ITempoReal
    {
        // Um pedido mudou: as telas dos envolvidos (e o feed da região) recarregam os dados
        Task PedidoAlteradoAsync(Guid pedidoId, string? regiao, IEnumerable<string?> usuarios);

        // Aviso visível dentro do app (mesmo conteúdo do push)
        Task AvisarUsuarioAsync(string usuarioId, string titulo, string mensagem, Guid pedidoId, string? tipo = null);
        Task AvisarRegiaoAsync(string regiao, string titulo, string mensagem, Guid pedidoId);

        // Chat: nova mensagem (para os dois participantes) e confirmação de leitura (para quem enviou)
        Task MensagemChatAsync(IEnumerable<string> usuarios, MensagemChatResponse mensagem);
        Task ChatLidoAsync(string usuarioId, Guid pedidoId);
    }
}
