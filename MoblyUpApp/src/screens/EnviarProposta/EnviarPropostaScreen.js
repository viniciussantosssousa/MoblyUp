import React, { useState } from 'react';
import { View, Text, TextInput, TouchableOpacity, ScrollView, Image, Alert, KeyboardAvoidingView, Platform } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation, useRoute } from '@react-navigation/native';
import * as Notifications from 'expo-notifications';
import { styles } from './EnviarPropostaStyles';
import { colors } from '../../theme/colors';

export default function EnviarPropostaScreen() {
  const navigation = useNavigation();
  const route = useRoute();
  
  const pedido = route.params?.pedido || {};
  const [valorProposta, setValorProposta] = useState('');
  
  const valorNumerico = parseFloat(valorProposta.replace(',', '.')) || 0;
  const taxaPlataforma = valorNumerico * 0.15;
  const valorLiquido = valorNumerico - taxaPlataforma;

  const formatarMoeda = (valor) => {
    return valor.toFixed(2).replace('.', ',');
  };

  const handleEnviarProposta = async () => {
    if (valorNumerico <= 0) {
      Alert.alert('Atenção', 'Digite um valor válido para enviar a proposta.');
      return;
    }

    try {
      await Notifications.scheduleNotificationAsync({
        content: {
          title: "Nova Proposta Recebida! 🔨",
          body: `Um montador enviou uma proposta de R$ ${formatarMoeda(valorNumerico)} para o seu pedido.`,
        },
        trigger: null, 
      });
      
      Alert.alert('Sucesso', 'Proposta enviada com sucesso! O cliente foi notificado.');
      navigation.goBack();
    } catch (error) {
      Alert.alert('Erro', 'Falha ao conectar com o servidor.');
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

        <TouchableOpacity style={styles.submitButton} onPress={handleEnviarProposta}>
          <Text style={styles.submitButtonText}>Enviar Proposta</Text>
        </TouchableOpacity>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}