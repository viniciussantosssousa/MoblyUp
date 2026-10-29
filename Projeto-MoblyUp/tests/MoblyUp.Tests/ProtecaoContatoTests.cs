using MoblyUp.Domain.ValueObjects;

namespace MoblyUp.Tests
{
    public class ProtecaoContatoTests
    {
        [Theory]
        // Telefones e disfarces
        [InlineData("meu cel é 34999998888")]
        [InlineData("(34) 99999-8888")]
        [InlineData("3 4 9 9 9 9 8 8 8 8")]
        [InlineData("34.99999.8888")]
        [InlineData("9999-8888")]
        [InlineData("nove nove nove oito sete seis cinco quatro")]
        [InlineData("nove nove 98 sete seis 54")]
        [InlineData("Nove, oito, sete, seis, cinco, quatro, três, dois")]
        // E-mails
        [InlineData("manda pra fulano@gmail.com")]
        [InlineData("fulano arroba gmail ponto com")]
        [InlineData("fulano (at) hotmail.com")]
        // Links
        [InlineData("olha meu portfolio www.montagens.com.br")]
        [InlineData("https://instagram.com/montador")]
        [InlineData("acessa wa.me/5534999998888")]
        // Outros aplicativos e redes sociais
        [InlineData("me chama no zap")]
        [InlineData("vamos falar pelo WhatsApp")]
        [InlineData("me passa seu whats")]
        [InlineData("me segue no insta")]
        [InlineData("me acha no telegram")]
        [InlineData("me segue lá @montador_top")]
        [InlineData("anota meu número")]
        public void Contato_e_bloqueado(string texto)
        {
            Assert.NotNull(ProtecaoContato.Verificar(texto));
        }

        [Theory]
        // Conversa normal de um serviço não pode ser bloqueada
        [InlineData("Bom dia! Posso chegar às 14h amanhã?")]
        [InlineData("Combinado, o guarda-roupa tem 6 portas e 3 gavetas")]
        [InlineData("O valor fica R$ 150,00 como na proposta")]
        [InlineData("Moro no apartamento 1203, bloco B")]
        [InlineData("O CEP é 38025-030")]
        [InlineData("Chego em uma hora e meia")]
        [InlineData("Preciso de 2 ou 3 horas para terminar")]
        [InlineData("Pode deixar, levo as ferramentas")]
        [InlineData("Ficou ótimo, obrigado!")]
        [InlineData("Estou no portão")]
        [InlineData("")]
        [InlineData(null)]
        public void Conversa_normal_e_permitida(string? texto)
        {
            Assert.Null(ProtecaoContato.Verificar(texto));
        }

        [Fact]
        public void Numero_picado_em_varias_mensagens_e_bloqueado()
        {
            // Mais recentes primeiro
            var anteriores = new[] { "9999", "34" };
            Assert.True(ProtecaoContato.CompletaNumeroComAnteriores("8888", anteriores));
        }

        [Fact]
        public void Numero_por_extenso_picado_e_bloqueado()
        {
            var anteriores = new[] { "nove nove", "três quatro" };
            Assert.True(ProtecaoContato.CompletaNumeroComAnteriores("oito oito oito oito", anteriores));
        }

        [Fact]
        public void Numeros_soltos_em_meio_a_conversa_nao_sao_somados()
        {
            // A mensagem anterior é texto normal: a sequência de fragmentos é interrompida
            var anteriores = new[] { "Chego às 14h, tudo bem?", "1203" };
            Assert.False(ProtecaoContato.CompletaNumeroComAnteriores("1500", anteriores));
        }

        [Fact]
        public void Mensagem_com_texto_nao_e_fragmento()
        {
            Assert.False(ProtecaoContato.CompletaNumeroComAnteriores("são 4 portas", new[] { "9999", "8888" }));
        }
    }
}
