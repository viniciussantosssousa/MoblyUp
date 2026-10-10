import React, { useState, useEffect, useContext, useCallback } from 'react';
import { View, Text, TouchableOpacity, ScrollView, Image } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation, useRoute, useIsFocused } from '@react-navigation/native';
import { styles } from './DetalhesPedidoStyles';
import { colors } from '../../theme/colors';
import { AuthContext } from '../../contexts/AuthContext';
import api, { mensagemDeErro } from '../../services/api';
import { avisar, confirmar } from '../../services/alerta';
import { STATUS_PEDIDO, formatarData, formatarMoeda } from '../../services/statusPedido';

// Texto e ícone exibidos para cada etapa da jornada do serviço
const ETAPAS = {
  [STATUS_PEDIDO.DISPONIVEL]: { icone: 'time', cor: '#D69E2E', titulo: 'Aguardando Propostas', texto: 'Seu pedido está visível para os montadores da região.' },
  [STATUS_PEDIDO.AGUARDANDO_APROVACAO_CLIENTE]: { icone: 'mail-unread', cor: '#3182CE', titulo: 'Proposta Recebida', texto: 'Um montador enviou uma proposta. Analise e responda.' },
  [STATUS_PEDIDO.AGUARDANDO_PAGAMENTO]: { icone: 'card', cor: '#D69E2E', titulo: 'Aguardando Pagamento', texto: 'Confirme o pagamento via PIX para liberar o montador.' },
  [STATUS_PEDIDO.EM_ANDAMENTO]: { icone: 'construct', cor: '#3182CE', titulo: 'Em Andamento', texto: 'Pagamento confirmado. O montador foi liberado para realizar o serviço.' },
  [STATUS_PEDIDO.CONCLUIDO]: { icone: 'checkmark-circle', cor: '#38A169', titulo: 'Serviço Concluído', texto: '' },
  [STATUS_PEDIDO.CANCELADO]: { icone: 'close-circle', cor: '#E53E3E', titulo: 'Pedido Cancelado', texto: 'Este pedido foi cancelado.' },
};

