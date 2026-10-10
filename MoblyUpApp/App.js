import React from 'react';
import Routes from './src/routes';
import { AuthProvider } from './src/contexts/AuthContext'; // <-- Importado

// A configuração de exibição das notificações fica em src/services/notificacoes.js

export default function App() {
  return (
    <AuthProvider>
      <Routes />
    </AuthProvider>
  );
}
