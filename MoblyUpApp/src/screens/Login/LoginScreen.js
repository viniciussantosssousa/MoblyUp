import React, { useState, useContext } from 'react';
import { View, Text, TextInput, TouchableOpacity, KeyboardAvoidingView, Platform, Alert } from 'react-native';
import { useNavigation } from '@react-navigation/native';
import { AuthContext } from '../../contexts/AuthContext';
import { styles } from './LoginStyles'; // <-- IMPORTAÇÃO CORRIGIDA AQUI!

export default function LoginScreen() {
  const navigation = useNavigation();
  const { signIn } = useContext(AuthContext);

  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');

  const handleLogin = async () => {
    if (!email || !senha) {
      const msg = 'Preencha e-mail e senha.';
      Platform.OS === 'web' ? window.alert(msg) : Alert.alert('Atenção', msg);
      return;
    }

    try {
      await signIn(email, senha);
    } catch (error) {
      let erroMsg = 'Erro ao fazer login. Verifique suas credenciais.';
      if (error.code === 'auth/user-not-found' || error.code === 'auth/wrong-password' || error.code === 'auth/invalid-credential') {
        erroMsg = 'E-mail ou senha incorretos.';
      }
      Platform.OS === 'web' ? window.alert(erroMsg) : Alert.alert('Erro', erroMsg);
    }
  };

  return (
    <KeyboardAvoidingView style={styles.container} behavior={Platform.OS === "ios" ? "padding" : "height"}>
      <View style={styles.content}>
        <Text style={styles.title}>MoblyUp</Text>
        <Text style={styles.subtitle}>Faça login para continuar.</Text>

        <View style={styles.inputContainer}>
          <Text style={styles.label}>E-mail</Text>
          <TextInput style={styles.input} placeholder="seu@email.com" keyboardType="email-address" autoCapitalize="none" value={email} onChangeText={setEmail} />
        </View>

        <View style={styles.inputContainer}>
          <Text style={styles.label}>Senha</Text>
          <TextInput style={styles.input} placeholder="********" secureTextEntry value={senha} onChangeText={setSenha} />
        </View>

        <TouchableOpacity style={styles.primaryButton} onPress={handleLogin}>
          <Text style={styles.primaryButtonText}>Entrar</Text>
        </TouchableOpacity>

        <TouchableOpacity style={styles.linkButton} onPress={() => navigation.navigate('Cadastro')}>
          <Text style={styles.linkText}>Ainda não tem uma conta? Cadastre-se</Text>
        </TouchableOpacity>
      </View>
    </KeyboardAvoidingView>
  );
}