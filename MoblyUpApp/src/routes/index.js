import React, { useContext, useEffect, useState } from 'react';
import { View, ActivityIndicator, Platform } from 'react-native';
import { NavigationContainer, createNavigationContainerRef } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import * as Notifications from 'expo-notifications';

import { AuthContext } from '../contexts/AuthContext';
import { conectarTempoReal, desconectarTempoReal } from '../services/tempoReal';
import AvisoTopo from '../components/AvisoTopo';

// Telas Deslogadas
import LoginScreen from '../screens/Login/LoginScreen';
import CadastroScreen from '../screens/Cadastro/CadastroScreen';

// Telas do Cliente
import HomeClienteScreen from '../screens/HomeCliente/HomeClienteScreen';
import SolicitarServicoScreen from '../screens/SolicitarServico/SolicitarServicoScreen';
import DetalhesPedidoScreen from '../screens/DetalhesPedido/DetalhesPedidoScreen';
import VerPropostasScreen from '../screens/VerPropostas/VerPropostasScreen';

// Telas do Montador
import HomeMontadorScreen from '../screens/HomeMontador/HomeMontadorScreen';
import HistoricoMontadorScreen from '../screens/HistoricoMontador/HistoricoMontadorScreen';
import EnviarPropostaScreen from '../screens/EnviarProposta/EnviarPropostaScreen';
import ConcluirServicoScreen from '../screens/ConcluirServico/ConcluirServicoScreen';

// Telas Comuns
import PerfilScreen from '../screens/Perfil/PerfilScreen';
import CompletarCadastroScreen from '../screens/CompletarCadastro/CompletarCadastroScreen';
import PerfilPublicoScreen from '../screens/PerfilPublico/PerfilPublicoScreen';
import AvaliarClienteScreen from '../screens/AvaliarCliente/AvaliarClienteScreen';
import ChatScreen from '../screens/Chat/ChatScreen';

const Stack = createNativeStackNavigator();
const navigationRef = createNavigationContainerRef();

export default function Routes() {
  const { signed, user, loading } = useContext(AuthContext);
  const [navegacaoPronta, setNavegacaoPronta] = useState(false);
  const [pedidoDaNotificacao, setPedidoDaNotificacao] = useState(null);

  // Guarda o pedido da notificação tocada (com o app aberto ou abrindo o app por ela)
  useEffect(() => {
    if (Platform.OS === 'web') return;

    const tratarResposta = (resposta) => {
      const dados = resposta?.notification?.request?.content?.data;
      if (dados?.pedidoId) setPedidoDaNotificacao({ pedidoId: dados.pedidoId, tipo: dados.tipo });
    };

    Notifications.getLastNotificationResponseAsync().then((resposta) => {
      tratarResposta(resposta);
      Notifications.clearLastNotificationResponseAsync();
    });

    const subscription = Notifications.addNotificationResponseReceivedListener(tratarResposta);
    return () => subscription.remove();
  }, []);

  // Conta antiga sem CPF: só libera o app depois de completar o cadastro.
  // "undefined" (API ainda não respondeu) não bloqueia; só o "false" confirmado pela API.
  const cpfPendente = signed && user?.cpfCadastrado === false;

  // Tempo real: conecta enquanto o usuário estiver logado. Reconecta se a região mudar
  // (o montador passa a receber os pedidos da nova cidade).
  useEffect(() => {
    if (!signed || !user?.id || cpfPendente) return undefined;
    conectarTempoReal();
    return () => { desconectarTempoReal(); };
  }, [signed, user?.id, user?.regiao, cpfPendente]);

  // Abre a tela certa assim que o usuário estiver logado e a navegação pronta
  useEffect(() => {
    if (!pedidoDaNotificacao || !signed || !navegacaoPronta || cpfPendente) return;

    const { pedidoId, tipo } = pedidoDaNotificacao;
    if (tipo === 'chat') {
      // Mensagem nova: abre direto a conversa
      navigationRef.navigate('Chat', { pedidoId });
    } else if (user?.tipo === 'cliente') {
      navigationRef.navigate('DetalhesPedido', { pedido: { id: pedidoId } });
    } else {
      // O montador acompanha propostas e serviços liberados na tela inicial
      navigationRef.navigate('HomeMontador');
    }
    setPedidoDaNotificacao(null);
  }, [pedidoDaNotificacao, signed, navegacaoPronta, cpfPendente, user?.tipo]);

  if (loading) {
    return (
      <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
        <ActivityIndicator size="large" color="#3182CE" />
      </View>
    );
  }

  return (
    <NavigationContainer ref={navigationRef} onReady={() => setNavegacaoPronta(true)}>
      <Stack.Navigator screenOptions={{ headerShown: false }}>
        
        {!signed ? (
          <>
            <Stack.Screen name="Login" component={LoginScreen} />
            <Stack.Screen name="Cadastro" component={CadastroScreen} />
          </>
        ) : cpfPendente ? (
          <Stack.Screen name="CompletarCadastro" component={CompletarCadastroScreen} />
        ) : (
          user?.tipo === 'cliente' ? (
            <>
              <Stack.Screen name="HomeCliente" component={HomeClienteScreen} />
              <Stack.Screen name="SolicitarServico" component={SolicitarServicoScreen} />
              <Stack.Screen name="DetalhesPedido" component={DetalhesPedidoScreen} />
              <Stack.Screen name="VerPropostas" component={VerPropostasScreen} />
              <Stack.Screen name="Perfil" component={PerfilScreen} />
              <Stack.Screen name="PerfilPublico" component={PerfilPublicoScreen} />
              <Stack.Screen name="Chat" component={ChatScreen} />
            </>
          ) : (
            <>
              <Stack.Screen name="HomeMontador" component={HomeMontadorScreen} />
              <Stack.Screen name="HistoricoMontador" component={HistoricoMontadorScreen} />
              <Stack.Screen name="EnviarProposta" component={EnviarPropostaScreen} />
              <Stack.Screen name="ConcluirServico" component={ConcluirServicoScreen} />
              <Stack.Screen name="AvaliarCliente" component={AvaliarClienteScreen} />
              <Stack.Screen name="Perfil" component={PerfilScreen} />
              <Stack.Screen name="PerfilPublico" component={PerfilPublicoScreen} />
              <Stack.Screen name="Chat" component={ChatScreen} />
            </>
          )
        )}

      </Stack.Navigator>

      {/* Avisos em tempo real; tocar abre o pedido como na notificação push */}
      {signed && !cpfPendente && (
        <AvisoTopo onAbrir={(aviso) => aviso?.pedidoId && setPedidoDaNotificacao({ pedidoId: aviso.pedidoId, tipo: aviso.tipo })} />
      )}
    </NavigationContainer>
  );
}