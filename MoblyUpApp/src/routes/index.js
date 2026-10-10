import React, { useContext } from 'react';
import { View, ActivityIndicator } from 'react-native';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';

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

// Telas Comuns
import PerfilScreen from '../screens/Perfil/PerfilScreen';

const Stack = createNativeStackNavigator();

export default function Routes() {
  const { signed, user, loading } = useContext(AuthContext);

  if (loading) {
    return (
      <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
        <ActivityIndicator size="large" color="#3182CE" />
      </View>
    );
  }

  return (
    <NavigationContainer>
      <Stack.Navigator screenOptions={{ headerShown: false }}>
        
        {!signed ? (
          <>
            <Stack.Screen name="Login" component={LoginScreen} />
            <Stack.Screen name="Cadastro" component={CadastroScreen} />
          </>
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
              <Stack.Screen name="Perfil" component={PerfilScreen} />
            </>
          )
        )}

      </Stack.Navigator>
    </NavigationContainer>
  );
}