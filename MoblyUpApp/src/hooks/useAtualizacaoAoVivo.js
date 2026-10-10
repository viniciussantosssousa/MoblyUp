import { useCallback, useRef } from 'react';
import { useFocusEffect } from '@react-navigation/native';
import { ouvir } from '../services/tempoReal';

// Recarrega a tela quando a API avisa que um pedido mudou (SignalR), só enquanto ela estiver
// visível. Vários avisos seguidos (ex.: aceitar + pagar) viram uma única recarga.
// Passe "pedidoId" para reagir só às mudanças daquele pedido.
export default function useAtualizacaoAoVivo(recarregar, pedidoId) {
  const recarregarRef = useRef(recarregar);
  recarregarRef.current = recarregar;

  useFocusEffect(useCallback(() => {
    let espera = null;

    const cancelar = ouvir('pedidoAlterado', (dados) => {
      // Sem pedidoId no aviso = reconexão (pode ter perdido eventos): recarrega sempre
      if (pedidoId && dados?.pedidoId && dados.pedidoId !== pedidoId) return;

      clearTimeout(espera);
      espera = setTimeout(() => recarregarRef.current(), 400);
    });

    return () => {
      clearTimeout(espera);
      cancelar();
    };
  }, [pedidoId]));
}
