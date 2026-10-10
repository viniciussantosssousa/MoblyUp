import React, { createContext, useState, useEffect } from 'react';
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

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const unsubscribe = onAuthStateChanged(auth, async (firebaseUser) => {
      try {
        if (firebaseUser) {
          const storedUser = await AsyncStorage.getItem('@MoblyUp:user');
          if (storedUser) {
            const userData = JSON.parse(storedUser);
            setUser(userData);
            sincronizarComApi(userData);
          } else {
            // Se não tem no AsyncStorage, busca do Firestore
            const docRef = doc(db, 'usuarios', firebaseUser.uid);
            const docSnap = await getDoc(docRef);
            if (docSnap.exists()) {
              const userData = { id: firebaseUser.uid, ...docSnap.data() };
              setUser(userData);
              await AsyncStorage.setItem('@MoblyUp:user', JSON.stringify(userData));
              sincronizarComApi(userData);
            }
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

  // CADASTRO: Salva na Autenticação E no Banco de Dados
  const signUp = async (email, senha, nome, tipoUsuario, regiao) => {
    // 1. Cria a conta (E-mail e Senha)
    const userCredential = await createUserWithEmailAndPassword(auth, email, senha);
    const uid = userCredential.user.uid;
    
    const newUser = {
      id: uid,
      nome: nome,
      email: email,
      tipo: tipoUsuario,
      regiao: regiao 
    };

    // 2. Salva o perfil no Firestore, na coleção 'usuarios', usando o UID como chave
    try {
      await setDoc(doc(db, 'usuarios', uid), {
        nome: nome,
        email: email,
        tipo: tipoUsuario,
        regiao: regiao
      });
    } catch (error) {
      // Sem perfil a conta fica inutilizável e o e-mail "preso": desfaz o login recém-criado
      await userCredential.user.delete().catch(() => {});
      throw error;
    }

    // 3. Cria o mesmo usuário no banco da API (SQL Server)
    await sincronizarComApi(newUser);

    // 4. Atualiza o app
    setUser(newUser);
    await AsyncStorage.setItem('@MoblyUp:user', JSON.stringify(newUser));
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

      // A foto de perfil fica só na API
      try {
        const { data } = await api.get('/usuarios/me');
        userData.fotoPerfilUrl = data.fotoPerfilUrl;
      } catch (error) {
        console.log('Não foi possível carregar a foto do perfil:', error);
      }

      setUser(userData);
      await AsyncStorage.setItem('@MoblyUp:user', JSON.stringify(userData));
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
      updateUser, changePassword, resetPassword, deleteAccount,
    }}>
      {children}
    </AuthContext.Provider>
  );
};