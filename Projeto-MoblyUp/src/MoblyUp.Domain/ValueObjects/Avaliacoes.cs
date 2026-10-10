namespace MoblyUp.Domain.ValueObjects
{
    // Uma avaliação que o usuário recebeu de alguém com quem fez um serviço
    public record AvaliacaoRecebida(int Nota, string? Comentario, DateTime Data, string NomeAutor, string TipoMovel);

    // Média e quantidade de avaliações (usado nas listagens, calculado em lote no banco)
    public record ResumoReputacao(double Media, int Total);
}
