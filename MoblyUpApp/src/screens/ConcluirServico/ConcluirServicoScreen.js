import React, { useState, useContext } from 'react';
import { View, Text, TouchableOpacity, ScrollView, Image, ActivityIndicator } from 'react-native';
import * as ImagePicker from 'expo-image-picker';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation, useRoute } from '@react-navigation/native';
import { styles } from './ConcluirServicoStyles';
import { colors } from '../../theme/colors';
import { AuthContext } from '../../contexts/AuthContext';
import api, { enviarImagem, mensagemDeErro } from '../../services/api';
import { avisar } from '../../services/alerta';
import { formatarMoeda } from '../../services/statusPedido';

export default function ConcluirServicoScreen() {
  const navigation = useNavigation();
  const route = useRoute();
  const { user } = useContext(AuthContext);

  const pedido = route.params?.pedido || {};
  const [fotoUri, setFotoUri] = useState('');
  const [enviando, setEnviando] = useState(false);

  // Regra de negócio: a comprovação precisa ser tirada na hora, pela câmera (sem galeria)
  const tirarFoto = async () => {
    const { status } = await ImagePicker.requestCameraPermissionsAsync();
    if (status !== 'granted') {
      avisar('Permissão Negada', 'Precisamos de acesso à câmera para registrar a montagem concluída.');
      return;
    }

    const result = await ImagePicker.launchCameraAsync({
      allowsEditing: true,
      aspect: [4, 3],
      quality: 0.5,
    });

    if (!result.canceled) {
      setFotoUri(result.assets[0].uri);
    }
  };

  const handleConcluir = async () => {
    if (!fotoUri) {
      avisar('Atenção', 'Tire uma foto do móvel montado para concluir o serviço.');
      return;
    }

    try {
      setEnviando(true);

      // 1. Armazena a imagem no servidor
      const fotoUrl = await enviarImagem(fotoUri);

      // 2. Vincula a foto ao pedido e 3. encerra o serviço (a API exige a foto antes)
      await api.put(`/pedidos/${pedido.id}/foto-conclusao`, { montadorId: user.id, fotoUrl });
      await api.put(`/pedidos/${pedido.id}/concluir`, { montadorId: user.id });

      avisar('Serviço concluído!', `Parabéns! ${formatarMoeda(pedido.valorMontador)} serão repassados para a sua chave Pix.`);
      navigation.navigate('HomeMontador');
    } catch (error) {
      avisar('Erro', mensagemDeErro(error, 'Não foi possível concluir o serviço.'));
    } finally {
      setEnviando(false);
    }
  };

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <Ionicons name="arrow-back" size={28} color={colors.primary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Concluir Serviço</Text>
      </View>

      <ScrollView contentContainerStyle={styles.content}>
        <View style={styles.card}>
          <Text style={styles.cardTitle}>{pedido.tipoMovel}</Text>
          <Text style={styles.cardText}>{pedido.enderecoServico}</Text>
          <Text style={styles.cardValor}>Você recebe {formatarMoeda(pedido.valorMontador)}</Text>
        </View>

        <Text style={styles.instrucao}>
          Para garantir a qualidade do serviço, tire uma foto do móvel montado. O serviço só é encerrado após o envio da foto.
        </Text>

        {fotoUri ? (
          <View>
            <Image source={{ uri: fotoUri }} style={styles.preview} />
            <TouchableOpacity onPress={tirarFoto} disabled={enviando}>
              <Text style={styles.trocarFoto}>Tirar outra foto</Text>
            </TouchableOpacity>
          </View>
        ) : (
          <TouchableOpacity style={styles.cameraButton} onPress={tirarFoto}>
            <Ionicons name="camera" size={40} color={colors.primary} />
            <Text style={styles.cameraButtonText}>Abrir Câmera</Text>
          </TouchableOpacity>
        )}

        <TouchableOpacity
          style={[styles.submitButton, (!fotoUri || enviando) && styles.submitButtonDisabled]}
          onPress={handleConcluir}
          disabled={!fotoUri || enviando}
        >
          {enviando ? (
            <ActivityIndicator color={colors.white} />
          ) : (
            <Text style={styles.submitButtonText}>Enviar Foto e Concluir</Text>
          )}
        </TouchableOpacity>
      </ScrollView>
    </View>
  );
}
