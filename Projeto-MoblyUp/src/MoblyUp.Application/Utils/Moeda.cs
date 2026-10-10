using System.Globalization;

namespace MoblyUp.Application.Utils
{
    public static class Moeda
    {
        private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

        // Ex.: 120.5m -> "R$ 120,50"
        public static string Formatar(decimal? valor) => (valor ?? 0).ToString("C", PtBr);
    }
}
