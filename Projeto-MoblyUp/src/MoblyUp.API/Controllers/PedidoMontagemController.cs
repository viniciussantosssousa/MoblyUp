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

        // GET api/pedidos/disponiveis?regiao=Lagoa Formosa - MG
        [HttpGet("disponiveis")]
        public async Task<IActionResult> ListarDisponiveis([FromQuery] string? regiao)
        {
            var pedidos = await _consultarPedidosUseCase.ObterDisponiveisAsync(regiao);
            return Ok(pedidos);
        }

        [HttpGet("cliente/{clienteId}")]
        public async Task<IActionResult> ListarDoCliente(string clienteId)
        {
            var pedidos = await _consultarPedidosUseCase.ObterDoClienteAsync(clienteId);
            return Ok(pedidos);
        }

        [HttpGet("montador/{montadorId}")]
        public async Task<IActionResult> ListarDoMontador(string montadorId)
        {
            var pedidos = await _consultarPedidosUseCase.ObterDoMontadorAsync(montadorId);
            return Ok(pedidos);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> ObterPorId(Guid id)
        {
            var pedido = await _consultarPedidosUseCase.ObterPorIdAsync(id);
            if (pedido == null)
                return NotFound(new { erro = "Pedido não encontrado." });

            return Ok(pedido);
        }

        [HttpPost]
        public Task<IActionResult> CriarPedido([FromBody] CriarPedidoMontagemRequest request) =>
            Executar(() => _criarPedidoUseCase.ExecutarAsync(request),
                "Pedido criado com sucesso e disponível para montadores!", 201);

        // Montador envia a proposta de valor (Disponível -> Aguardando Aprovação)
        [HttpPut("{id}/proposta")]
        public Task<IActionResult> EnviarProposta(Guid id, [FromBody] AceitarPedidoRequest request) =>
            Executar(() => _aceitarPedidoUseCase.ExecutarAsync(id, request),
                "Proposta enviada ao cliente com sucesso!");

        // Cliente aceita a proposta (Aguardando Aprovação -> Aguardando Pagamento)
        [HttpPut("{id}/aceitar-proposta")]
        public Task<IActionResult> AceitarProposta(Guid id, [FromBody] AcaoClienteRequest request) =>
            Executar(() => _responderPropostaUseCase.AceitarAsync(id, request.ClienteId),
                "Proposta aceita! Realize o pagamento para liberar o montador.");

        // Cliente recusa a proposta (volta para Disponível)
        [HttpPut("{id}/recusar-proposta")]
        public Task<IActionResult> RecusarProposta(Guid id, [FromBody] AcaoClienteRequest request) =>
            Executar(() => _responderPropostaUseCase.RecusarAsync(id, request.ClienteId),
                "Proposta recusada. O pedido voltou a ficar disponível para os montadores.");

        // Cliente confirma o pagamento (Aguardando Pagamento -> Em Andamento)
        [HttpPut("{id}/confirmar-pagamento")]
        public Task<IActionResult> ConfirmarPagamento(Guid id, [FromBody] AcaoClienteRequest request) =>
            Executar(() => _responderPropostaUseCase.ConfirmarPagamentoAsync(id, request.ClienteId),
                "Pagamento confirmado! O montador foi liberado para o serviço.");

        [HttpPut("{id}/foto-conclusao")]
        public Task<IActionResult> EnviarFoto(Guid id, [FromBody] EnviarFotoRequest request) =>
            Executar(() => _enviarFotoUseCase.ExecutarAsync(id, request.MontadorId, request.FotoUrl),
                "Foto enviada com sucesso!");

        // Montador conclui o serviço (Em Andamento -> Concluído), exige a foto enviada antes
        [HttpPut("{id}/concluir")]
        public Task<IActionResult> ConcluirPedido(Guid id, [FromBody] AcaoMontadorRequest request) =>
            Executar(() => _concluirPedidoUseCase.ExecutarAsync(id, request.MontadorId),
                "Pedido concluído com sucesso!");

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