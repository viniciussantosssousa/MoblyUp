using MoblyUp.Application.DTOs;
using MoblyUp.Application.UseCases;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Enums;

namespace MoblyUp.Tests
{
    public class ChatTests
    {
        private readonly UsuarioRepositoryFake _usuarios = new();
        private readonly PedidoRepositoryFake _pedidos = new();
        private readonly MensagemChatRepositoryFake _mensagens = new();
        private readonly NotificacaoServiceFake _notificacoes = new();
        private readonly TempoRealFake _tempoReal = new();

        public ChatTests()
        {
            _usuarios.Usuarios.Add(Cenario.Cliente("cliente-1"));
            _usuarios.Usuarios.Add(Cenario.Cliente("cliente-2"));
            _usuarios.Usuarios.Add(Cenario.Montador("montador-1"));
            _usuarios.Usuarios.Add(Cenario.Montador("montador-2"));
        }

        private ChatUseCase Chat() => new(_pedidos, _mensagens, _notificacoes, _tempoReal);

        // Pedido entre cliente-1 e montador-1 até a etapa pedida
        private PedidoMontagem Pedido(StatusPedido ate)
        {
            var pedido = Cenario.Pedido("cliente-1");
            _pedidos.Pedidos.Add(pedido);
            if (ate == StatusPedido.Disponivel) return pedido;
            pedido.ReceberProposta("montador-1", 100m);
            if (ate == StatusPedido.AguardandoAprovacaoCliente) return pedido;
            pedido.AceitarProposta();
            if (ate == StatusPedido.AguardandoPagamento) return pedido;
            pedido.ConfirmarPagamento();
            if (ate == StatusPedido.EmAndamento) return pedido;
            pedido.EnviarFotoConclusao("http://foto.jpg");
            pedido.ConcluirPedido();
            return pedido;
        }

        [Fact]
        public async Task Chat_so_abre_depois_que_o_cliente_aceita_a_proposta()
        {
            var pedido = Pedido(StatusPedido.AguardandoAprovacaoCliente);

            await Assert.ThrowsAsync<InvalidOperationException>(() => Chat().EnviarAsync(pedido.Id, "cliente-1", "Olá"));

            pedido.AceitarProposta();
            await Chat().EnviarAsync(pedido.Id, "cliente-1", "Olá, quando pode vir?");

            Assert.Single(_mensagens.Mensagens);
        }

        [Fact]
        public async Task Conversa_dos_dois_lados_chega_em_tempo_real_e_por_push()
        {
            var pedido = Pedido(StatusPedido.EmAndamento);

            await Chat().EnviarAsync(pedido.Id, "cliente-1", "Pode vir às 14h?");
            await Chat().EnviarAsync(pedido.Id, "montador-1", "Combinado, chego às 14h.");

            Assert.Equal(2, _tempoReal.MensagensChat.Count);
            Assert.Contains(_tempoReal.MensagensChat, m => m.Usuarios.Contains("montador-1") && m.Usuarios.Contains("cliente-1"));
            Assert.Contains(_notificacoes.EnviadasComTipo, n => n.UsuarioId == "montador-1" && n.Tipo == "chat");
            Assert.Contains(_notificacoes.EnviadasComTipo, n => n.UsuarioId == "cliente-1" && n.Tipo == "chat");
        }

        [Fact]
        public async Task Apos_concluir_a_conversa_fica_so_para_leitura()
        {
            var pedido = Pedido(StatusPedido.EmAndamento);
            await Chat().EnviarAsync(pedido.Id, "cliente-1", "Obrigado!");
            pedido.EnviarFotoConclusao("http://foto.jpg");
            pedido.ConcluirPedido();

            var conversa = await Chat().ObterConversaAsync(pedido.Id, "montador-1");

            Assert.False(conversa.Aberto);
            Assert.Single(conversa.Mensagens);
            await Assert.ThrowsAsync<InvalidOperationException>(() => Chat().EnviarAsync(pedido.Id, "montador-1", "De nada"));
        }

        [Fact]
        public async Task Quem_nao_participa_do_servico_nao_acessa_a_conversa()
        {
            var pedido = Pedido(StatusPedido.EmAndamento);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Chat().ObterConversaAsync(pedido.Id, "cliente-2"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Chat().EnviarAsync(pedido.Id, "montador-2", "Oi"));
        }

