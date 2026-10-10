namespace MoblyUp.Domain.ValueObjects
{
    // Região no formato "Cidade - UF" (vem do CEP). O endereço do pedido termina com ela:
    // "Rua A, 10, Centro - Uberaba - MG" -> "Uberaba - MG"
    public static class Regiao
    {
        public static string? DeEndereco(string? endereco)
        {
            var partes = (endereco ?? "").Split(" - ", StringSplitOptions.TrimEntries);
            return partes.Length >= 3 ? $"{partes[^2]} - {partes[^1]}" : null;
        }

        // Chave usada para agrupar (sem diferença de maiúsculas/espaços)
        public static string Chave(string regiao) => regiao.Trim().ToLowerInvariant();
    }
}
