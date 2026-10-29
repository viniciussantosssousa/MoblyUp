namespace MoblyUp.Application.DTOs
{
    // O cliente não vem no corpo: é o usuário autenticado pelo token do Firebase
    public class CriarPedidoMontagemRequest
    {
        public string TipoMovel { get; set; } = string.Empty;
        public string TamanhoMovel { get; set; } = string.Empty;
        public string? UrlFotoMovelTemporaria { get; set; }
        public string EnderecoServico { get; set; } = string.Empty;
        // Dia em que o cliente quer o serviço ("2026-11-05"); o horário é combinado pelo chat
        public DateOnly DataDesejada { get; set; }
        // Medidas aproximadas em metros (obrigatórias)
        public decimal? AlturaM { get; set; }
        public decimal? ComprimentoM { get; set; }
        public decimal? LarguraM { get; set; }
    }
}
