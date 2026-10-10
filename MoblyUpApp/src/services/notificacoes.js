import { Platform } from 'react-native';
import * as Notifications from 'expo-notifications';
import Constants from 'expo-constants';
import api from './api';

// Mostra a notificação mesmo com o app aberto
Notifications.setNotificationHandler({
  handleNotification: async () => ({
    shouldShowBanner: true,
    shouldShowList: true,
    shouldPlaySound: true,
    shouldSetBadge: false,
  }),
});

let tokenAtual = null;

// Pede permissão, obtém o token do Expo Push e registra na API para o usuário logado.
// Nunca lança erro: sem notificação o app continua funcionando normalmente.
export const registrarNotificacoes = async () => {
  try {
    if (Platform.OS === 'web') return null;

    // Android 8+: a notificação precisa de um canal (o back-end envia channelId "default").
    // Android 13+: o pedido de permissão só aparece depois que existe um canal.
    if (Platform.OS === 'android') {
      await Notifications.setNotificationChannelAsync('default', {
        name: 'Pedidos',
        importance: Notifications.AndroidImportance.HIGH,
        vibrationPattern: [0, 250, 250, 250],
      });
    }

    const { status: atual } = await Notifications.getPermissionsAsync();
    const status = atual === 'granted' ? atual : (await Notifications.requestPermissionsAsync()).status;
    if (status !== 'granted') {
      console.log('Permissão de notificação negada pelo usuário.');
      return null;
    }

    // O projectId vem do EAS (npx eas-cli init grava em app.json > extra.eas.projectId)
    const projectId = Constants?.expoConfig?.extra?.eas?.projectId ?? Constants?.easConfig?.projectId;
    if (!projectId) {
      console.log('Notificações push desativadas: projeto EAS não configurado (rode "npx eas-cli init").');
      return null;
    }

    const { data: token } = await Notifications.getExpoPushTokenAsync({ projectId });
    await api.put('/usuarios/me/push-token', { token });
    tokenAtual = token;
    return token;
  } catch (error) {
    console.log('Não foi possível ativar as notificações push:', error);
    return null;
  }
};

// No logout o aparelho deixa de receber as notificações dessa conta
export const removerNotificacoes = async () => {
  if (!tokenAtual) return;
  try {
    await api.delete('/usuarios/me/push-token');
  } catch (error) {
    console.log('Erro ao remover o token de notificação:', error);
  } finally {
    tokenAtual = null;
  }
};
