import React, { useState, useCallback } from 'react';
import { View, Text, TouchableOpacity, ScrollView, Image, ActivityIndicator, StyleSheet, Platform } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation, useRoute, useFocusEffect } from '@react-navigation/native';
import { colors } from '../../theme/colors';
import api, { mensagemDeErro } from '../../services/api';
import { formatarData } from '../../services/statusPedido';
import { QuadroReputacao, ListaAvaliacoes } from '../../components/Reputacao';

// Perfil de outra pessoa (cliente ou montador): reputação e comentários.
// Sem telefone ou outros contatos: a conversa acontece pelo chat do app.
export default function PerfilPublicoScreen() {
  const navigation = useNavigation();
  const route = useRoute();
  const usuarioId = route.params?.usuarioId;

  const [perfil, setPerfil] = useState(null);
  const [erro, setErro] = useState('');

  useFocusEffect(useCallback(() => {
    let ativo = true;
    setErro('');
    api.get(`/usuarios/${usuarioId}/perfil-publico`)
      .then(({ data }) => ativo && setPerfil(data))
      .catch((e) => ativo && setErro(mensagemDeErro(e, 'Não foi possível carregar o perfil.')));
    return () => { ativo = false; };
  }, [usuarioId]));

  const montador = perfil?.tipo === 'montador';

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <Ionicons name="arrow-back" size={28} color={colors.primary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>{montador ? 'Perfil do Montador' : 'Perfil do Cliente'}</Text>
      </View>

      {erro ? (
        <View style={styles.centro}><Text style={styles.erro}>{erro}</Text></View>
      ) : !perfil ? (
        <View style={styles.centro}><ActivityIndicator size="large" color={colors.primary} /></View>
      ) : (
        <ScrollView contentContainerStyle={styles.content}>
          <View style={styles.topo}>
            <View style={styles.avatar}>
              {perfil.fotoPerfilUrl
                ? <Image source={{ uri: perfil.fotoPerfilUrl }} style={styles.avatarImagem} />
                : <Ionicons name="person" size={44} color={colors.primary} />}
            </View>
            <Text style={styles.nome}>{perfil.nome}</Text>
            <View style={styles.badge}><Text style={styles.badgeTexto}>{montador ? 'Montador' : 'Cliente'}</Text></View>
            {perfil.regiao ? <Text style={styles.info}><Ionicons name="location-outline" size={13} /> {perfil.regiao}</Text> : null}
            <Text style={styles.info}>No MoblyUp desde {formatarData(perfil.dataCadastro)}</Text>
            <Text style={styles.info}>
              {perfil.servicosConcluidos} {perfil.servicosConcluidos === 1 ? 'serviço concluído' : 'serviços concluídos'}
            </Text>
          </View>

          <View style={styles.avisoContato}>
            <Ionicons name="shield-checkmark" size={18} color="#2F855A" />
            <Text style={styles.avisoContatoTexto}>
              Para sua segurança, a conversa com {montador ? 'o montador' : 'o cliente'} acontece pelo chat do MoblyUp, aberto quando a proposta é aceita.
            </Text>
          </View>

          <View style={styles.card}>
            <Text style={styles.cardTitulo}>Reputação</Text>
            <QuadroReputacao reputacao={perfil.reputacao} />
          </View>

          <View style={styles.card}>
            <Text style={styles.cardTitulo}>
              O que {montador ? 'os clientes' : 'os montadores'} dizem
            </Text>
            <ListaAvaliacoes
              avaliacoes={perfil.reputacao.avaliacoes}
              vazio={montador ? 'Este montador ainda não recebeu avaliações.' : 'Este cliente ainda não recebeu avaliações.'}
            />
          </View>
        </ScrollView>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#F5F7FA' },
  header: {
    flexDirection: 'row', alignItems: 'center', backgroundColor: colors.white, paddingHorizontal: 24,
    paddingTop: Platform.OS === 'ios' ? 60 : 40, paddingBottom: 20, borderBottomWidth: 1, borderBottomColor: colors.border,
  },
  backButton: { marginRight: 16 },
  headerTitle: { fontSize: 20, fontWeight: 'bold', color: colors.primary },
  centro: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: 24 },
  erro: { fontSize: 15, color: '#718096', textAlign: 'center' },
  content: { padding: 24, paddingBottom: 40 },
  topo: { alignItems: 'center', marginBottom: 24 },
  avatar: {
    width: 96, height: 96, borderRadius: 48, backgroundColor: '#EDF2F7', alignItems: 'center',
    justifyContent: 'center', borderWidth: 2, borderColor: colors.primary, marginBottom: 12,
  },
  avatarImagem: { width: '100%', height: '100%', borderRadius: 48 },
  nome: { fontSize: 20, fontWeight: 'bold', color: colors.primary },
  badge: { backgroundColor: '#EDF2F7', paddingHorizontal: 12, paddingVertical: 4, borderRadius: 12, marginTop: 8 },
  badgeTexto: { fontSize: 13, fontWeight: 'bold', color: colors.primary },
  info: { fontSize: 13, color: '#718096', marginTop: 6 },
  avisoContato: {
    flexDirection: 'row', alignItems: 'center', backgroundColor: '#F0FFF4', borderWidth: 1,
    borderColor: '#C6F6D5', borderRadius: 12, padding: 14, marginBottom: 16,
  },
  avisoContatoTexto: { flex: 1, fontSize: 13, color: '#276749', marginLeft: 10, lineHeight: 18 },
  card: { backgroundColor: colors.white, borderRadius: 12, padding: 20, marginBottom: 16, borderWidth: 1, borderColor: colors.border },
  cardTitulo: { fontSize: 16, fontWeight: 'bold', color: colors.primary, marginBottom: 16 },
});
