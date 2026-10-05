using System;

namespace MoblyUp.Application.DTOs
{
    public class AceitarPedidoRequest
    {
        public string MontadorId { get; set; }
        public decimal ValorProposto { get; set; }
    }
}