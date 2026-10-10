import React, { useState, useEffect, useContext } from 'react';
import { View, Text, TouchableOpacity, ScrollView, ActivityIndicator } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation, useIsFocused } from '@react-navigation/native';
import { styles } from './HomeMontadorStyles';
import { colors } from '../../theme/colors';
import { AuthContext } from '../../contexts/AuthContext';
import api from '../../services/api';
import { STATUS_PEDIDO, formatarMoeda } from '../../services/statusPedido';

// Serviços do montador que ainda exigem acompanhamento
const STATUS_ATIVOS = [
  STATUS_PEDIDO.AGUARDANDO_APROVACAO_CLIENTE,
  STATUS_PEDIDO.AGUARDANDO_PAGAMENTO,
  STATUS_PEDIDO.EM_ANDAMENTO,
];

export default function HomeMontadorScreen() {
  const navigation = useNavigation();
  const isFocused = useIsFocused();
  const { user } = useContext(AuthContext);

  const [pedidosDisponiveis, setPedidosDisponiveis] = useState([]);
  const [meusServicos, setMeusServicos] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchPedidos = async () => {
      try {
        setLoading(true);
        // A API já devolve só os pedidos abertos da região do montador (ex: "Lagoa Formosa - MG")
        const [disponiveis, meus] = await Promise.all([
          api.get('/pedidos/disponiveis', { params: { regiao: user?.regiao } }),
          api.get(`/pedidos/montador/${user.id}`),
        ]);
        setPedidosDisponiveis(disponiveis.data);
        setMeusServicos(meus.data.filter(p => STATUS_ATIVOS.includes(p.status)));

      } catch (error) {
        console.log('Erro ao buscar pedidos no montador:', error);
      } finally {
        setLoading(false);
      }
    };

    if (isFocused && user?.id) {
      fetchPedidos();
    }
  }, [isFocused, user?.id, user?.regiao]);

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <View>
          <Text style={styles.greeting}>Olá, {user?.nome || 'Montador'}!</Text>
          <Text style={styles.subGreeting}>Serviços em: {user?.regiao}</Text>
        </View>
        
        <View style={{ flexDirection: 'row', alignItems: 'center' }}>
          <TouchableOpacity onPress={() => navigation.navigate('HistoricoMontador')} style={{ marginRight: 16 }}>
            <Ionicons name="time-outline" size={28} color={colors.white} />
          </TouchableOpacity>
          <TouchableOpacity onPress={() => navigation.navigate('Perfil')}>
            <Ionicons name="person-circle" size={32} color={colors.white} />
          </TouchableOpacity>
        </View>
      </View>

      <ScrollView contentContainerStyle={styles.content}>
        {!loading && meusServicos.length > 0 && (
          <>
            <View style={styles.sectionHeader}>
              <Text style={styles.sectionTitle}>Meus Serviços</Text>
            </View>

            {meusServicos.map((pedido) => {
              const liberado = pedido.status === STATUS_PEDIDO.EM_ANDAMENTO;
              return (
                <View key={pedido.id} style={styles.orderCard}>
                  <View style={styles.orderHeader}>
                    <Text style={styles.orderTitle}>{pedido.tipoMovel}</Text>
                    <View style={[styles.distanceBadge, liberado && styles.badgeLiberado]}>
                      <Text style={[styles.distanceText, liberado && styles.badgeLiberadoText]}>{pedido.statusDescricao}</Text>
                    </View>
                  </View>

                  <View style={styles.orderDetailRow}>
                    <Ionicons name="location-outline" size={16} color="#718096" />
                    <Text style={styles.orderDetailText}>{pedido.enderecoServico}</Text>
                  </View>

                  <View style={styles.orderDetailRow}>
                    <Ionicons name="cash-outline" size={16} color="#718096" />
                    <Text style={styles.orderDetailText}>Você recebe {formatarMoeda(pedido.valorMontador)}</Text>
                  </View>

                  {liberado ? (
                    <TouchableOpacity
                      style={styles.actionButton}
                      activeOpacity={0.8}
                      onPress={() => navigation.navigate('ConcluirServico', { pedido })}
                    >
                      <Text style={styles.actionButtonText}>Finalizar com Foto</Text>
                    </TouchableOpacity>
                  ) : (
                    <Text style={styles.aguardandoText}>
                      {pedido.status === STATUS_PEDIDO.AGUARDANDO_PAGAMENTO
                        ? 'Proposta aceita! Aguarde a confirmação do pagamento.'
                        : 'Aguardando o cliente responder sua proposta.'}
                    </Text>
                  )}
                </View>
              );
            })}
          </>
        )}

        <View style={styles.sectionHeader}>
          <Text style={styles.sectionTitle}>Novas Solicitações</Text>
        </View>

        {loading ? (
          <ActivityIndicator size="large" color={colors.primary} style={styles.loader} />
        ) : pedidosDisponiveis.length === 0 ? (
          <Text style={styles.emptyText}>Nenhum serviço novo disponível na sua região.</Text>
        ) : (
          pedidosDisponiveis.map((pedido) => (
            <View key={pedido.id} style={styles.orderCard}>
              <View style={styles.orderHeader}>
                <Text style={styles.orderTitle}>{pedido.tipoMovel || 'Sem Tipo'}</Text>
                <View style={styles.distanceBadge}>
                  <Text style={styles.distanceText}>Nova</Text>
                </View>
              </View>

              <View style={styles.orderDetailRow}>
                <Ionicons name="resize-outline" size={16} color="#718096" />
                <Text style={styles.orderDetailText}>{pedido.tamanhoMovel || 'Tamanho não informado'}</Text>
              </View>

              <View style={styles.orderDetailRow}>
                <Ionicons name="location-outline" size={16} color="#718096" />
                <Text style={styles.orderDetailText}>{pedido.enderecoServico || 'Endereço não informado'}</Text>
              </View>

              <TouchableOpacity 
                style={styles.actionButton}
                activeOpacity={0.8}
                onPress={() => navigation.navigate('EnviarProposta', { pedido })}
              >
                <Text style={styles.actionButtonText}>Analisar e Enviar Proposta</Text>
              </TouchableOpacity>
            </View>
          ))
        )}
      </ScrollView>
    </View>
  );
}