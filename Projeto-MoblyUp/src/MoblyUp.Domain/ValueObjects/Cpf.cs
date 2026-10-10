namespace MoblyUp.Domain.ValueObjects
{
    // Validação de CPF pelos dígitos verificadores (algoritmo da Receita Federal)
    public static class Cpf
    {
        public static string SomenteNumeros(string? cpf) =>
            new string((cpf ?? "").Where(char.IsDigit).ToArray());

        public static bool Valido(string? cpf)
        {
            var numeros = SomenteNumeros(cpf);
            if (numeros.Length != 11)
                return false;

            // 000.000.000-00, 111.111.111-11... passam no cálculo mas não são CPFs reais
            if (numeros.Distinct().Count() == 1)
                return false;

            return numeros[9] - '0' == CalcularDigito(numeros, 9)
                && numeros[10] - '0' == CalcularDigito(numeros, 10);
        }

        // Exibe só os dígitos do meio: ***.982.247-**
        public static string? Mascarar(string? cpf)
        {
            var numeros = SomenteNumeros(cpf);
            return numeros.Length == 11 ? $"***.{numeros.Substring(3, 3)}.{numeros.Substring(6, 3)}-**" : null;
        }

        private static int CalcularDigito(string numeros, int tamanho)
        {
            var soma = 0;
            for (var i = 0; i < tamanho; i++)
                soma += (numeros[i] - '0') * (tamanho + 1 - i);

            var resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }
    }
}
