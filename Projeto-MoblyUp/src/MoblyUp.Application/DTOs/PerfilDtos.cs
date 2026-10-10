namespace MoblyUp.Application.DTOs
{
    public class SincronizarUsuarioRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string? Regiao { get; set; }
        public string? Cpf { get; set; }
    }

    public class CadastrarCpfRequest
    {
        public string Cpf { get; set; } = string.Empty;
    }

    // Campos nulos não são alterados
    public class AtualizarPerfilRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public string? Regiao { get; set; }
        public string? ChavePix { get; set; }
        public string? FotoPerfilUrl { get; set; }
    }

    public class PushTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }

    public class PerfilResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        // O CPF completo nunca sai da API: o app só precisa saber se existe e exibir mascarado
        public bool CpfCadastrado { get; set; }
        public string? CpfMascarado { get; set; }
        public string? Telefone { get; set; }
        public string? Regiao { get; set; }
        public string? ChavePix { get; set; }
        public string? FotoPerfilUrl { get; set; }
        public DateTime DataCadastro { get; set; }
        public EstatisticasPerfil Estatisticas { get; set; } = new();
    }

    public class EstatisticasPerfil
    {
        public int Total { get; set; }        // cliente: pedidos feitos | montador: serviços em que atuou
        public int EmAndamento { get; set; }
        public int Concluidos { get; set; }
        public decimal ValorTotal { get; set; } // cliente: total pago | montador: total recebido (líquido)
    }
}
