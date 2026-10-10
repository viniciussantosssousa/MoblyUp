using MoblyUp.Application.DTOs;
using MoblyUp.Application.UseCases;
using MoblyUp.Domain.Entities;

namespace MoblyUp.Tests
{
    public class AvaliacaoTests
    {
        private readonly UsuarioRepositoryFake _usuarios = new();
        private readonly PedidoRepositoryFake _pedidos = new();
        private readonly NotificacaoServiceFake _notificacoes = new();

        public AvaliacaoTests()
        {
            _usuarios.Usuarios.Add(Cenario.Cliente("cliente-1"));
            _usuarios.Usuarios.Add(Cenario.Cliente("cliente-2"));
            _usuarios.Usuarios.Add(Cenario.Montador("montador-1"));
        }

        private ResponderPropostaUseCase Responder() => new(_pedidos, _notificacoes);

        // Pedido percorrido até "Concluído" pelo montador-1
        private PedidoMontagem PedidoConcluido(string clienteId = "cliente-1", decimal valor = 100m)
        {
            var pedido = Cenario.Pedido(clienteId);
            pedido.ReceberProposta("montador-1", valor);
            pedido.AceitarProposta();
            pedido.ConfirmarPagamento();
            pedido.EnviarFotoConclusao("http://foto.jpg");
            pedido.ConcluirPedido();
            _pedidos.Pedidos.Add(pedido);
            return pedido;
        }

        [Fact]
        public void Nao_avalia_antes_da_conclusao()
        {
            var pedido = Cenario.Pedido();
            pedido.ReceberProposta("montador-1", 100m);
            Assert.ThrowsAny<Exception>(() => pedido.Avaliar(5, null));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public void Nota_fora_de_1_a_5_e_recusada(int nota)
        {
            Assert.ThrowsAny<Exception>(() => PedidoConcluido().Avaliar(nota, null));
        }

        [Fact]
        public void Servico_so_pode_ser_avaliado_uma_vez()
        {
            var pedido = PedidoConcluido();
            pedido.Avaliar(4, "  Muito bom  ");

            Assert.Equal(4, pedido.AvaliacaoNota);
            Assert.Equal("Muito bom", pedido.AvaliacaoComentario);
            Assert.ThrowsAny<Exception>(() => pedido.Avaliar(5, null));
        }

        [Fact]
        public void Comentario_muito_longo_e_recusado()
        {
            Assert.ThrowsAny<Exception>(() => PedidoConcluido().Avaliar(5, new string('a', 501)));
        }

        [Fact]
        public async Task Somente_o_cliente_do_pedido_avalia_e_o_montador_e_avisado()
        {
            var pedido = PedidoConcluido("cliente-1");

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Responder().AvaliarAsync(pedido.Id, "cliente-2", 1, null));

            await Responder().AvaliarAsync(pedido.Id, "cliente-1", 5, "Excelente");

            Assert.Contains(_notificacoes.Enviadas, n => n.UsuarioId == "montador-1" && n.Titulo.Contains("avaliação"));
        }

        [Fact]
        public async Task Media_do_montador_aparece_na_proposta_e_no_perfil()
        {
            await Responder().AvaliarAsync(PedidoConcluido().Id, "cliente-1", 5, null);
            await Responder().AvaliarAsync(PedidoConcluido().Id, "cliente-1", 4, null);
            await Responder().AvaliarAsync(PedidoConcluido().Id, "cliente-1", 4, null);

            // Nova proposta do mesmo montador: o cliente vê a reputação dele
            var novo = Cenario.Pedido("cliente-2");
            novo.ReceberProposta("montador-1", 150m);
            _pedidos.Pedidos.Add(novo);

            var detalhe = await new ConsultarPedidosUseCase(_pedidos, _usuarios).ObterPorIdAsync(novo.Id, "cliente-2");
            var perfil = await new GerenciarPerfilUseCase(_usuarios, _pedidos).ObterAsync("montador-1");

            Assert.Equal(4.3, detalhe!.MediaAvaliacaoMontador);
            Assert.Equal(3, detalhe.TotalAvaliacoesMontador);
            Assert.Equal(4.3, perfil!.Estatisticas.MediaAvaliacao);
            Assert.Equal(3, perfil.Estatisticas.TotalAvaliacoes);
        }

        [Fact]
        public async Task Montador_sem_avaliacoes_nao_tem_media()
        {
            var novo = Cenario.Pedido("cliente-1");
            novo.ReceberProposta("montador-1", 150m);
            _pedidos.Pedidos.Add(novo);

            var detalhe = await new ConsultarPedidosUseCase(_pedidos, _usuarios).ObterPorIdAsync(novo.Id, "cliente-1");

            Assert.Null(detalhe!.MediaAvaliacaoMontador);
            Assert.Equal(0, detalhe.TotalAvaliacoesMontador);
        }
    }
}
