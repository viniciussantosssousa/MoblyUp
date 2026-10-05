namespace MoblyUp.Application.DTOs
{
    public class CriarPedidoMontagemRequest
    {
        public string ClienteId { get; set; }
        public string TipoMovel { get; set; } = string.Empty;
        public string TamanhoMovel { get; set; } = string.Empty;
        public string? UrlFotoMovelTemporaria { get; set; }
        public string EnderecoServico { get; set; } = string.Empty;
    }
}