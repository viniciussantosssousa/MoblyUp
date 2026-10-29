using System.Text.Json;
using MoblyUp.Application.UseCases;
using MoblyUp.Domain.Entities;

namespace MoblyUp.Tests
{
    // Quem pode ver o perfil de quem; dados de contato nunca aparecem
    public class PerfilPublicoTests
    {
        private readonly UsuarioRepositoryFake _usuarios = new();
        private readonly PedidoRepositoryFake _pedidos = new();

        public PerfilPublicoTests()
        {
            var cliente = Cenario.Cliente("cliente-1");
            cliente.AtualizarTelefone("34999998888");
            var montador = Cenario.Montador("montador-1");
            montador.AtualizarTelefone("34988887777");

            _usuarios.Usuarios.Add(cliente);
            _usuarios.Usuarios.Add(Cenario.Cliente("cliente-2"));
            _usuarios.Usuarios.Add(montador);
            _usuarios.Usuarios.Add(Cenario.Montador("montador-2"));
        }

        private PerfilPublicoUseCase PerfilPublico() => new(_usuarios, _pedidos);

        private PedidoMontagem Pedido(string clienteId = "cliente-1")
        {
            var pedido = Cenario.Pedido(clienteId);
            _pedidos.Pedidos.Add(pedido);
            return pedido;
        }

        [Fact]
        public async Task Desconhecidos_nao_veem_o_perfil_um_do_outro()
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => PerfilPublico().ObterAsync("montador-1", "cliente-2"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => PerfilPublico().ObterAsync("cliente-2", "cliente-1"));
        }

        [Fact]
        public async Task Montador_ve_o_cliente_que_tem_pedido_aberto_no_feed()
        {
            Pedido("cliente-1");

            var perfil = await PerfilPublico().ObterAsync("cliente-1", "montador-2");

            Assert.Equal("cliente", perfil.Tipo);
        }

        [Fact]
        public async Task Cliente_ve_o_montador_que_enviou_proposta()
        {
            Pedido("cliente-1").ReceberProposta("montador-1", 100m);

            var perfil = await PerfilPublico().ObterAsync("montador-1", "cliente-1");

            Assert.Equal("montador", perfil.Tipo);
        }

        [Fact]
        public async Task Telefone_e_documentos_nunca_aparecem_nem_com_servico_em_andamento()
        {
            var pedido = Pedido("cliente-1");
            pedido.ReceberProposta("montador-1", 100m);
            pedido.AceitarProposta();
            pedido.ConfirmarPagamento();

            var json = JsonSerializer.Serialize(await PerfilPublico().ObterAsync("montador-1", "cliente-1"));

            Assert.DoesNotContain("34988887777", json);
            Assert.DoesNotContain("elefone", json);
            Assert.DoesNotContain("@teste.com", json);
            Assert.DoesNotContain("pix", json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("cpf", json, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Perfil_publico_traz_reputacao_e_servicos_concluidos()
        {
            var pedido = Pedido("cliente-1");
            pedido.ReceberProposta("montador-1", 100m);
            pedido.AceitarProposta();
            pedido.ConfirmarPagamento();
            pedido.EnviarFotoConclusao("http://foto.jpg");
            pedido.ConcluirPedido();
            pedido.AvaliarMontador(5, "Excelente trabalho");

            var perfil = await PerfilPublico().ObterAsync("montador-1", "cliente-1");

            Assert.Equal(1, perfil.ServicosConcluidos);
            Assert.Equal(5, perfil.Reputacao.Media);
            Assert.Equal("Excelente trabalho", perfil.Reputacao.Avaliacoes.Single().Comentario);
        }

        [Fact]
        public async Task Usuario_ve_o_proprio_perfil_publico()
        {
            var perfil = await PerfilPublico().ObterAsync("cliente-2", "cliente-2");
            Assert.Equal("cliente-2", perfil.Id);
        }

        [Fact]
        public async Task Conta_excluida_nao_tem_perfil()
        {
            var conta = _usuarios.Usuarios.Single(u => u.Id == "cliente-2");
            conta.DesativarConta();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => PerfilPublico().ObterAsync("cliente-2", "montador-1"));
        }
    }
}
