import axios from 'axios';
import { Platform } from 'react-native';

// No celular, "localhost" é o próprio aparelho. Defina EXPO_PUBLIC_API_URL no arquivo .env
// com o IP do computador que roda a API (veja .env.example).
export const API_URL = process.env.EXPO_PUBLIC_API_URL || 'http://localhost:5007/api';

const api = axios.create({
  baseURL: API_URL,
  timeout: 10000,
});

// Extrai a mensagem de erro enviada pela API ({ erro: "..." }) ou usa a mensagem padrão
export const mensagemDeErro = (error, padrao = 'Não foi possível conectar com o servidor.') =>
  error?.response?.data?.erro || padrao;

// Envia uma foto (URI local da câmera/galeria) para a API e devolve a URL pública
export const enviarImagem = async (uri) => {
  const formData = new FormData();

  if (Platform.OS === 'web') {
    const blob = await (await fetch(uri)).blob();
    formData.append('arquivo', blob, 'foto.jpg');
  } else {
    formData.append('arquivo', { uri, name: 'foto.jpg', type: 'image/jpeg' });
  }

  const response = await fetch(`${API_URL}/uploads`, { method: 'POST', body: formData });
  const data = await response.json();

  if (!response.ok) {
    throw { response: { data } };
  }

  return data.url;
};

export default api;
