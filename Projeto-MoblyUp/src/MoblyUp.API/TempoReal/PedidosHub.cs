using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MoblyUp.API.Extensions;
using MoblyUp.Application.Interfaces;
using MoblyUp.Domain.Enums;
using MoblyUp.Domain.Interfaces;
using MoblyUp.Domain.ValueObjects;

namespace MoblyUp.API.TempoReal
{
    // Conexão em tempo real com o app (WebSocket). Cada usuário entra no grupo dele e,
    // se for montador, no grupo da sua região para receber os pedidos novos da cidade.
    [Authorize]
    public class PedidosHub : Hub
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public PedidosHub(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public static string GrupoUsuario(string usuarioId) => $"usuario:{usuarioId}";
        public static string GrupoRegiao(string regiao) => $"regiao:{Regiao.Chave(regiao)}";

        public override async Task OnConnectedAsync()
        {
            var usuarioId = Context.User!.ObterUsuarioId();
            await Groups.AddToGroupAsync(Context.ConnectionId, GrupoUsuario(usuarioId));

            var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
            if (usuario?.Perfil == TipoPerfil.Montador && !string.IsNullOrWhiteSpace(usuario.Regiao))
                await Groups.AddToGroupAsync(Context.ConnectionId, GrupoRegiao(usuario.Regiao));

            await base.OnConnectedAsync();
        }
    }

    // Eventos enviados ao app:
    // - "pedidoAlterado" { pedidoId }: a tela recarrega os dados
    // - "aviso" { titulo, mensagem, pedidoId }: banner no topo da tela
    public class SignalRTempoReal : ITempoReal
    {
        private readonly IHubContext<PedidosHub> _hub;

        public SignalRTempoReal(IHubContext<PedidosHub> hub)
        {
            _hub = hub;
        }

        public Task PedidoAlteradoAsync(Guid pedidoId, string? regiao, IEnumerable<string?> usuarios)
        {
            var grupos = usuarios
                .Where(u => !string.IsNullOrEmpty(u))
                .Distinct()
                .Select(u => PedidosHub.GrupoUsuario(u!))
                .ToList();

            // O feed dos montadores da cidade também muda (pedido novo, que saiu ou que voltou)
            if (!string.IsNullOrWhiteSpace(regiao))
                grupos.Add(PedidosHub.GrupoRegiao(regiao));

            return _hub.Clients.Groups(grupos).SendAsync("pedidoAlterado", new { pedidoId });
        }

        public Task AvisarUsuarioAsync(string usuarioId, string titulo, string mensagem, Guid pedidoId) =>
            _hub.Clients.Group(PedidosHub.GrupoUsuario(usuarioId)).SendAsync("aviso", new { titulo, mensagem, pedidoId });

        public Task AvisarRegiaoAsync(string regiao, string titulo, string mensagem, Guid pedidoId) =>
            _hub.Clients.Group(PedidosHub.GrupoRegiao(regiao)).SendAsync("aviso", new { titulo, mensagem, pedidoId });
    }
}
