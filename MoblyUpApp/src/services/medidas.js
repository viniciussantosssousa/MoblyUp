// Medidas aproximadas do móvel, em metros (altura x comprimento x largura)

// Aceita "2,10", "2.10" e "2"; devolve 0 se não for número
export const converterMedida = (texto) => parseFloat(String(texto || '').replace(',', '.')) || 0;

// Mesma faixa validada pela API
export const MEDIDA_MINIMA = 0.05;
export const MEDIDA_MAXIMA = 10;

const metros = (valor) => Number(valor).toFixed(2).replace('.', ',');

// "A 2,10 × C 1,80 × L 0,55 m" (pedidos antigos não têm medidas)
export const formatarMedidas = (pedido) =>
  pedido?.alturaM && pedido?.comprimentoM && pedido?.larguraM
    ? `A ${metros(pedido.alturaM)} × C ${metros(pedido.comprimentoM)} × L ${metros(pedido.larguraM)} m`
    : null;
