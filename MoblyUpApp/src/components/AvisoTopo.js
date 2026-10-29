import React, { useEffect, useRef, useState } from 'react';
import { Animated, Text, TouchableOpacity, View, StyleSheet, Platform } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { ouvir, obterChatNaTela } from '../services/tempoReal';
import { pushAtivo } from '../services/notificacoes';
import { colors } from '../theme/colors';

const DURACAO = 6000;

// Banner no topo da tela para avisos em tempo real (nova proposta, pedido novo na região...).
// No celular com push ativo o próprio sistema já mostra a notificação, então o banner não aparece
// para não duplicar; no navegador (sem push) ele é o aviso principal.
export default function AvisoTopo({ onAbrir }) {
  const [aviso, setAviso] = useState(null);
  const posicao = useRef(new Animated.Value(-160)).current;
  const timer = useRef(null);

  const esconder = () => {
    clearTimeout(timer.current);
    Animated.timing(posicao, { toValue: -160, duration: 220, useNativeDriver: Platform.OS !== 'web' }).start();
    // Remove pelo relógio, não pelo fim da animação: com a aba em segundo plano o navegador
    // pausa as animações e o aviso ficaria preso na tela
    timer.current = setTimeout(() => setAviso(null), 260);
  };

  useEffect(() => ouvir('aviso', (dados) => {
    if (pushAtivo()) return;
    // Mensagem da conversa que já está aberta na tela: ela aparece no próprio chat
    if (dados?.tipo === 'chat' && dados.pedidoId === obterChatNaTela()) return;
    setAviso(dados);
    Animated.spring(posicao, { toValue: 0, useNativeDriver: Platform.OS !== 'web' }).start();
    clearTimeout(timer.current);
    timer.current = setTimeout(esconder, DURACAO);
  }), []);

  useEffect(() => () => clearTimeout(timer.current), []);

  if (!aviso) return null;

  return (
    <Animated.View style={[styles.container, { transform: [{ translateY: posicao }] }]} pointerEvents="box-none">
      <TouchableOpacity
        style={styles.cartao}
        activeOpacity={0.9}
        onPress={() => { esconder(); onAbrir?.(aviso); }}
      >
        <View style={styles.icone}>
          <Ionicons name="notifications" size={20} color={colors.white} />
        </View>
        <View style={{ flex: 1 }}>
          <Text style={styles.titulo} numberOfLines={1}>{aviso.titulo}</Text>
          <Text style={styles.mensagem} numberOfLines={2}>{aviso.mensagem}</Text>
        </View>
        <TouchableOpacity onPress={esconder} hitSlop={{ top: 10, bottom: 10, left: 10, right: 10 }}>
          <Ionicons name="close" size={20} color="#A0AEC0" />
        </TouchableOpacity>
      </TouchableOpacity>
    </Animated.View>
  );
}

const styles = StyleSheet.create({
  container: {
    position: 'absolute', top: 0, left: 0, right: 0, zIndex: 1000,
    paddingTop: Platform.OS === 'ios' ? 54 : 32, paddingHorizontal: 12,
  },
  cartao: {
    flexDirection: 'row', alignItems: 'center', backgroundColor: colors.white, borderRadius: 14, padding: 14,
    maxWidth: 520, width: '100%', alignSelf: 'center',
    shadowColor: '#000', shadowOffset: { width: 0, height: 4 }, shadowOpacity: 0.18, shadowRadius: 12, elevation: 8,
    borderLeftWidth: 4, borderLeftColor: '#38A169',
  },
  icone: {
    width: 36, height: 36, borderRadius: 18, backgroundColor: colors.primary,
    alignItems: 'center', justifyContent: 'center', marginRight: 12,
  },
  titulo: { fontSize: 15, fontWeight: 'bold', color: colors.primary },
  mensagem: { fontSize: 13, color: '#4A5568', marginTop: 2 },
});
