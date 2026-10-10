namespace MoblyUp.Application.DTOs
{
    // O montador não vem no corpo: é o usuário autenticado pelo token do Firebase
    public class AceitarPedidoRequest
    {
        public decimal ValorProposto { get; set; }
    }
}
