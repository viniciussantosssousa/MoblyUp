namespace MoblyUp.Domain.Entities
{
    // Mensagem do chat entre cliente e montador de um pedido
    public class MensagemChat
    {
        public const int TamanhoMaximo = 1000;

        public Guid Id { get; private set; }
        public Guid PedidoId { get; private set; }
        public string RemetenteId { get; private set; }
        public string Texto { get; private set; }
        public DateTime DataEnvio { get; private set; }
        public DateTime? DataLeitura { get; private set; }

        protected MensagemChat()
        {
            RemetenteId = string.Empty;
            Texto = string.Empty;
        }

        public MensagemChat(Guid pedidoId, string remetenteId, string texto)
        {
            var conteudo = texto?.Trim() ?? "";
            if (conteudo.Length == 0)
                throw new InvalidOperationException("A mensagem está vazia.");
            if (conteudo.Length > TamanhoMaximo)
                throw new InvalidOperationException($"A mensagem pode ter no máximo {TamanhoMaximo} caracteres.");

            Id = Guid.NewGuid();
            PedidoId = pedidoId;
            RemetenteId = remetenteId;
            Texto = conteudo;
            DataEnvio = DateTime.UtcNow;
        }

        public void MarcarComoLida()
        {
            DataLeitura ??= DateTime.UtcNow;
        }
    }
}
