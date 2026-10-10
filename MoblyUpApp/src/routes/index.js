import React, { useContext, useEffect, useState } from 'react';
import { View, ActivityIndicator, Platform } from 'react-native';
import { NavigationContainer, createNavigationContainerRef } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import * as Notifications from 'expo-notifications';

import { AuthContext } from '../contexts/AuthContext';

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
      const pedidoId = resposta?.notification?.request?.content?.data?.pedidoId;
      if (pedidoId) setPedidoDaNotificacao(pedidoId);
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

  // Abre a tela certa assim que o usuário estiver logado e a navegação pronta
  useEffect(() => {
    if (!pedidoDaNotificacao || !signed || !navegacaoPronta || cpfPendente) return;

    if (user?.tipo === 'cliente') {
      navigationRef.navigate('DetalhesPedido', { pedido: { id: pedidoDaNotificacao } });
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
            </>
          ) : (
            <>
              <Stack.Screen name="HomeMontador" component={HomeMontadorScreen} />
              <Stack.Screen name="HistoricoMontador" component={HistoricoMontadorScreen} />
              <Stack.Screen name="EnviarProposta" component={EnviarPropostaScreen} />
              <Stack.Screen name="ConcluirServico" component={ConcluirServicoScreen} />
              <Stack.Screen name="Perfil" component={PerfilScreen} />
            </>
          )
        )}

      </Stack.Navigator>
    </NavigationContainer>
  );
}