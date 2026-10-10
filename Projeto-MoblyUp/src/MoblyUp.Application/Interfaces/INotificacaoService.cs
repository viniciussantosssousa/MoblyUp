using System;
using System.Threading.Tasks;

namespace MoblyUp.Application.Interfaces
{
    // Envia notificações push. Falhas no envio nunca devem desfazer a operação do pedido.
    public interface INotificacaoService
    {
        Task NotificarUsuarioAsync(string? usuarioId, string titulo, string mensagem, Guid pedidoId);

        // Avisa os montadores cuja região ("Cidade - UF") bate com o final do endereço do serviço
        Task NotificarMontadoresDaRegiaoAsync(string enderecoServico, string titulo, string mensagem, Guid pedidoId);
    }
}
