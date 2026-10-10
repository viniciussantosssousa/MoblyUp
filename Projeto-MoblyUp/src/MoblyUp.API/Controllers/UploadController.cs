using Microsoft.AspNetCore.Mvc;

namespace MoblyUp.API.Controllers
{
    // Recebe as fotos tiradas no app (móvel do cliente e comprovação do montador)
    // e devolve a URL pública para ser gravada no pedido.
    [ApiController]
    [Route("api/uploads")]
    public class UploadController : ControllerBase
    {
        private const long TamanhoMaximo = 10 * 1024 * 1024; // 10 MB

        private static readonly Dictionary<string, string> ExtensoesPermitidas = new()
        {
            ["image/jpeg"] = ".jpg",
            ["image/jpg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp",
        };

        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        [RequestSizeLimit(TamanhoMaximo)]
        public async Task<IActionResult> EnviarImagem(IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
                return BadRequest(new { erro = "Nenhuma imagem foi enviada." });

            if (!ExtensoesPermitidas.TryGetValue(arquivo.ContentType.ToLower(), out var extensao))
                return BadRequest(new { erro = "Formato não suportado. Envie uma imagem JPG, PNG ou WEBP." });

            var pasta = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");
            Directory.CreateDirectory(pasta);

            var nomeArquivo = $"{Guid.NewGuid()}{extensao}";
            await using (var destino = System.IO.File.Create(Path.Combine(pasta, nomeArquivo)))
            {
                await arquivo.CopyToAsync(destino);
            }

            // Usa o mesmo host que o app chamou, assim a URL funciona no celular
            var url = $"{Request.Scheme}://{Request.Host}/uploads/{nomeArquivo}";
            return StatusCode(201, new { url });
        }
    }
}
