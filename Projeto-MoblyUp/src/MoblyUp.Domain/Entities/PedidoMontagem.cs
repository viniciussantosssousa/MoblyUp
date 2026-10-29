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

        // Medidas aproximadas em metros. Nulas só em pedidos criados antes de virarem obrigatórias.
        public decimal? AlturaM { get; private set; }
        public decimal? ComprimentoM { get; private set; }
        public decimal? LarguraM { get; private set; }

        public decimal? ValorProposto { get; private set; }
        public decimal? TaxaPlataforma { get; private set; }
        public decimal? ValorMontador { get; private set; }

        public StatusPedido Status { get; private set; }

        // Dia do serviço (o horário é combinado pelo chat depois do aceite):
        // o cliente pede uma data e o montador concorda ou sugere outra junto com o valor.
        // Depois que o cliente aceita a proposta, DataProposta é a data combinada.
        public DateOnly DataDesejada { get; private set; }
        public DateOnly? DataProposta { get; private set; }

        public bool PropostaMudaData => DataProposta.HasValue && DataProposta.Value != DataDesejada;

        public DateTime DataCriacao { get; private set; }
        public DateTime? DataAceite { get; private set; }
        // Quando o cliente aceitou o valor do montador (é o que libera o chat)
        public DateTime? DataPropostaAceita { get; private set; }
        public DateTime? DataConclusao { get; private set; }
        public string? FotoConclusaoUrl { get; private set; }

        // Avaliações mútuas (1 a 5 estrelas), cada lado avalia uma vez após a conclusão.
        // "Montador" = avaliação RECEBIDA pelo montador (dada pelo cliente).
        public int? NotaMontador { get; private set; }
        public string? ComentarioMontador { get; private set; }
        public DateTime? DataAvaliacaoMontador { get; private set; }

        // "Cliente" = avaliação RECEBIDA pelo cliente (dada pelo montador).
        public int? NotaCliente { get; private set; }
        public string? ComentarioCliente { get; private set; }
        public DateTime? DataAvaliacaoCliente { get; private set; }

        public Usuario Cliente { get; private set; }
        public Usuario? Montador { get; private set; }

        protected PedidoMontagem() { }

        public PedidoMontagem(string clienteId, string tipoMovel, string tamanhoMovel, string? urlFotoMovelTemporaria, string enderecoServico, DateOnly dataDesejada,
            decimal? alturaM, decimal? comprimentoM, decimal? larguraM)
        {
            ValueObjects.DataServico.Validar(dataDesejada, "data desejada para o serviço");
            AlturaM = ValueObjects.MedidasMovel.Validar(alturaM, "altura");
            ComprimentoM = ValueObjects.MedidasMovel.Validar(comprimentoM, "comprimento");
            LarguraM = ValueObjects.MedidasMovel.Validar(larguraM, "largura");

            Id = Guid.NewGuid();
            DataDesejada = dataDesejada;
            ClienteId = clienteId;
            TipoMovel = tipoMovel;
            TamanhoMovel = tamanhoMovel;
            UrlFotoMovelTemporaria = urlFotoMovelTemporaria;
            EnderecoServico = enderecoServico;
            Status = StatusPedido.Disponivel;
            DataCriacao = DateTime.UtcNow;
        }

        // dataProposta: o montador concorda com a data do cliente (null ou a mesma data) ou sugere outra
        public void ReceberProposta(string montadorId, decimal valorProposto, DateOnly? dataProposta = null)
        {
            if (Status != StatusPedido.Disponivel)
                throw new Exception("Este pedido não está mais disponível para propostas.");

            if (valorProposto <= 0)
                throw new Exception("O valor da proposta deve ser maior que zero.");

            if (montadorId == ClienteId)
                throw new Exception("Você não pode enviar proposta para o seu próprio pedido.");

            var data = dataProposta ?? DataDesejada;
            if (data == DataDesejada && data < ValueObjects.DataServico.Hoje())
                throw new Exception("A data pedida pelo cliente já passou. Sugira outra data na proposta.");
            ValueObjects.DataServico.Validar(data, "data sugerida");

            MontadorId = montadorId;
            ValorProposto = valorProposto;
            DataProposta = data;

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
            DataProposta = null;
            DataPropostaAceita = null;
            Status = StatusPedido.Disponivel;
        }

        public void AceitarProposta()
        {
            if (Status != StatusPedido.AguardandoAprovacaoCliente)
                throw new Exception("Não há proposta pendente para aceite.");

            Status = StatusPedido.AguardandoPagamento;
            DataPropostaAceita = DateTime.UtcNow;
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

        // Chat: abre quando o cliente aceita a proposta e dura enquanto o serviço está em andamento.
        // Depois de concluído ou cancelado, a conversa fica só para leitura.
        public bool ChatAberto =>
            Status == StatusPedido.AguardandoPagamento || Status == StatusPedido.EmAndamento;

        // Conversa visível (aberta ou só leitura): durante o serviço e depois que ele termina
        // Só existe conversa se o cliente chegou a aceitar o valor do montador
        // (cancelar com proposta ainda pendente não abre chat nenhum)
        public bool ChatDisponivel =>
            ChatAberto ||
            ((Status == StatusPedido.Concluido || Status == StatusPedido.Cancelado) && DataPropostaAceita.HasValue);

        public bool Participa(string usuarioId) => ClienteId == usuarioId || MontadorId == usuarioId;

        public string? OutraParte(string usuarioId) =>
            usuarioId == ClienteId ? MontadorId : usuarioId == MontadorId ? ClienteId : null;

        // Cliente avalia o montador
        public void AvaliarMontador(int nota, string? comentario)
        {
            var texto = ValidarAvaliacao(NotaMontador, nota, comentario, "Você já avaliou este montador.");
            NotaMontador = nota;
            ComentarioMontador = texto;
            DataAvaliacaoMontador = DateTime.UtcNow;
        }

        // Montador avalia o cliente
        public void AvaliarCliente(int nota, string? comentario)
        {
            var texto = ValidarAvaliacao(NotaCliente, nota, comentario, "Você já avaliou este cliente.");
            NotaCliente = nota;
            ComentarioCliente = texto;
            DataAvaliacaoCliente = DateTime.UtcNow;
        }

        private string? ValidarAvaliacao(int? notaAtual, int nota, string? comentario, string mensagemJaAvaliado)
        {
            if (Status != StatusPedido.Concluido)
                throw new Exception("Só é possível avaliar depois que o serviço for concluído.");

            if (notaAtual.HasValue)
                throw new Exception(mensagemJaAvaliado);

            if (nota < 1 || nota > 5)
                throw new Exception("A nota deve ser de 1 a 5 estrelas.");

            var texto = comentario?.Trim();
            if (texto?.Length > 500)
                throw new Exception("O comentário pode ter no máximo 500 caracteres.");

            // Comentário público não pode virar canal para passar contato
            if (ValueObjects.ProtecaoContato.Verificar(texto) != null)
                throw new Exception(ValueObjects.ProtecaoContato.MensagemBloqueio);

            return string.IsNullOrEmpty(texto) ? null : texto;
        }
    }
}