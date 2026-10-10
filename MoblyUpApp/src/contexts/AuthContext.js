import React, { createContext, useState, useEffect } from 'react';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { auth, db } from '../services/firebaseConfig'; // <-- Importando o db
import { 
  createUserWithEmailAndPassword, 
  signInWithEmailAndPassword, 
  signOut as firebaseSignOut,
  onAuthStateChanged,
  updatePassword
} from 'firebase/auth';
import { doc, setDoc, getDoc, updateDoc } from 'firebase/firestore'; // <-- Funções do banco de dados
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
      setUser(userData);
      await AsyncStorage.setItem('@MoblyUp:user', JSON.stringify(userData));
    } else {
      // Login existe mas o perfil não (cadastro interrompido): não deixa o usuário "meio logado"
      await firebaseSignOut(auth);
      throw { code: 'perfil-nao-encontrado' };
    }
  };

  const signOut = async () => {
    // Precisa ser antes do logout do Firebase, enquanto ainda há token para a API
    await removerNotificacoes();
    await firebaseSignOut(auth);
    await AsyncStorage.removeItem('@MoblyUp:user');
    setUser(null);
  };

  // PERFIL: atualiza nome (Firestore + API), chave Pix (API, só montador) e senha (Firebase)
  const updateUser = async ({ nome, chavePix, senha }) => {
    await api.put('/usuarios/me', { nome, chavePix });
    await updateDoc(doc(db, 'usuarios', user.id), { nome });

    if (senha) {
      await updatePassword(auth.currentUser, senha);
    }

    const userData = { ...user, nome };
    setUser(userData);
    await AsyncStorage.setItem('@MoblyUp:user', JSON.stringify(userData));
  };

  return (
    <AuthContext.Provider value={{ signed: !!user, user, loading, signIn, signUp, signOut, updateUser }}>
      {children}
    </AuthContext.Provider>
  );
};