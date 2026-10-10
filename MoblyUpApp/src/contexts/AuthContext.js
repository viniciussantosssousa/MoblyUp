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

export const AuthContext = createContext({});

// Garante que o usuário do Firebase também exista no SQL Server (a API não duplica).
// Os pedidos dependem disso, pois o ClienteId é chave estrangeira para Usuarios.
const sincronizarComApi = async (userData) => {
  try {
    await api.post('/usuarios', {
      id: userData.id,
      nome: userData.nome,
      email: userData.email,
      tipo: userData.tipo,
    });
  } catch (error) {
    console.log('Erro ao sincronizar usuário com a API:', error);
  }
};

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const unsubscribe = onAuthStateChanged(auth, async (firebaseUser) => {
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
      setLoading(false);
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
    await setDoc(doc(db, 'usuarios', uid), {
      nome: nome,
      email: email,
      tipo: tipoUsuario,
      regiao: regiao
    });

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
      console.log("Perfil não encontrado no banco de dados!");
    }
  };

  const signOut = async () => {
    await firebaseSignOut(auth);
    await AsyncStorage.removeItem('@MoblyUp:user');
    setUser(null);
  };

  // PERFIL: atualiza nome (Firestore + API), chave Pix (API, só montador) e senha (Firebase)
  const updateUser = async ({ nome, chavePix, senha }) => {
    await api.put(`/usuarios/${user.id}`, { nome, chavePix });
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