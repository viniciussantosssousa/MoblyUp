import React, { createContext, useState, useEffect, useRef } from 'react';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { auth, db } from '../services/firebaseConfig'; // <-- Importando o db
import { 
  createUserWithEmailAndPassword, 
  signInWithEmailAndPassword, 
  signOut as firebaseSignOut,
  onAuthStateChanged,
  updatePassword,
  reauthenticateWithCredential,
  EmailAuthProvider,
  sendPasswordResetEmail,
  deleteUser
} from 'firebase/auth';
import { doc, setDoc, getDoc, updateDoc, deleteDoc } from 'firebase/firestore'; // <-- Funções do banco de dados
import api from '../services/api';
import { registrarNotificacoes, removerNotificacoes } from '../services/notificacoes';

export const AuthContext = createContext({});

// Garante que o usuário do Firebase também exista no SQL Server (a API não duplica).
// Os pedidos dependem disso, pois o ClienteId é chave estrangeira para Usuarios.
// O ID do usuário vai no token (api.js), não no corpo da requisição.
const sincronizarComApi = async (userData) => {
  try {
    await api.post('/usuarios', {
      nome: userData.nome,
      email: userData.email,
      tipo: userData.tipo,
      regiao: userData.regiao, // usada para avisar montadores sobre pedidos na cidade
    });

    // Com o usuário já na API, registra este aparelho para receber notificações push
    await registrarNotificacoes();
  } catch (error) {
    console.log('Erro ao sincronizar usuário com a API:', error);
  }
};

