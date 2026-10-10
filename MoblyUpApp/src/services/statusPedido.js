// Espelha o enum StatusPedido do back-end (MoblyUp.Domain/Enums/StatusPedido.cs)
export const STATUS_PEDIDO = {
  DISPONIVEL: 1,
  AGUARDANDO_APROVACAO_CLIENTE: 2,
  AGUARDANDO_PAGAMENTO: 3,
  EM_ANDAMENTO: 4,
  CONCLUIDO: 5,
  CANCELADO: 6,
};

export const isConcluido = (pedido) => pedido?.status === STATUS_PEDIDO.CONCLUIDO;

export const formatarData = (dataIso) => {
  if (!dataIso) return '';
  return new Date(dataIso).toLocaleDateString('pt-BR');
};

export const formatarMoeda = (valor) => {
  if (valor == null || valor === '') return 'A definir';
  return `R$ ${parseFloat(valor).toFixed(2).replace('.', ',')}`;
};
