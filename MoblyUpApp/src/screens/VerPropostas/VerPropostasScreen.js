import React, { useState, useEffect, useContext, useCallback } from 'react';
import { View, Text, TouchableOpacity, ScrollView, ActivityIndicator } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation, useRoute } from '@react-navigation/native';
import { styles } from './VerPropostasStyles';
import { colors } from '../../theme/colors';
import { AuthContext } from '../../contexts/AuthContext';
import api, { mensagemDeErro } from '../../services/api';
import { avisar, confirmar } from '../../services/alerta';
import { STATUS_PEDIDO, formatarMoeda } from '../../services/statusPedido';

export default function VerPropostasScreen() {
  const navigation = useNavigation();
  const route = useRoute();
  const { user } = useContext(AuthContext);

  const pedidoId = route.params?.pedido?.id;

  const [pedido, setPedido] = useState(route.params?.pedido || {});
  const [loading, setLoading] = useState(true);
  const [processando, setProcessando] = useState(false);

  const carregarPedido = useCallback(async () => {
    try {
      setLoading(true);
      const response = await api.get(`/pedidos/${pedidoId}`);
      setPedido(response.data);

      // Depois do aceite, o acompanhamento (pagamento, execução) fica na tela de detalhes
      if (response.data.status > STATUS_PEDIDO.AGUARDANDO_APROVACAO_CLIENTE) {
        navigation.replace('DetalhesPedido', { pedido: response.data });
      }
    } catch (error) {
      avisar('Erro', mensagemDeErro(error, 'Não foi possível carregar o pedido.'));
    } finally {
      setLoading(false);
    }
  }, [pedidoId, navigation]);

  useEffect(() => {
    if (pedidoId) carregarPedido();
  }, [pedidoId, carregarPedido]);

  const handleAceitar = async () => {
    const ok = await confirmar(
      'Aceitar proposta',
      `Aceitar a proposta de ${formatarMoeda(pedido.valorProposto)} de ${pedido.nomeMontador}?`,
      'Aceitar'
    );
    if (!ok) return;

    try {
      setProcessando(true);
      await api.put(`/pedidos/${pedidoId}/aceitar-proposta`, { clienteId: user.id });
      avisar('Proposta aceita!', 'Agora realize o pagamento para liberar o montador.');
      await carregarPedido();
    } catch (error) {
      avisar('Erro', mensagemDeErro(error, 'Não foi possível aceitar a proposta no momento.'));
    } finally {
      setProcessando(false);
    }
  };

  const handleRecusar = async () => {
    const ok = await confirmar(
      'Recusar proposta',
      'O pedido voltará a ficar disponível para outros montadores. Deseja recusar?',
      'Recusar'
    );
    if (!ok) return;

    try {
      setProcessando(true);
      await api.put(`/pedidos/${pedidoId}/recusar-proposta`, { clienteId: user.id });
      avisar('Proposta recusada', 'Seu pedido voltou a ficar visível para os montadores da região.');
      await carregarPedido();
    } catch (error) {
      avisar('Erro', mensagemDeErro(error, 'Não foi possível recusar a proposta no momento.'));
    } finally {
      setProcessando(false);
    }
  };

  const temProposta = pedido.status === STATUS_PEDIDO.AGUARDANDO_APROVACAO_CLIENTE;

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <Ionicons name="arrow-back" size={28} color={colors.primary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Propostas Recebidas</Text>
      </View>

      <ScrollView contentContainerStyle={styles.content}>
        <Text style={styles.sectionTitle}>{pedido.tipoMovel} {pedido.tamanhoMovel ? `· ${pedido.tamanhoMovel}` : ''}</Text>

        {loading ? (
          <ActivityIndicator size="large" color={colors.primary} style={styles.loader} />
        ) : !temProposta ? (
          <View>
            <Text style={styles.emptyText}>Nenhum montador enviou proposta para este pedido ainda. Aguarde mais um pouco.</Text>
            <TouchableOpacity style={styles.refreshButton} onPress={carregarPedido}>
              <Ionicons name="refresh" size={18} color={colors.primary} />
              <Text style={styles.refreshButtonText}>Atualizar</Text>
            </TouchableOpacity>
          </View>
        ) : (
          <View style={styles.propostaCard}>
            <View style={styles.montadorInfoRow}>
              <Ionicons name="person-circle-outline" size={32} color={colors.primary} />
              <Text style={styles.montadorNome}>{pedido.nomeMontador || 'Montador'}</Text>
            </View>

            <Text style={styles.valorProposta}>{formatarMoeda(pedido.valorProposto)}</Text>
            <Text style={styles.pagamentoInfo}>Pagamento via PIX após o aceite</Text>

            <TouchableOpacity
              style={[styles.acceptButton, processando && { opacity: 0.6 }]}
              activeOpacity={0.8}
              disabled={processando}
              onPress={handleAceitar}
            >
              <Text style={styles.acceptButtonText}>Aceitar Montador</Text>
            </TouchableOpacity>

            <TouchableOpacity
              style={styles.rejectButton}
              activeOpacity={0.8}
              disabled={processando}
              onPress={handleRecusar}
            >
              <Text style={styles.rejectButtonText}>Recusar Proposta</Text>
            </TouchableOpacity>
          </View>
        )}
      </ScrollView>
    </View>
  );
}
