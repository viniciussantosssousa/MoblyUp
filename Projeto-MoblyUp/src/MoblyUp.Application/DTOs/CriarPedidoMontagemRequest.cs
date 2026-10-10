namespace MoblyUp.Application.DTOs
{
    // O cliente não vem no corpo: é o usuário autenticado pelo token do Firebase
    public class CriarPedidoMontagemRequest
    {
        public string TipoMovel { get; set; } = string.Empty;
        public string TamanhoMovel { get; set; } = string.Empty;
        public string? UrlFotoMovelTemporaria { get; set; }
        public string EnderecoServico { get; set; } = string.Empty;
    }
}