// Dados que ficam só na API: foto de perfil e se o CPF já foi cadastrado.
// Retorna {} se a API não responder (o app segue com o que tem salvo).
const carregarDadosDaApi = async () => {
  try {
    const { data } = await api.get('/usuarios/me');
    return { fotoPerfilUrl: data.fotoPerfilUrl, cpfCadastrado: data.cpfCadastrado };
  } catch (error) {
    console.log('Não foi possível carregar o perfil da API:', error);
    return {};
  }
};

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  // Durante o cadastro, quem sincroniza com a API é o próprio signUp (com o CPF)
  const cadastrandoRef = useRef(false);

  const salvarUsuario = async (userData) => {
    setUser(userData);
    await AsyncStorage.setItem('@MoblyUp:user', JSON.stringify(userData));
  };

  useEffect(() => {
    const unsubscribe = onAuthStateChanged(auth, async (firebaseUser) => {
      try {
        if (cadastrandoRef.current) return;

        if (firebaseUser) {
          const storedUser = await AsyncStorage.getItem('@MoblyUp:user');
          let userData = null;

          if (storedUser) {
            userData = JSON.parse(storedUser);
            setUser(userData);
          } else {
            // Se não tem no AsyncStorage, busca do Firestore
            const docSnap = await getDoc(doc(db, 'usuarios', firebaseUser.uid));
            if (docSnap.exists()) {
              userData = { id: firebaseUser.uid, ...docSnap.data() };
              await salvarUsuario(userData);
            }
          }

          if (userData) {
            // Em segundo plano: o app abre com os dados salvos e atualiza quando a API responder
            const dadosLocais = userData;
            (async () => {
              await sincronizarComApi(dadosLocais);
              const dadosApi = await carregarDadosDaApi();
              // Se o usuário saiu enquanto a API respondia, não "reloga" a tela
              const aindaLogado = auth.currentUser?.uid === dadosLocais.id;
              if (aindaLogado && Object.keys(dadosApi).length) await salvarUsuario({ ...dadosLocais, ...dadosApi });
            })();
          }
        } else {
          setUser(null);
        }
      } catch (error) {
        // Ex.: regras do Firestore bloqueando a leitura. Sem isso o app ficaria preso no carregamento.
        console.log('Erro ao restaurar a sessão:', error);
      } finally {
        setLoading(false);
      }
    });

    return unsubscribe;
  }, []);

  // CADASTRO: Firebase (login) -> Firestore (perfil) -> API (dados completos, incluindo o CPF).
  // Se qualquer etapa falhar, desfaz as anteriores para não deixar conta pela metade.
  const signUp = async (email, senha, nome, tipoUsuario, regiao, cpf) => {
    cadastrandoRef.current = true;
    try {
      // 1. Cria a conta (E-mail e Senha)
      const userCredential = await createUserWithEmailAndPassword(auth, email, senha);
      const uid = userCredential.user.uid;
      const desfazerLogin = () => userCredential.user.delete().catch(() => {});

      // 2. Salva o perfil no Firestore, na coleção 'usuarios', usando o UID como chave
      try {
        await setDoc(doc(db, 'usuarios', uid), { nome, email, tipo: tipoUsuario, regiao });
      } catch (error) {
        await desfazerLogin();
        throw error;
      }

      // 3. Cria o usuário na API com o CPF (a API valida o CPF e recusa CPF já usado)
      try {
        await api.post('/usuarios', { nome, email, tipo: tipoUsuario, regiao, cpf });
      } catch (error) {
        await deleteDoc(doc(db, 'usuarios', uid)).catch(() => {});
        await desfazerLogin();
        throw error;
      }

      await registrarNotificacoes();

      // 4. Atualiza o app
      await salvarUsuario({ id: uid, nome, email, tipo: tipoUsuario, regiao, cpfCadastrado: true });
    } finally {
      cadastrandoRef.current = false;
    }
  };

  // LOGIN: Valida a senha e puxa o perfil da nuvem
  const signIn = async (email, senha) => {
    // 1. Faz o login
    const userCredential = await signInWithEmailAndPassword(auth, email, senha);
    const uid = userCredential.user.uid;

    // 2. Vai no Firestore e busca quem é esse usuário (Região, Nome, Tipo)
    const docRef = doc(db, 'usuarios', uid);
    const docSnap = await getDoc(docRef);

    if (docSnap.exists()) {
      const userData = { id: uid, ...docSnap.data() };
      // Cobre contas criadas antes da sincronização existir
      await sincronizarComApi(userData);

      // Foto de perfil e situação do CPF ficam só na API
      await salvarUsuario({ ...userData, ...(await carregarDadosDaApi()) });
    } else {
      // Login existe mas o perfil não (cadastro interrompido): não deixa o usuário "meio logado"
      await firebaseSignOut(auth);
      throw { code: 'perfil-nao-encontrado' };
    }
  };

  const limparSessaoLocal = async () => {
    await AsyncStorage.removeItem('@MoblyUp:user');
    setUser(null);
  };

  const signOut = async () => {
    try {
      // Precisa ser antes do logout do Firebase, enquanto ainda há token para a API
      await removerNotificacoes();
      await firebaseSignOut(auth);
    } finally {
      // Mesmo sem internet o usuário sai do app
      await limparSessaoLocal();
    }
  };

  // PERFIL: dados na API (fonte principal) e o essencial no Firestore (nome e região do login)
  const updateUser = async ({ nome, telefone, regiao, chavePix, fotoPerfilUrl }) => {
    await api.put('/usuarios/me', { nome, telefone, regiao, chavePix, fotoPerfilUrl });

    const dadosFirestore = { nome };
    if (regiao) dadosFirestore.regiao = regiao;
    await updateDoc(doc(db, 'usuarios', user.id), dadosFirestore);

    const userData = {
      ...user,
      nome,
      ...(regiao ? { regiao } : {}),
      ...(fotoPerfilUrl !== undefined ? { fotoPerfilUrl } : {}),
    };
    setUser(userData);
    await AsyncStorage.setItem('@MoblyUp:user', JSON.stringify(userData));
  };

  // O Firebase exige login recente para ações sensíveis: confirmamos com a senha atual
  const reautenticar = async (senhaAtual) => {
    const credencial = EmailAuthProvider.credential(auth.currentUser.email, senhaAtual);
    await reauthenticateWithCredential(auth.currentUser, credencial);
  };

  // Completar cadastro: contas criadas antes do CPF ser obrigatório
  const completarCpf = async (cpf) => {
    await api.put('/usuarios/me/cpf', { cpf });
    await salvarUsuario({ ...user, cpfCadastrado: true });
  };

  const changePassword = async (senhaAtual, novaSenha) => {
    await reautenticar(senhaAtual);
    await updatePassword(auth.currentUser, novaSenha);
  };

  // "Esqueci minha senha": o Firebase envia um link de redefinição para o e-mail
  const resetPassword = async (email) => {
    await sendPasswordResetEmail(auth, email.trim());
  };

  // Exclui a conta: API (anonimiza os dados e libera o e-mail), Firestore e o login no Firebase.
  // A API recusa se houver serviço em negociação ou em andamento.
  const deleteAccount = async (senhaAtual) => {
    await reautenticar(senhaAtual);
    await api.delete('/usuarios/me');
    await deleteDoc(doc(db, 'usuarios', user.id)).catch((error) =>
      console.log('Perfil do Firestore não foi removido:', error));
    await deleteUser(auth.currentUser);
    await limparSessaoLocal();
  };

  return (
    <AuthContext.Provider value={{
      signed: !!user, user, loading,
      signIn, signUp, signOut,
      updateUser, completarCpf, changePassword, resetPassword, deleteAccount,
    }}>
      {children}
    </AuthContext.Provider>
  );
};