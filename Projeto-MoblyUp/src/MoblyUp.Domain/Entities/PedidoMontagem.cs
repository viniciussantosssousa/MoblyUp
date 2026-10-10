using MoblyUp.Domain.Enums;

namespace MoblyUp.Domain.Entities
{
    public class PedidoMontagem
    {
        public Guid Id { get; private set; }
        public string ClienteId { get; private set; }
        public string? MontadorId { get; private set; }

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

        public PedidoMontagem(string clienteId, string tipoMovel, string tamanhoMovel, string? urlFotoMovelTemporaria, string enderecoServico)
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

        public void ReceberProposta(string montadorId, decimal valorProposto)
        {
            if (Status != StatusPedido.Disponivel)
                throw new Exception("Este pedido não está mais disponível para propostas.");

            if (valorProposto <= 0)
                throw new Exception("O valor da proposta deve ser maior que zero.");

            if (montadorId == ClienteId)
                throw new Exception("Você não pode enviar proposta para o seu próprio pedido.");

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

            ReabrirParaPropostas();
        }

        // Montador desiste do serviço antes do pagamento: o pedido volta para o feed
        public void DesistirProposta(string montadorId)
        {
            if (MontadorId != montadorId)
                throw new UnauthorizedAccessException("Apenas o montador da proposta pode desistir dela.");

            if (Status != StatusPedido.AguardandoAprovacaoCliente && Status != StatusPedido.AguardandoPagamento)
                throw new Exception("Após a confirmação do pagamento não é possível desistir pelo app. Fale com o suporte.");

            ReabrirParaPropostas();
        }

        // Cliente cancela o pedido enquanto ainda não pagou
        public void Cancelar()
        {
            if (Status == StatusPedido.Cancelado)
                throw new Exception("Este pedido já foi cancelado.");

            if (Status == StatusPedido.EmAndamento || Status == StatusPedido.Concluido)
                throw new Exception("Pedidos já pagos não podem ser cancelados pelo app. Fale com o suporte.");

            Status = StatusPedido.Cancelado;
        }

        private void ReabrirParaPropostas()
        {
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
            if (Status != StatusPedido.EmAndamento)
                throw new Exception("Apenas pedidos em andamento podem ser concluídos.");
            if (string.IsNullOrEmpty(FotoConclusaoUrl))
                throw new Exception("É necessário enviar a foto antes de concluir.");
            Status = StatusPedido.Concluido;
            DataConclusao = DateTime.UtcNow;
        }
    }
}