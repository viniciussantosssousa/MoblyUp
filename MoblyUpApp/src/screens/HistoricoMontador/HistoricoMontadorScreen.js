import React, { useState, useEffect, useContext } from 'react';
import { View, Text, TouchableOpacity, StyleSheet, Platform, ScrollView, ActivityIndicator } from 'react-native';
import { useNavigation, useIsFocused } from '@react-navigation/native';
import { Ionicons } from '@expo/vector-icons';
import { colors } from '../../theme/colors';
import { AuthContext } from '../../contexts/AuthContext';
import api from '../../services/api';
import { STATUS_PEDIDO, isConcluido, formatarData } from '../../services/statusPedido';

export default function HistoricoMontadorScreen() {
  const navigation = useNavigation();
  const isFocused = useIsFocused();
  const { user } = useContext(AuthContext);

  const [pedidos, setPedidos] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchHistorico = async () => {
      try {
        setLoading(true);
        // Todos os pedidos em que este montador enviou proposta ou trabalhou
        const response = await api.get('/pedidos/montador');
        setPedidos(response.data);
      } catch (error) {
        console.log('Erro ao buscar histórico do montador:', error);
      } finally {
        setLoading(false);
      }
    };

    if (isFocused && user?.id) {
      fetchHistorico();
    }
  }, [isFocused, user?.id]);

  const formatarMoeda = (valor) => {
    if (!valor) return '';
    return `R$ ${parseFloat(valor).toFixed(2).replace('.', ',')}`;
  };

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <Ionicons name="arrow-back" size={28} color={colors.primary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Meu Histórico</Text>
      </View>

      {loading ? (
        <View style={styles.centered}>
          <ActivityIndicator size="large" color={colors.primary} />
        </View>
      ) : pedidos.length === 0 ? (
        <View style={styles.centered}>
          <Text style={styles.emptyText}>Você ainda não possui serviços.</Text>
        </View>
      ) : (
        <ScrollView contentContainerStyle={styles.list}>
          {pedidos.map((pedido) => {
            const concluido = isConcluido(pedido);
            const emAndamento = pedido.status === STATUS_PEDIDO.EM_ANDAMENTO;
            return (
              <TouchableOpacity
                key={pedido.id}
                style={styles.card}
                activeOpacity={emAndamento ? 0.7 : 1}
                disabled={!emAndamento}
                onPress={() => navigation.navigate('ConcluirServico', { pedido })}
              >
                <View style={styles.cardHeader}>
                  <Text style={styles.cardTitle}>{pedido.tipoMovel}</Text>
                  <Text style={styles.cardDate}>{formatarData(pedido.dataConclusao || pedido.dataCriacao)}</Text>
                </View>
                <Text style={styles.cardAddress}>{pedido.enderecoServico}</Text>
                <View style={styles.cardFooter}>
                  <Text style={[styles.status, concluido ? styles.statusDone : styles.statusPending]}>
                    {pedido.statusDescricao}
                  </Text>
                  <Text style={styles.valor}>{formatarMoeda(pedido.valorMontador)}</Text>
                </View>
              </TouchableOpacity>
            );
          })}
        </ScrollView>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#F5F7FA' },
  header: { flexDirection: 'row', alignItems: 'center', backgroundColor: colors.white, paddingHorizontal: 24, paddingTop: Platform.OS === 'ios' ? 60 : 40, paddingBottom: 20, borderBottomWidth: 1, borderBottomColor: colors.border },
  backButton: { marginRight: 16 },
  headerTitle: { fontSize: 20, fontWeight: 'bold', color: colors.primary },
  centered: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  emptyText: { fontSize: 16, color: '#718096' },
  list: { padding: 24 },
  card: { backgroundColor: colors.white, borderRadius: 12, padding: 16, marginBottom: 12, borderWidth: 1, borderColor: colors.border },
  cardHeader: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 6 },
  cardTitle: { fontSize: 16, fontWeight: 'bold', color: colors.primary },
  cardDate: { fontSize: 13, color: '#718096' },
  cardAddress: { fontSize: 14, color: '#4A5568', marginBottom: 10 },
  cardFooter: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  status: { fontSize: 13, fontWeight: '600' },
  statusDone: { color: '#38A169' },
  statusPending: { color: '#D69E2E' },
  valor: { fontSize: 15, fontWeight: 'bold', color: colors.primary },
});
