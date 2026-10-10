import React from 'react';
import { View, Text, TouchableOpacity, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import Estrelas, { descreverReputacao } from './Estrelas';
import { colors } from '../theme/colors';
import { formatarData } from '../services/statusPedido';

// "★★★★☆ 4,3 (12 avaliações)" — com onPress vira um link para o perfil
export function ResumoReputacao({ media, total, tamanho = 13, onPress, rotuloLink = 'Ver perfil', style }) {
  const conteudo = (
    <View style={[styles.resumo, style]}>
      {total > 0 && <Estrelas nota={media} tamanho={tamanho} style={{ marginRight: 6 }} />}
      <Text style={styles.resumoTexto}>{descreverReputacao(media, total)}</Text>
      {onPress && (
        <View style={styles.link}>
          <Text style={styles.linkTexto}>{rotuloLink}</Text>
          <Ionicons name="chevron-forward" size={14} color="#3182CE" />
        </View>
      )}
    </View>
  );

  return onPress ? <TouchableOpacity onPress={onPress} activeOpacity={0.7}>{conteudo}</TouchableOpacity> : conteudo;
}

// Quadro com a média grande e as barras de 5 a 1 estrela
export function QuadroReputacao({ reputacao }) {
  const { media, total, distribuicao = [0, 0, 0, 0, 0] } = reputacao || {};
  const maior = Math.max(1, ...distribuicao);

  return (
    <View style={styles.quadro}>
      <View style={styles.quadroMedia}>
        <Text style={styles.mediaNumero}>{total > 0 ? media.toFixed(1).replace('.', ',') : '–'}</Text>
        <Estrelas nota={media || 0} tamanho={14} />
        <Text style={styles.mediaTotal}>
          {total > 0 ? `${total} ${total === 1 ? 'avaliação' : 'avaliações'}` : 'Sem avaliações'}
        </Text>
      </View>

      <View style={styles.barras}>
        {[5, 4, 3, 2, 1].map((nota) => (
          <View key={nota} style={styles.barraLinha}>
            <Text style={styles.barraRotulo}>{nota}</Text>
            <Ionicons name="star" size={10} color="#D69E2E" />
            <View style={styles.barraFundo}>
              <View style={[styles.barraCheia, { width: `${(distribuicao[nota - 1] / maior) * 100}%` }]} />
            </View>
            <Text style={styles.barraQtd}>{distribuicao[nota - 1]}</Text>
          </View>
        ))}
      </View>
    </View>
  );
}

// Lista de comentários recebidos
export function ListaAvaliacoes({ avaliacoes = [], vazio = 'Ainda não há avaliações.' }) {
  if (!avaliacoes.length) return <Text style={styles.vazio}>{vazio}</Text>;

  return avaliacoes.map((a, i) => (
    <View key={i} style={[styles.avaliacao, i > 0 && styles.avaliacaoSeparada]}>
      <View style={styles.avaliacaoTopo}>
        <Estrelas nota={a.nota} tamanho={13} />
        <Text style={styles.avaliacaoData}>{formatarData(a.data)}</Text>
      </View>
      {a.comentario ? <Text style={styles.avaliacaoComentario}>"{a.comentario}"</Text> : null}
      <Text style={styles.avaliacaoAutor}>{a.autor} · {a.tipoMovel}</Text>
    </View>
  ));
}

const styles = StyleSheet.create({
  resumo: { flexDirection: 'row', alignItems: 'center', flexWrap: 'wrap' },
  resumoTexto: { fontSize: 12, color: '#718096' },
  link: { flexDirection: 'row', alignItems: 'center', marginLeft: 8 },
  linkTexto: { fontSize: 12, color: '#3182CE', fontWeight: '600' },

  quadro: { flexDirection: 'row', alignItems: 'center' },
  quadroMedia: { alignItems: 'center', paddingRight: 20, marginRight: 16, borderRightWidth: 1, borderRightColor: colors.border },
  mediaNumero: { fontSize: 34, fontWeight: 'bold', color: colors.primary },
  mediaTotal: { fontSize: 11, color: '#718096', marginTop: 4 },
  barras: { flex: 1 },
  barraLinha: { flexDirection: 'row', alignItems: 'center', marginVertical: 2 },
  barraRotulo: { fontSize: 11, color: '#4A5568', width: 10, textAlign: 'right', marginRight: 3 },
  barraFundo: { flex: 1, height: 6, borderRadius: 3, backgroundColor: '#EDF2F7', marginHorizontal: 6, overflow: 'hidden' },
  barraCheia: { height: 6, borderRadius: 3, backgroundColor: '#D69E2E' },
  barraQtd: { fontSize: 11, color: '#718096', width: 18, textAlign: 'right' },

  vazio: { fontSize: 13, color: '#A0AEC0', fontStyle: 'italic' },
  avaliacao: { paddingVertical: 10 },
  avaliacaoSeparada: { borderTopWidth: 1, borderTopColor: colors.border },
  avaliacaoTopo: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  avaliacaoData: { fontSize: 11, color: '#A0AEC0' },
  avaliacaoComentario: { fontSize: 14, color: '#2D3748', marginTop: 6, lineHeight: 20 },
  avaliacaoAutor: { fontSize: 12, color: '#718096', marginTop: 4 },
});
