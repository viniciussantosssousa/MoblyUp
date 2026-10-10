using System;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Enums;

namespace MoblyUp.Application.DTOs
{
    public class PedidoMontagemResponse
    {
        public Guid Id { get; set; }
        public string ClienteId { get; set; } = string.Empty;
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

        public static PedidoMontagemResponse DeEntidade(PedidoMontagem pedido)
        {
            return new PedidoMontagemResponse
            {
                Id = pedido.Id,
                ClienteId = pedido.ClienteId,
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
                FotoConclusaoUrl = pedido.FotoConclusaoUrl
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
