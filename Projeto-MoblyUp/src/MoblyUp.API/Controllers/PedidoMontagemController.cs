using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoblyUp.API.Extensions;
using MoblyUp.Application.DTOs;
using MoblyUp.Application.UseCases;
using System;
using System.Threading.Tasks;

namespace MoblyUp.API.Controllers
{
    // Todas as rotas exigem o ID token do Firebase. O usuário que age é sempre
    // o dono do token (UsuarioId), nunca um ID enviado no corpo da requisição.
    [Authorize]
    [ApiController]
    [Route("api/pedidos")]
    public class PedidoMontagemController : ControllerBase
    {
        private readonly CriarPedidoMontagemUseCase _criarPedidoUseCase;
        private readonly AceitarPedidoUseCase _aceitarPedidoUseCase;
        private readonly EnviarFotoConclusaoUseCase _enviarFotoUseCase;
        private readonly ConcluirPedidoUseCase _concluirPedidoUseCase;
        private readonly ConsultarPedidosUseCase _consultarPedidosUseCase;
        private readonly ResponderPropostaUseCase _responderPropostaUseCase;

        public PedidoMontagemController(
            CriarPedidoMontagemUseCase criarPedidoUseCase,
            AceitarPedidoUseCase aceitarPedidoUseCase,
            EnviarFotoConclusaoUseCase enviarFotoUseCase,
            ConcluirPedidoUseCase concluirPedidoUseCase,
            ConsultarPedidosUseCase consultarPedidosUseCase,
            ResponderPropostaUseCase responderPropostaUseCase)
        {
            _responderPropostaUseCase = responderPropostaUseCase;
            _criarPedidoUseCase = criarPedidoUseCase;
            _aceitarPedidoUseCase = aceitarPedidoUseCase;
            _enviarFotoUseCase = enviarFotoUseCase;
            _concluirPedidoUseCase = concluirPedidoUseCase;
            _consultarPedidosUseCase = consultarPedidosUseCase;
        }

        private string UsuarioId => User.ObterUsuarioId();

        // GET api/pedidos/disponiveis?regiao=Lagoa Formosa - MG (somente montadores)
        [HttpGet("disponiveis")]
        public Task<IActionResult> ListarDisponiveis([FromQuery] string? regiao) =>
            Consultar(async () => await _consultarPedidosUseCase.ObterDisponiveisAsync(UsuarioId, regiao));

        // Pedidos criados pelo cliente logado
        [HttpGet("cliente")]
        public Task<IActionResult> ListarDoCliente() =>
            Consultar(async () => await _consultarPedidosUseCase.ObterDoClienteAsync(UsuarioId));

        // Pedidos em que o montador logado enviou proposta ou trabalhou
        [HttpGet("montador")]
        public Task<IActionResult> ListarDoMontador() =>
            Consultar(async () => await _consultarPedidosUseCase.ObterDoMontadorAsync(UsuarioId));

        [HttpGet("{id:guid}")]
        public Task<IActionResult> ObterPorId(Guid id) =>
            Consultar(async () => await _consultarPedidosUseCase.ObterPorIdAsync(id, UsuarioId));

        [HttpPost]
        public Task<IActionResult> CriarPedido([FromBody] CriarPedidoMontagemRequest request) =>
            Executar(() => _criarPedidoUseCase.ExecutarAsync(UsuarioId, request),
                "Pedido criado com sucesso e disponível para montadores!", 201);

        // Montador envia a proposta de valor (Disponível -> Aguardando Aprovação)
        [HttpPut("{id}/proposta")]
        public Task<IActionResult> EnviarProposta(Guid id, [FromBody] AceitarPedidoRequest request) =>
            Executar(() => _aceitarPedidoUseCase.ExecutarAsync(id, UsuarioId, request),
                "Proposta enviada ao cliente com sucesso!");

        // Cliente aceita a proposta (Aguardando Aprovação -> Aguardando Pagamento)
        [HttpPut("{id}/aceitar-proposta")]
        public Task<IActionResult> AceitarProposta(Guid id) =>
            Executar(() => _responderPropostaUseCase.AceitarAsync(id, UsuarioId),
                "Proposta aceita! Realize o pagamento para liberar o montador.");

        // Cliente recusa a proposta (volta para Disponível)
        [HttpPut("{id}/recusar-proposta")]
        public Task<IActionResult> RecusarProposta(Guid id) =>
            Executar(() => _responderPropostaUseCase.RecusarAsync(id, UsuarioId),
                "Proposta recusada. O pedido voltou a ficar disponível para os montadores.");

        // Cliente confirma o pagamento (Aguardando Pagamento -> Em Andamento)
        [HttpPut("{id}/confirmar-pagamento")]
        public Task<IActionResult> ConfirmarPagamento(Guid id) =>
            Executar(() => _responderPropostaUseCase.ConfirmarPagamentoAsync(id, UsuarioId),
                "Pagamento confirmado! O montador foi liberado para o serviço.");

        [HttpPut("{id}/foto-conclusao")]
        public Task<IActionResult> EnviarFoto(Guid id, [FromBody] EnviarFotoRequest request) =>
            Executar(() => _enviarFotoUseCase.ExecutarAsync(id, UsuarioId, request.FotoUrl),
                "Foto enviada com sucesso!");

        // Montador conclui o serviço (Em Andamento -> Concluído), exige a foto enviada antes
        [HttpPut("{id}/concluir")]
        public Task<IActionResult> ConcluirPedido(Guid id) =>
            Executar(() => _concluirPedidoUseCase.ExecutarAsync(id, UsuarioId),
                "Pedido concluído com sucesso!");

        private async Task<IActionResult> Consultar(Func<Task<object?>> consulta)
        {
            try
            {
                var resultado = await consulta();
                if (resultado == null)
                    return NotFound(new { erro = "Pedido não encontrado." });

                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { erro = ex.Message });
            }
        }

        private async Task<IActionResult> Executar(Func<Task> acao, string mensagemSucesso, int statusSucesso = 200)
        {
            try
            {
                await acao();
                return StatusCode(statusSucesso, new { mensagem = mensagemSucesso });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}
