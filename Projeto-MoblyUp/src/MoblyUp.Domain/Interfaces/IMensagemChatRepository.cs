using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoblyUp.Domain.Entities;

namespace MoblyUp.Domain.Interfaces
{
    public interface IMensagemChatRepository
    {
        Task AdicionarAsync(MensagemChat mensagem);
        Task<IReadOnlyList<MensagemChat>> ObterDoPedidoAsync(Guid pedidoId);

        // Últimas mensagens de um remetente (mais recentes primeiro), para detectar número picado
        Task<IReadOnlyList<string>> ObterTextosRecentesAsync(Guid pedidoId, string remetenteId, TimeSpan janela, int limite);

        // Marca como lidas as mensagens recebidas pelo leitor; devolve quantas foram marcadas
        Task<int> MarcarComoLidasAsync(Guid pedidoId, string leitorId);

        // Mensagens não lidas pelo usuário em cada pedido (para os contadores das telas)
        Task<Dictionary<Guid, int>> ContarNaoLidasAsync(string usuarioId, IEnumerable<Guid> pedidoIds);
    }
}
