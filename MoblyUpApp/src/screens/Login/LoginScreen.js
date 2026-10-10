import React, { useState, useContext } from 'react';
import { View, Text, TextInput, TouchableOpacity, KeyboardAvoidingView, Platform, ActivityIndicator } from 'react-native';
import { useNavigation } from '@react-navigation/native';
import { AuthContext } from '../../contexts/AuthContext';
import { styles } from './LoginStyles';
import { avisar } from '../../services/alerta';
import { traduzirErro } from '../../services/erros';

export default function LoginScreen() {
  const navigation = useNavigation();
  const { signIn, resetPassword } = useContext(AuthContext);

  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [entrando, setEntrando] = useState(false);

  const handleLogin = async () => {
    if (!email || !senha) {
      avisar('Atenção', 'Preencha e-mail e senha.');
      return;
    }

    try {
      setEntrando(true);
      await signIn(email.trim(), senha);
    } catch (error) {
      console.log('Erro no login:', error);
      avisar('Erro', traduzirErro(error, 'Erro ao fazer login. Verifique suas credenciais.'));
    } finally {
      setEntrando(false);
    }
  };

  // O Firebase envia um e-mail com o link para criar uma nova senha
  const handleEsqueciSenha = async () => {
    if (!email.trim()) {
      avisar('Esqueci minha senha', 'Digite seu e-mail no campo acima e toque novamente em "Esqueci minha senha".');
      return;
    }

    try {
      await resetPassword(email);
      avisar('E-mail enviado', `Enviamos um link para redefinir sua senha para ${email.trim()}. Confira também a caixa de spam.`);
    } catch (error) {
      avisar('Erro', traduzirErro(error, 'Não foi possível enviar o e-mail de redefinição.'));
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
          <TextInput style={styles.input} placeholder="********" secureTextEntry value={senha} onChangeText={setSenha} onSubmitEditing={handleLogin} />
        </View>

        <TouchableOpacity style={styles.forgotButton} onPress={handleEsqueciSenha}>
          <Text style={styles.forgotText}>Esqueci minha senha</Text>
        </TouchableOpacity>

        <TouchableOpacity style={[styles.primaryButton, entrando && { opacity: 0.6 }]} onPress={handleLogin} disabled={entrando}>
          {entrando ? <ActivityIndicator color="#FFFFFF" /> : <Text style={styles.primaryButtonText}>Entrar</Text>}
        </TouchableOpacity>

        <TouchableOpacity style={styles.linkButton} onPress={() => navigation.navigate('Cadastro')}>
          <Text style={styles.linkText}>Ainda não tem uma conta? <Text style={styles.linkTextBold}>Cadastre-se</Text></Text>
        </TouchableOpacity>
      </View>
    </KeyboardAvoidingView>
  );
}
