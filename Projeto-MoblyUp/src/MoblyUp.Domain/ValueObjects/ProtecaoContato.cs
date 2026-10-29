using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MoblyUp.Domain.ValueObjects
{
    // Impede que cliente e montador troquem contatos pelo app (telefone, e-mail, links, outros
    // aplicativos) e fechem o serviço por fora. Usado no chat, nas avaliações e na descrição do pedido.
    // Pega os disfarces mais comuns: número com espaços/pontos, escrito por extenso,
    // "arroba ... ponto com" e números picados em várias mensagens seguidas.
    public static class ProtecaoContato
    {
        public const string MensagemBloqueio =
            "Por segurança, não é permitido compartilhar telefone, e-mail, links ou redes sociais. " +
            "Combine tudo pelo chat do MoblyUp: assim o serviço e o pagamento ficam protegidos.";

        // Telefone tem pelo menos 8 dígitos (fixo sem DDD); CEP e preços ficam abaixo disso
        private const int DigitosDeTelefone = 8;

        private static readonly string[] NumerosPorExtenso =
        {
            "zero", "um", "uma", "dois", "duas", "tres", "quatro", "cinco", "seis", "meia", "sete", "oito", "nove",
        };

        private static readonly Regex DigitosSeguidos =
            new(@"\d(?:[\s.\-()/_*+|]*\d){" + (DigitosDeTelefone - 1) + ",}", RegexOptions.Compiled);

        private static readonly Regex Email =
            new(@"[a-z0-9._%+\-]+\s*(@|\(at\)|\barroba\b)\s*[a-z0-9.\-]+\s*(\.|\bponto\b)\s*[a-z]{2,}", RegexOptions.Compiled);

        private static readonly Regex Link =
            new(@"(https?://|www\.|\b[a-z0-9\-]+\.(com|net|org|me|br|io|app|link|ly)\b)", RegexOptions.Compiled);

        // Outros canais de conversa: o objetivo é manter a negociação dentro do app
        private static readonly Regex OutrosCanais =
            new(@"\b(whats\s*app|whats|wats|zap|zapzap|wpp|whatsap|telegram|t\.me|wa\.me|insta|instagram|facebook|face|messenger|signal|tiktok|meu\s+numero|meu\s+telefone|meu\s+cel|me\s+liga|liga\s+pra\s+mim)\b",
                RegexOptions.Compiled);

        private static readonly Regex Cep = new(@"\b\d{5}-\d{3}\b", RegexOptions.Compiled);

        // Usuário de rede social: "@fulano"
        private static readonly Regex Arroba = new(@"(^|\s)@[a-z0-9._]{3,}", RegexOptions.Compiled);

        // Retorna o motivo do bloqueio, ou null se o texto pode ser enviado
        public static string? Verificar(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return null;

            // CEP no formato 00000-000 não é telefone (e o endereço já está no pedido)
            var normalizado = Cep.Replace(Normalizar(texto), " ");

            if (DigitosSeguidos.IsMatch(normalizado)) return "telefone";
            if (ContarNumerosEmSequencia(normalizado) >= DigitosDeTelefone) return "telefone";
            if (Email.IsMatch(normalizado)) return "e-mail";
            if (Link.IsMatch(normalizado)) return "link";
            if (OutrosCanais.IsMatch(normalizado)) return "outro aplicativo";
            if (Arroba.IsMatch(normalizado)) return "rede social";

            return null;
        }

        // Número picado em várias mensagens ("9988" ... "7766" ... "5544"): soma os dígitos das
        // mensagens recentes do mesmo remetente que são praticamente só números
        public static bool CompletaNumeroComAnteriores(string texto, IEnumerable<string> mensagensRecentes)
        {
            if (!EhFragmentoNumerico(texto))
                return false;

            var digitos = ContarDigitos(texto);
            foreach (var anterior in mensagensRecentes)
            {
                if (!EhFragmentoNumerico(anterior))
                    break;
                digitos += ContarDigitos(anterior);
            }

            return digitos >= DigitosDeTelefone;
        }

        // Mensagem curta formada basicamente por números (dígitos ou por extenso)
        private static bool EhFragmentoNumerico(string texto)
        {
            var normalizado = Normalizar(texto);
            var semEspacos = Regex.Replace(normalizado, @"[\s.\-()/_*+|,]", "");
            if (semEspacos.Length == 0)
                return false;

            var digitos = semEspacos.Count(char.IsDigit);
            var porExtenso = ContarNumerosEmSequencia(normalizado);
            return digitos + porExtenso >= 2 && (digitos >= semEspacos.Length * 0.7 || porExtenso >= 2);
        }

        // Dígitos ("98" = 2) + números por extenso ("nove" = 1), sem contar nada duas vezes
        private static int ContarDigitos(string texto)
        {
            var normalizado = Normalizar(texto);
            var porExtenso = Regex.Split(normalizado, @"[\s,.\-()/_*+|]+").Count(p => NumerosPorExtenso.Contains(p));
            return normalizado.Count(char.IsDigit) + porExtenso;
        }

        // Maior sequência de "números" (palavra por extenso = 1 dígito; "98" = 2 dígitos)
        private static int ContarNumerosEmSequencia(string normalizado)
        {
            var maior = 0;
            var atual = 0;
            foreach (var palavra in Regex.Split(normalizado, @"[\s,.\-()/_*+|]+"))
            {
                if (palavra.Length == 0) continue;

                if (NumerosPorExtenso.Contains(palavra))
                    atual += 1;
                else if (palavra.All(char.IsDigit))
                    atual += palavra.Length;
                else if (palavra is "e")
                    continue; // "nove e oito" mantém a sequência
                else
                    atual = 0;

                maior = Math.Max(maior, atual);
            }
            return maior;
        }

        // Minúsculas e sem acentos ("três" -> "tres", "número" -> "numero")
        private static string Normalizar(string texto)
        {
            var decomposto = texto.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(decomposto.Length);
            foreach (var c in decomposto)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            return sb.ToString();
        }
    }
}
