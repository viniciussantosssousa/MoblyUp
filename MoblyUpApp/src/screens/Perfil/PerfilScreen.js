import React, { useState, useContext, useCallback } from 'react';
import {
  View, Text, TextInput, TouchableOpacity, ScrollView, KeyboardAvoidingView,
  Platform, Image, ActivityIndicator, RefreshControl,
} from 'react-native';
import * as ImagePicker from 'expo-image-picker';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation, useFocusEffect } from '@react-navigation/native';
import { styles } from './PerfilStyles';
import { colors } from '../../theme/colors';
import { AuthContext } from '../../contexts/AuthContext';
import api, { enviarImagem } from '../../services/api';
import { avisar, confirmar } from '../../services/alerta';
import { traduzirErro } from '../../services/erros';
import { formatarData, formatarMoeda } from '../../services/statusPedido';

// (34) 99999-8888
const formatarTelefone = (valor) => {
  const n = (valor || '').replace(/\D/g, '').slice(0, 11);
  if (n.length <= 2) return n;
  if (n.length <= 6) return `(${n.slice(0, 2)}) ${n.slice(2)}`;
  if (n.length <= 10) return `(${n.slice(0, 2)}) ${n.slice(2, 6)}-${n.slice(6)}`;
  return `(${n.slice(0, 2)}) ${n.slice(2, 7)}-${n.slice(7)}`;
};

