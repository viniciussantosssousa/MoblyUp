namespace MoblyUp.Domain.ValueObjects
{
    // Data em que o serviço vai acontecer (só o dia; o horário é combinado pelo chat).
    // "Hoje" é sempre o dia no horário de Brasília, independente do fuso do servidor.
    public static class DataServico
    {
        public const int DiasDeAntecedenciaMaxima = 90;

        private static readonly TimeZoneInfo FusoBrasilia = ObterFusoBrasilia();

        public static DateOnly Hoje() =>
            DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, FusoBrasilia));

        // Não pode ser no passado nem muito distante (o pedido ficaria esquecido no feed)
        public static void Validar(DateOnly data, string descricao)
        {
            var hoje = Hoje();
            if (data < hoje)
                throw new Exception($"A {descricao} não pode ser no passado.");
            if (data > hoje.AddDays(DiasDeAntecedenciaMaxima))
                throw new Exception($"A {descricao} pode ser no máximo daqui a {DiasDeAntecedenciaMaxima} dias.");
        }

        private static TimeZoneInfo ObterFusoBrasilia()
        {
            // "America/Sao_Paulo" (Linux/IANA) e "E. South America Standard Time" (Windows)
            foreach (var id in new[] { "America/Sao_Paulo", "E. South America Standard Time" })
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
                catch (TimeZoneNotFoundException) { }
            }
            return TimeZoneInfo.CreateCustomTimeZone("Brasilia", TimeSpan.FromHours(-3), "Brasília", "Brasília");
        }
    }
}
