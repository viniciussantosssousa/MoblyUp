using MoblyUp.Application.DTOs;
using MoblyUp.Application.UseCases;
using MoblyUp.Domain.Enums;

namespace MoblyUp.Tests
{
    // Casos de uso: permissões, notificações e o "negar serviço" do montador
    public class UseCaseTests
    {
        private readonly UsuarioRepositoryFake _usuarios = new();
        private readonly PedidoRepositoryFake _pedidos = new();
        private readonly NotificacaoServiceFake _notificacoes = new();

        public UseCaseTests()
        {
            _usuarios.Usuarios.Add(Cenario.Cliente("cliente-1"));
            _usuarios.Usuarios.Add(Cenario.Cliente("cliente-2"));
            _usuarios.Usuarios.Add(Cenario.Montador("montador-1"));
            _usuarios.Usuarios.Add(Cenario.Montador("montador-2"));
            _usuarios.Usuarios.Add(Cenario.Montador("montador-sem-pix", comPix: false));
        }

        private ConsultarPedidosUseCase Consultar() => new(_pedidos, _usuarios);
        private AcoesMontadorUseCase AcoesMontador() => new(_pedidos, _usuarios, _notificacoes);
        private ResponderPropostaUseCase Responder() => new(_pedidos, _notificacoes);
        private AceitarPedidoUseCase Propor() => new(_pedidos, _usuarios, _notificacoes);
        private GerenciarPerfilUseCase Perfil() => new(_usuarios, _pedidos);

        private async Task<Guid> CriarPedidoAsync(string clienteId = "cliente-1")
        {
            await new CriarPedidoMontagemUseCase(_pedidos, _usuarios, _notificacoes).ExecutarAsync(clienteId,
                new CriarPedidoMontagemRequest { TipoMovel = "Guarda-roupa", TamanhoMovel = "6 portas", EnderecoServico = Cenario.Endereco });
            return _pedidos.Pedidos.Last().Id;
        }

        [Fact]
        public async Task Novo_pedido_avisa_montadores_da_regiao()
        {
            await CriarPedidoAsync();
            Assert.Single(_notificacoes.ParaRegiao);
        }

