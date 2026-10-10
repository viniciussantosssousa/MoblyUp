import React, { useState, useContext, useCallback } from 'react';
import { View, Text, TouchableOpacity, ScrollView, ActivityIndicator, RefreshControl } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation, useFocusEffect } from '@react-navigation/native';
import { styles } from './HomeMontadorStyles';
import { colors } from '../../theme/colors';
import { AuthContext } from '../../contexts/AuthContext';
import api, { mensagemDeErro } from '../../services/api';
import { avisar, confirmar } from '../../services/alerta';
import { STATUS_PEDIDO, formatarMoeda } from '../../services/statusPedido';

// Serviços do montador que ainda exigem acompanhamento
const STATUS_ATIVOS = [
  STATUS_PEDIDO.AGUARDANDO_APROVACAO_CLIENTE,
  STATUS_PEDIDO.AGUARDANDO_PAGAMENTO,
  STATUS_PEDIDO.EM_ANDAMENTO,
];

export default function HomeMontadorScreen() {
  const navigation = useNavigation();
  const { user } = useContext(AuthContext);

  const [pedidosDisponiveis, setPedidosDisponiveis] = useState([]);
  const [meusServicos, setMeusServicos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [atualizando, setAtualizando] = useState(false);
  const [processandoId, setProcessandoId] = useState(null);

  const carregar = useCallback(async () => {
    if (!user?.id) return;
    try {
      // A API já devolve só os pedidos abertos da região do montador (ex: "Lagoa Formosa - MG"),
      // sem os que ele recusou
      const [disponiveis, meus] = await Promise.all([
        api.get('/pedidos/disponiveis', { params: { regiao: user?.regiao } }),
        api.get('/pedidos/montador'),
      ]);
      setPedidosDisponiveis(disponiveis.data);
      setMeusServicos(meus.data.filter(p => STATUS_ATIVOS.includes(p.status)));
    } catch (error) {
      console.log('Erro ao buscar pedidos no montador:', error);
    } finally {
      setLoading(false);
      setAtualizando(false);
    }
  }, [user?.id, user?.regiao]);

  useFocusEffect(useCallback(() => { carregar(); }, [carregar]));

  const handleAtualizar = () => {
    setAtualizando(true);
    carregar();
  };

  // Negar um pedido do feed: some só para este montador e continua aberto para os outros
  const handleRecusar = async (pedido) => {
    const ok = await confirmar(
      'Recusar serviço',
      `Não tem interesse em montar ${pedido.tipoMovel}? O pedido deixará de aparecer para você.`,
      'Recusar'
    );
    if (!ok) return;

    try {
      setProcessandoId(pedido.id);
      await api.put(`/pedidos/${pedido.id}/recusar`);
      setPedidosDisponiveis(atual => atual.filter(p => p.id !== pedido.id));
    } catch (error) {
      avisar('Erro', mensagemDeErro(error, 'Não foi possível recusar o pedido.'));
      carregar();
    } finally {
      setProcessandoId(null);
    }
  };

  // Desistir de um serviço já proposto/aceito (somente antes do pagamento)
  const handleDesistir = async (pedido) => {
    const ok = await confirmar(
      'Desistir do serviço',
      'O cliente será avisado e o pedido voltará a ficar disponível para outros montadores. Deseja desistir?',
      'Desistir'
    );
    if (!ok) return;

    try {
      setProcessandoId(pedido.id);
      await api.put(`/pedidos/${pedido.id}/desistir`);
      setMeusServicos(atual => atual.filter(p => p.id !== pedido.id));
      avisar('Pronto', 'Você desistiu deste serviço. O cliente foi avisado.');
    } catch (error) {
      avisar('Erro', mensagemDeErro(error, 'Não foi possível desistir do serviço.'));
      carregar();
    } finally {
      setProcessandoId(null);
    }
  };

  const primeiroNome = (user?.nome || 'Montador').split(' ')[0];

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <View style={{ flex: 1 }}>
          <Text style={styles.greeting}>Olá, {primeiroNome}!</Text>
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

      <ScrollView
        contentContainerStyle={styles.content}
        refreshControl={<RefreshControl refreshing={atualizando} onRefresh={handleAtualizar} />}
      >
        {!loading && meusServicos.length > 0 && (
          <>
            <View style={styles.sectionHeader}>
              <Text style={styles.sectionTitle}>Meus Serviços</Text>
            </View>

            {meusServicos.map((pedido) => {
              const liberado = pedido.status === STATUS_PEDIDO.EM_ANDAMENTO;
              const processando = processandoId === pedido.id;
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
                    <>
                      <Text style={styles.aguardandoText}>
                        {pedido.status === STATUS_PEDIDO.AGUARDANDO_PAGAMENTO
                          ? 'Proposta aceita! Aguarde a confirmação do pagamento.'
                          : 'Aguardando o cliente responder sua proposta.'}
                      </Text>
                      <TouchableOpacity
                        style={[styles.declineButton, processando && styles.buttonDisabled]}
                        disabled={processando}
                        onPress={() => handleDesistir(pedido)}
                      >
                        <Text style={styles.declineButtonText}>{processando ? 'Aguarde...' : 'Desistir do serviço'}</Text>
                      </TouchableOpacity>
                    </>
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
          <Text style={styles.emptyText}>Nenhum serviço novo disponível na sua região. Puxe a tela para baixo para atualizar.</Text>
        ) : (
          pedidosDisponiveis.map((pedido) => {
            const processando = processandoId === pedido.id;
            return (
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

                <View style={styles.cardActions}>
                  <TouchableOpacity
                    style={[styles.declineButton, styles.cardActionLeft, processando && styles.buttonDisabled]}
                    disabled={processando}
                    onPress={() => handleRecusar(pedido)}
                  >
                    <Text style={styles.declineButtonText}>{processando ? 'Aguarde...' : 'Recusar'}</Text>
                  </TouchableOpacity>
                  <TouchableOpacity
                    style={[styles.actionButton, styles.cardActionRight]}
                    activeOpacity={0.8}
                    disabled={processando}
                    onPress={() => navigation.navigate('EnviarProposta', { pedido })}
                  >
                    <Text style={styles.actionButtonText}>Enviar Proposta</Text>
                  </TouchableOpacity>
                </View>
              </View>
            );
          })
        )}
      </ScrollView>
    </View>
  );
}