        [Theory]
        [InlineData("me chama no zap 34 99999-8888")]
        [InlineData("meu email é fulano@gmail.com")]
        [InlineData("me segue no insta")]
        [InlineData("nove nove nove oito sete seis cinco quatro")]
        public async Task Mensagem_com_contato_e_recusada_e_nao_e_entregue(string texto)
        {
            var pedido = Pedido(StatusPedido.EmAndamento);

            var erro = await Assert.ThrowsAsync<InvalidOperationException>(() => Chat().EnviarAsync(pedido.Id, "montador-1", texto));

            Assert.Contains("não é permitido compartilhar", erro.Message);
            Assert.Empty(_mensagens.Mensagens);
            Assert.Empty(_tempoReal.MensagensChat);
            Assert.Empty(_notificacoes.Enviadas);
        }

        [Fact]
        public async Task Numero_picado_em_mensagens_seguidas_e_recusado()
        {
            var pedido = Pedido(StatusPedido.EmAndamento);

            await Chat().EnviarAsync(pedido.Id, "montador-1", "34");
            await Chat().EnviarAsync(pedido.Id, "montador-1", "9999");

            await Assert.ThrowsAsync<InvalidOperationException>(() => Chat().EnviarAsync(pedido.Id, "montador-1", "8888"));
            Assert.Equal(2, _mensagens.Mensagens.Count);
        }

        [Fact]
        public async Task Abrir_a_conversa_marca_como_lidas_e_avisa_quem_enviou()
        {
            var pedido = Pedido(StatusPedido.EmAndamento);
            await Chat().EnviarAsync(pedido.Id, "cliente-1", "Oi!");
            await Chat().EnviarAsync(pedido.Id, "cliente-1", "Tudo certo para amanhã?");

            var naoLidasAntes = await _mensagens.ContarNaoLidasAsync("montador-1", new[] { pedido.Id });
            await Chat().ObterConversaAsync(pedido.Id, "montador-1");
            var naoLidasDepois = await _mensagens.ContarNaoLidasAsync("montador-1", new[] { pedido.Id });

            Assert.Equal(2, naoLidasAntes[pedido.Id]);
            Assert.Empty(naoLidasDepois);
            Assert.Contains(_tempoReal.Leituras, l => l.UsuarioId == "cliente-1" && l.PedidoId == pedido.Id);
        }

        [Fact]
        public async Task Lista_de_pedidos_mostra_mensagens_nao_lidas()
        {
            var pedido = Pedido(StatusPedido.EmAndamento);
            await Chat().EnviarAsync(pedido.Id, "montador-1", "Estou a caminho");

            var lista = await new ConsultarPedidosUseCase(_pedidos, _usuarios, _mensagens).ObterDoClienteAsync("cliente-1");

            var item = lista.Single();
            Assert.True(item.ChatAberto);
            Assert.Equal(1, item.MensagensNaoLidas);
        }

        [Fact]
        public async Task Mensagem_vazia_ou_longa_demais_e_recusada()
        {
            var pedido = Pedido(StatusPedido.EmAndamento);
            await Assert.ThrowsAsync<InvalidOperationException>(() => Chat().EnviarAsync(pedido.Id, "cliente-1", "   "));
            await Assert.ThrowsAsync<InvalidOperationException>(() => Chat().EnviarAsync(pedido.Id, "cliente-1", new string('a', 1001)));
        }

        [Fact]
        public void Comentario_de_avaliacao_com_contato_e_recusado()
        {
            var pedido = Pedido(StatusPedido.Concluido);
            Assert.ThrowsAny<Exception>(() => pedido.AvaliarMontador(5, "Ótimo! Chama ele no zap"));
            Assert.Null(pedido.NotaMontador);
        }

        [Fact]
        public async Task Descricao_do_pedido_com_telefone_e_recusada()
        {
            var criar = new CriarPedidoMontagemUseCase(_pedidos, _usuarios, _notificacoes);
            var request = new CriarPedidoMontagemRequest
            {
                TipoMovel = "Guarda-roupa",
                TamanhoMovel = "6 portas - ligar 34999998888",
                EnderecoServico = Cenario.Endereco,
            };

            await Assert.ThrowsAnyAsync<Exception>(() => criar.ExecutarAsync("cliente-1", request));
            Assert.Empty(_pedidos.Pedidos);
        }
    }
}
