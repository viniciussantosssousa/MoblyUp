import React, { useState, useCallback, useContext, useRef, useEffect } from 'react';
import {
  View, Text, TextInput, TouchableOpacity, FlatList, ActivityIndicator,
  KeyboardAvoidingView, Platform, Image,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation, useRoute, useFocusEffect } from '@react-navigation/native';
import { styles } from './ChatStyles';
import { colors } from '../../theme/colors';
import { AuthContext } from '../../contexts/AuthContext';
import api, { mensagemDeErro } from '../../services/api';
import { ouvir, definirChatNaTela } from '../../services/tempoReal';
import useAtualizacaoAoVivo from '../../hooks/useAtualizacaoAoVivo';

const formatarHora = (data) =>
  new Date(data).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });

const formatarDia = (data) => {
  const dia = new Date(data);
  const hoje = new Date();
  const ontem = new Date(); ontem.setDate(hoje.getDate() - 1);
  if (dia.toDateString() === hoje.toDateString()) return 'Hoje';
  if (dia.toDateString() === ontem.toDateString()) return 'Ontem';
  return dia.toLocaleDateString('pt-BR');
};

// Chat do serviço (cliente <-> montador), em tempo real. Abre quando o cliente aceita a proposta
// e fica só para leitura depois que o serviço termina. A API recusa mensagens com contato.
export default function ChatScreen() {
  const navigation = useNavigation();
  const route = useRoute();
  const { user } = useContext(AuthContext);
  const pedidoId = route.params?.pedidoId;

  const [conversa, setConversa] = useState(null);
  const [mensagens, setMensagens] = useState([]);
  const [texto, setTexto] = useState('');
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState('');
  const [aviso, setAviso] = useState('');
  const lista = useRef(null);

  const carregar = useCallback(async () => {
    try {
      const { data } = await api.get(`/pedidos/${pedidoId}/chat`);
      setConversa(data);
      setMensagens(data.mensagens);
      setErro('');
    } catch (e) {
      setErro(mensagemDeErro(e, 'Não foi possível abrir a conversa.'));
    }
  }, [pedidoId]);

  // Enquanto a tela está aberta: carrega, marca "conversa na tela" (sem banner) e escuta o tempo real
  useFocusEffect(useCallback(() => {
    definirChatNaTela(pedidoId);
    carregar();

    const pararMensagens = ouvir('mensagemChat', (mensagem) => {
      if (mensagem.pedidoId !== pedidoId) return;
      // Evita duplicar a própria mensagem (ela volta pelo tempo real depois do envio)
      setMensagens((atuais) => (atuais.some((m) => m.id === mensagem.id) ? atuais : [...atuais, mensagem]));
      // Chegou mensagem da outra parte com a conversa aberta: já está lida
      if (mensagem.remetenteId !== user?.id) api.put(`/pedidos/${pedidoId}/chat/lidas`).catch(() => {});
    });

    // A outra parte leu: marca as minhas mensagens com "✓✓"
    const pararLeituras = ouvir('chatLido', (dados) => {
      if (dados.pedidoId !== pedidoId) return;
      setMensagens((atuais) => atuais.map((m) => (m.remetenteId === user?.id ? { ...m, lida: true } : m)));
    });

    return () => {
      definirChatNaTela(null);
      pararMensagens();
      pararLeituras();
    };
  }, [pedidoId, carregar, user?.id]));

  // Serviço concluído/cancelado enquanto a conversa está aberta: fecha o envio na hora
  useAtualizacaoAoVivo(carregar, pedidoId, { incluirChat: false });

  useEffect(() => {
    if (mensagens.length) setTimeout(() => lista.current?.scrollToEnd({ animated: true }), 50);
  }, [mensagens.length]);

  const handleEnviar = async () => {
    const conteudo = texto.trim();
    if (!conteudo || enviando) return;

    try {
      setEnviando(true);
      setAviso('');
      const { data } = await api.post(`/pedidos/${pedidoId}/chat`, { texto: conteudo });
      setMensagens((atuais) => (atuais.some((m) => m.id === data.id) ? atuais : [...atuais, data]));
      setTexto('');
    } catch (e) {
      // Ex.: mensagem com telefone bloqueada. O texto fica no campo para a pessoa corrigir.
      setAviso(mensagemDeErro(e, 'Não foi possível enviar a mensagem.'));
    } finally {
      setEnviando(false);
    }
  };

  const renderMensagem = ({ item, index }) => {
    const minha = item.remetenteId === user?.id;
    const anterior = mensagens[index - 1];
    const novoDia = !anterior || formatarDia(anterior.dataEnvio) !== formatarDia(item.dataEnvio);

    return (
      <View>
        {novoDia && <Text style={styles.dia}>{formatarDia(item.dataEnvio)}</Text>}
        <View style={[styles.balao, minha ? styles.balaoMeu : styles.balaoOutro]}>
          <Text style={[styles.balaoTexto, minha && styles.balaoTextoMeu]}>{item.texto}</Text>
          <View style={styles.balaoRodape}>
            <Text style={[styles.hora, minha && styles.horaMinha]}>{formatarHora(item.dataEnvio)}</Text>
            {minha && (
              <Ionicons
                name={item.lida ? 'checkmark-done' : 'checkmark'}
                size={14}
                color={item.lida ? '#90CDF4' : '#CBD5E0'}
                style={{ marginLeft: 4 }}
              />
            )}
          </View>
        </View>
      </View>
    );
  };

  return (
    <KeyboardAvoidingView style={styles.container} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <Ionicons name="arrow-back" size={26} color={colors.primary} />
        </TouchableOpacity>

        <TouchableOpacity
          style={styles.headerPessoa}
          activeOpacity={0.7}
          disabled={!conversa?.outraParteId}
          onPress={() => navigation.navigate('PerfilPublico', { usuarioId: conversa.outraParteId })}
        >
          {conversa?.outraParteFoto
            ? <Image source={{ uri: conversa.outraParteFoto }} style={styles.avatar} />
            : <Ionicons name="person-circle" size={40} color={colors.primary} />}
          <View style={{ flex: 1, marginLeft: 10 }}>
            <Text style={styles.headerNome} numberOfLines={1}>{conversa?.outraParteNome || 'Conversa'}</Text>
            <Text style={styles.headerInfo} numberOfLines={1}>
              {conversa ? `${conversa.outraParteTipo === 'montador' ? 'Montador' : 'Cliente'} · ${conversa.tipoMovel}` : ''}
            </Text>
          </View>
        </TouchableOpacity>
      </View>

      <View style={styles.seguranca}>
        <Ionicons name="shield-checkmark" size={16} color="#2F855A" />
        <Text style={styles.segurancaTexto}>
          Combine tudo por aqui. Por segurança, telefone, e-mail e redes sociais não podem ser compartilhados.
        </Text>
      </View>

      {erro ? (
        <View style={styles.centro}><Text style={styles.erro}>{erro}</Text></View>
      ) : !conversa ? (
        <View style={styles.centro}><ActivityIndicator size="large" color={colors.primary} /></View>
      ) : (
        <>
          <FlatList
            ref={lista}
            data={mensagens}
            keyExtractor={(m) => m.id}
            renderItem={renderMensagem}
            contentContainerStyle={styles.lista}
            ListEmptyComponent={
              <Text style={styles.vazio}>
                {conversa.aberto
                  ? 'Mande a primeira mensagem para combinar os detalhes do serviço.'
                  : 'Nenhuma mensagem nesta conversa.'}
              </Text>
            }
            onContentSizeChange={() => lista.current?.scrollToEnd({ animated: false })}
          />

          {conversa.aberto ? (
            <>
              {aviso ? (
                <View style={styles.bloqueio}>
                  <Ionicons name="alert-circle" size={16} color="#C53030" />
                  <Text style={styles.bloqueioTexto}>{aviso}</Text>
                </View>
              ) : null}
              <View style={styles.entrada}>
                <TextInput
                  style={styles.input}
                  placeholder="Escreva uma mensagem..."
                  value={texto}
                  onChangeText={(t) => { setTexto(t); if (aviso) setAviso(''); }}
                  multiline
                  maxLength={1000}
                  onSubmitEditing={Platform.OS === 'web' ? handleEnviar : undefined}
                  blurOnSubmit={false}
                />
                <TouchableOpacity
                  style={[styles.enviar, (!texto.trim() || enviando) && styles.enviarDesativado]}
                  onPress={handleEnviar}
                  disabled={!texto.trim() || enviando}
                  accessibilityLabel="Enviar mensagem"
                >
                  {enviando ? <ActivityIndicator color={colors.white} size="small" /> : <Ionicons name="send" size={20} color={colors.white} />}
                </TouchableOpacity>
              </View>
            </>
          ) : (
            <View style={styles.encerrado}>
              <Ionicons name="lock-closed" size={16} color="#718096" />
              <Text style={styles.encerradoTexto}>Serviço encerrado: a conversa fica disponível só para leitura.</Text>
            </View>
          )}
        </>
      )}
    </KeyboardAvoidingView>
  );
}
