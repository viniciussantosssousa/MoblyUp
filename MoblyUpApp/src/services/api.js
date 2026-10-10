import axios from 'axios';
import { Platform } from 'react-native';
import { auth } from './firebaseConfig';

// No celular, "localhost" é o próprio aparelho. Defina EXPO_PUBLIC_API_URL no arquivo .env
// com o IP do computador que roda a API (veja .env.example).
export const API_URL = process.env.EXPO_PUBLIC_API_URL || 'http://localhost:5007/api';

const api = axios.create({
  baseURL: API_URL,
  timeout: 10000,
});

// MODO DE TESTE (só em desenvolvimento): entra com usuários de teste sem login no Firebase.
// A API só aceita com ASPNETCORE_ENVIRONMENT=Development e LoginDeTeste:Habilitado=true.
// Para esconder os botões no app, defina EXPO_PUBLIC_LOGIN_TESTE=false no .env.
export const LOGIN_TESTE_ATIVO = __DEV__ && process.env.EXPO_PUBLIC_LOGIN_TESTE !== 'false';

let usuarioDeTeste = null;
export const definirUsuarioDeTeste = (id) => {
  usuarioDeTeste = id;
};
export const obterUsuarioDeTeste = () => usuarioDeTeste;

// A API identifica o usuário pelo ID token do Firebase. O getIdToken renova
// o token automaticamente quando ele expira (a cada 1 hora).
const obterCabecalhoAuth = async () => {
  if (usuarioDeTeste) return { 'X-Usuario-Teste': usuarioDeTeste };

  const token = await auth.currentUser?.getIdToken();
  return token ? { Authorization: `Bearer ${token}` } : {};
};

api.interceptors.request.use(async (config) => {
  Object.assign(config.headers, await obterCabecalhoAuth());
  return config;
});

// Extrai a mensagem de erro enviada pela API ou usa a mensagem padrão.
// A API responde { erro: "..." }; erros de validação do ASP.NET vêm como { errors: { Campo: [...] } }.
export const mensagemDeErro = (error, padrao = 'Não foi possível conectar com o servidor.') => {
  const data = error?.response?.data;
  if (data?.erro) return data.erro;

  const errosValidacao = data?.errors && Object.values(data.errors).flat();
  if (errosValidacao?.length) return `Dados inválidos: ${errosValidacao.join(' ')}`;

  return padrao;
};

// Envia uma foto (URI local da câmera/galeria) para a API e devolve a URL pública
export const enviarImagem = async (uri) => {
  const formData = new FormData();

  if (Platform.OS === 'web') {
    const blob = await (await fetch(uri)).blob();
    formData.append('arquivo', blob, 'foto.jpg');
  } else {
    formData.append('arquivo', { uri, name: 'foto.jpg', type: 'image/jpeg' });
  }

  const response = await fetch(`${API_URL}/uploads`, {
    method: 'POST',
    body: formData,
    headers: await obterCabecalhoAuth(),
  });
  const data = await response.json();

  if (!response.ok) {
    throw { response: { data } };
  }

  return data.url;
};

export default api;
