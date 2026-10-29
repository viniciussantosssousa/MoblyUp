using MoblyUp.Application.DTOs;
using MoblyUp.Application.UseCases;
using MoblyUp.Application.Utils;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.ValueObjects;

namespace MoblyUp.Tests
{
    // Data do serviço: o cliente pede um dia, o montador concorda ou sugere outro junto com o valor
    public class DataServicoTests
    {
        private static DateOnly Hoje => DataServico.Hoje();

        private static PedidoMontagem PedidoPara(DateOnly data) =>
            new("cliente-1", "Guarda-roupa", "6 portas", null, Cenario.Endereco, data);

        [Fact]
        public void Pedido_guarda_a_data_desejada()
        {
            var pedido = PedidoPara(Hoje.AddDays(5));
            Assert.Equal(Hoje.AddDays(5), pedido.DataDesejada);
            Assert.Null(pedido.DataProposta);
        }

        [Fact]
        public void Pedido_para_hoje_e_aceito()
        {
            Assert.Equal(Hoje, PedidoPara(Hoje).DataDesejada);
        }

        [Fact]
        public void Data_no_passado_ou_distante_demais_e_recusada()
        {
            Assert.ThrowsAny<Exception>(() => PedidoPara(Hoje.AddDays(-1)));
            Assert.ThrowsAny<Exception>(() => PedidoPara(Hoje.AddDays(DataServico.DiasDeAntecedenciaMaxima + 1)));
        }

        [Fact]
        public void Montador_que_concorda_mantem_a_data_do_cliente()
        {
            var pedido = PedidoPara(Hoje.AddDays(5));

            pedido.ReceberProposta("montador-1", 150m);

            Assert.Equal(Hoje.AddDays(5), pedido.DataProposta);
            Assert.False(pedido.PropostaMudaData);
        }

        [Fact]
        public void Montador_pode_sugerir_outra_data()
        {
            var pedido = PedidoPara(Hoje.AddDays(5));

            pedido.ReceberProposta("montador-1", 150m, Hoje.AddDays(7));

            Assert.Equal(Hoje.AddDays(7), pedido.DataProposta);
            Assert.True(pedido.PropostaMudaData);
            Assert.Equal(Hoje.AddDays(5), pedido.DataDesejada);
        }

        [Fact]
        public void Data_sugerida_no_passado_e_recusada()
        {
            var pedido = PedidoPara(Hoje.AddDays(5));
            Assert.ThrowsAny<Exception>(() => pedido.ReceberProposta("montador-1", 150m, Hoje.AddDays(-1)));
            Assert.Null(pedido.MontadorId);
        }

        [Fact]
        public void Recusar_a_proposta_limpa_a_data_sugerida()
        {
            var pedido = PedidoPara(Hoje.AddDays(5));
            pedido.ReceberProposta("montador-1", 150m, Hoje.AddDays(9));

            pedido.RecusarProposta();

            Assert.Null(pedido.DataProposta);
            Assert.Equal(Hoje.AddDays(5), pedido.DataDesejada);
        }

        [Fact]
        public void Datas_sao_formatadas_com_dia_da_semana()
        {
            Assert.Equal("sex, 30/10", Datas.Formatar(new DateOnly(2026, 10, 30)));
            Assert.Equal("dom, 01/11", Datas.Formatar(new DateOnly(2026, 11, 1)));
        }

        [Fact]
        public async Task Notificacoes_informam_a_data_e_avisam_quando_o_montador_sugere_outra()
        {
            var usuarios = new UsuarioRepositoryFake();
            var pedidos = new PedidoRepositoryFake();
            var notificacoes = new NotificacaoServiceFake();
            usuarios.Usuarios.Add(Cenario.Cliente("cliente-1"));
            usuarios.Usuarios.Add(Cenario.Montador("montador-1"));

            var dia = Hoje.AddDays(5);
            await new CriarPedidoMontagemUseCase(pedidos, usuarios, notificacoes).ExecutarAsync("cliente-1",
                new CriarPedidoMontagemRequest { TipoMovel = "Mesa", TamanhoMovel = "4 lugares", EnderecoServico = Cenario.Endereco, DataDesejada = dia });
            var pedido = pedidos.Pedidos.Single();

            await new AceitarPedidoUseCase(pedidos, usuarios, notificacoes)
                .ExecutarAsync(pedido.Id, "montador-1", new AceitarPedidoRequest { ValorProposto = 200, DataProposta = dia.AddDays(2) });

            var resposta = PedidoMontagemResponse.DeEntidade(pedido);
            Assert.True(resposta.PropostaMudaData);
            Assert.Equal(dia.AddDays(2), resposta.DataProposta);
            Assert.Contains(notificacoes.Enviadas, n => n.UsuarioId == "cliente-1");
        }

        [Fact]
        public async Task Cliente_cria_pedido_sem_data_e_e_recusado()
        {
            var usuarios = new UsuarioRepositoryFake();
            var pedidos = new PedidoRepositoryFake();
            usuarios.Usuarios.Add(Cenario.Cliente("cliente-1"));

            // DataDesejada não informada = 01/01/0001 (data no passado)
            await Assert.ThrowsAnyAsync<Exception>(() =>
                new CriarPedidoMontagemUseCase(pedidos, usuarios, new NotificacaoServiceFake()).ExecutarAsync("cliente-1",
                    new CriarPedidoMontagemRequest { TipoMovel = "Mesa", TamanhoMovel = "4", EnderecoServico = Cenario.Endereco }));
            Assert.Empty(pedidos.Pedidos);
        }
    }
}
