using Microsoft.AspNetCore.Mvc;
using MoblyUp.Application.DTOs;
using MoblyUp.Application.UseCases;
using System;
using System.Threading.Tasks;

namespace MoblyUp.API.Controllers
{
    [ApiController]
    [Route("api/pedidos")]
    public class PedidoMontagemController : ControllerBase
    {
        private readonly CriarPedidoMontagemUseCase _criarPedidoUseCase;
        private readonly AceitarPedidoUseCase _aceitarPedidoUseCase;
        private readonly EnviarFotoConclusaoUseCase _enviarFotoUseCase;
        private readonly ConcluirPedidoUseCase _concluirPedidoUseCase;

        public PedidoMontagemController(
            CriarPedidoMontagemUseCase criarPedidoUseCase,
            AceitarPedidoUseCase aceitarPedidoUseCase,
            EnviarFotoConclusaoUseCase enviarFotoUseCase,
            ConcluirPedidoUseCase concluirPedidoUseCase)
        {
            _criarPedidoUseCase = criarPedidoUseCase;
            _aceitarPedidoUseCase = aceitarPedidoUseCase;
            _enviarFotoUseCase = enviarFotoUseCase;
            _concluirPedidoUseCase = concluirPedidoUseCase;
        }

        [HttpPost]
        public async Task<IActionResult> CriarPedido([FromBody] CriarPedidoMontagemRequest request)
        {
            try
            {
                await _criarPedidoUseCase.ExecutarAsync(request);
                return StatusCode(201, new { mensagem = "Pedido criado com sucesso e disponível para montadores!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpPut("{id}/proposta")]
        public async Task<IActionResult> EnviarProposta(Guid id, [FromBody] AceitarPedidoRequest request)
        {
            try
            {
                await _aceitarPedidoUseCase.ExecutarAsync(id, request);
                return Ok(new { mensagem = "Proposta enviada ao cliente com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpPut("{id}/foto-conclusao")]
        public async Task<IActionResult> EnviarFoto(Guid id, [FromBody] EnviarFotoRequest request)
        {
            try
            {
                await _enviarFotoUseCase.ExecutarAsync(id, request.FotoUrl);
                return Ok(new { mensagem = "Foto enviada com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpPut("{id}/concluir")]
        public async Task<IActionResult> ConcluirPedido(Guid id)
        {
            try
            {
                await _concluirPedidoUseCase.ExecutarAsync(id);
                return Ok(new { mensagem = "Pedido concluído com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}