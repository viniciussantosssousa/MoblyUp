import React, { useState } from 'react';
import { View, Text, TextInput, TouchableOpacity, ScrollView, Image, KeyboardAvoidingView, Platform } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation, useRoute } from '@react-navigation/native';
import { styles } from './EnviarPropostaStyles';
import { colors } from '../../theme/colors';
import api, { mensagemDeErro } from '../../services/api';
import { avisar, confirmar } from '../../services/alerta';

// Aceita "150", "150,50" e "1.250,00"
const converterValor = (texto) => {
  const normalizado = texto.includes(',') ? texto.replace(/\./g, '').replace(',', '.') : texto;
  return parseFloat(normalizado) || 0;
};

export default function EnviarPropostaScreen() {
  const navigation = useNavigation();
  const route = useRoute();

  const pedido = route.params?.pedido || {};
  const [valorProposta, setValorProposta] = useState('');
  const [enviando, setEnviando] = useState(false);

  // Apenas uma prévia: o valor oficial da taxa (15%) é calculado no back-end
  const valorNumerico = Math.round(converterValor(valorProposta) * 100) / 100;
  const taxaPlataforma = valorNumerico * 0.15;
  const valorLiquido = valorNumerico - taxaPlataforma;

  const formatarMoeda = (valor) => {
    return valor.toFixed(2).replace('.', ',');
  };

  const handleEnviarProposta = async () => {
    if (valorNumerico <= 0) {
      avisar('Atenção', 'Digite um valor válido para enviar a proposta.');
      return;
    }

    try {
      setEnviando(true);
      await api.put(`/pedidos/${pedido.id}/proposta`, { valorProposto: valorNumerico });

      avisar('Sucesso', 'Proposta enviada! Agora é só aguardar a resposta do cliente.');
      navigation.goBack();
    } catch (error) {
      const msg = mensagemDeErro(error);
      // Sem chave Pix a API recusa a proposta: leva o montador direto ao Perfil
      if (msg.includes('chave Pix')) {
        const irParaPerfil = await confirmar('Chave Pix necessária', msg, 'Ir para o Perfil');
        if (irParaPerfil) navigation.navigate('Perfil');
      } else {
        avisar('Erro', msg);
      }
    } finally {
      setEnviando(false);
    }
  };

  const handleRecusar = async () => {
    const ok = await confirmar(
      'Recusar serviço',
      'O pedido deixará de aparecer para você e continuará disponível para outros montadores.',
      'Recusar'
    );
    if (!ok) return;

    try {
      setEnviando(true);
      await api.put(`/pedidos/${pedido.id}/recusar`);
      navigation.goBack();
    } catch (error) {
      avisar('Erro', mensagemDeErro(error, 'Não foi possível recusar o pedido.'));
    } finally {
      setEnviando(false);
    }
  };

  return (
    <KeyboardAvoidingView style={styles.container} behavior={Platform.OS === "ios" ? "padding" : "height"}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <Ionicons name="arrow-back" size={28} color={colors.primary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Enviar Proposta</Text>
      </View>

      <ScrollView contentContainerStyle={styles.content}>
        <Text style={styles.sectionTitle}>Detalhes do Serviço</Text>
        
        <View style={styles.detailsCard}>
          <View style={styles.infoRow}>
            <Ionicons name="hammer-outline" size={20} color={colors.primary} />
            <View style={styles.infoTextContainer}>
              <Text style={styles.infoLabel}>Móvel</Text>
              <Text style={styles.infoValue}>{pedido.tipoMovel || 'Não especificado'}</Text>
            </View>
          </View>

          <View style={styles.infoRow}>
            <Ionicons name="resize-outline" size={20} color={colors.primary} />
            <View style={styles.infoTextContainer}>
              <Text style={styles.infoLabel}>Tamanho/Detalhes</Text>
              <Text style={styles.infoValue}>{pedido.tamanhoMovel || 'Não especificado'}</Text>
            </View>
          </View>

          <View style={styles.infoRow}>
            <Ionicons name="location-outline" size={20} color={colors.primary} />
            <View style={styles.infoTextContainer}>
              <Text style={styles.infoLabel}>Endereço do Cliente</Text>
              <Text style={styles.infoValue}>{pedido.enderecoServico || 'Não especificado'}</Text>
            </View>
          </View>

          {pedido.urlFotoMovelTemporaria ? (
            <Image source={{ uri: pedido.urlFotoMovelTemporaria }} style={styles.imagePreview} />
          ) : (
            <Text style={styles.noImageText}>O cliente não enviou foto deste móvel.</Text>
          )}
        </View>

        <View style={styles.inputContainer}>
          <Text style={styles.inputLabel}>Valor Cobrado do Cliente (PIX)</Text>
          <View style={styles.inputWrapper}>
            <Text style={styles.currencySymbol}>R$</Text>
            <TextInput
              style={styles.input}
              placeholder="0,00"
              keyboardType="numeric"
              value={valorProposta}
              onChangeText={setValorProposta}
            />
          </View>
        </View>

        {valorNumerico > 0 && (
          <View style={styles.summaryCard}>
            <View style={styles.summaryRow}>
              <Text style={styles.summaryLabel}>Taxa da Plataforma (15%)</Text>
              <Text style={styles.summaryValue}>- R$ {formatarMoeda(taxaPlataforma)}</Text>
            </View>
            <View style={styles.divider} />
            <View style={styles.summaryRow}>
              <Text style={styles.netValueLabel}>Você recebe no PIX</Text>
              <Text style={styles.netValueAmount}>R$ {formatarMoeda(valorLiquido)}</Text>
            </View>
          </View>
        )}

        <TouchableOpacity style={[styles.submitButton, enviando && { opacity: 0.6 }]} onPress={handleEnviarProposta} disabled={enviando}>
          <Text style={styles.submitButtonText}>{enviando ? 'Enviando...' : 'Enviar Proposta'}</Text>
        </TouchableOpacity>

        <TouchableOpacity style={styles.declineButton} onPress={handleRecusar} disabled={enviando}>
          <Text style={styles.declineButtonText}>Não tenho interesse neste serviço</Text>
        </TouchableOpacity>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}