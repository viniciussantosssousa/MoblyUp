using System;
using System.Collections.Generic;
using System.Linq;
using MoblyUp.Domain.ValueObjects;

namespace MoblyUp.Application.DTOs
{
    public class ReputacaoResponse
    {
        public double? Media { get; set; }
        public int Total { get; set; }

        // Quantidade de avaliações por nota: índice 0 = 1 estrela ... índice 4 = 5 estrelas
        public int[] Distribuicao { get; set; } = new int[5];

        public List<AvaliacaoResponse> Avaliacoes { get; set; } = new();

        public static ReputacaoResponse DeAvaliacoes(IReadOnlyList<AvaliacaoRecebida> avaliacoes, int limiteComentarios = 30)
        {
            var distribuicao = new int[5];
            foreach (var a in avaliacoes) distribuicao[a.Nota - 1]++;

            return new ReputacaoResponse
            {
                Media = avaliacoes.Count > 0 ? Math.Round(avaliacoes.Average(a => a.Nota), 1) : null,
                Total = avaliacoes.Count,
                Distribuicao = distribuicao,
                Avaliacoes = avaliacoes.Take(limiteComentarios).Select(a => new AvaliacaoResponse
                {
                    Nota = a.Nota,
                    Comentario = a.Comentario,
                    Data = a.Data,
                    // Só o primeiro nome de quem avaliou (privacidade)
                    Autor = PrimeiroNome(a.NomeAutor),
                    TipoMovel = a.TipoMovel,
                }).ToList(),
            };
        }

        public static string PrimeiroNome(string? nome) =>
            string.IsNullOrWhiteSpace(nome) ? "Usuário" : nome.Trim().Split(' ')[0];
    }

    public class AvaliacaoResponse
    {
        public int Nota { get; set; }
        public string? Comentario { get; set; }
        public DateTime Data { get; set; }
        public string Autor { get; set; } = string.Empty;
        public string TipoMovel { get; set; } = string.Empty;
    }

    // O que um usuário pode ver de outro com quem tem (ou pode ter) um serviço
    public class PerfilPublicoResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string? FotoPerfilUrl { get; set; }
        public string? Regiao { get; set; }
        public DateTime DataCadastro { get; set; }
        public int ServicosConcluidos { get; set; }
        public ReputacaoResponse Reputacao { get; set; } = new();

        // Contato só é liberado enquanto os dois têm um serviço pago em andamento
        public bool ContatoLiberado { get; set; }
        public string? Telefone { get; set; }
    }
}
