import { Alert, Platform } from 'react-native';

// Alert.alert não funciona no navegador, então usamos window.alert/confirm na web
export const avisar = (titulo, mensagem) => {
  if (Platform.OS === 'web') {
    window.alert(mensagem);
  } else {
    Alert.alert(titulo, mensagem);
  }
};

export const confirmar = (titulo, mensagem, textoConfirmar = 'Confirmar') => {
  if (Platform.OS === 'web') {
    return Promise.resolve(window.confirm(mensagem));
  }

  return new Promise((resolve) => {
    Alert.alert(titulo, mensagem, [
      { text: 'Cancelar', style: 'cancel', onPress: () => resolve(false) },
      { text: textoConfirmar, onPress: () => resolve(true) },
    ]);
  });
};
