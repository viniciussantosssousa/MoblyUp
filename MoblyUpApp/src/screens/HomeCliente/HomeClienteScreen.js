import React, { useState, useEffect, useContext } from 'react';
import { View, Text, TouchableOpacity, ScrollView, ActivityIndicator } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation, useIsFocused } from '@react-navigation/native';
import { styles } from './HomeClienteStyles';
import { colors } from '../../theme/colors';
import { AuthContext } from '../../contexts/AuthContext'; 
import api from '../../services/api'; // <-- Importando o Axios
import { STATUS_PEDIDO, isConcluido, formatarData } from '../../services/statusPedido';

export default function HomeClienteScreen() {
  const navigation = useNavigation();
  const isFocused = useIsFocused();
  const { user } = useContext(AuthContext); 
  
  const [pedidos, setPedidos] = useState([]);
  const [loading, setLoading] = useState(true);

  const clienteIdLogado = user?.id; 

  useEffect(() => {
    const fetchPedidos = async () => {
      try {
        setLoading(true);
        // Busca apenas os pedidos desse cliente
        const response = await api.get('/pedidos/cliente');
        setPedidos(response.data);
        
      } catch (error) {
        console.log('Erro ao buscar pedidos no cliente:', error);
      } finally {
        setLoading(false);
      }
    };

    if (isFocused && clienteIdLogado) {
      fetchPedidos();
    }
  }, [isFocused, clienteIdLogado]);

  // Com proposta pendente abre a tela de propostas; nos demais casos, o acompanhamento do pedido
  const handleAbrirPedido = (pedido) => {
    if (pedido.status === STATUS_PEDIDO.AGUARDANDO_APROVACAO_CLIENTE) {
      navigation.navigate('VerPropostas', { pedido });
    } else {
      navigation.navigate('DetalhesPedido', { pedido });
    }
  };

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <View>
          <Text style={styles.greeting}>Olá, {user?.nome || 'Cliente'}!</Text>
          <Text style={styles.subGreeting}>O que vamos montar hoje?</Text>
        </View>
        <TouchableOpacity onPress={() => navigation.navigate('Perfil')}>
          <Ionicons name="person-circle" size={48} color={colors.primary} />
        </TouchableOpacity>
      </View>

      <ScrollView contentContainerStyle={styles.content}>
        
        <TouchableOpacity 
          style={styles.ctaCard} 
          activeOpacity={0.8}
          onPress={() => navigation.navigate('SolicitarServico')}
        >
          <View style={styles.ctaTextContainer}>
            <Text style={styles.ctaTitle}>Nova Montagem</Text>
            <Text style={styles.ctaSubtitle}>Encontre os melhores montadores da sua região agora mesmo.</Text>
          </View>
          <Ionicons name="add-circle" size={56} color={colors.white} />
        </TouchableOpacity>

        <TouchableOpacity style={styles.supportCard} activeOpacity={0.7}>
          <Ionicons name="chatbubbles" size={28} color={colors.primary} />
          <View style={styles.supportTextContainer}>
            <Text style={styles.supportTitle}>Central de Suporte</Text>
            <Text style={styles.supportSubtitle}>Precisa de ajuda com um pedido?</Text>
          </View>
        </TouchableOpacity>

        <View style={styles.sectionHeader}>
          <Text style={styles.sectionTitle}>Meus Pedidos</Text>
          {pedidos.length > 0 && (
            <TouchableOpacity>
              <Text style={styles.seeAll}>Ver todos</Text>
            </TouchableOpacity>
          )}
        </View>

        {loading ? (
          <ActivityIndicator size="large" color={colors.primary} style={styles.loader} />
        ) : pedidos.length === 0 ? (
          <Text style={styles.emptyText}>Você ainda não possui pedidos recentes.</Text>
        ) : (
          pedidos.map((pedido) => {
            const concluido = isConcluido(pedido);

            return (
              <TouchableOpacity 
                key={pedido.id}
                style={styles.orderCard} 
                activeOpacity={0.7}
                onPress={() => handleAbrirPedido(pedido)}
              >
                <View style={styles.orderHeader}>
                  <Text style={styles.orderTitle}>{pedido.tipoMovel}</Text>
                  <Text style={styles.orderDate}>{formatarData(pedido.dataCriacao) || 'Recente'}</Text>
                </View>
                <Text style={styles.orderAddress}>{pedido.enderecoServico}</Text>
                <View style={concluido ? styles.statusContainerDone : styles.statusContainerPending}>
                  <Text style={concluido ? styles.statusTextDone : styles.statusTextPending}>
                    {pedido.statusDescricao}
                  </Text>
                </View>
              </TouchableOpacity>
            );
          })
        )}
      </ScrollView>
    </View>
  );
}