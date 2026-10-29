import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, TouchableOpacity, ScrollView, ActivityIndicator } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation, useRoute } from '@react-navigation/native';
import { styles } from './VerPropostasStyles';
import { colors } from '../../theme/colors';
import api, { mensagemDeErro } from '../../services/api';
import { avisar, confirmar } from '../../services/alerta';
import { STATUS_PEDIDO, formatarMoeda } from '../../services/statusPedido';
import { ResumoReputacao } from '../../components/Reputacao';
import useAtualizacaoAoVivo from '../../hooks/useAtualizacaoAoVivo';
import { descreverDataServico } from '../../services/datas';

export default function VerPropostasScreen() {
  const navigation = useNavigation();
  const route = useRoute();

  const pedidoId = route.params?.pedido?.id;

  const [pedido, setPedido] = useState(route.params?.pedido || {});
  const [loading, setLoading] = useState(true);
  const [processando, setProcessando] = useState(false);

  // O indicador de carregamento só aparece na primeira vez (as atualizações ao vivo são discretas)
  const carregarPedido = useCallback(async () => {
    try {
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

  // A proposta aparece aqui no instante em que o montador envia (sem tocar em "Atualizar")
  useAtualizacaoAoVivo(carregarPedido, pedidoId);

  const handleAceitar = async () => {
    const ok = await confirmar(
      'Aceitar proposta',
      `Aceitar a proposta de ${formatarMoeda(pedido.valorProposto)} de ${pedido.nomeMontador}?`,
      'Aceitar'
    );
    if (!ok) return;

    try {
      setProcessando(true);
      await api.put(`/pedidos/${pedidoId}/aceitar-proposta`);
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
      await api.put(`/pedidos/${pedidoId}/recusar-proposta`);
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
              <View style={{ flex: 1 }}>
                <Text style={styles.montadorNome}>{pedido.nomeMontador || 'Montador'}</Text>
                <ResumoReputacao
                  style={styles.reputacaoRow}
                  media={pedido.mediaAvaliacaoMontador}
                  total={pedido.totalAvaliacoesMontador}
                  rotuloLink="Ver perfil e avaliações"
                  onPress={() => navigation.navigate('PerfilPublico', { usuarioId: pedido.montadorId })}
                />
              </View>
            </View>

            <Text style={styles.valorProposta}>{formatarMoeda(pedido.valorProposto)}</Text>
            <Text style={styles.pagamentoInfo}>Pagamento via PIX após o aceite</Text>

            {/* Dia: o montador pode ter concordado com o dia pedido ou sugerido outro */}
            {pedido.propostaMudaData ? (
              <View style={[styles.dataCaixa, styles.dataCaixaMudou]}>
                <Ionicons name="calendar" size={18} color="#B7791F" />
                <Text style={styles.dataTexto}>
                  Sugeriu outro dia: <Text style={styles.dataDestaque}>{descreverDataServico(pedido.dataProposta)}</Text>
                  {'\n'}Você pediu {descreverDataServico(pedido.dataDesejada)}. Aceitando, o serviço fica para o dia sugerido.
                </Text>
              </View>
            ) : (
              <View style={styles.dataCaixa}>
                <Ionicons name="calendar" size={18} color="#2F855A" />
                <Text style={styles.dataTexto}>
                  Pode no dia que você pediu: <Text style={styles.dataDestaque}>{descreverDataServico(pedido.dataProposta || pedido.dataDesejada)}</Text>
                </Text>
              </View>
            )}
            <Text style={styles.pagamentoInfo}>O horário vocês combinam pelo chat depois do aceite</Text>

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
