import AsyncStorage from '@react-native-async-storage/async-storage';
import { STATUS_PEDIDO } from './statusPedido';

// Pedidos em que o usuário tocou "Agora não" na janela de avaliação: ela não aparece de novo
// para eles (a avaliação continua disponível em Detalhes/Histórico). Guardado por usuário.
const chave = (usuarioId) => `@MoblyUp:avaliacoesAdiadas:${usuarioId}`;

export const obterAdiadas = async (usuarioId) => {
  try {
    return JSON.parse((await AsyncStorage.getItem(chave(usuarioId))) || '[]');
  } catch {
    return [];
  }
};

export const adiarAvaliacao = async (usuarioId, pedidoId) => {
  const adiadas = await obterAdiadas(usuarioId);
  if (!adiadas.includes(pedidoId)) {
    // Mantém só os mais recentes para a lista não crescer para sempre
    await AsyncStorage.setItem(chave(usuarioId), JSON.stringify([...adiadas, pedidoId].slice(-50)));
  }
};

// Primeiro serviço concluído que ainda espera a avaliação deste usuário e não foi adiado
export const encontrarAvaliacaoPendente = async (usuarioId, pedidos, campoNota) => {
  const adiadas = await obterAdiadas(usuarioId);
  return pedidos.find((p) => p.status === STATUS_PEDIDO.CONCLUIDO && !p[campoNota] && !adiadas.includes(p.id)) || null;
};
