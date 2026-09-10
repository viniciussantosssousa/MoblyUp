using MoblyUp.Domain.Enums;

namespace MoblyUp.Domain.Entities
{
    public class PedidoMontagem
    {
        public Guid Id { get; private set; }
        public Guid ClienteId { get; private set; }
        public Guid? MontadorId { get; private set; }

        public string TipoMovel { get; private set; }
        public string TamanhoMovel { get; private set; }
        public string? UrlFotoMovelTemporaria { get; private set; }
        public string EnderecoServico { get; private set; }

        public decimal? ValorProposto { get; private set; }
        public decimal? TaxaPlataforma { get; private set; }
        public decimal? ValorMontador { get; private set; }

        public StatusPedido Status { get; private set; }

        public DateTime DataCriacao { get; private set; }
        public DateTime? DataAceite { get; private set; }
        public DateTime? DataConclusao { get; private set; }
        public string? FotoConclusaoUrl { get; private set; }

        public Usuario Cliente { get; private set; }
        public Usuario? Montador { get; private set; }

        protected PedidoMontagem() { }

        public PedidoMontagem(Guid clienteId, string tipoMovel, string tamanhoMovel, string? urlFotoMovelTemporaria, string enderecoServico)
        {
            Id = Guid.NewGuid();
            ClienteId = clienteId;
            TipoMovel = tipoMovel;
            TamanhoMovel = tamanhoMovel;
            UrlFotoMovelTemporaria = urlFotoMovelTemporaria;
            EnderecoServico = enderecoServico;
            Status = StatusPedido.Disponivel;
            DataCriacao = DateTime.UtcNow;
        }


        public void ReceberProposta(Guid montadorId, decimal valorProposto)
        {
            if (Status != StatusPedido.Disponivel)
                throw new Exception("Este pedido não está mais disponível para propostas.");

            MontadorId = montadorId;
            ValorProposto = valorProposto;

            TaxaPlataforma = valorProposto * 0.15m;
            ValorMontador = valorProposto - TaxaPlataforma;

            Status = StatusPedido.AguardandoAprovacaoCliente;
        }

        public void RecusarProposta()
        {
            if (Status != StatusPedido.AguardandoAprovacaoCliente)
                throw new Exception("Não há proposta pendente para recusa.");

            MontadorId = null;
            ValorProposto = null;
            TaxaPlataforma = null;
            ValorMontador = null;
            Status = StatusPedido.Disponivel;
        }

        public void AceitarProposta()
        {
            if (Status != StatusPedido.AguardandoAprovacaoCliente)
                throw new Exception("Não há proposta pendente para aceite.");

            Status = StatusPedido.AguardandoPagamento;
        }

        public void ConfirmarPagamento()
        {
            if (Status != StatusPedido.AguardandoPagamento)
                throw new Exception("Pedido não está aguardando pagamento.");

            Status = StatusPedido.EmAndamento;
            DataAceite = DateTime.UtcNow;
        }
        public void EnviarFotoConclusao(string urlFoto)
        {
            if (Status != StatusPedido.EmAndamento)
                throw new Exception("O pedido precisa estar em andamento para receber a foto.");
            FotoConclusaoUrl = urlFoto;
        }

        public void ConcluirPedido()
        {
            if (string.IsNullOrEmpty(FotoConclusaoUrl))
                throw new Exception("É necessário enviar a foto antes de concluir.");
            Status = StatusPedido.Concluido;
            DataConclusao = DateTime.UtcNow;
        }
    }
}