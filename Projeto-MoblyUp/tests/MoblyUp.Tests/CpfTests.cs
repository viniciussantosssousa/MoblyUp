using MoblyUp.Application.DTOs;
using MoblyUp.Application.UseCases;
using MoblyUp.Domain.ValueObjects;

namespace MoblyUp.Tests
{
    public class CpfTests
    {
        [Theory]
        [InlineData("529.982.247-25")]
        [InlineData("52998224725")]
        [InlineData("111.444.777-35")]
        public void Cpf_valido_e_aceito(string cpf) => Assert.True(Cpf.Valido(cpf));

        [Theory]
        [InlineData("529.982.247-26")] // dígito verificador errado
        [InlineData("123.456.789-00")]
        [InlineData("111.111.111-11")] // todos iguais
        [InlineData("5299822472")]     // 10 dígitos
        [InlineData("")]
        [InlineData(null)]
        public void Cpf_invalido_e_recusado(string? cpf) => Assert.False(Cpf.Valido(cpf));

        [Fact]
        public void Gerador_dos_testes_produz_cpfs_validos()
        {
            foreach (var semente in new[] { "cliente-1", "cliente-2", "montador-1", "montador-2", "x" })
                Assert.True(Cpf.Valido(Cenario.GerarCpf(semente)), semente);
        }

        [Fact]
        public void Cpf_e_mascarado_para_exibicao()
        {
            Assert.Equal("***.982.247-**", Cpf.Mascarar("52998224725"));
        }

        [Fact]
        public void Cpf_e_salvo_so_com_numeros_e_nao_pode_ser_trocado()
        {
            var usuario = Cenario.Cliente(comCpf: false);

            usuario.DefinirCpf("529.982.247-25");

            Assert.Equal("52998224725", usuario.Cpf);
            Assert.Throws<InvalidOperationException>(() => usuario.DefinirCpf("111.444.777-35"));
        }

        [Fact]
        public void Cpf_invalido_nao_e_salvo()
        {
            var usuario = Cenario.Cliente(comCpf: false);
            Assert.Throws<InvalidOperationException>(() => usuario.DefinirCpf("123.456.789-00"));
            Assert.False(usuario.PossuiCpf);
        }
    }

    public class CadastroComCpfTests
    {
        private readonly UsuarioRepositoryFake _usuarios = new();
        private readonly PedidoRepositoryFake _pedidos = new();
        private readonly NotificacaoServiceFake _notificacoes = new();

        private GerenciarPerfilUseCase Perfil() => new(_usuarios, _pedidos);

        private static SincronizarUsuarioRequest Cadastro(string tipo, string? cpf) =>
            new() { Nome = "Novo", Tipo = tipo, Regiao = "Uberaba - MG", Cpf = cpf };

        [Fact]
        public async Task Cadastro_grava_o_cpf()
        {
            await Perfil().SincronizarAsync("u-1", "u1@teste.com", Cadastro("cliente", "529.982.247-25"));

            var perfil = await Perfil().ObterAsync("u-1");
            Assert.True(perfil!.CpfCadastrado);
            Assert.Equal("***.982.247-**", perfil.CpfMascarado);
        }

        [Fact]
        public async Task Cpf_enviado_depois_de_uma_sincronizacao_sem_cpf_ainda_e_gravado()
        {
            // O app pode sincronizar a conta logo após o login no Firebase, antes do cadastro mandar o CPF
            await Perfil().SincronizarAsync("u-1", "u1@teste.com", Cadastro("cliente", null));
            await Perfil().SincronizarAsync("u-1", "u1@teste.com", Cadastro("cliente", "529.982.247-25"));

            Assert.True((await Perfil().ObterAsync("u-1"))!.CpfCadastrado);
        }

        [Fact]
        public async Task Mesmo_cpf_nao_pode_ter_duas_contas()
        {
            await Perfil().SincronizarAsync("u-1", "u1@teste.com", Cadastro("cliente", "529.982.247-25"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                Perfil().SincronizarAsync("u-2", "u2@teste.com", Cadastro("montador", "52998224725")));
            Assert.Null(await Perfil().ObterAsync("u-2"));
        }

        [Fact]
        public async Task Cadastro_com_cpf_invalido_e_recusado()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                Perfil().SincronizarAsync("u-1", "u1@teste.com", Cadastro("cliente", "123.456.789-00")));
        }

        [Fact]
        public async Task Conta_antiga_sem_cpf_nao_cria_pedido_ate_completar_o_cadastro()
        {
            _usuarios.Usuarios.Add(Cenario.Cliente("antigo", comCpf: false));
            var criar = new CriarPedidoMontagemUseCase(_pedidos, _usuarios, _notificacoes);
            var pedido = new CriarPedidoMontagemRequest { TipoMovel = "Mesa", TamanhoMovel = "4 lugares", EnderecoServico = Cenario.Endereco };

            await Assert.ThrowsAnyAsync<Exception>(() => criar.ExecutarAsync("antigo", pedido));
            Assert.False((await Perfil().ObterAsync("antigo"))!.CpfCadastrado);

            await Perfil().CadastrarCpfAsync("antigo", "111.444.777-35");
            await criar.ExecutarAsync("antigo", pedido);

            Assert.Single(_pedidos.Pedidos);
        }

        [Fact]
        public async Task Montador_sem_cpf_nao_envia_proposta()
        {
            _usuarios.Usuarios.Add(Cenario.Cliente("cliente-1"));
            _usuarios.Usuarios.Add(Cenario.Montador("montador-antigo", comCpf: false));
            await new CriarPedidoMontagemUseCase(_pedidos, _usuarios, _notificacoes).ExecutarAsync("cliente-1",
                new CriarPedidoMontagemRequest { TipoMovel = "Mesa", TamanhoMovel = "4 lugares", EnderecoServico = Cenario.Endereco });

            await Assert.ThrowsAnyAsync<Exception>(() =>
                new AceitarPedidoUseCase(_pedidos, _usuarios, _notificacoes)
                    .ExecutarAsync(_pedidos.Pedidos.Single().Id, "montador-antigo", new AceitarPedidoRequest { ValorProposto = 100 }));
        }

        [Fact]
        public async Task Cpf_de_outra_conta_nao_pode_ser_usado_ao_completar_cadastro()
        {
            _usuarios.Usuarios.Add(Cenario.Cliente("com-cpf"));
            _usuarios.Usuarios.Add(Cenario.Cliente("sem-cpf", comCpf: false));
            var cpfEmUso = _usuarios.Usuarios.Single(u => u.Id == "com-cpf").Cpf!;

            await Assert.ThrowsAsync<InvalidOperationException>(() => Perfil().CadastrarCpfAsync("sem-cpf", cpfEmUso));
        }
    }
}
