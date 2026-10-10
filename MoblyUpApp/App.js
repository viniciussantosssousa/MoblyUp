import React from 'react';
import * as Notifications from 'expo-notifications';
import Routes from './src/routes';
import { AuthProvider } from './src/contexts/AuthContext'; // <-- Importado

Notifications.setNotificationHandler({
  handleNotification: async () => ({
    shouldShowAlert: true,
    shouldPlaySound: true,
    shouldSetBadge: false,
  }),
});

export default function App() {
  return (
    <AuthProvider>
      <Routes />
    </AuthProvider>
  );
}