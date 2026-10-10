namespace MoblyUp.Application.DTOs
{
    // Identifica quem está agindo sobre o pedido (cliente ou montador)
    public class AcaoClienteRequest
    {
        public string ClienteId { get; set; } = string.Empty;
    }

    public class AcaoMontadorRequest
    {
        public string MontadorId { get; set; } = string.Empty;
    }
}
