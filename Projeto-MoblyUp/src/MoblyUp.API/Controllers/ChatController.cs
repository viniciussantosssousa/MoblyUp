using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoblyUp.API.Extensions;
using MoblyUp.Application.DTOs;
using MoblyUp.Application.UseCases;

namespace MoblyUp.API.Controllers
{
    // Chat entre cliente e montador de um pedido (só os dois participantes)
    [Authorize]
    [ApiController]
    [Route("api/pedidos/{pedidoId:guid}/chat")]
    public class ChatController : ControllerBase
    {
        private readonly ChatUseCase _chatUseCase;

        public ChatController(ChatUseCase chatUseCase)
        {
            _chatUseCase = chatUseCase;
        }

        // Conversa completa (abrir já marca as mensagens recebidas como lidas)
        [HttpGet]
        public Task<IActionResult> ObterConversa(Guid pedidoId) =>
            Executar(async () => Ok(await _chatUseCase.ObterConversaAsync(pedidoId, User.ObterUsuarioId())));

        [HttpPost]
        public Task<IActionResult> Enviar(Guid pedidoId, [FromBody] EnviarMensagemRequest request) =>
            Executar(async () => StatusCode(201, await _chatUseCase.EnviarAsync(pedidoId, User.ObterUsuarioId(), request.Texto)));

        // Chamado quando chega mensagem com a conversa aberta na tela
        [HttpPut("lidas")]
        public Task<IActionResult> MarcarComoLidas(Guid pedidoId) =>
            Executar(async () =>
            {
                await _chatUseCase.MarcarComoLidasAsync(pedidoId, User.ObterUsuarioId());
                return NoContent();
            });

        private async Task<IActionResult> Executar(Func<Task<IActionResult>> acao)
        {
            try
            {
                return await acao();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { erro = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}