        [Fact]
        public async Task Montador_nao_cria_pedido()
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => CriarPedidoAsync("montador-1"));
        }

        [Fact]
        public async Task Proposta_exige_chave_pix_e_avisa_o_cliente()
        {
            var id = await CriarPedidoAsync();

            await Assert.ThrowsAnyAsync<Exception>(() =>
                Propor().ExecutarAsync(id, "montador-sem-pix", new AceitarPedidoRequest { ValorProposto = 100 }));

            await Propor().ExecutarAsync(id, "montador-1", new AceitarPedidoRequest { ValorProposto = 100 });

            Assert.Contains(_notificacoes.Enviadas, n => n.UsuarioId == "cliente-1");
        }

        [Fact]
        public async Task Montador_que_recusa_nao_ve_mais_o_pedido_mas_os_outros_veem()
        {
            var id = await CriarPedidoAsync();

            await AcoesMontador().RecusarPedidoAsync(id, "montador-1");

            var paraQuemRecusou = await Consultar().ObterDisponiveisAsync("montador-1", "Uberaba - MG");
            var paraOutro = await Consultar().ObterDisponiveisAsync("montador-2", "Uberaba - MG");

            Assert.Empty(paraQuemRecusou);
            Assert.Single(paraOutro);
            Assert.Equal(StatusPedido.Disponivel, _pedidos.Pedidos.Single().Status);
        }

        [Fact]
        public async Task Desistir_reabre_o_pedido_avisa_o_cliente_e_some_do_feed_de_quem_desistiu()
        {
            var id = await CriarPedidoAsync();
            await Propor().ExecutarAsync(id, "montador-1", new AceitarPedidoRequest { ValorProposto = 100 });
            await Responder().AceitarAsync(id, "cliente-1");

            await AcoesMontador().DesistirAsync(id, "montador-1");

            Assert.Equal(StatusPedido.Disponivel, _pedidos.Pedidos.Single().Status);
            Assert.Contains(_notificacoes.Enviadas, n => n.UsuarioId == "cliente-1" && n.Titulo.Contains("desistiu"));
            Assert.Empty(await Consultar().ObterDisponiveisAsync("montador-1", null));
            Assert.Single(await Consultar().ObterDisponiveisAsync("montador-2", null));
        }

        [Fact]
        public async Task Somente_o_dono_responde_a_proposta()
        {
            var id = await CriarPedidoAsync("cliente-1");
            await Propor().ExecutarAsync(id, "montador-1", new AceitarPedidoRequest { ValorProposto = 100 });

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Responder().AceitarAsync(id, "cliente-2"));
        }

        [Fact]
        public async Task Cancelar_avisa_o_montador_da_proposta()
        {
            var id = await CriarPedidoAsync();
            await Propor().ExecutarAsync(id, "montador-1", new AceitarPedidoRequest { ValorProposto = 100 });

            await Responder().CancelarAsync(id, "cliente-1");

            Assert.Equal(StatusPedido.Cancelado, _pedidos.Pedidos.Single().Status);
            Assert.Contains(_notificacoes.Enviadas, n => n.UsuarioId == "montador-1" && n.Titulo == "Pedido cancelado");
        }

        [Fact]
        public async Task Estranho_nao_ve_pedido_de_outro_cliente()
        {
            var id = await CriarPedidoAsync("cliente-1");
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Consultar().ObterPorIdAsync(id, "cliente-2"));
        }

        [Fact]
        public async Task Cliente_nao_ve_o_feed_de_montadores()
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Consultar().ObterDisponiveisAsync("cliente-1", null));
        }

        [Fact]
        public async Task Excluir_conta_e_bloqueado_com_servico_em_andamento()
        {
            var id = await CriarPedidoAsync();
            await Propor().ExecutarAsync(id, "montador-1", new AceitarPedidoRequest { ValorProposto = 100 });

            await Assert.ThrowsAsync<InvalidOperationException>(() => Perfil().ExcluirContaAsync("cliente-1"));
            await Assert.ThrowsAsync<InvalidOperationException>(() => Perfil().ExcluirContaAsync("montador-1"));
        }

        [Fact]
        public async Task Excluir_conta_de_cliente_cancela_pedidos_abertos()
        {
            await CriarPedidoAsync();

            await Perfil().ExcluirContaAsync("cliente-1");

            Assert.Equal(StatusPedido.Cancelado, _pedidos.Pedidos.Single().Status);
            Assert.Null(await Perfil().ObterAsync("cliente-1"));
        }

        [Fact]
        public async Task Perfil_do_montador_soma_o_valor_liquido_recebido()
        {
            var id = await CriarPedidoAsync();
            await Propor().ExecutarAsync(id, "montador-1", new AceitarPedidoRequest { ValorProposto = 200 });
            await Responder().AceitarAsync(id, "cliente-1");
            await Responder().ConfirmarPagamentoAsync(id, "cliente-1");
            var pedido = _pedidos.Pedidos.Single();
            pedido.EnviarFotoConclusao("http://foto.jpg");
            pedido.ConcluirPedido();

            var perfilMontador = await Perfil().ObterAsync("montador-1");
            var perfilCliente = await Perfil().ObterAsync("cliente-1");

            Assert.Equal(1, perfilMontador!.Estatisticas.Concluidos);
            Assert.Equal(170m, perfilMontador.Estatisticas.ValorTotal); // 200 - 15%
            Assert.Equal(200m, perfilCliente!.Estatisticas.ValorTotal);
        }

        [Fact]
        public async Task Push_token_passa_para_a_ultima_conta_logada_no_aparelho()
        {
            const string token = "ExponentPushToken[aparelho-1]";

            await Perfil().RegistrarPushTokenAsync("cliente-1", token);
            await Perfil().RegistrarPushTokenAsync("montador-1", token);

            Assert.Null(_usuarios.Usuarios.Single(u => u.Id == "cliente-1").ExpoPushToken);
            Assert.Equal(token, _usuarios.Usuarios.Single(u => u.Id == "montador-1").ExpoPushToken);
        }

        [Fact]
        public async Task Sincronizar_nao_duplica_usuario()
        {
            var request = new SincronizarUsuarioRequest { Nome = "Novo", Tipo = "cliente", Regiao = "Uberaba - MG" };

            Assert.True(await Perfil().SincronizarAsync("novo-1", "novo@teste.com", request));
            Assert.False(await Perfil().SincronizarAsync("novo-1", "novo@teste.com", request));
            Assert.Single(_usuarios.Usuarios, u => u.Id == "novo-1");
        }
    }
}
