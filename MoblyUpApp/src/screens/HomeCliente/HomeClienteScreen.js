import React, { useState, useContext, useCallback } from 'react';
import { View, Text, TouchableOpacity, ScrollView, ActivityIndicator, RefreshControl, Image } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation, useFocusEffect } from '@react-navigation/native';
import { styles } from './HomeClienteStyles';
import { colors } from '../../theme/colors';
import { AuthContext } from '../../contexts/AuthContext';
import api from '../../services/api'; // <-- Importando o Axios
import { STATUS_PEDIDO, isConcluido, formatarData } from '../../services/statusPedido';

// Quantos pedidos aparecem antes de tocar em "Ver todos"
const LIMITE_RECENTES = 5;

export default function HomeClienteScreen() {
  const navigation = useNavigation();
  const { user } = useContext(AuthContext);

  const [pedidos, setPedidos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [atualizando, setAtualizando] = useState(false);
  const [mostrarTodos, setMostrarTodos] = useState(false);

  const carregar = useCallback(async () => {
    if (!user?.id) return;
    try {
      // Busca apenas os pedidos desse cliente (já vêm do mais recente para o mais antigo)
      const response = await api.get('/pedidos/cliente');
      setPedidos(response.data);
    } catch (error) {
      console.log('Erro ao buscar pedidos no cliente:', error);
    } finally {
      setLoading(false);
      setAtualizando(false);
    }
  }, [user?.id]);

  useFocusEffect(useCallback(() => { carregar(); }, [carregar]));

  const handleAtualizar = () => {
    setAtualizando(true);
    carregar();
  };

  // Com proposta pendente abre a tela de propostas; nos demais casos, o acompanhamento do pedido
  const handleAbrirPedido = (pedido) => {
    if (pedido.status === STATUS_PEDIDO.AGUARDANDO_APROVACAO_CLIENTE) {
      navigation.navigate('VerPropostas', { pedido });
    } else {
      navigation.navigate('DetalhesPedido', { pedido });
    }
  };

  const pedidosVisiveis = mostrarTodos ? pedidos : pedidos.slice(0, LIMITE_RECENTES);
  const primeiroNome = (user?.nome || 'Cliente').split(' ')[0];

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <View style={{ flex: 1 }}>
          <Text style={styles.greeting}>Olá, {primeiroNome}!</Text>
          <Text style={styles.subGreeting}>O que vamos montar hoje?</Text>
        </View>
        <TouchableOpacity onPress={() => navigation.navigate('Perfil')}>
          {user?.fotoPerfilUrl ? (
            <Image source={{ uri: user.fotoPerfilUrl }} style={styles.headerAvatar} />
          ) : (
            <Ionicons name="person-circle" size={48} color={colors.primary} />
          )}
        </TouchableOpacity>
      </View>

      <ScrollView
        contentContainerStyle={styles.content}
        refreshControl={<RefreshControl refreshing={atualizando} onRefresh={handleAtualizar} />}
      >

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
          {pedidos.length > LIMITE_RECENTES && (
            <TouchableOpacity onPress={() => setMostrarTodos(!mostrarTodos)}>
              <Text style={styles.seeAll}>{mostrarTodos ? 'Ver menos' : `Ver todos (${pedidos.length})`}</Text>
            </TouchableOpacity>
          )}
        </View>

        {loading ? (
          <ActivityIndicator size="large" color={colors.primary} style={styles.loader} />
        ) : pedidos.length === 0 ? (
          <Text style={styles.emptyText}>Você ainda não possui pedidos recentes.</Text>
        ) : (
          pedidosVisiveis.map((pedido) => {
            const concluido = isConcluido(pedido);
            const cancelado = pedido.status === STATUS_PEDIDO.CANCELADO;
            const temProposta = pedido.status === STATUS_PEDIDO.AGUARDANDO_APROVACAO_CLIENTE;

            return (
              <TouchableOpacity
                key={pedido.id}
                style={[styles.orderCard, cancelado && styles.orderCardCancelado]}
                activeOpacity={0.7}
                onPress={() => handleAbrirPedido(pedido)}
              >
                <View style={styles.orderHeader}>
                  <Text style={styles.orderTitle}>{pedido.tipoMovel}</Text>
                  <Text style={styles.orderDate}>{formatarData(pedido.dataCriacao) || 'Recente'}</Text>
                </View>
                <Text style={styles.orderAddress}>{pedido.enderecoServico}</Text>
                <View style={[
                  concluido ? styles.statusContainerDone : styles.statusContainerPending,
                  cancelado && styles.statusContainerCancelado,
                  temProposta && styles.statusContainerDestaque,
                ]}>
                  <Text style={[
                    concluido ? styles.statusTextDone : styles.statusTextPending,
                    cancelado && styles.statusTextCancelado,
                    temProposta && styles.statusTextDestaque,
                  ]}>
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
