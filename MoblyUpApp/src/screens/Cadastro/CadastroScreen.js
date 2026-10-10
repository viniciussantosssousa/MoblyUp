import React, { useState, useContext } from 'react';
import { View, Text, TextInput, TouchableOpacity, KeyboardAvoidingView, Platform, Alert, ScrollView, ActivityIndicator } from 'react-native';
import { useNavigation } from '@react-navigation/native';
import { styles } from './CadastroStyles';
import { AuthContext } from '../../contexts/AuthContext';
import { traduzirErro } from '../../services/erros';

export default function CadastroScreen() {
  const navigation = useNavigation();
  const { signUp } = useContext(AuthContext); 

  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [tipo, setTipo] = useState('cliente');
  
  // Novos estados para o CEP e a Região automática
  const [cep, setCep] = useState('');
  const [cidadeEstado, setCidadeEstado] = useState('');
  const [buscandoCep, setBuscandoCep] = useState(false);
  const [cadastrando, setCadastrando] = useState(false);

  // Função que busca o CEP automaticamente
  const handleCepChange = async (cepDigitado) => {
    // Remove tudo que não for número
    const cepLimpo = cepDigitado.replace(/\D/g, '');
    setCep(cepLimpo);

    // Quando chegar em 8 números, faz a busca na API do ViaCEP
    if (cepLimpo.length === 8) {
      try {
        setBuscandoCep(true);
        // Usamos o fetch padrão do JS pois a URL é externa ao nosso back-end
        const response = await fetch(`https://viacep.com.br/ws/${cepLimpo}/json/`);
        const data = await response.json();

        if (data.erro) {
          Platform.OS === 'web' ? window.alert('CEP não encontrado.') : Alert.alert('Atenção', 'CEP não encontrado.');
          setCidadeEstado('');
        } else {
          // Preenche o campo bloqueado com Cidade - UF
          setCidadeEstado(`${data.localidade} - ${data.uf}`);
        }
      } catch (error) {
        console.log("Erro ao buscar CEP:", error);
        Platform.OS === 'web' ? window.alert('Erro ao buscar o CEP.') : Alert.alert('Erro', 'Falha na conexão ao buscar o CEP.');
      } finally {
        setBuscandoCep(false);
      }
    } else {
      // Limpa a cidade se o usuário apagar o CEP
      setCidadeEstado('');
    }
  };

  const handleCadastro = async () => {
    if (!nome || !email || !senha || !cidadeEstado) {
      const msg = 'Preencha todos os campos e digite um CEP válido.';
      Platform.OS === 'web' ? window.alert(msg) : Alert.alert('Atenção', msg);
      return;
    }

    try {
      setCadastrando(true);
      // Passa a string gerada (ex: "Lagoa Formosa - MG") como a região para o Contexto
      await signUp(email.trim(), senha, nome.trim(), tipo, cidadeEstado);

      const msgSucesso = 'Conta criada com sucesso!';
      Platform.OS === 'web' ? window.alert(msgSucesso) : Alert.alert('Bem-vindo!', msgSucesso);

    } catch (error) {
      console.log('Erro no cadastro:', error);
      const erroMsg = traduzirErro(error, 'Ocorreu um erro ao criar a conta.');
      Platform.OS === 'web' ? window.alert(erroMsg) : Alert.alert('Erro', erroMsg);
    } finally {
      setCadastrando(false);
    }
  };

  return (
    <KeyboardAvoidingView style={styles.container} behavior={Platform.OS === "ios" ? "padding" : "height"}>
      <ScrollView contentContainerStyle={styles.scrollContainer}>
        <Text style={styles.title}>Criar uma conta</Text>
        <Text style={styles.subtitle}>Junte-se à nossa plataforma.</Text>

        <View style={styles.typeContainer}>
          <TouchableOpacity 
            style={[styles.typeButton, tipo === 'cliente' && styles.typeButtonActive]}
            onPress={() => setTipo('cliente')}
          >
            <Text style={[styles.typeText, tipo === 'cliente' && styles.typeTextActive]}>Sou Cliente</Text>
          </TouchableOpacity>

          <TouchableOpacity 
            style={[styles.typeButton, tipo === 'montador' && styles.typeButtonActive]}
            onPress={() => setTipo('montador')}
          >
            <Text style={[styles.typeText, tipo === 'montador' && styles.typeTextActive]}>Sou Montador</Text>
          </TouchableOpacity>
        </View>

        <View style={styles.inputContainer}>
          <Text style={styles.label}>Nome Completo</Text>
          <TextInput style={styles.input} placeholder="Seu nome" value={nome} onChangeText={setNome} />
        </View>

        <View style={styles.inputContainer}>
          <Text style={styles.label}>CEP</Text>
          <TextInput 
            style={styles.input} 
            placeholder="Apenas números (Ex: 38750000)" 
            keyboardType="numeric"
            maxLength={8}
            value={cep} 
            onChangeText={handleCepChange} 
          />
        </View>

        <View style={styles.inputContainer}>
          <Text style={styles.label}>Sua Região</Text>
          <View style={{ flexDirection: 'row', alignItems: 'center' }}>
            <TextInput 
              style={[styles.input, { flex: 1, backgroundColor: '#f1f5f9', color: '#64748b' }]} 
              placeholder="Preenchido automaticamente pelo CEP" 
              value={cidadeEstado} 
              editable={false} // Impede o usuário de digitar e estragar o padrão
            />
            {buscandoCep && <ActivityIndicator size="small" color="#1E3A8A" style={{ position: 'absolute', right: 15 }} />}
          </View>
        </View>

        <View style={styles.inputContainer}>
          <Text style={styles.label}>E-mail</Text>
          <TextInput style={styles.input} placeholder="seu@email.com" keyboardType="email-address" autoCapitalize="none" value={email} onChangeText={setEmail} />
        </View>

        <View style={styles.inputContainer}>
          <Text style={styles.label}>Senha</Text>
          <TextInput style={styles.input} placeholder="Mínimo 6 caracteres" secureTextEntry value={senha} onChangeText={setSenha} />
        </View>

        <TouchableOpacity style={[styles.primaryButton, cadastrando && { opacity: 0.6 }]} onPress={handleCadastro} disabled={cadastrando}>
          <Text style={styles.primaryButtonText}>{cadastrando ? 'Criando conta...' : 'Cadastrar'}</Text>
        </TouchableOpacity>

        <View style={styles.footerTextContainer}>
          <Text style={styles.footerText}>Já tem uma conta?</Text>
          <TouchableOpacity onPress={() => navigation.navigate('Login')}>
            <Text style={styles.footerLink}>Entrar</Text>
          </TouchableOpacity>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}