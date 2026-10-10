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
        private readonly PerfilPublicoUseCase _perfilPublicoUseCase;

        public UsuarioController(GerenciarPerfilUseCase perfilUseCase, PerfilPublicoUseCase perfilPublicoUseCase)
        {
            _perfilUseCase = perfilUseCase;
            _perfilPublicoUseCase = perfilPublicoUseCase;
        }

        // Perfil de outro usuário: reputação, comentários e contato (quando liberado).
        // Só para quem tem (ou pode ter) um serviço com ele; nunca expõe CPF, e-mail ou Pix.
        [HttpGet("{id}/perfil-publico")]
        public async Task<IActionResult> ObterPerfilPublico(string id)
        {
            try
            {
                return Ok(await _perfilPublicoUseCase.ObterAsync(id, User.ObterUsuarioId()));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { erro = ex.Message });
            }
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

        // Completar cadastro: contas antigas informam o CPF uma única vez
        [HttpPut("me/cpf")]
        public Task<IActionResult> CadastrarCpf([FromBody] CadastrarCpfRequest request) =>
            Executar(async () =>
            {
                await _perfilUseCase.CadastrarCpfAsync(User.ObterUsuarioId(), request.Cpf);
                return Ok(new { mensagem = "CPF cadastrado com sucesso!" });
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
