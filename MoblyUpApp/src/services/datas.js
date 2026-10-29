// Datas do serviço vêm da API só com o dia ("2026-10-31", sem horário).
// Atenção: new Date("2026-10-31") é meia-noite em UTC = dia 30 no Brasil. Por isso tudo aqui
// monta a data pelo calendário local, sem passar por UTC.

const DIAS = ['dom', 'seg', 'ter', 'qua', 'qui', 'sex', 'sáb'];
const DIAS_EXTENSO = ['domingo', 'segunda', 'terça', 'quarta', 'quinta', 'sexta', 'sábado'];
const MESES = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];

const doisDigitos = (n) => String(n).padStart(2, '0');

// Date local -> "2026-10-31"
export const paraIso = (data) =>
  `${data.getFullYear()}-${doisDigitos(data.getMonth() + 1)}-${doisDigitos(data.getDate())}`;

// "2026-10-31" -> Date local (meio-dia, longe de qualquer virada de dia)
export const deIso = (iso) => {
  const [ano, mes, dia] = String(iso).slice(0, 10).split('-').map(Number);
  return new Date(ano, mes - 1, dia, 12);
};

export const hojeIso = () => paraIso(new Date());

// "2026-10-31" -> "sex, 31/10"
export const formatarDataServico = (iso) => {
  if (!iso) return '';
  const d = deIso(iso);
  return `${DIAS[d.getDay()]}, ${doisDigitos(d.getDate())}/${doisDigitos(d.getMonth() + 1)}`;
};

// "2026-10-31" -> "Hoje", "Amanhã" ou "sexta, 31/10"
export const descreverDataServico = (iso) => {
  if (!iso) return '';
  const diferenca = Math.round((deIso(iso) - deIso(hojeIso())) / 86400000);
  if (diferenca === 0) return 'Hoje';
  if (diferenca === 1) return 'Amanhã';
  const d = deIso(iso);
  return `${DIAS_EXTENSO[d.getDay()]}, ${doisDigitos(d.getDate())}/${doisDigitos(d.getMonth() + 1)}`;
};

export const dataJaPassou = (iso) => !!iso && iso.slice(0, 10) < hojeIso();

// Próximos dias para o seletor: [{ iso, diaSemana, dia, mes }]
export const proximosDias = (quantidade, aPartirDe = 0) =>
  Array.from({ length: quantidade }, (_, i) => {
    const d = new Date();
    d.setHours(12, 0, 0, 0);
    d.setDate(d.getDate() + aPartirDe + i);
    return { iso: paraIso(d), diaSemana: DIAS[d.getDay()], dia: d.getDate(), mes: MESES[d.getMonth()] };
  });
