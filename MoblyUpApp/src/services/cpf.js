// Mesmo algoritmo da API (MoblyUp.Domain/ValueObjects/Cpf.cs): valida antes de enviar

export const somenteNumeros = (cpf) => (cpf || '').replace(/\D/g, '');

// 529.982.247-25
export const formatarCpf = (valor) => {
  const n = somenteNumeros(valor).slice(0, 11);
  if (n.length <= 3) return n;
  if (n.length <= 6) return `${n.slice(0, 3)}.${n.slice(3)}`;
  if (n.length <= 9) return `${n.slice(0, 3)}.${n.slice(3, 6)}.${n.slice(6)}`;
  return `${n.slice(0, 3)}.${n.slice(3, 6)}.${n.slice(6, 9)}-${n.slice(9)}`;
};

const calcularDigito = (numeros, tamanho) => {
  let soma = 0;
  for (let i = 0; i < tamanho; i++) soma += Number(numeros[i]) * (tamanho + 1 - i);
  const resto = soma % 11;
  return resto < 2 ? 0 : 11 - resto;
};

export const cpfValido = (cpf) => {
  const n = somenteNumeros(cpf);
  if (n.length !== 11 || /^(\d)\1{10}$/.test(n)) return false;
  return Number(n[9]) === calcularDigito(n, 9) && Number(n[10]) === calcularDigito(n, 10);
};
