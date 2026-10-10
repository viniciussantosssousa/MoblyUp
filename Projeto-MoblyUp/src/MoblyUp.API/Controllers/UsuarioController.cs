using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoblyUp.API.Extensions;
using MoblyUp.Application.DTOs;
using MoblyUp.Application.UseCases;

namespace MoblyUp.API.Controllers
{
    // O usuário é sempre identificado pelo ID token do Firebase
    [Authorize]
    [Route("api/usuarios")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly GerenciarPerfilUseCase _perfilUseCase;

        public UsuarioController(GerenciarPerfilUseCase perfilUseCase)
        {
            _perfilUseCase = perfilUseCase;
        }

        // Cria o usuário na API com o mesmo UID do Firebase (não duplica)
        [HttpPost]
        public Task<IActionResult> SincronizarUsuario([FromBody] SincronizarUsuarioRequest request) =>
            Executar(async () =>
            {
                var criado = await _perfilUseCase.SincronizarAsync(User.ObterUsuarioId(), User.ObterEmail(), request);
                return criado
                    ? StatusCode(201, new { mensagem = "Usuário sincronizado com sucesso!" })
                    : Ok(new { mensagem = "Usuário já sincronizado." });
            });

        [HttpGet("me")]
        public async Task<IActionResult> ObterPerfil()
        {
            var perfil = await _perfilUseCase.ObterAsync(User.ObterUsuarioId());
            return perfil == null ? NotFound(new { erro = "Usuário não encontrado." }) : Ok(perfil);
        }

        [HttpPut("me")]
        public Task<IActionResult> AtualizarPerfil([FromBody] AtualizarPerfilRequest request) =>
            Executar(async () =>
            {
                await _perfilUseCase.AtualizarAsync(User.ObterUsuarioId(), request);
                return Ok(new { mensagem = "Perfil atualizado com sucesso!" });
            });

        // Exclui a conta na API (o app exclui o login no Firebase logo em seguida)
        [HttpDelete("me")]
        public Task<IActionResult> ExcluirConta() =>
            Executar(async () =>
            {
                await _perfilUseCase.ExcluirContaAsync(User.ObterUsuarioId());
                return NoContent();
            });

        // Registra o aparelho do usuário logado para receber notificações push
        [HttpPut("me/push-token")]
        public Task<IActionResult> RegistrarPushToken([FromBody] PushTokenRequest request) =>
            Executar(async () =>
            {
                await _perfilUseCase.RegistrarPushTokenAsync(User.ObterUsuarioId(), request.Token);
                return Ok(new { mensagem = "Notificações ativadas." });
            });

        // Chamado no logout: o aparelho para de receber notificações dessa conta
        [HttpDelete("me/push-token")]
        public async Task<IActionResult> RemoverPushToken()
        {
            await _perfilUseCase.RemoverPushTokenAsync(User.ObterUsuarioId());
            return NoContent();
        }

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
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}
