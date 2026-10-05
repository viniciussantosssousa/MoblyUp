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
                return Ok(usuarioExistente);

            var perfil = request.Tipo.ToLower() == "montador" ? TipoPerfil.Montador : TipoPerfil.Cliente;

            // Cria a entidade usando o ID gerado lá no Firebase
            var usuario = new Usuario(
                request.Nome,
                "00000000000", // CPF provisório
                request.Email,
                "", // Senha vazia pois o Firebase já cuida da segurança
                "", // Telefone provisório
                perfil,
                request.Id 
            );

            await _usuarioRepository.AdicionarAsync(usuario);
            return Created("", new { mensagem = "Usuário sincronizado com sucesso!", id = usuario.Id });
        }
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