namespace MoblyUp.Application.DTOs
{
    public class CriarContaMontadorRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string ChavePix { get; set; } = string.Empty;
        public string DocumentoFotoUrl { get; set; } = string.Empty;
    }
}