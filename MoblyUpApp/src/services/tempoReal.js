import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { API_URL, obterUsuarioDeTeste } from './api';
import { auth } from './firebaseConfig';

// Conexão em tempo real com a API (SignalR / WebSocket).
// Eventos recebidos:
// - "pedidoAlterado": algum pedido do usuário (ou do feed da região) mudou -> telas recarregam
// - "aviso": mensagem para mostrar no topo da tela (nova proposta, pedido novo na região...)

const URL_HUB = API_URL.replace(/\/api\/?$/, '') + '/hubs/pedidos';

const ouvintes = { pedidoAlterado: new Set(), aviso: new Set() };
let conexao = null;

const emitir = (evento, dados) => ouvintes[evento].forEach((ouvinte) => {
  try { ouvinte(dados); } catch (error) { console.log(`Erro ao tratar "${evento}":`, error); }
});

// Inscreve uma função num evento; devolve a função que cancela a inscrição
export const ouvir = (evento, ouvinte) => {
  ouvintes[evento].add(ouvinte);
  return () => ouvintes[evento].delete(ouvinte);
};

export const conectarTempoReal = async () => {
  await desconectarTempoReal();

  // O WebSocket do navegador não envia cabeçalhos: o login vai na URL (só aceito na rota /hubs)
  const usuarioTeste = obterUsuarioDeTeste();
  const url = usuarioTeste ? `${URL_HUB}?usuario_teste=${encodeURIComponent(usuarioTeste)}` : URL_HUB;

  conexao = new HubConnectionBuilder()
    .withUrl(url, {
      // Token do Firebase renovado automaticamente a cada (re)conexão
      accessTokenFactory: usuarioTeste ? undefined : async () => (await auth.currentUser?.getIdToken()) || '',
      withCredentials: false,
    })
    // Tenta reconectar sozinho se a internet cair (0s, 2s, 10s, 30s e depois a cada 30s)
    .withAutomaticReconnect({ nextRetryDelayInMilliseconds: (ctx) => [0, 2000, 10000, 30000][ctx.previousRetryCount] ?? 30000 })
    .configureLogging(LogLevel.Warning)
    .build();

  conexao.on('pedidoAlterado', (dados) => emitir('pedidoAlterado', dados));
  conexao.on('aviso', (dados) => emitir('aviso', dados));
  // Ao reconectar pode ter perdido eventos: pede para as telas recarregarem
  conexao.onreconnected(() => emitir('pedidoAlterado', {}));

  try {
    await conexao.start();
  } catch (error) {
    console.log('Tempo real indisponível (o app continua funcionando, atualize puxando a tela):', error?.message);
  }
};

export const desconectarTempoReal = async () => {
  const atual = conexao;
  conexao = null;
  if (atual && atual.state !== HubConnectionState.Disconnected) {
    await atual.stop().catch(() => {});
  }
};
