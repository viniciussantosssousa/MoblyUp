import React, { useMemo, useRef, useEffect } from 'react';
import { ScrollView, TouchableOpacity, Text, View, StyleSheet } from 'react-native';
import { colors } from '../theme/colors';
import { proximosDias, hojeIso } from '../services/datas';

const DIAS_DISPONIVEIS = 60;

// Faixa horizontal com os próximos dias ("Hoje", "Amanhã", "sex 31 out"...).
// Funciona igual no celular e no navegador, sem depender de calendário nativo.
export default function SeletorData({ valor, onChange, destacar }) {
  const dias = useMemo(() => proximosDias(DIAS_DISPONIVEIS), []);
  const hoje = hojeIso();
  const lista = useRef(null);

  // Ao abrir, rola até o dia já selecionado (ex.: a data pedida pelo cliente, mais adiante na faixa).
  // Só na primeira vez: depois quem rola é a própria pessoa.
  const valorInicial = useRef(valor);
  useEffect(() => {
    const indice = dias.findIndex((d) => d.iso === valorInicial.current);
    if (indice > 3) setTimeout(() => lista.current?.scrollTo({ x: (indice - 1) * 68, animated: false }), 50);
  }, [dias]);

  return (
    <ScrollView ref={lista} horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.faixa}>
      {dias.map((d, i) => {
        const selecionado = d.iso === valor;
        const rotulo = d.iso === hoje ? 'Hoje' : i === 1 ? 'Amanhã' : d.diaSemana;
        const marcado = destacar && d.iso === destacar && !selecionado;
        return (
          <TouchableOpacity
            key={d.iso}
            style={[styles.dia, selecionado && styles.diaSelecionado, marcado && styles.diaDestacado]}
            onPress={() => onChange(d.iso)}
            accessibilityLabel={`Escolher ${rotulo} ${d.dia} de ${d.mes}`}
          >
            <Text style={[styles.semana, selecionado && styles.textoSelecionado]}>{rotulo}</Text>
            <Text style={[styles.numero, selecionado && styles.textoSelecionado]}>{d.dia}</Text>
            <Text style={[styles.mes, selecionado && styles.textoSelecionado]}>{d.mes}</Text>
            {marcado ? <View style={styles.ponto} /> : null}
          </TouchableOpacity>
        );
      })}
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  faixa: { paddingVertical: 4 },
  dia: {
    width: 60, marginRight: 8, paddingVertical: 10, borderRadius: 12, alignItems: 'center',
    backgroundColor: colors.white, borderWidth: 1, borderColor: colors.border,
  },
  diaSelecionado: { backgroundColor: colors.primary, borderColor: colors.primary },
  diaDestacado: { borderColor: '#D69E2E', borderWidth: 2 },
  semana: { fontSize: 11, color: '#718096', textTransform: 'capitalize' },
  numero: { fontSize: 20, fontWeight: 'bold', color: colors.primary, marginVertical: 2 },
  mes: { fontSize: 11, color: '#718096' },
  textoSelecionado: { color: colors.white },
  ponto: { width: 6, height: 6, borderRadius: 3, backgroundColor: '#D69E2E', marginTop: 3 },
});
