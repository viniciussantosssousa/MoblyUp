import React, { useState, useEffect } from 'react';
import { View, Text, TouchableOpacity, ScrollView, ActivityIndicator, Alert } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation, useRoute } from '@react-navigation/native';
import * as Notifications from 'expo-notifications';
import { styles } from './VerPropostasStyles';
import { colors } from '../../theme/colors';

export default function VerPropostasScreen() {
  const navigation = useNavigation();
  const route = useRoute();
  
  const pedido = route.params?.pedido || {};
  
  const [propostas, setPropostas] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchPropostas = async () => {
      try {
        setLoading(true);
        
        // Exemplo de integração futura:
        // const response = await fetch(`http://COLOQUE_SEU_IP_AQUI:5007/api/propostas/pedido/${pedido.id}`);
        // if (response.ok) {
        //   const data = await response.json();
        //   setPropostas(data);
        // }

        setPropostas([]); 
      } catch (error) {
        console.log('Erro ao buscar propostas:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchPropostas();
  }, [pedido.id]);

  const handleAceitarProposta = async (proposta) => {
    try {
      // Exemplo de integração futura para confirmar o aceite:
      // await fetch(`http://COLOQUE_SEU_IP_AQUI:5007/api/propostas/${proposta.id}/aceitar`, { method: 'POST' });

      await Notifications.scheduleNotificationAsync({
        content: {
          title: "Proposta Aceita! 🎉",
          body: `O cliente aceitou sua oferta. Prepare-se para a montagem!`,
        },
        trigger: null, 
      });

      Alert.alert('Sucesso', 'Você aceitou esta proposta! O montador foi notificado e entrará em contato.');
      navigation.navigate('HomeCliente');
    } catch (error) {
      Alert.alert('Erro', 'Não foi possível aceitar a proposta no momento.');
    }
  };

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <Ionicons name="arrow-back" size={28} color={colors.primary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Propostas Recebidas</Text>
      </View>

      <ScrollView contentContainerStyle={styles.content}>
        <Text style={styles.sectionTitle}>Montadores Interessados</Text>

        {loading ? (
          <ActivityIndicator size="large" color={colors.primary} style={styles.loader} />
        ) : propostas.length === 0 ? (
          <Text style={styles.emptyText}>Nenhum montador enviou proposta para este pedido ainda. Aguarde mais um pouco.</Text>
        ) : (
          propostas.map((proposta) => (
            <View key={proposta.id} style={styles.propostaCard}>
              <View style={styles.montadorInfoRow}>
                <Ionicons name="person-circle-outline" size={32} color={colors.primary} />
                <Text style={styles.montadorNome}>{proposta.nomeMontador}</Text>
                <View style={styles.avaliacaoContainer}>
                  <Ionicons name="star" size={12} color="#D69E2E" />
                  <Text style={styles.avaliacaoTexto}>{proposta.avaliacaoMedia}</Text>
                </View>
              </View>

              <Text style={styles.valorProposta}>
                R$ {parseFloat(proposta.valorTotalCobrado).toFixed(2).replace('.', ',')}
              </Text>

              <TouchableOpacity 
                style={styles.acceptButton}
                activeOpacity={0.8}
                onPress={() => handleAceitarProposta(proposta)}
              >
                <Text style={styles.acceptButtonText}>Aceitar Montador</Text>
              </TouchableOpacity>
            </View>
          ))
        )}
      </ScrollView>
    </View>
  );
}