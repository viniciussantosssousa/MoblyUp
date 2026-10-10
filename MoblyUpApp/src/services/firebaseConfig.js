import { initializeApp } from 'firebase/app';
import { getAuth } from 'firebase/auth';
import { getFirestore } from 'firebase/firestore'; 

const firebaseConfig = {
  apiKey: "AIzaSyBp_0Z7U3c1kt3FRtbaVQCVaLAPIbgUYeA",
  authDomain: "moblyup-ad763.firebaseapp.com",
  projectId: "moblyup-ad763",
  storageBucket: "moblyup-ad763.firebasestorage.app",
  messagingSenderId: "974004846808",
  appId: "1:974004846808:web:cd21ed0a59c7189a0fd7ad"
};

const app = initializeApp(firebaseConfig);
export const auth = getAuth(app);
export const db = getFirestore(app); 