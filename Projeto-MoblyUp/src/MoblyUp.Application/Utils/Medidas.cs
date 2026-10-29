using System.Globalization;
using MoblyUp.Domain.Entities;

namespace MoblyUp.Application.Utils
{
    public static class Medidas
    {
        private static readonly CultureInfo PtBr = new("pt-BR");

        // "A 2,10 x C 1,80 x L 0,55 m" (ou "medidas não informadas" em pedidos antigos)
        public static string Formatar(PedidoMontagem pedido) =>
            pedido.AlturaM is null || pedido.ComprimentoM is null || pedido.LarguraM is null
                ? "medidas não informadas"
                : string.Format(PtBr, "A {0:0.00} x C {1:0.00} x L {2:0.00} m", pedido.AlturaM, pedido.ComprimentoM, pedido.LarguraM);
    }
}
