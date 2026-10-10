import React, { useState, useContext, useEffect } from 'react';
import { View, Text, TextInput, TouchableOpacity, ScrollView, Alert, KeyboardAvoidingView, Platform } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import { styles } from './PerfilStyles';
import { colors } from '../../theme/colors';
import { AuthContext } from '../../contexts/AuthContext';
import api, { mensagemDeErro } from '../../services/api';
import { avisar } from '../../services/alerta';

export default function PerfilScreen() {
  const navigation = useNavigation();
  const { user, signOut, updateUser } = useContext(AuthContext);
  const isMontador = user?.tipo === 'montador';

  const [nome, setNome] = useState(user?.nome || '');
  const [chavePix, setChavePix] = useState('');
  const [senha, setSenha] = useState('');
  const [salvando, setSalvando] = useState(false);

  // A chave Pix fica só no back-end (usada para os repasses ao montador)
  useEffect(() => {
    if (!isMontador || !user?.id) return;
    api.get(`/usuarios/${user.id}`)
      .then(response => setChavePix(response.data.chavePix || ''))
      .catch(error => console.log('Erro ao carregar perfil:', error));
  }, [isMontador, user?.id]);

  const handleSalvar = async () => {
    if (!nome.trim()) {
      avisar('Atenção', 'O nome não pode ficar vazio.');
      return;
    }
    if (senha && senha.length < 6) {
      avisar('Atenção', 'A nova senha deve ter pelo menos 6 caracteres.');
      return;
    }

    try {
      setSalvando(true);
      await updateUser({ nome: nome.trim(), chavePix: isMontador ? chavePix : null, senha });
      avisar('Sucesso', 'Seus dados foram atualizados com sucesso!');
      navigation.goBack();
    } catch (error) {
      const msg = error?.code === 'auth/requires-recent-login'
        ? 'Por segurança, saia e entre novamente no app antes de trocar a senha.'
        : mensagemDeErro(error, 'Não foi possível salvar as alterações.');
      avisar('Erro', msg);
    } finally {
      setSalvando(false);
    }
  };

  const handleLogout = async () => {
    if (Platform.OS === 'web') {
      const confirma = window.confirm('Tem certeza que deseja sair do aplicativo?');
      if (confirma) {
        await signOut();
      }
    } else {
      Alert.alert('Sair', 'Tem certeza que deseja sair do aplicativo?', [
        { text: 'Cancelar', style: 'cancel' },
        { text: 'Sair', style: 'destructive', onPress: async () => await signOut() }
      ]);
    }
  };

  const handleExcluirConta = () => {
    if (Platform.OS === 'web') {
      const confirma = window.confirm('Deseja excluir sua conta permanentemente?');
      if (confirma) {
        window.alert('Sua conta foi removida.');
        signOut();
      }
    } else {
      Alert.alert('Atenção!', 'Deseja excluir sua conta permanentemente?', [
        { text: 'Cancelar', style: 'cancel' },
        { text: 'Excluir', style: 'destructive', onPress: async () => {
            Alert.alert('Conta excluída', 'Sua conta foi removida.');
            await signOut();
          }
        }
      ]);
    }
  };

  return (
    <KeyboardAvoidingView style={styles.container} behavior={Platform.OS === "ios" ? "padding" : "height"}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <Ionicons name="arrow-back" size={28} color={colors.primary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Meu Perfil</Text>
      </View>

      <ScrollView contentContainerStyle={styles.content}>
        <View style={styles.profileHeader}>
          <View style={styles.avatarContainer}>
            <Ionicons name="person" size={48} color={colors.primary} />
          </View>
          <Text style={styles.userName}>{user?.nome || 'Usuário'}</Text>
          <Text style={styles.userType}>{user?.tipo === 'cliente' ? 'Cliente' : 'Montador'}</Text>
        </View>

        <View style={styles.sectionCard}>
          <View style={styles.inputContainer}>
            <Text style={styles.label}>Nome Completo</Text>
            <TextInput style={styles.input} value={nome} onChangeText={setNome} />
          </View>

          <View style={styles.inputContainer}>
            <Text style={styles.label}>E-mail</Text>
            <TextInput style={[styles.input, { backgroundColor: '#f1f5f9', color: '#64748b' }]} value={user?.email || ''} editable={false} />
          </View>

          {isMontador && (
            <View style={styles.inputContainer}>
              <Text style={styles.label}>Chave Pix (para receber os repasses)</Text>
              <TextInput
                style={styles.input}
                placeholder="CPF, e-mail, telefone ou chave aleatória"
                autoCapitalize="none"
                value={chavePix}
                onChangeText={setChavePix}
              />
            </View>
          )}

          <View style={styles.inputContainer}>
            <Text style={styles.label}>Nova Senha</Text>
            <TextInput style={styles.input} placeholder="Digite para alterar..." secureTextEntry value={senha} onChangeText={setSenha} />
          </View>

          <TouchableOpacity style={[styles.saveButton, salvando && { opacity: 0.6 }]} onPress={handleSalvar} disabled={salvando}>
            <Text style={styles.saveButtonText}>{salvando ? 'Salvando...' : 'Salvar Alterações'}</Text>
          </TouchableOpacity>
        </View>

        <View style={styles.dangerZone}>
          <TouchableOpacity style={styles.logoutButton} onPress={handleLogout}>
            <Ionicons name="log-out-outline" size={24} color={colors.primary} />
            <Text style={styles.logoutButtonText}>Sair do Aplicativo</Text>
          </TouchableOpacity>

          <TouchableOpacity style={styles.deleteButton} onPress={handleExcluirConta}>
            <Ionicons name="trash-outline" size={20} color="#E53E3E" />
            <Text style={styles.deleteButtonText}>Excluir minha conta</Text>
          </TouchableOpacity>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}