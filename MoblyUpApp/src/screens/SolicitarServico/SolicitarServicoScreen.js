import React, { useState, useContext } from 'react';
import { View, Text, TextInput, TouchableOpacity, Alert, KeyboardAvoidingView, Platform, ScrollView, Image } from 'react-native';
import * as ImagePicker from 'expo-image-picker';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import * as Notifications from 'expo-notifications';
import { styles } from './SolicitarServicoStyles';
import { colors } from '../../theme/colors';
import api, { enviarImagem, mensagemDeErro } from '../../services/api';
import { AuthContext } from '../../contexts/AuthContext'; 

export default function SolicitarServicoScreen() {
  const navigation = useNavigation();
  const { user } = useContext(AuthContext); // Pegando os dados e a região do cliente logado

  const [tipoMovel, setTipoMovel] = useState('');
  const [tamanhoMovel, setTamanhoMovel] = useState('');
  const [urlFoto, setUrlFoto] = useState('');
  
  // Novos estados para dividir o endereço
  const [rua, setRua] = useState('');
  const [numero, setNumero] = useState('');
  const [bairro, setBairro] = useState('');
  const [enviando, setEnviando] = useState(false);

  const handleVoltar = () => {
    navigation.goBack();
  };

  const tirarFoto = async () => {
    const { status } = await ImagePicker.requestCameraPermissionsAsync();
    if (status !== 'granted') {
      const msg = 'Precisamos de acesso à câmera para tirar a foto.';
      Platform.OS === 'web' ? window.alert(msg) : Alert.alert('Permissão Negada', msg);
      return;
    }

    const result = await ImagePicker.launchCameraAsync({
      allowsEditing: true,
      aspect: [4, 3],
      quality: 0.5,
    });

    if (!result.canceled) {
      setUrlFoto(result.assets[0].uri);
    }
  };

  const escolherDaGaleria = async () => {
    const { status } = await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (status !== 'granted') {
      const msg = 'Precisamos de acesso à galeria para escolher a foto.';
      Platform.OS === 'web' ? window.alert(msg) : Alert.alert('Permissão Negada', msg);
      return;
    }

    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ImagePicker.MediaTypeOptions.Images,
      allowsEditing: true,
      aspect: [4, 3],
      quality: 0.5,
    });

    if (!result.canceled) {
      setUrlFoto(result.assets[0].uri);
    }
  };

  const handleSolicitar = async () => {
    if (!tipoMovel || !tamanhoMovel || !rua || !numero || !bairro) {
      const msg = 'Preencha o tipo, tamanho e os dados do endereço do móvel.';
      Platform.OS === 'web' ? window.alert(msg) : Alert.alert('Atenção', msg);
      return;
    }

    // MÁGICA DA REGIÃO: Junta o que o usuário digitou com a cidade/estado do Contexto
    const enderecoCompleto = `${rua}, ${numero}, ${bairro} - ${user?.regiao || ''}`;

    try {
      setEnviando(true);

      // A foto fica só no celular até ser enviada; o montador precisa da URL pública
      const urlFotoPublica = urlFoto ? await enviarImagem(urlFoto) : null;

      const response = await api.post('/pedidos', {
        clienteId: user?.id,
        tipoMovel,
        tamanhoMovel,
        urlFotoMovelTemporaria: urlFotoPublica,
        enderecoServico: enderecoCompleto // Envia a string completinha para o C#
      });

      if (response.status === 201 || response.status === 200) {
        
        await Notifications.scheduleNotificationAsync({
          content: {
            title: "Serviço Solicitado! 📍",
            body: `Seu pedido para montar um(a) ${tipoMovel} foi enviado aos montadores da região.`,
          },
          trigger: null, 
        });

        const msgSucesso = 'Pedido enviado! Os montadores da região foram notificados.';
        Platform.OS === 'web' ? window.alert(msgSucesso) : Alert.alert('Sucesso!', msgSucesso);
        
        setTipoMovel('');
        setTamanhoMovel('');
        setUrlFoto('');
        setRua('');
        setNumero('');
        setBairro('');
        navigation.goBack();
      }
    } catch (error) {
      console.log('Erro ao criar pedido:', error);
      const erroMsg = mensagemDeErro(error, 'Não foi possível conectar com o servidor para enviar o pedido.');
      Platform.OS === 'web' ? window.alert(erroMsg) : Alert.alert('Erro', erroMsg);
    } finally {
      setEnviando(false);
    }
  };

  return (
    <KeyboardAvoidingView style={styles.container} behavior={Platform.OS === "ios" ? "padding" : "height"}>
      <ScrollView contentContainerStyle={styles.scrollContainer}>
        
        <View style={styles.header}>
          <TouchableOpacity style={styles.backButton} onPress={handleVoltar}>
            <Ionicons name="arrow-back" size={28} color={colors.primary} />
          </TouchableOpacity>
        </View>

        <Text style={styles.title}>Solicitar Montagem</Text>
        <Text style={styles.subtitle}>Preencha os dados para receber ofertas</Text>

        <View style={styles.inputContainer}>
          <Text style={styles.label}>Tipo de Móvel</Text>
          <TextInput style={styles.input} placeholder="Ex: Guarda-Roupa" value={tipoMovel} onChangeText={setTipoMovel} />
        </View>

        <View style={styles.inputContainer}>
          <Text style={styles.label}>Tamanho / Detalhes</Text>
          <TextInput style={styles.input} placeholder="Ex: 6 portas" value={tamanhoMovel} onChangeText={setTamanhoMovel} />
        </View>

        <View style={styles.inputContainer}>
          <Text style={styles.label}>Foto do Móvel (Opcional)</Text>
          
          {!urlFoto ? (
            <View style={styles.fotoButtonsContainer}>
              <TouchableOpacity style={styles.fotoButton} onPress={tirarFoto}>
                <Text style={styles.fotoButtonText}>📷 Tirar Foto</Text>
              </TouchableOpacity>
              <TouchableOpacity style={styles.fotoButton} onPress={escolherDaGaleria}>
                <Text style={styles.fotoButtonText}>🖼️ Galeria</Text>
              </TouchableOpacity>
            </View>
          ) : (
            <View>
              <Image source={{ uri: urlFoto }} style={styles.imagePreview} />
              <TouchableOpacity onPress={() => setUrlFoto('')}>
                <Text style={styles.removerFotoText}>Remover Foto</Text>
              </TouchableOpacity>
            </View>
          )}
        </View>

        {/* NOVOS CAMPOS DE ENDEREÇO DIVIDIDOS */}
        <View style={styles.inputContainer}>
          <Text style={styles.label}>Rua</Text>
          <TextInput style={styles.input} placeholder="Ex: Av. Principal" value={rua} onChangeText={setRua} />
        </View>

        <View style={{ flexDirection: 'row', justifyContent: 'space-between' }}>
          <View style={[styles.inputContainer, { flex: 0.48 }]}>
            <Text style={styles.label}>Número</Text>
            <TextInput style={styles.input} placeholder="Ex: 123" value={numero} onChangeText={setNumero} keyboardType="numeric" />
          </View>

          <View style={[styles.inputContainer, { flex: 0.48 }]}>
            <Text style={styles.label}>Bairro</Text>
            <TextInput style={styles.input} placeholder="Ex: Centro" value={bairro} onChangeText={setBairro} />
          </View>
        </View>

        {/* CAMPO BLOQUEADO EXIBINDO A REGIÃO SALVA DO USUÁRIO */}
        <View style={styles.inputContainer}>
          <Text style={styles.label}>Cidade / Estado</Text>
          <TextInput 
            style={[styles.input, { backgroundColor: '#f1f5f9', color: '#64748b' }]} 
            value={user?.regiao || 'Região não informada'} 
            editable={false} 
          />
        </View>

        <TouchableOpacity style={[styles.button, enviando && { opacity: 0.6 }]} onPress={handleSolicitar} disabled={enviando}>
          <Text style={styles.buttonText}>{enviando ? 'Enviando...' : 'Encontrar Montador'}</Text>
        </TouchableOpacity>

      </ScrollView>
    </KeyboardAvoidingView>
  );
}