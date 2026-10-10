import React, { useState, useEffect } from 'react';
import { Modal, View, Text, TextInput, TouchableOpacity, StyleSheet, ActivityIndicator, KeyboardAvoidingView, Platform } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import Estrelas from './Estrelas';
import { colors } from '../theme/colors';
import { avisar } from '../services/alerta';
import { mensagemDeErro } from '../services/api';

const LEGENDAS = ['', 'Muito ruim', 'Ruim', 'Regular', 'Bom', 'Excelente'];

// Janela de avaliação que aparece ao fim do serviço (cliente avalia montador e vice-versa).
// onEnviar(nota, comentario) faz a chamada à API; onFechar é chamado em "Agora não" e após enviar.
export default function ModalAvaliacao({ visivel, titulo, subtitulo, placeholder, onEnviar, onFechar }) {
  const [nota, setNota] = useState(0);
  const [comentario, setComentario] = useState('');
  const [enviando, setEnviando] = useState(false);

  // Cada vez que abre, começa do zero
  useEffect(() => {
    if (visivel) {
      setNota(0);
      setComentario('');
    }
  }, [visivel]);

  const handleEnviar = async () => {
    try {
      setEnviando(true);
      await onEnviar(nota, comentario.trim() || null);
      avisar('Obrigado!', 'Sua avaliação foi enviada.');
      onFechar(true);
    } catch (error) {
      avisar('Erro', mensagemDeErro(error, 'Não foi possível enviar a avaliação.'));
    } finally {
      setEnviando(false);
    }
  };

  return (
    <Modal visible={visivel} transparent animationType="fade" onRequestClose={() => onFechar(false)}>
      <KeyboardAvoidingView style={styles.fundo} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
        <View style={styles.janela}>
          <View style={styles.icone}>
            <Ionicons name="checkmark-circle" size={44} color="#38A169" />
          </View>

          <Text style={styles.titulo}>{titulo}</Text>
          {subtitulo ? <Text style={styles.subtitulo}>{subtitulo}</Text> : null}

          <Estrelas nota={nota} tamanho={38} onChange={setNota} style={styles.estrelas} />
          <Text style={styles.legenda}>{LEGENDAS[nota] || 'Toque nas estrelas'}</Text>

          <TextInput
            style={styles.comentario}
            placeholder={placeholder || 'Deixe um comentário (opcional)'}
            multiline
            maxLength={500}
            value={comentario}
            onChangeText={setComentario}
          />

          <TouchableOpacity
            style={[styles.enviar, (!nota || enviando) && styles.desativado]}
            disabled={!nota || enviando}
            onPress={handleEnviar}
          >
            {enviando ? <ActivityIndicator color={colors.white} /> : <Text style={styles.enviarTexto}>Enviar avaliação</Text>}
          </TouchableOpacity>

          <TouchableOpacity style={styles.depois} onPress={() => onFechar(false)} disabled={enviando}>
            <Text style={styles.depoisTexto}>Agora não</Text>
          </TouchableOpacity>
        </View>
      </KeyboardAvoidingView>
    </Modal>
  );
}

const styles = StyleSheet.create({
  fundo: { flex: 1, backgroundColor: 'rgba(17, 17, 78, 0.55)', justifyContent: 'center', padding: 24 },
  janela: {
    backgroundColor: colors.white, borderRadius: 16, padding: 24, width: '100%', maxWidth: 420, alignSelf: 'center',
  },
  icone: { alignItems: 'center', marginBottom: 8 },
  titulo: { fontSize: 19, fontWeight: 'bold', color: colors.primary, textAlign: 'center' },
  subtitulo: { fontSize: 14, color: '#718096', textAlign: 'center', marginTop: 6, lineHeight: 20 },
  estrelas: { justifyContent: 'center', marginTop: 18 },
  legenda: { textAlign: 'center', color: '#B7791F', fontWeight: 'bold', marginTop: 6, minHeight: 20 },
  comentario: {
    backgroundColor: '#F7FAFC', borderWidth: 1, borderColor: colors.border, borderRadius: 8, padding: 12,
    minHeight: 80, fontSize: 14, color: colors.text, textAlignVertical: 'top', marginTop: 14,
  },
  enviar: { backgroundColor: colors.primary, borderRadius: 8, paddingVertical: 14, alignItems: 'center', marginTop: 16 },
  desativado: { opacity: 0.5 },
  enviarTexto: { color: colors.white, fontSize: 16, fontWeight: 'bold' },
  depois: { alignItems: 'center', paddingVertical: 12, marginTop: 4 },
  depoisTexto: { color: '#718096', fontSize: 14, fontWeight: '600' },
});
