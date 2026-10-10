using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MoblyUp.Application.Interfaces;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Interfaces;

namespace MoblyUp.Infrastructure.Services
{
    // Envia as notificações pelo serviço gratuito do Expo (https://exp.host/--/api/v2/push/send),
    // que repassa para o Firebase Cloud Messaging (Android) e para o APNs (iOS).
    public class ExpoNotificacaoService : INotificacaoService
    {
        private const string UrlExpoPush = "https://exp.host/--/api/v2/push/send";
        private const int LoteMaximo = 100; // limite de mensagens por requisição do Expo

        private readonly HttpClient _httpClient;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ILogger<ExpoNotificacaoService> _logger;

        public ExpoNotificacaoService(HttpClient httpClient, IUsuarioRepository usuarioRepository, ILogger<ExpoNotificacaoService> logger)
        {
            _httpClient = httpClient;
            _usuarioRepository = usuarioRepository;
            _logger = logger;
        }

        public async Task NotificarUsuarioAsync(string? usuarioId, string titulo, string mensagem, Guid pedidoId)
        {
            if (string.IsNullOrEmpty(usuarioId))
                return;

            var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
            if (usuario?.ExpoPushToken == null)
                return;

            await EnviarAsync(new[] { usuario }, titulo, mensagem, pedidoId);
        }

        public async Task NotificarMontadoresDaRegiaoAsync(string enderecoServico, string titulo, string mensagem, Guid pedidoId)
        {
            var montadores = await _usuarioRepository.ObterMontadoresComPushAsync();
            var daRegiao = montadores
                .Where(m => m.Regiao != null && enderecoServico.EndsWith(m.Regiao, StringComparison.OrdinalIgnoreCase))
                .ToList();

            await EnviarAsync(daRegiao, titulo, mensagem, pedidoId);
        }

        private async Task EnviarAsync(IReadOnlyCollection<Usuario> destinatarios, string titulo, string mensagem, Guid pedidoId)
        {
            if (destinatarios.Count == 0)
                return;

            foreach (var lote in destinatarios.Chunk(LoteMaximo))
            {
                var mensagens = lote.Select(u => new
                {
                    to = u.ExpoPushToken,
                    title = titulo,
                    body = mensagem,
                    sound = "default",
                    channelId = "default",
                    // O app usa estes dados para abrir a tela certa ao tocar na notificação
                    data = new { pedidoId }
                }).ToList();

                try
                {
                    var resposta = await _httpClient.PostAsJsonAsync(UrlExpoPush, mensagens);
                    var conteudo = await resposta.Content.ReadAsStringAsync();

                    if (!resposta.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Expo Push recusou o envio ({Status}): {Conteudo}", (int)resposta.StatusCode, conteudo);
                        continue;
                    }

                    await TratarTicketsAsync(lote, conteudo);
                }
                catch (Exception ex)
                {
                    // Notificação é um "extra": o pedido já foi salvo e não pode falhar por causa dela
                    _logger.LogWarning(ex, "Falha ao enviar notificação push.");
                }
            }
        }

        // Cada mensagem gera um "ticket". Se o aparelho não está mais registrado
        // (app desinstalado, permissão revogada), remove o token para não insistir.
        private async Task TratarTicketsAsync(Usuario[] lote, string conteudo)
        {
            using var json = JsonDocument.Parse(conteudo);
            if (!json.RootElement.TryGetProperty("data", out var tickets) || tickets.ValueKind != JsonValueKind.Array)
                return;

            var i = 0;
            foreach (var ticket in tickets.EnumerateArray())
            {
                var usuario = lote[i++];
                if (ticket.GetProperty("status").GetString() != "error")
                    continue;

                var erro = ticket.TryGetProperty("details", out var detalhes) && detalhes.TryGetProperty("error", out var e)
                    ? e.GetString()
                    : null;

                _logger.LogWarning("Push para o usuário {UsuarioId} falhou: {Erro}", usuario.Id, erro ?? ticket.ToString());

                if (erro == "DeviceNotRegistered")
                {
                    usuario.RemoverPushToken();
                    await _usuarioRepository.AtualizarAsync(usuario);
                }
            }
        }
    }
}
