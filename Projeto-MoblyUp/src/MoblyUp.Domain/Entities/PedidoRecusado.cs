namespace MoblyUp.Domain.Entities
{
    // Pedido que um montador recusou (ou do qual desistiu): deixa de aparecer no feed dele
    public class PedidoRecusado
    {
        public Guid PedidoId { get; private set; }
        public string MontadorId { get; private set; }
        public DateTime DataRecusa { get; private set; }

        protected PedidoRecusado()
        {
            MontadorId = string.Empty;
        }

        public PedidoRecusado(Guid pedidoId, string montadorId)
        {
            PedidoId = pedidoId;
            MontadorId = montadorId;
            DataRecusa = DateTime.UtcNow;
        }
    }
}
