namespace MoblyUp.Domain.ValueObjects
{
    // Medidas aproximadas do móvel em metros (altura x comprimento x largura).
    // Ajudam o montador a estimar o trabalho antes de dar o preço.
    public static class MedidasMovel
    {
        public const decimal Minima = 0.05m;  // 5 cm
        public const decimal Maxima = 10m;    // 10 m

        public static decimal Validar(decimal? valor, string nome)
        {
            if (valor is null || valor <= 0)
                throw new Exception($"Informe a {nome} aproximada do móvel.");

            if (valor < Minima || valor > Maxima)
                throw new Exception($"A {nome} deve estar em metros, entre 0,05 e 10 (ex.: 2,10).");

            return Math.Round(valor.Value, 2);
        }
    }
}
