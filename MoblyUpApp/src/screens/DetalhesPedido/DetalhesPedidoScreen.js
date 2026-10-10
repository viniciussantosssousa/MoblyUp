import React from 'react';
import { View, Text, TouchableOpacity, ScrollView } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation, useRoute } from '@react-navigation/native';
import { styles } from './DetalhesPedidoStyles';
import { colors } from '../../theme/colors';

export default function DetalhesPedidoScreen() {
  const navigation = useNavigation();
  const route = useRoute();
  
  const pedidoData = route.params?.pedido || {};
  const isConcluido = pedidoData.status === 'Concluído';

  const formatarMoeda = (valor) => {
    if (!valor) return 'A definir';
    return `R$ ${parseFloat(valor).toFixed(2).replace('.', ',')}`;
  };

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <Ionicons name="arrow-back" size={28} color={colors.primary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Detalhes do Pedido</Text>
      </View>

      <ScrollView contentContainerStyle={styles.content}>
        
        <View style={styles.statusCard}>
          <Ionicons 
            name={isConcluido ? "checkmark-circle" : "time"} 
            size={48} 
            color={isConcluido ? "#38A169" : "#D69E2E"} 
            style={styles.statusIcon} 
          />
          <Text style={isConcluido ? styles.statusTitleDone : styles.statusTitlePending}>
            {isConcluido ? 'Serviço Concluído' : 'Aguardando Propostas'}
          </Text>
          <Text style={styles.statusSubtitle}>
            {isConcluido 
              ? `A montagem foi finalizada com sucesso em ${pedidoData.data}.` 
              : 'Seu pedido está visível para os montadores da região.'}
          </Text>
        </View>

        <View style={styles.sectionCard}>
          <Text style={styles.sectionTitle}>Informações do Móvel</Text>
          
          <View style={styles.infoRow}>
            <Text style={styles.infoLabel}>Tipo</Text>
            <Text style={styles.infoValue}>{pedidoData.tipoMovel}</Text>
          </View>
          <View style={styles.infoRow}>
            <Text style={styles.infoLabel}>Tamanho</Text>
            <Text style={styles.infoValue}>{pedidoData.tamanhoMovel || 'Não informado'}</Text>
          </View>
          <View style={styles.infoRow}>
            <Text style={styles.infoLabel}>Endereço</Text>
            <Text style={styles.infoValue}>{pedidoData.endereco}</Text>
          </View>
        </View>

        <View style={styles.sectionCard}>
          <Text style={styles.sectionTitle}>Resumo Financeiro</Text>
          
          <View style={styles.infoRow}>
            <Text style={styles.infoLabel}>Forma de Pagamento</Text>
            <Text style={styles.infoValue}>PIX</Text>
          </View>

          <View style={styles.divider} />

          <View style={styles.priceRow}>
            <Text style={styles.priceLabel}>Total Pago</Text>
            <Text style={isConcluido ? styles.priceValueDone : styles.priceValuePending}>
              {formatarMoeda(pedidoData.valorPago)}
            </Text>
          </View>
        </View>

      </ScrollView>
    </View>
  );
}