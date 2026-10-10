import React, { useState, useContext } from 'react';
import { View, Text, TextInput, TouchableOpacity, ScrollView, Alert, KeyboardAvoidingView, Platform } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import { styles } from './PerfilStyles';
import { colors } from '../../theme/colors';
import { AuthContext } from '../../contexts/AuthContext'; 

export default function PerfilScreen() {
  const navigation = useNavigation();
  const { user, signOut, updateUser } = useContext(AuthContext); 
  
  const [nome, setNome] = useState(user?.nome || '');
  const [email, setEmail] = useState(user?.email || '');
  const [senha, setSenha] = useState('');

  const handleSalvar = async () => {
    // Salva direto, sem pedir código
    await updateUser({ nome, email });
    Alert.alert('Sucesso', 'Seus dados foram atualizados com sucesso!');
    navigation.goBack(); 
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
            <TextInput style={styles.input} value={email} keyboardType="email-address" autoCapitalize="none" onChangeText={setEmail} />
          </View>

          <View style={styles.inputContainer}>
            <Text style={styles.label}>Nova Senha</Text>
            <TextInput style={styles.input} placeholder="Digite para alterar..." secureTextEntry value={senha} onChangeText={setSenha} />
          </View>

          <TouchableOpacity style={styles.saveButton} onPress={handleSalvar}>
            <Text style={styles.saveButtonText}>Salvar Alterações</Text>
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