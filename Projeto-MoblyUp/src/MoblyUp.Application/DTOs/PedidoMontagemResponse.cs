using System;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Enums;

namespace MoblyUp.Application.DTOs
{
    public class PedidoMontagemResponse
    {
        public Guid Id { get; set; }
        public string ClienteId { get; set; } = string.Empty;
        public string? NomeCliente { get; set; }
        public string? MontadorId { get; set; }
        public string? NomeMontador { get; set; }

        public string TipoMovel { get; set; } = string.Empty;
        public string TamanhoMovel { get; set; } = string.Empty;
        public string? UrlFotoMovelTemporaria { get; set; }
        public string EnderecoServico { get; set; } = string.Empty;

        public decimal? ValorProposto { get; set; }
        public decimal? TaxaPlataforma { get; set; }
        public decimal? ValorMontador { get; set; }

        public StatusPedido Status { get; set; }
        public string StatusDescricao { get; set; } = string.Empty;

        public DateTime DataCriacao { get; set; }
        public DateTime? DataAceite { get; set; }
        public DateTime? DataConclusao { get; set; }
        public string? FotoConclusaoUrl { get; set; }

        // Avaliações deste serviço: a que o montador recebeu (do cliente) e a que o cliente recebeu (do montador)
        public int? NotaMontador { get; set; }
        public string? ComentarioMontador { get; set; }
        public int? NotaCliente { get; set; }
        public string? ComentarioCliente { get; set; }

        // Reputação geral de cada parte, para um decidir se faz negócio com o outro
        public double? MediaAvaliacaoMontador { get; set; }
        public int TotalAvaliacoesMontador { get; set; }
        public double? MediaAvaliacaoCliente { get; set; }
        public int TotalAvaliacoesCliente { get; set; }

        public static PedidoMontagemResponse DeEntidade(PedidoMontagem pedido)
        {
            return new PedidoMontagemResponse
            {
                Id = pedido.Id,
                ClienteId = pedido.ClienteId,
                NomeCliente = pedido.Cliente == null ? null : ReputacaoResponse.PrimeiroNome(pedido.Cliente.Nome),
                MontadorId = pedido.MontadorId,
                NomeMontador = pedido.Montador?.Nome,
                TipoMovel = pedido.TipoMovel,
                TamanhoMovel = pedido.TamanhoMovel,
                UrlFotoMovelTemporaria = pedido.UrlFotoMovelTemporaria,
                EnderecoServico = pedido.EnderecoServico,
                ValorProposto = pedido.ValorProposto,
                TaxaPlataforma = pedido.TaxaPlataforma,
                ValorMontador = pedido.ValorMontador,
                Status = pedido.Status,
                StatusDescricao = DescreverStatus(pedido.Status),
                DataCriacao = pedido.DataCriacao,
                DataAceite = pedido.DataAceite,
                DataConclusao = pedido.DataConclusao,
                FotoConclusaoUrl = pedido.FotoConclusaoUrl,
                NotaMontador = pedido.NotaMontador,
                ComentarioMontador = pedido.ComentarioMontador,
                NotaCliente = pedido.NotaCliente,
                ComentarioCliente = pedido.ComentarioCliente
            };
        }

        private static string DescreverStatus(StatusPedido status) => status switch
        {
            StatusPedido.Disponivel => "Aguardando Propostas",
            StatusPedido.AguardandoAprovacaoCliente => "Proposta Recebida",
            StatusPedido.AguardandoPagamento => "Aguardando Pagamento",
            StatusPedido.EmAndamento => "Em Andamento",
            StatusPedido.Concluido => "Concluído",
            StatusPedido.Cancelado => "Cancelado",
            _ => status.ToString()
        };
    }
}