export default function DetalhesPedidoScreen() {
  const navigation = useNavigation();
  const route = useRoute();
  const isFocused = useIsFocused();
  const { user } = useContext(AuthContext);

  const pedidoId = route.params?.pedido?.id;

  // Mostra o que veio da lista e atualiza com os dados mais recentes da API
  const [pedidoData, setPedidoData] = useState(route.params?.pedido || {});
  const [processando, setProcessando] = useState(false);

  const carregarPedido = useCallback(async () => {
    try {
      const response = await api.get(`/pedidos/${pedidoId}`);
      setPedidoData(response.data);
    } catch (error) {
      console.log('Erro ao buscar detalhes do pedido:', error);
    }
  }, [pedidoId]);

  useEffect(() => {
    if (pedidoId && isFocused) carregarPedido();
  }, [pedidoId, isFocused, carregarPedido]);

  const handleConfirmarPagamento = async () => {
    const ok = await confirmar(
      'Pagamento via PIX',
      `Confirmar o pagamento de ${formatarMoeda(pedidoData.valorProposto)} para ${pedidoData.nomeMontador}?`,
      'Confirmar'
    );
    if (!ok) return;

    try {
      setProcessando(true);
      await api.put(`/pedidos/${pedidoId}/confirmar-pagamento`, { clienteId: user.id });
      avisar('Pagamento confirmado!', 'O montador foi liberado e irá até o endereço cadastrado.');
      await carregarPedido();
    } catch (error) {
      avisar('Erro', mensagemDeErro(error, 'Não foi possível confirmar o pagamento.'));
    } finally {
      setProcessando(false);
    }
  };

  const status = pedidoData.status;
  const etapa = ETAPAS[status] || ETAPAS[STATUS_PEDIDO.DISPONIVEL];
  const concluido = status === STATUS_PEDIDO.CONCLUIDO;
  const textoEtapa = concluido
    ? `A montagem foi finalizada com sucesso em ${formatarData(pedidoData.dataConclusao)}.`
    : etapa.texto;

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <Ionicons name="arrow-back" size={28} color={colors.primary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Detalhes do Pedido</Text>
      </View>

      <ScrollView contentContainerStyle={styles.content}>

        <View style={styles.statusCard}>
          <Ionicons name={etapa.icone} size={48} color={etapa.cor} style={styles.statusIcon} />
          <Text style={[styles.statusTitlePending, { color: etapa.cor }]}>{etapa.titulo}</Text>
          <Text style={styles.statusSubtitle}>{textoEtapa}</Text>

          {status === STATUS_PEDIDO.AGUARDANDO_APROVACAO_CLIENTE && (
            <TouchableOpacity style={styles.actionButton} onPress={() => navigation.navigate('VerPropostas', { pedido: pedidoData })}>
              <Text style={styles.actionButtonText}>Ver Proposta</Text>
            </TouchableOpacity>
          )}

          {status === STATUS_PEDIDO.AGUARDANDO_PAGAMENTO && (
            <TouchableOpacity
              style={[styles.actionButton, processando && { opacity: 0.6 }]}
              disabled={processando}
              onPress={handleConfirmarPagamento}
            >
              <Text style={styles.actionButtonText}>Pagar {formatarMoeda(pedidoData.valorProposto)} via PIX</Text>
            </TouchableOpacity>
          )}
        </View>

        {concluido && pedidoData.fotoConclusaoUrl ? (
          <View style={styles.sectionCard}>
            <Text style={styles.sectionTitle}>Foto da Montagem Concluída</Text>
            <Image source={{ uri: pedidoData.fotoConclusaoUrl }} style={styles.foto} />
          </View>
        ) : null}

        <View style={styles.sectionCard}>
          <Text style={styles.sectionTitle}>Informações do Móvel</Text>

          <View style={styles.infoRow}>
            <Text style={styles.infoLabel}>Tipo</Text>
            <Text style={styles.infoValue}>{pedidoData.tipoMovel}</Text>
          </View>
          <View style={styles.infoRow}>
            <Text style={styles.infoLabel}>Tamanho</Text>
            <Text style={styles.infoValue}>{pedidoData.tamanhoMovel || 'Não informado'}</Text>
          </View>
          <View style={styles.infoRow}>
            <Text style={styles.infoLabel}>Endereço</Text>
            <Text style={styles.infoValue}>{pedidoData.enderecoServico}</Text>
          </View>
          {pedidoData.nomeMontador ? (
            <View style={styles.infoRow}>
              <Text style={styles.infoLabel}>Montador</Text>
              <Text style={styles.infoValue}>{pedidoData.nomeMontador}</Text>
            </View>
          ) : null}
          {pedidoData.urlFotoMovelTemporaria ? (
            <Image source={{ uri: pedidoData.urlFotoMovelTemporaria }} style={styles.foto} />
          ) : null}
        </View>

        <View style={styles.sectionCard}>
          <Text style={styles.sectionTitle}>Resumo Financeiro</Text>

          <View style={styles.infoRow}>
            <Text style={styles.infoLabel}>Forma de Pagamento</Text>
            <Text style={styles.infoValue}>PIX</Text>
          </View>

          <View style={styles.divider} />

          <View style={styles.priceRow}>
            <Text style={styles.priceLabel}>
              {status >= STATUS_PEDIDO.EM_ANDAMENTO ? 'Total Pago' : 'Valor Proposto'}
            </Text>
            <Text style={status >= STATUS_PEDIDO.EM_ANDAMENTO ? styles.priceValueDone : styles.priceValuePending}>
              {formatarMoeda(pedidoData.valorProposto)}
            </Text>
          </View>
        </View>

      </ScrollView>
    </View>
  );
}
