using MoblyUp.Application.DTOs;
using MoblyUp.Application.UseCases;
using MoblyUp.Application.Utils;
using MoblyUp.Domain.Entities;
using Xunit;

namespace MoblyUp.Tests
{
    public class MedidasTests
    {
        private readonly UsuarioRepositoryFake _usuarios = new();
        private readonly PedidoRepositoryFake _pedidos = new();
        private readonly NotificacaoServiceFake _notificacoes = new();

        public MedidasTests()
        {
            _usuarios.Usuarios.Add(Cenario.Cliente("cliente-1"));
        }

        private Task Criar(decimal? altura, decimal? comprimento, decimal? largura) =>
            new CriarPedidoMontagemUseCase(_pedidos, _usuarios, _notificacoes).ExecutarAsync("cliente-1",
                new CriarPedidoMontagemRequest
                {
                    TipoMovel = "Guarda-roupa", TamanhoMovel = "6 portas", EnderecoServico = Cenario.Endereco,
                    DataDesejada = Cenario.Data, AlturaM = altura, ComprimentoM = comprimento, LarguraM = largura,
                });

        [Fact]
        public async Task Pedido_guarda_as_medidas_arredondadas()
        {
            await Criar(2.104m, 1.8m, 0.55m);

            var pedido = _pedidos.Pedidos.Single();
            Assert.Equal(2.10m, pedido.AlturaM);
            Assert.Equal(1.80m, pedido.ComprimentoM);
            Assert.Equal(0.55m, pedido.LarguraM);
            Assert.Equal("A 2,10 x C 1,80 x L 0,55 m", Medidas.Formatar(pedido));
        }

        [Theory]
        [InlineData(null, 1.8, 0.55)]
        [InlineData(2.1, null, 0.55)]
        [InlineData(2.1, 1.8, null)]
        [InlineData(0.0, 1.8, 0.55)]
        public async Task Medida_faltando_e_recusada(double? altura, double? comprimento, double? largura)
        {
            var ex = await Assert.ThrowsAnyAsync<Exception>(() =>
                Criar((decimal?)altura, (decimal?)comprimento, (decimal?)largura));

            Assert.Contains("Informe", ex.Message);
            Assert.Empty(_pedidos.Pedidos);
        }

        [Theory]
        [InlineData(210)]   // digitou em centímetros
        [InlineData(0.01)]
        public async Task Medida_fora_da_faixa_em_metros_e_recusada(double altura)
        {
            var ex = await Assert.ThrowsAnyAsync<Exception>(() => Criar((decimal)altura, 1.8m, 0.55m));

            Assert.Contains("metros", ex.Message);
            Assert.Empty(_pedidos.Pedidos);
        }
    }
}
