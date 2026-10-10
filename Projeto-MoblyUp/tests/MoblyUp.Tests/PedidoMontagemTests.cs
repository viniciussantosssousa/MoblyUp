using MoblyUp.Domain.Enums;

namespace MoblyUp.Tests
{
    // Máquina de estados do pedido (regras do domínio)
    public class PedidoMontagemTests
    {
        [Fact]
        public void Proposta_calcula_taxa_de_15_por_cento_e_valor_liquido()
        {
            var pedido = Cenario.Pedido();

            pedido.ReceberProposta("montador-1", 120m);

            Assert.Equal(18m, pedido.TaxaPlataforma);
            Assert.Equal(102m, pedido.ValorMontador);
            Assert.Equal(StatusPedido.AguardandoAprovacaoCliente, pedido.Status);
        }

        [Fact]
        public void Proposta_com_valor_zero_e_recusada()
        {
            var pedido = Cenario.Pedido();
            Assert.ThrowsAny<Exception>(() => pedido.ReceberProposta("montador-1", 0m));
        }

        [Fact]
        public void Cliente_nao_pode_propor_no_proprio_pedido()
        {
            var pedido = Cenario.Pedido("cliente-1");
            Assert.ThrowsAny<Exception>(() => pedido.ReceberProposta("cliente-1", 100m));
        }

        [Fact]
        public void Fluxo_completo_ate_concluido()
        {
            var pedido = Cenario.Pedido();

            pedido.ReceberProposta("montador-1", 100m);
            pedido.AceitarProposta();
            pedido.ConfirmarPagamento();
            pedido.EnviarFotoConclusao("http://foto.jpg");
            pedido.ConcluirPedido();

            Assert.Equal(StatusPedido.Concluido, pedido.Status);
            Assert.NotNull(pedido.DataConclusao);
        }

        [Fact]
        public void Nao_conclui_sem_foto()
        {
            var pedido = Cenario.Pedido();
            pedido.ReceberProposta("montador-1", 100m);
            pedido.AceitarProposta();
            pedido.ConfirmarPagamento();

            Assert.ThrowsAny<Exception>(() => pedido.ConcluirPedido());
        }

        [Fact]
        public void Nao_paga_antes_de_aceitar()
        {
            var pedido = Cenario.Pedido();
            pedido.ReceberProposta("montador-1", 100m);

            Assert.ThrowsAny<Exception>(() => pedido.ConfirmarPagamento());
        }

        [Fact]
        public void Recusar_proposta_reabre_o_pedido_e_limpa_os_valores()
        {
            var pedido = Cenario.Pedido();
            pedido.ReceberProposta("montador-1", 100m);

            pedido.RecusarProposta();

            Assert.Equal(StatusPedido.Disponivel, pedido.Status);
            Assert.Null(pedido.MontadorId);
            Assert.Null(pedido.ValorProposto);
        }

        [Theory]
        [InlineData(false)] // desistindo com a proposta pendente
        [InlineData(true)]  // desistindo depois do aceite, antes do pagamento
        public void Montador_pode_desistir_antes_do_pagamento(bool aceita)
        {
            var pedido = Cenario.Pedido();
            pedido.ReceberProposta("montador-1", 100m);
            if (aceita) pedido.AceitarProposta();

            pedido.DesistirProposta("montador-1");

            Assert.Equal(StatusPedido.Disponivel, pedido.Status);
            Assert.Null(pedido.MontadorId);
        }

        [Fact]
        public void Montador_nao_desiste_depois_do_pagamento()
        {
            var pedido = Cenario.Pedido();
            pedido.ReceberProposta("montador-1", 100m);
            pedido.AceitarProposta();
            pedido.ConfirmarPagamento();

            Assert.ThrowsAny<Exception>(() => pedido.DesistirProposta("montador-1"));
            Assert.Equal(StatusPedido.EmAndamento, pedido.Status);
        }

        [Fact]
        public void Outro_montador_nao_desiste_pela_proposta_alheia()
        {
            var pedido = Cenario.Pedido();
            pedido.ReceberProposta("montador-1", 100m);

            Assert.Throws<UnauthorizedAccessException>(() => pedido.DesistirProposta("montador-2"));
        }

        [Fact]
        public void Cliente_cancela_antes_do_pagamento()
        {
            var pedido = Cenario.Pedido();
            pedido.ReceberProposta("montador-1", 100m);

            pedido.Cancelar();

            Assert.Equal(StatusPedido.Cancelado, pedido.Status);
        }

        [Fact]
        public void Pedido_pago_nao_pode_ser_cancelado()
        {
            var pedido = Cenario.Pedido();
            pedido.ReceberProposta("montador-1", 100m);
            pedido.AceitarProposta();
            pedido.ConfirmarPagamento();

            Assert.ThrowsAny<Exception>(() => pedido.Cancelar());
        }

        [Fact]
        public void Pedido_cancelado_nao_recebe_proposta()
        {
            var pedido = Cenario.Pedido();
            pedido.Cancelar();

            Assert.ThrowsAny<Exception>(() => pedido.ReceberProposta("montador-1", 100m));
        }
    }
}
