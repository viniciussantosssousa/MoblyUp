namespace MoblyUp.Application.DTOs
{
    // O montador não vem no corpo: é o usuário autenticado pelo token do Firebase
    public class AceitarPedidoRequest
    {
        public decimal ValorProposto { get; set; }

        // Vazio (ou igual à data do cliente) = concorda com a data pedida; outra data = sugestão
        public DateOnly? DataProposta { get; set; }
    }
}
