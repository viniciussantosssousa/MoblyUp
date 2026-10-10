using Microsoft.AspNetCore.Mvc;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Interfaces;
using MoblyUp.Domain.Enums;
using System.Threading.Tasks;

namespace MoblyUp.API.Controllers
{
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
            // Verifica se o usuário já existe para não duplicar
            var usuarioExistente = await _usuarioRepository.ObterPorIdAsync(request.Id);
            if (usuarioExistente != null)
                return Ok(new { mensagem = "Usuário já sincronizado.", id = usuarioExistente.Id });

            var perfil = request.Tipo.ToLower() == "montador" ? TipoPerfil.Montador : TipoPerfil.Cliente;

            // Cria a entidade usando o ID gerado lá no Firebase
            var usuario = new Usuario(
                request.Nome,
                null, // CPF ainda não é coletado no cadastro do app
                request.Email,
                "", // Senha vazia pois o Firebase já cuida da segurança
                "", // Telefone provisório
                perfil,
                request.Id 
            );

            await _usuarioRepository.AdicionarAsync(usuario);
            return Created("", new { mensagem = "Usuário sincronizado com sucesso!", id = usuario.Id });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPerfil(string id)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(id);
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

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarPerfil(string id, [FromBody] AtualizarPerfilRequest request)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(id);
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
    }

    public class AtualizarPerfilRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string? ChavePix { get; set; }
    }

    // DTO simplificado para receber os dados do React Native
    public class SincronizarUsuarioRequest
    {
        public string Id { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
    }
}