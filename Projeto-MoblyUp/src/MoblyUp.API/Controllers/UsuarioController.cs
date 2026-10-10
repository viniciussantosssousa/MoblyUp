using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoblyUp.API.Extensions;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Interfaces;
using MoblyUp.Domain.Enums;
using System.Threading.Tasks;

namespace MoblyUp.API.Controllers
{
    // O usuário é sempre identificado pelo ID token do Firebase
    [Authorize]
    [Route("api/usuarios")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioController(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        [HttpPost]
        public async Task<IActionResult> SincronizarUsuario([FromBody] SincronizarUsuarioRequest request)
        {
            // ID e e-mail vêm do token: ninguém consegue sincronizar em nome de outra conta
            var id = User.ObterUsuarioId();
            var email = User.ObterEmail() ?? request.Email;

            // Verifica se o usuário já existe para não duplicar
            var usuarioExistente = await _usuarioRepository.ObterPorIdAsync(id);
            if (usuarioExistente != null)
            {
                // Contas antigas ainda não tinham a região salva na API
                if (!string.IsNullOrWhiteSpace(request.Regiao) && usuarioExistente.Regiao != request.Regiao.Trim())
                {
                    usuarioExistente.DefinirRegiao(request.Regiao);
                    await _usuarioRepository.AtualizarAsync(usuarioExistente);
                }
                return Ok(new { mensagem = "Usuário já sincronizado.", id = usuarioExistente.Id });
            }

            var perfil = request.Tipo.ToLower() == "montador" ? TipoPerfil.Montador : TipoPerfil.Cliente;

            // Cria a entidade usando o ID gerado lá no Firebase
            var usuario = new Usuario(
                request.Nome,
                null, // CPF ainda não é coletado no cadastro do app
                email,
                "", // Senha vazia pois o Firebase já cuida da segurança
                "", // Telefone provisório
                perfil,
                id
            );
            usuario.DefinirRegiao(request.Regiao);

            await _usuarioRepository.AdicionarAsync(usuario);
            return Created("", new { mensagem = "Usuário sincronizado com sucesso!", id = usuario.Id });
        }

        [HttpGet("me")]
        public async Task<IActionResult> ObterPerfil()
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(User.ObterUsuarioId());
            if (usuario == null)
                return NotFound(new { erro = "Usuário não encontrado." });

            // Nunca devolve SenhaHash, CPF ou documento
            return Ok(new
            {
                id = usuario.Id,
                nome = usuario.Nome,
                email = usuario.Email,
                tipo = usuario.Perfil == TipoPerfil.Montador ? "montador" : "cliente",
                chavePix = usuario.ChavePix
            });
        }

        [HttpPut("me")]
        public async Task<IActionResult> AtualizarPerfil([FromBody] AtualizarPerfilRequest request)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(User.ObterUsuarioId());
            if (usuario == null)
                return NotFound(new { erro = "Usuário não encontrado." });

            try
            {
                usuario.AtualizarNome(request.Nome);

                // A chave Pix é usada para o repasse do valor líquido ao montador
                if (usuario.Perfil == TipoPerfil.Montador && request.ChavePix != null)
                    usuario.DefinirDadosBancarios(request.ChavePix.Trim());

                await _usuarioRepository.AtualizarAsync(usuario);
                return Ok(new { mensagem = "Perfil atualizado com sucesso!" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        // Registra o aparelho do usuário logado para receber notificações push
        [HttpPut("me/push-token")]
        public async Task<IActionResult> RegistrarPushToken([FromBody] PushTokenRequest request)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(User.ObterUsuarioId());
            if (usuario == null)
                return NotFound(new { erro = "Usuário não encontrado." });

            try
            {
                // Se outra conta entrou antes neste mesmo aparelho, ela deixa de receber aqui
                foreach (var outro in await _usuarioRepository.ObterPorPushTokenAsync(request.Token))
                {
                    if (outro.Id == usuario.Id) continue;
                    outro.RemoverPushToken();
                    await _usuarioRepository.AtualizarAsync(outro);
                }

                usuario.RegistrarPushToken(request.Token);
                await _usuarioRepository.AtualizarAsync(usuario);
                return Ok(new { mensagem = "Notificações ativadas." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        // Chamado no logout: o aparelho para de receber notificações dessa conta
        [HttpDelete("me/push-token")]
        public async Task<IActionResult> RemoverPushToken()
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(User.ObterUsuarioId());
            if (usuario == null)
                return NotFound(new { erro = "Usuário não encontrado." });

            usuario.RemoverPushToken();
            await _usuarioRepository.AtualizarAsync(usuario);
            return NoContent();
        }
    }

    public class PushTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }

    // DTO simplificado para receber os dados do React Native
    public class SincronizarUsuarioRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string? Regiao { get; set; }
    }

    public class AtualizarPerfilRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string? ChavePix { get; set; }
    }
}
