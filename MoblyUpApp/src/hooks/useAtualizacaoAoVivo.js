import { useCallback, useRef } from 'react';
import { useFocusEffect } from '@react-navigation/native';
import { ouvir } from '../services/tempoReal';

// Recarrega a tela quando a API avisa que um pedido mudou (SignalR), só enquanto ela estiver
// visível. Vários avisos seguidos (ex.: aceitar + pagar) viram uma única recarga.
// Passe "pedidoId" para reagir só às mudanças daquele pedido.
// Por padrão também recarrega com mensagens novas do chat (contador de não lidas);
// a própria tela do chat desliga isso, porque já trata as mensagens.
export default function useAtualizacaoAoVivo(recarregar, pedidoId, { incluirChat = true } = {}) {
  const recarregarRef = useRef(recarregar);
  recarregarRef.current = recarregar;

  useFocusEffect(useCallback(() => {
    let espera = null;

    const agendar = (dados) => {
      // Sem pedidoId no aviso = reconexão (pode ter perdido eventos): recarrega sempre
      if (pedidoId && dados?.pedidoId && dados.pedidoId !== pedidoId) return;

      clearTimeout(espera);
      espera = setTimeout(() => recarregarRef.current(), 400);
    };

    const cancelarPedidos = ouvir('pedidoAlterado', agendar);
    const cancelarChat = incluirChat ? ouvir('mensagemChat', agendar) : () => {};

    return () => {
      clearTimeout(espera);
      cancelarPedidos();
      cancelarChat();
    };
  }, [pedidoId, incluirChat]));
}
