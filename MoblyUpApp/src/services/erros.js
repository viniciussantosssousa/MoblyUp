import { mensagemDeErro } from './api';

// Erros do Firebase (Auth/Firestore) traduzidos para o usuário
const MENSAGENS_FIREBASE = {
  'auth/invalid-credential': 'E-mail ou senha incorretos.',
  'auth/wrong-password': 'Senha incorreta.',
  'auth/user-not-found': 'Não existe conta com este e-mail.',
  'auth/invalid-email': 'E-mail inválido.',
  'auth/email-already-in-use': 'Este e-mail já está em uso.',
  'auth/weak-password': 'A senha deve ter pelo menos 6 caracteres.',
  'auth/too-many-requests': 'Muitas tentativas seguidas. Aguarde alguns minutos e tente de novo.',
  'auth/requires-recent-login': 'Por segurança, saia e entre novamente no app antes de continuar.',
  'auth/network-request-failed': 'Sem conexão com a internet.',
  'auth/missing-email': 'Informe o seu e-mail.',
  'permission-denied': 'O servidor recusou o acesso (regras do Firestore).',
  'perfil-nao-encontrado': 'Esta conta não tem perfil cadastrado. Crie a conta novamente.',
};

// Firebase primeiro; depois a mensagem enviada pela nossa API; por último o texto padrão
export const traduzirErro = (error, padrao) =>
  MENSAGENS_FIREBASE[error?.code] || mensagemDeErro(error, padrao);
