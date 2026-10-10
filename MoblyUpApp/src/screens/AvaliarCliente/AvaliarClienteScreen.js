import React, { useState } from 'react';
import { View, Text, TextInput, TouchableOpacity, ScrollView, StyleSheet, Platform, KeyboardAvoidingView } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation, useRoute } from '@react-navigation/native';
import { colors } from '../../theme/colors';
import api, { mensagemDeErro } from '../../services/api';
import { avisar } from '../../services/alerta';
import Estrelas from '../../components/Estrelas';

const LEGENDAS = ['', 'Muito ruim', 'Ruim', 'Regular', 'Bom', 'Excelente'];

// Montador avalia o cliente depois de concluir o serviço
export default function AvaliarClienteScreen() {
  const navigation = useNavigation();
  const route = useRoute();
  const pedido = route.params?.pedido || {};

  const [nota, setNota] = useState(0);
  const [comentario, setComentario] = useState('');
  const [enviando, setEnviando] = useState(false);

  const handleEnviar = async () => {
    try {
      setEnviando(true);
      await api.put(`/pedidos/${pedido.id}/avaliar-cliente`, { nota, comentario: comentario.trim() || null });
      avisar('Obrigado!', 'Sua avaliação ajuda outros montadores a conhecerem os clientes.');
      navigation.goBack();
    } catch (error) {
      avisar('Erro', mensagemDeErro(error, 'Não foi possível enviar a avaliação.'));
    } finally {
      setEnviando(false);
    }
  };

  return (
    <KeyboardAvoidingView style={styles.container} behavior={Platform.OS === 'ios' ? 'padding' : 'height'}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <Ionicons name="arrow-back" size={28} color={colors.primary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Avaliar Cliente</Text>
      </View>

      <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
        <View style={styles.card}>
          <Text style={styles.servico}>{pedido.tipoMovel}</Text>
          <Text style={styles.endereco}>{pedido.enderecoServico}</Text>

          <Text style={styles.pergunta}>Como foi atender {pedido.nomeCliente || 'este cliente'}?</Text>
          <Estrelas nota={nota} tamanho={40} onChange={setNota} style={styles.estrelas} />
          <Text style={styles.legenda}>{LEGENDAS[nota]}</Text>

          <TextInput
            style={styles.comentario}
            placeholder="Pontualidade, comunicação, local do serviço... (opcional)"
            multiline
            maxLength={500}
            value={comentario}
            onChangeText={setComentario}
          />
          <Text style={styles.contador}>{comentario.length}/500</Text>

          <TouchableOpacity
            style={[styles.botao, (!nota || enviando) && { opacity: 0.5 }]}
            disabled={!nota || enviando}
            onPress={handleEnviar}
          >
            <Text style={styles.botaoTexto}>{enviando ? 'Enviando...' : 'Enviar avaliação'}</Text>
          </TouchableOpacity>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
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
  content: { padding: 24 },
  card: { backgroundColor: colors.white, borderRadius: 12, padding: 20, borderWidth: 1, borderColor: colors.border },
  servico: { fontSize: 18, fontWeight: 'bold', color: colors.primary },
  endereco: { fontSize: 13, color: '#718096', marginTop: 4, marginBottom: 24 },
  pergunta: { fontSize: 16, fontWeight: '600', color: colors.primary, textAlign: 'center' },
  estrelas: { justifyContent: 'center', marginTop: 16 },
  legenda: { textAlign: 'center', color: '#B7791F', fontWeight: 'bold', marginTop: 8, minHeight: 20 },
  comentario: {
    backgroundColor: '#F7FAFC', borderWidth: 1, borderColor: colors.border, borderRadius: 8, padding: 12,
    minHeight: 100, fontSize: 14, color: colors.text, textAlignVertical: 'top', marginTop: 16,
  },
  contador: { fontSize: 11, color: '#A0AEC0', textAlign: 'right', marginTop: 4 },
  botao: { backgroundColor: colors.primary, borderRadius: 8, paddingVertical: 14, alignItems: 'center', marginTop: 16 },
  botaoTexto: { color: colors.white, fontSize: 16, fontWeight: 'bold' },
});
