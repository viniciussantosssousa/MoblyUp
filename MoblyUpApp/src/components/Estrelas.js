import React from 'react';
import { View, TouchableOpacity } from 'react-native';
import { Ionicons } from '@expo/vector-icons';

const COR = '#D69E2E';

// Mostra de 0 a 5 estrelas (aceita meia estrela). Com "onChange", vira seletor de nota.
export default function Estrelas({ nota = 0, tamanho = 16, onChange, style }) {
  return (
    <View style={[{ flexDirection: 'row', alignItems: 'center' }, style]}>
      {[1, 2, 3, 4, 5].map((posicao) => {
        const icone = nota >= posicao ? 'star' : nota >= posicao - 0.5 ? 'star-half' : 'star-outline';
        const estrela = <Ionicons name={icone} size={tamanho} color={COR} />;

        return onChange ? (
          <TouchableOpacity
            key={posicao}
            onPress={() => onChange(posicao)}
            style={{ paddingHorizontal: 4 }}
            accessibilityLabel={`${posicao} ${posicao === 1 ? 'estrela' : 'estrelas'}`}
          >
            {estrela}
          </TouchableOpacity>
        ) : (
          <View key={posicao} style={{ marginRight: 1 }}>{estrela}</View>
        );
      })}
    </View>
  );
}

// "4,8 (12 avaliações)" ou "Novo no MoblyUp"
export const descreverReputacao = (media, total) =>
  total > 0
    ? `${media.toFixed(1).replace('.', ',')} (${total} ${total === 1 ? 'avaliação' : 'avaliações'})`
    : 'Novo no MoblyUp';