export default function PerfilScreen() {
  const navigation = useNavigation();
  const { user, signOut, updateUser, changePassword, deleteAccount } = useContext(AuthContext);
  const isMontador = user?.tipo === 'montador';

  const [perfil, setPerfil] = useState(null);
  const [carregando, setCarregando] = useState(true);

  // Dados pessoais
  const [nome, setNome] = useState(user?.nome || '');
  const [telefone, setTelefone] = useState('');
  const [chavePix, setChavePix] = useState('');
  const [cep, setCep] = useState('');
  const [novaRegiao, setNovaRegiao] = useState('');
  const [buscandoCep, setBuscandoCep] = useState(false);
  const [salvando, setSalvando] = useState(false);
  const [enviandoFoto, setEnviandoFoto] = useState(false);

  // Segurança
  const [senhaAtual, setSenhaAtual] = useState('');
  const [novaSenha, setNovaSenha] = useState('');
  const [confirmacaoSenha, setConfirmacaoSenha] = useState('');
  const [trocandoSenha, setTrocandoSenha] = useState(false);

  // Exclusão de conta
  const [mostrarExclusao, setMostrarExclusao] = useState(false);
  const [senhaExclusao, setSenhaExclusao] = useState('');
  const [excluindo, setExcluindo] = useState(false);

  const carregarPerfil = useCallback(async () => {
    try {
      setCarregando(true);
      const { data } = await api.get('/usuarios/me');
      setPerfil(data);
      setNome(data.nome);
      setTelefone(formatarTelefone(data.telefone));
      setChavePix(data.chavePix || '');
    } catch (error) {
      avisar('Erro', traduzirErro(error, 'Não foi possível carregar o seu perfil.'));
    } finally {
      setCarregando(false);
    }
  }, []);

  useFocusEffect(useCallback(() => { carregarPerfil(); }, [carregarPerfil]));

  // Trocar a região pelo CEP (mesma busca do cadastro, via ViaCEP)
  const handleCepChange = async (texto) => {
    const cepLimpo = texto.replace(/\D/g, '').slice(0, 8);
    setCep(cepLimpo);
    setNovaRegiao('');
    if (cepLimpo.length !== 8) return;

    try {
      setBuscandoCep(true);
      const resposta = await fetch(`https://viacep.com.br/ws/${cepLimpo}/json/`);
      const dados = await resposta.json();
      if (dados.erro) {
        avisar('Atenção', 'CEP não encontrado.');
      } else {
        setNovaRegiao(`${dados.localidade} - ${dados.uf}`);
      }
    } catch (error) {
      avisar('Erro', 'Falha na conexão ao buscar o CEP.');
    } finally {
      setBuscandoCep(false);
    }
  };

  const handleTrocarFoto = async () => {
    const { status } = await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (status !== 'granted') {
      avisar('Permissão Negada', 'Precisamos de acesso à galeria para escolher a foto.');
      return;
    }

    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ['images'],
      allowsEditing: true,
      aspect: [1, 1],
      quality: 0.4,
    });
    if (result.canceled) return;

    try {
      setEnviandoFoto(true);
      const url = await enviarImagem(result.assets[0].uri);
      await updateUser({ nome: perfil.nome, fotoPerfilUrl: url });
      setPerfil((atual) => ({ ...atual, fotoPerfilUrl: url }));
    } catch (error) {
      avisar('Erro', traduzirErro(error, 'Não foi possível atualizar a foto.'));
    } finally {
      setEnviandoFoto(false);
    }
  };

  const handleSalvar = async () => {
    if (!nome.trim()) {
      avisar('Atenção', 'O nome não pode ficar vazio.');
      return;
    }
    if (cep && !novaRegiao) {
      avisar('Atenção', 'Digite um CEP válido ou apague o campo para manter a região atual.');
      return;
    }

    try {
      setSalvando(true);
      await updateUser({
        nome: nome.trim(),
        telefone,
        regiao: novaRegiao || undefined,
        // Campo vazio mantém a chave atual (a API não aceita chave Pix em branco)
        chavePix: isMontador && chavePix.trim() ? chavePix : undefined,
      });
      setCep('');
      setNovaRegiao('');
      await carregarPerfil();
      avisar('Sucesso', 'Seus dados foram atualizados.');
    } catch (error) {
      avisar('Erro', traduzirErro(error, 'Não foi possível salvar as alterações.'));
    } finally {
      setSalvando(false);
    }
  };

  const handleTrocarSenha = async () => {
    if (!senhaAtual || !novaSenha) {
      avisar('Atenção', 'Preencha a senha atual e a nova senha.');
      return;
    }
    if (novaSenha.length < 6) {
      avisar('Atenção', 'A nova senha deve ter pelo menos 6 caracteres.');
      return;
    }
    if (novaSenha !== confirmacaoSenha) {
      avisar('Atenção', 'A confirmação não confere com a nova senha.');
      return;
    }

    try {
      setTrocandoSenha(true);
      await changePassword(senhaAtual, novaSenha);
      setSenhaAtual('');
      setNovaSenha('');
      setConfirmacaoSenha('');
      avisar('Senha alterada', 'Use a nova senha no próximo login.');
    } catch (error) {
      avisar('Erro', traduzirErro(error, 'Não foi possível alterar a senha.'));
    } finally {
      setTrocandoSenha(false);
    }
  };

  const handleLogout = async () => {
    const ok = await confirmar('Sair', 'Tem certeza que deseja sair do aplicativo?', 'Sair');
    if (ok) await signOut();
  };

  const handleExcluirConta = async () => {
    if (!senhaExclusao) {
      avisar('Atenção', 'Digite sua senha para confirmar a exclusão.');
      return;
    }
    const ok = await confirmar(
      'Excluir conta',
      'Esta ação é permanente: seus dados pessoais serão apagados e você não poderá mais entrar com esta conta. Continuar?',
      'Excluir'
    );
    if (!ok) return;

    try {
      setExcluindo(true);
      await deleteAccount(senhaExclusao);
      avisar('Conta excluída', 'Sua conta foi removida. Sentiremos sua falta!');
    } catch (error) {
      avisar('Não foi possível excluir', traduzirErro(error, 'Tente novamente mais tarde.'));
    } finally {
      setExcluindo(false);
    }
  };

  const estatisticas = perfil?.estatisticas;

  return (
    <KeyboardAvoidingView style={styles.container} behavior={Platform.OS === 'ios' ? 'padding' : 'height'}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <Ionicons name="arrow-back" size={28} color={colors.primary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Meu Perfil</Text>
      </View>

      <ScrollView
        contentContainerStyle={styles.content}
        keyboardShouldPersistTaps="handled"
        refreshControl={<RefreshControl refreshing={carregando && !!perfil} onRefresh={carregarPerfil} />}
      >
        <View style={styles.profileHeader}>
          <TouchableOpacity onPress={handleTrocarFoto} disabled={enviandoFoto || !perfil} activeOpacity={0.8}>
            <View style={styles.avatarContainer}>
              {perfil?.fotoPerfilUrl ? (
                <Image source={{ uri: perfil.fotoPerfilUrl }} style={styles.avatarImage} />
              ) : (
                <Ionicons name="person" size={48} color={colors.primary} />
              )}
              {enviandoFoto && (
                <View style={styles.avatarOverlay}>
                  <ActivityIndicator color={colors.white} />
                </View>
              )}
            </View>
            <View style={styles.cameraBadge}>
              <Ionicons name="camera" size={14} color={colors.white} />
            </View>
          </TouchableOpacity>

          <Text style={styles.userName}>{perfil?.nome || user?.nome || 'Usuário'}</Text>
          <View style={styles.typeBadge}>
            <Text style={styles.typeBadgeText}>{isMontador ? 'Montador' : 'Cliente'}</Text>
          </View>
          {perfil?.regiao || user?.regiao ? (
            <Text style={styles.userInfo}>
              <Ionicons name="location-outline" size={13} color="#718096" /> {perfil?.regiao || user?.regiao}
            </Text>
          ) : null}
          {perfil?.dataCadastro ? (
            <Text style={styles.userInfo}>No MoblyUp desde {formatarData(perfil.dataCadastro)}</Text>
          ) : null}
        </View>

        {carregando && !perfil ? (
          <ActivityIndicator size="large" color={colors.primary} style={{ marginVertical: 32 }} />
        ) : estatisticas ? (
          <View style={styles.statsCard}>
            <View style={styles.statsRow}>
              <View style={styles.statItem}>
                <Text style={styles.statValue}>{estatisticas.total}</Text>
                <Text style={styles.statLabel}>{isMontador ? 'Serviços' : 'Pedidos'}</Text>
              </View>
              <View style={styles.statItem}>
                <Text style={styles.statValue}>{estatisticas.emAndamento}</Text>
                <Text style={styles.statLabel}>Em andamento</Text>
              </View>
              <View style={styles.statItem}>
                <Text style={styles.statValue}>{estatisticas.concluidos}</Text>
                <Text style={styles.statLabel}>Concluídos</Text>
              </View>
            </View>
            <View style={styles.statsDivider} />
            <View style={styles.statsTotalRow}>
              <Text style={styles.statsTotalLabel}>{isMontador ? 'Total recebido' : 'Total investido'}</Text>
              <Text style={styles.statsTotalValue}>{formatarMoeda(estatisticas.valorTotal)}</Text>
            </View>
          </View>
        ) : null}

        {isMontador && perfil && !perfil.chavePix && (
          <View style={styles.alertCard}>
            <Ionicons name="alert-circle" size={20} color="#C05621" />
            <Text style={styles.alertText}>Cadastre sua chave Pix para poder enviar propostas e receber os pagamentos.</Text>
          </View>
        )}

        <View style={styles.sectionCard}>
          <Text style={styles.sectionTitle}>Dados pessoais</Text>

          <View style={styles.inputContainer}>
            <Text style={styles.label}>Nome completo</Text>
            <TextInput style={styles.input} value={nome} onChangeText={setNome} maxLength={150} />
          </View>

          <View style={styles.inputContainer}>
            <Text style={styles.label}>E-mail</Text>
            <TextInput style={[styles.input, styles.inputDisabled]} value={perfil?.email || user?.email || ''} editable={false} />
          </View>

          <View style={styles.inputContainer}>
            <Text style={styles.label}>CPF</Text>
            <TextInput style={[styles.input, styles.inputDisabled]} value={perfil?.cpfMascarado || 'Não informado'} editable={false} />
            <Text style={styles.helperText}>O CPF não pode ser alterado.</Text>
          </View>

          <View style={styles.inputContainer}>
            <Text style={styles.label}>Telefone / WhatsApp</Text>
            <TextInput
              style={styles.input}
              placeholder="(34) 99999-8888"
              keyboardType="phone-pad"
              value={telefone}
              onChangeText={(t) => setTelefone(formatarTelefone(t))}
            />
          </View>

          <View style={styles.inputContainer}>
            <Text style={styles.label}>Mudar de cidade (CEP)</Text>
            <View style={styles.inputRow}>
              <TextInput
                style={[styles.input, { flex: 1 }]}
                placeholder="Novo CEP (opcional)"
                keyboardType="numeric"
                maxLength={8}
                value={cep}
                onChangeText={handleCepChange}
              />
              {buscandoCep && <ActivityIndicator size="small" color={colors.primary} style={styles.inputSpinner} />}
            </View>
            <Text style={styles.helperText}>
              {novaRegiao
                ? `Nova região: ${novaRegiao}`
                : `Região atual: ${perfil?.regiao || user?.regiao || 'não informada'}`}
            </Text>
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
                maxLength={100}
              />
            </View>
          )}

          <TouchableOpacity style={[styles.saveButton, salvando && styles.buttonDisabled]} onPress={handleSalvar} disabled={salvando}>
            <Text style={styles.saveButtonText}>{salvando ? 'Salvando...' : 'Salvar alterações'}</Text>
          </TouchableOpacity>
        </View>

        <View style={styles.sectionCard}>
          <Text style={styles.sectionTitle}>Alterar senha</Text>

          <View style={styles.inputContainer}>
            <Text style={styles.label}>Senha atual</Text>
            <TextInput style={styles.input} secureTextEntry value={senhaAtual} onChangeText={setSenhaAtual} />
          </View>
          <View style={styles.inputContainer}>
            <Text style={styles.label}>Nova senha</Text>
            <TextInput style={styles.input} placeholder="Mínimo 6 caracteres" secureTextEntry value={novaSenha} onChangeText={setNovaSenha} />
          </View>
          <View style={styles.inputContainer}>
            <Text style={styles.label}>Confirmar nova senha</Text>
            <TextInput style={styles.input} secureTextEntry value={confirmacaoSenha} onChangeText={setConfirmacaoSenha} />
          </View>

          <TouchableOpacity style={[styles.secondaryButton, trocandoSenha && styles.buttonDisabled]} onPress={handleTrocarSenha} disabled={trocandoSenha}>
            <Text style={styles.secondaryButtonText}>{trocandoSenha ? 'Alterando...' : 'Alterar senha'}</Text>
          </TouchableOpacity>
        </View>

        <View style={styles.dangerZone}>
          <TouchableOpacity style={styles.logoutButton} onPress={handleLogout}>
            <Ionicons name="log-out-outline" size={24} color={colors.primary} />
            <Text style={styles.logoutButtonText}>Sair do aplicativo</Text>
          </TouchableOpacity>

          {!mostrarExclusao ? (
            <TouchableOpacity style={styles.deleteButton} onPress={() => setMostrarExclusao(true)}>
              <Ionicons name="trash-outline" size={20} color="#E53E3E" />
              <Text style={styles.deleteButtonText}>Excluir minha conta</Text>
            </TouchableOpacity>
          ) : (
            <View style={styles.deleteCard}>
              <Text style={styles.deleteTitle}>Excluir conta</Text>
              <Text style={styles.deleteText}>
                Seus dados pessoais serão apagados e o histórico de pedidos ficará anônimo. Não é possível excluir com serviços em negociação ou em andamento.
              </Text>
              <TextInput
                style={styles.input}
                placeholder="Digite sua senha para confirmar"
                secureTextEntry
                value={senhaExclusao}
                onChangeText={setSenhaExclusao}
              />
              <View style={styles.deleteActions}>
                <TouchableOpacity style={styles.cancelDeleteButton} onPress={() => { setMostrarExclusao(false); setSenhaExclusao(''); }}>
                  <Text style={styles.cancelDeleteText}>Cancelar</Text>
                </TouchableOpacity>
                <TouchableOpacity style={[styles.confirmDeleteButton, excluindo && styles.buttonDisabled]} onPress={handleExcluirConta} disabled={excluindo}>
                  <Text style={styles.confirmDeleteText}>{excluindo ? 'Excluindo...' : 'Excluir definitivamente'}</Text>
                </TouchableOpacity>
              </View>
            </View>
          )}
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}
