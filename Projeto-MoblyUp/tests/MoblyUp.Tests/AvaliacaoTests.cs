using MoblyUp.Application.DTOs;
using MoblyUp.Application.UseCases;
using MoblyUp.Domain.Entities;

namespace MoblyUp.Tests
{
    // Avaliação mútua: o cliente avalia o montador e o montador avalia o cliente
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
            _usuarios.Usuarios.Add(Cenario.Montador("montador-2"));
        }

        private ResponderPropostaUseCase Responder() => new(_pedidos, _notificacoes);
        private AcoesMontadorUseCase AcoesMontador() => new(_pedidos, _usuarios, _notificacoes);

        // Pedido percorrido até "Concluído"
        private PedidoMontagem PedidoConcluido(string clienteId = "cliente-1", string montadorId = "montador-1", decimal valor = 100m)
        {
            var pedido = Cenario.Pedido(clienteId);
            pedido.ReceberProposta(montadorId, valor);
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
            Assert.ThrowsAny<Exception>(() => pedido.AvaliarMontador(5, null));
            Assert.ThrowsAny<Exception>(() => pedido.AvaliarCliente(5, null));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public void Nota_fora_de_1_a_5_e_recusada(int nota)
        {
            Assert.ThrowsAny<Exception>(() => PedidoConcluido().AvaliarMontador(nota, null));
            Assert.ThrowsAny<Exception>(() => PedidoConcluido().AvaliarCliente(nota, null));
        }

        [Fact]
        public void Cada_lado_avalia_uma_vez_e_de_forma_independente()
        {
            var pedido = PedidoConcluido();

            pedido.AvaliarMontador(4, "  Muito bom  ");
            pedido.AvaliarCliente(5, "Cliente atencioso");

            Assert.Equal(4, pedido.NotaMontador);
            Assert.Equal("Muito bom", pedido.ComentarioMontador);
            Assert.Equal(5, pedido.NotaCliente);
            Assert.ThrowsAny<Exception>(() => pedido.AvaliarMontador(5, null));
            Assert.ThrowsAny<Exception>(() => pedido.AvaliarCliente(1, null));
        }

        [Fact]
        public void Comentario_muito_longo_e_recusado()
        {
            Assert.ThrowsAny<Exception>(() => PedidoConcluido().AvaliarCliente(5, new string('a', 501)));
        }

        [Fact]
        public async Task Somente_o_cliente_do_pedido_avalia_o_montador_e_ele_e_avisado()
        {
            var pedido = PedidoConcluido("cliente-1");

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Responder().AvaliarAsync(pedido.Id, "cliente-2", 1, null));
            await Responder().AvaliarAsync(pedido.Id, "cliente-1", 5, "Excelente");

            Assert.Contains(_notificacoes.Enviadas, n => n.UsuarioId == "montador-1" && n.Titulo.Contains("avaliação"));
        }

        [Fact]
        public async Task Somente_o_montador_do_servico_avalia_o_cliente_e_ele_e_avisado()
        {
            var pedido = PedidoConcluido("cliente-1", "montador-1");

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                AcoesMontador().AvaliarClienteAsync(pedido.Id, "montador-2", 1, null));
            await AcoesMontador().AvaliarClienteAsync(pedido.Id, "montador-1", 4, "Tudo certo");

            Assert.Equal(4, pedido.NotaCliente);
            Assert.Contains(_notificacoes.Enviadas, n => n.UsuarioId == "cliente-1" && n.Titulo.Contains("avaliação"));
        }

        [Fact]
        public async Task Reputacao_das_duas_partes_aparece_nos_pedidos()
        {
            var p1 = PedidoConcluido("cliente-1", "montador-1");
            var p2 = PedidoConcluido("cliente-1", "montador-1");
            await Responder().AvaliarAsync(p1.Id, "cliente-1", 5, null);
            await Responder().AvaliarAsync(p2.Id, "cliente-1", 4, null);
            await AcoesMontador().AvaliarClienteAsync(p1.Id, "montador-1", 3, null);

            // Novo pedido do mesmo cliente, aberto no feed: o montador vê a reputação do cliente
            var novo = Cenario.Pedido("cliente-1");
            _pedidos.Pedidos.Add(novo);
            var feed = await new ConsultarPedidosUseCase(_pedidos, _usuarios, new MensagemChatRepositoryFake()).ObterDisponiveisAsync("montador-2", null);
            var noFeed = feed.Single(p => p.Id == novo.Id);
            Assert.Equal(3, noFeed.MediaAvaliacaoCliente);
            Assert.Equal(1, noFeed.TotalAvaliacoesCliente);

            // Proposta do montador-1: o cliente vê a reputação do montador
            novo.ReceberProposta("montador-1", 150m);
            var detalhe = await new ConsultarPedidosUseCase(_pedidos, _usuarios, new MensagemChatRepositoryFake()).ObterPorIdAsync(novo.Id, "cliente-1");
            Assert.Equal(4.5, detalhe!.MediaAvaliacaoMontador);
            Assert.Equal(2, detalhe.TotalAvaliacoesMontador);
        }

        [Fact]
        public async Task Perfil_proprio_mostra_media_distribuicao_e_comentarios()
        {
            await Responder().AvaliarAsync(PedidoConcluido().Id, "cliente-1", 5, "Ótimo");
            await Responder().AvaliarAsync(PedidoConcluido().Id, "cliente-1", 4, null);
            await Responder().AvaliarAsync(PedidoConcluido().Id, "cliente-1", 4, "Bom");

            var perfil = await new GerenciarPerfilUseCase(_usuarios, _pedidos).ObterAsync("montador-1");

            Assert.Equal(4.3, perfil!.Reputacao.Media);
            Assert.Equal(3, perfil.Reputacao.Total);
            Assert.Equal(new[] { 0, 0, 0, 2, 1 }, perfil.Reputacao.Distribuicao);
            Assert.Equal(3, perfil.Reputacao.Avaliacoes.Count);
            Assert.Equal(4.3, perfil.Estatisticas.MediaAvaliacao);
        }

        [Fact]
        public async Task Usuario_sem_avaliacoes_nao_tem_media()
        {
            var perfil = await new GerenciarPerfilUseCase(_usuarios, _pedidos).ObterAsync("cliente-2");

            Assert.Null(perfil!.Reputacao.Media);
            Assert.Equal(0, perfil.Reputacao.Total);
            Assert.Empty(perfil.Reputacao.Avaliacoes);
        }
    }
}
