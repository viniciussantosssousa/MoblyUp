import React, { useState, useEffect, useContext } from 'react';
import { View, Text, TouchableOpacity, ScrollView, ActivityIndicator } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation, useIsFocused } from '@react-navigation/native';
import { styles } from './HomeMontadorStyles';
import { colors } from '../../theme/colors';
import { AuthContext } from '../../contexts/AuthContext';
import api from '../../services/api'; 

export default function HomeMontadorScreen() {
  const navigation = useNavigation();
  const isFocused = useIsFocused();
  const { user } = useContext(AuthContext);
  
  const [pedidosDisponiveis, setPedidosDisponiveis] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchPedidos = async () => {
      try {
        setLoading(true);
        const response = await api.get('/pedidos');
        
        // 1. Pega a região do montador logado (ex: "lagoa formosa - mg")
        const minhaRegiao = user?.regiao ? user.regiao.toLowerCase() : '';

        // 2. Filtra a lista que veio do banco de dados
        const pedidosFiltrados = response.data.filter(pedido => {
          const isPendente = pedido.status === 'Pendente' || pedido.status === 0 || pedido.status == null;
          
          // Pega o endereço do pedido (que agora tem a cidade no final)
          const enderecoPedido = pedido.enderecoServico ? pedido.enderecoServico.toLowerCase() : '';
          
          // Retorna apenas se for pendente E se a cidade do pedido bater com a cidade do montador
          return isPendente && enderecoPedido.includes(minhaRegiao);
        });
        
        setPedidosDisponiveis(pedidosFiltrados);
        
      } catch (error) {
        console.log('Erro ao buscar pedidos no montador:', error);
      } finally {
        setLoading(false);
      }
    };

    if (isFocused) {
      fetchPedidos();
    }
  }, [isFocused, user?.regiao]);

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