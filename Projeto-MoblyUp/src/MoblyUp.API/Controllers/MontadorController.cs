using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MoblyUp.Application.DTOs;
using MoblyUp.Application.UseCases;

namespace MoblyUp.API.Controllers
{
    [ApiController]
    [Route("api/montadores")]
    public class MontadorController : ControllerBase
    {
        private readonly CriarContaMontadorUseCase _criarContaUseCase;

        public MontadorController(CriarContaMontadorUseCase criarContaUseCase)
        {
            _criarContaUseCase = criarContaUseCase;
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] CriarContaMontadorRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { Erro = "Os dados do montador não foram enviados." });

                await _criarContaUseCase.ExecutarAsync(request);

                return StatusCode(201, new { Mensagem = "Conta de montador criada com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Erro = ex.Message });
            }
        }
    }
}