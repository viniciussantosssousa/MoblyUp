using System;

namespace MoblyUp.Application.DTOs
{
    public class AceitarPedidoRequest
    {
        public Guid MontadorId { get; set; }
        public decimal ValorProposto { get; set; }
    }
}