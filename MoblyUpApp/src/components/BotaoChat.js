import React from 'react';
import { TouchableOpacity, Text, View, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { colors } from '../theme/colors';

// Botão que abre o chat do pedido, com o número de mensagens não lidas
export default function BotaoChat({ pedido, rotulo, onPress, style }) {
  if (!pedido?.chatDisponivel) return null;

  const naoLidas = pedido.mensagensNaoLidas || 0;
  const aberto = pedido.chatAberto;

  return (
    <TouchableOpacity
      style={[styles.botao, !aberto && styles.botaoLeitura, style]}
      activeOpacity={0.8}
      onPress={onPress}
    >
      <Ionicons name={aberto ? 'chatbubbles' : 'chatbubbles-outline'} size={18} color={aberto ? colors.white : colors.primary} />
      <Text style={[styles.texto, !aberto && styles.textoLeitura]}>{aberto ? rotulo : 'Ver conversa'}</Text>
      {naoLidas > 0 && (
        <View style={styles.badge}>
          <Text style={styles.badgeTexto}>{naoLidas > 99 ? '99+' : naoLidas}</Text>
        </View>
      )}
    </TouchableOpacity>
  );
}

const styles = StyleSheet.create({
  botao: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'center',
    backgroundColor: '#2B6CB0', borderRadius: 8, paddingVertical: 12, paddingHorizontal: 14, marginTop: 12,
  },
  botaoLeitura: { backgroundColor: colors.white, borderWidth: 1, borderColor: colors.border },
  texto: { color: colors.white, fontSize: 14, fontWeight: 'bold', marginLeft: 8 },
  textoLeitura: { color: colors.primary },
  badge: {
    minWidth: 20, height: 20, borderRadius: 10, backgroundColor: '#E53E3E', marginLeft: 8,
    alignItems: 'center', justifyContent: 'center', paddingHorizontal: 5,
  },
  badgeTexto: { color: colors.white, fontSize: 11, fontWeight: 'bold' },
});
