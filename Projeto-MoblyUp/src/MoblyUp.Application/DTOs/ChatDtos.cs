using System;
using System.Collections.Generic;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Enums;

namespace MoblyUp.Application.DTOs
{
    public class EnviarMensagemRequest
    {
        public string Texto { get; set; } = string.Empty;
    }

    public class MensagemChatResponse
    {
        public Guid Id { get; set; }
        public Guid PedidoId { get; set; }
        public string RemetenteId { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;
        public DateTime DataEnvio { get; set; }
        public bool Lida { get; set; }

        public static MensagemChatResponse DeEntidade(MensagemChat m) => new()
        {
            Id = m.Id,
            PedidoId = m.PedidoId,
            RemetenteId = m.RemetenteId,
            Texto = m.Texto,
            DataEnvio = m.DataEnvio,
            Lida = m.DataLeitura.HasValue,
        };
    }

    public class ConversaResponse
    {
        public Guid PedidoId { get; set; }
        public string TipoMovel { get; set; } = string.Empty;
        public StatusPedido Status { get; set; }
        // Aberto = dá para enviar mensagens; fechado = conversa só para leitura
        public bool Aberto { get; set; }
        public string? OutraParteId { get; set; }
        public string OutraParteNome { get; set; } = string.Empty;
        public string? OutraParteFoto { get; set; }
        public string OutraParteTipo { get; set; } = string.Empty;
        public List<MensagemChatResponse> Mensagens { get; set; } = new();
    }
}
