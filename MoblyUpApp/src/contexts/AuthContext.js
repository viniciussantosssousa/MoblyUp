import React, { createContext, useState, useEffect } from 'react';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { auth, db } from '../services/firebaseConfig'; // <-- Importando o db
import { 
  createUserWithEmailAndPassword, 
  signInWithEmailAndPassword, 
  signOut as firebaseSignOut, 
  onAuthStateChanged 
} from 'firebase/auth';
import { doc, setDoc, getDoc } from 'firebase/firestore'; // <-- Funções do banco de dados

export const AuthContext = createContext({});

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const unsubscribe = onAuthStateChanged(auth, async (firebaseUser) => {
      if (firebaseUser) {
        const storedUser = await AsyncStorage.getItem('@MoblyUp:user');
        if (storedUser) {
          setUser(JSON.parse(storedUser));
        } else {
          // Se não tem no AsyncStorage, busca do Firestore
          const docRef = doc(db, 'usuarios', firebaseUser.uid);
          const docSnap = await getDoc(docRef);
          if (docSnap.exists()) {
            const userData = { id: firebaseUser.uid, ...docSnap.data() };
            setUser(userData);
            await AsyncStorage.setItem('@MoblyUp:user', JSON.stringify(userData));
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

    // 3. Atualiza o app
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

  return (
    <AuthContext.Provider value={{ signed: !!user, user, loading, signIn, signUp, signOut }}>
      {children}
    </AuthContext.Provider>
  );
};