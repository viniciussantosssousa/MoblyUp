namespace MoblyUp.Application.Utils
{
    public static class Datas
    {
        private static readonly string[] DiasDaSemana = { "dom", "seg", "ter", "qua", "qui", "sex", "sáb" };

        // 2026-10-31 -> "sex, 31/10"
        public static string Formatar(DateOnly data) =>
            $"{DiasDaSemana[(int)data.DayOfWeek]}, {data:dd/MM}";
    }
}
