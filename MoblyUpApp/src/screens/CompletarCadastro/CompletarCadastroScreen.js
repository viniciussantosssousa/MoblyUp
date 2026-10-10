import React, { useState, useContext } from 'react';
import { View, Text, TextInput, TouchableOpacity, KeyboardAvoidingView, Platform, ActivityIndicator, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { AuthContext } from '../../contexts/AuthContext';
import { colors } from '../../theme/colors';
import { avisar } from '../../services/alerta';
import { traduzirErro } from '../../services/erros';
import { formatarCpf, cpfValido, somenteNumeros } from '../../services/cpf';

// Contas criadas antes do CPF ser obrigatório caem aqui até informarem o CPF
export default function CompletarCadastroScreen() {
  const { user, completarCpf, signOut } = useContext(AuthContext);

  const [cpf, setCpf] = useState('');
  const [salvando, setSalvando] = useState(false);

  const cpfInvalido = somenteNumeros(cpf).length === 11 && !cpfValido(cpf);

  const handleSalvar = async () => {
    if (!cpfValido(cpf)) {
      avisar('Atenção', 'Digite um CPF válido.');
      return;
    }

    try {
      setSalvando(true);
      await completarCpf(somenteNumeros(cpf));
    } catch (error) {
      avisar('Erro', traduzirErro(error, 'Não foi possível salvar o CPF.'));
    } finally {
      setSalvando(false);
    }
  };

  return (
    <KeyboardAvoidingView style={styles.container} behavior={Platform.OS === 'ios' ? 'padding' : 'height'}>
      <View style={styles.content}>
        <View style={styles.icone}>
          <Ionicons name="id-card-outline" size={40} color={colors.primary} />
        </View>

        <Text style={styles.title}>Complete seu cadastro</Text>
        <Text style={styles.subtitle}>
          Olá, {(user?.nome || '').split(' ')[0]}! Para garantir a segurança de clientes e montadores, o MoblyUp agora exige o CPF de todos os usuários.
        </Text>

        <Text style={styles.label}>CPF</Text>
        <TextInput
          style={[styles.input, cpfInvalido && styles.inputErro]}
          placeholder="000.000.000-00"
          keyboardType="numeric"
          maxLength={14}
          value={cpf}
          onChangeText={(texto) => setCpf(formatarCpf(texto))}
          onSubmitEditing={handleSalvar}
        />
        {cpfInvalido && <Text style={styles.erroTexto}>CPF inválido. Confira os números.</Text>}
        <Text style={styles.ajuda}>O CPF não pode ser alterado depois e não é exibido para outros usuários.</Text>

        <TouchableOpacity style={[styles.button, salvando && { opacity: 0.6 }]} onPress={handleSalvar} disabled={salvando}>
          {salvando ? <ActivityIndicator color={colors.white} /> : <Text style={styles.buttonText}>Salvar e continuar</Text>}
        </TouchableOpacity>

        <TouchableOpacity style={styles.sairButton} onPress={signOut} disabled={salvando}>
          <Text style={styles.sairText}>Sair da conta</Text>
        </TouchableOpacity>
      </View>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background, justifyContent: 'center', padding: 24 },
  content: { width: '100%', maxWidth: 420, alignSelf: 'center' },
  icone: {
    width: 72, height: 72, borderRadius: 36, backgroundColor: '#EDF2F7',
    alignItems: 'center', justifyContent: 'center', alignSelf: 'center', marginBottom: 20,
  },
  title: { fontSize: 24, fontWeight: 'bold', color: colors.primary, textAlign: 'center', marginBottom: 8 },
  subtitle: { fontSize: 14, color: '#718096', textAlign: 'center', marginBottom: 32, lineHeight: 20 },
  label: { fontSize: 14, fontWeight: '600', color: colors.primary, marginBottom: 8 },
  input: {
    backgroundColor: colors.white, borderWidth: 1, borderColor: colors.border, borderRadius: 8,
    paddingHorizontal: 16, paddingVertical: 12, fontSize: 18, color: colors.text, letterSpacing: 1,
  },
  inputErro: { borderColor: '#E53E3E' },
  erroTexto: { fontSize: 12, color: '#E53E3E', marginTop: 4 },
  ajuda: { fontSize: 12, color: '#A0AEC0', marginTop: 8 },
  button: { backgroundColor: colors.primary, borderRadius: 8, paddingVertical: 16, alignItems: 'center', marginTop: 24 },
  buttonText: { color: colors.white, fontSize: 16, fontWeight: 'bold' },
  sairButton: { alignItems: 'center', padding: 16, marginTop: 8 },
  sairText: { color: '#718096', fontSize: 14, fontWeight: '600' },
});
