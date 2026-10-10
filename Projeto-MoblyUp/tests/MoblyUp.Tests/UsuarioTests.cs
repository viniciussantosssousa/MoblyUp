using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Enums;

namespace MoblyUp.Tests
{
    public class UsuarioTests
    {
        [Fact]
        public void Cliente_nao_cadastra_chave_pix()
        {
            var cliente = Cenario.Cliente();
            Assert.Throws<InvalidOperationException>(() => cliente.DefinirDadosBancarios("abc@pix.com"));
        }

        [Fact]
        public void Chave_pix_vazia_e_recusada()
        {
            var montador = Cenario.Montador(comPix: false);
            Assert.Throws<InvalidOperationException>(() => montador.DefinirDadosBancarios("   "));
        }

        [Theory]
        [InlineData("(34) 99999-8888", "34999998888")]
        [InlineData("34 3333-4444", "3433334444")]
        [InlineData("", "")]
        public void Telefone_guarda_somente_numeros(string digitado, string esperado)
        {
            var usuario = Cenario.Cliente();
            usuario.AtualizarTelefone(digitado);
            Assert.Equal(esperado, usuario.Telefone);
        }

        [Theory]
        [InlineData("123")]
        [InlineData("349999988887777")]
        public void Telefone_invalido_e_recusado(string digitado)
        {
            var usuario = Cenario.Cliente();
            Assert.Throws<InvalidOperationException>(() => usuario.AtualizarTelefone(digitado));
        }

        [Fact]
        public void Push_token_precisa_ser_do_expo()
        {
            var usuario = Cenario.Cliente();
            Assert.Throws<InvalidOperationException>(() => usuario.RegistrarPushToken("token-qualquer"));

            usuario.RegistrarPushToken("ExponentPushToken[abc123]");
            Assert.Equal("ExponentPushToken[abc123]", usuario.ExpoPushToken);
        }

        [Fact]
        public void Excluir_conta_apaga_dados_pessoais_e_libera_o_email()
        {
            var montador = new Usuario("Fulano", "12345678901", "fulano@teste.com", "", "34999998888", TipoPerfil.Montador, "m-1");
            montador.DefinirDadosBancarios("fulano@pix.com");
            montador.RegistrarPushToken("ExponentPushToken[abc]");

            montador.DesativarConta();

            Assert.False(montador.Ativo);
            Assert.NotEqual("fulano@teste.com", montador.Email);
            Assert.Null(montador.Cpf);
            Assert.Null(montador.ChavePix);
            Assert.Null(montador.ExpoPushToken);
            Assert.Equal("", montador.Telefone);
        }
    }
}
