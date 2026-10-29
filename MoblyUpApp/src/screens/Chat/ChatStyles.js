import { StyleSheet, Platform } from 'react-native';
import { colors } from '../../theme/colors';

export const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#EEF1F6' },
  header: {
    flexDirection: 'row', alignItems: 'center', backgroundColor: colors.white,
    paddingHorizontal: 16, paddingTop: Platform.OS === 'ios' ? 56 : 36, paddingBottom: 12,
    borderBottomWidth: 1, borderBottomColor: colors.border,
  },
  backButton: { marginRight: 8, padding: 4 },
  headerPessoa: { flex: 1, flexDirection: 'row', alignItems: 'center' },
  avatar: { width: 40, height: 40, borderRadius: 20 },
  headerNome: { fontSize: 17, fontWeight: 'bold', color: colors.primary },
  headerInfo: { fontSize: 12, color: '#718096', marginTop: 2 },

  seguranca: {
    flexDirection: 'row', alignItems: 'center', backgroundColor: '#F0FFF4',
    paddingHorizontal: 16, paddingVertical: 8, borderBottomWidth: 1, borderBottomColor: '#C6F6D5',
  },
  segurancaTexto: { flex: 1, fontSize: 12, color: '#276749', marginLeft: 8 },

  centro: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: 24 },
  erro: { fontSize: 15, color: '#718096', textAlign: 'center' },

  lista: { padding: 12, paddingBottom: 16, flexGrow: 1 },
  vazio: { textAlign: 'center', color: '#A0AEC0', marginTop: 40, paddingHorizontal: 24 },
  dia: {
    alignSelf: 'center', fontSize: 11, color: '#4A5568', backgroundColor: '#E2E8F0',
    paddingHorizontal: 10, paddingVertical: 3, borderRadius: 10, marginVertical: 10, overflow: 'hidden',
  },

  balao: { maxWidth: '80%', paddingHorizontal: 12, paddingVertical: 8, borderRadius: 14, marginVertical: 3 },
  balaoMeu: { alignSelf: 'flex-end', backgroundColor: colors.primary, borderBottomRightRadius: 4 },
  balaoOutro: { alignSelf: 'flex-start', backgroundColor: colors.white, borderBottomLeftRadius: 4 },
  balaoTexto: { fontSize: 15, color: colors.text, lineHeight: 20 },
  balaoTextoMeu: { color: colors.white },
  balaoRodape: { flexDirection: 'row', alignItems: 'center', alignSelf: 'flex-end', marginTop: 2 },
  hora: { fontSize: 10, color: '#A0AEC0' },
  horaMinha: { color: '#CBD5E0' },

  bloqueio: {
    flexDirection: 'row', alignItems: 'flex-start', backgroundColor: '#FFF5F5',
    borderTopWidth: 1, borderTopColor: '#FEB2B2', paddingHorizontal: 14, paddingVertical: 8,
  },
  bloqueioTexto: { flex: 1, fontSize: 12, color: '#C53030', marginLeft: 6, lineHeight: 17 },

  entrada: {
    flexDirection: 'row', alignItems: 'flex-end', backgroundColor: colors.white,
    paddingHorizontal: 10, paddingVertical: 8, borderTopWidth: 1, borderTopColor: colors.border,
  },
  input: {
    flex: 1, maxHeight: 120, backgroundColor: '#F7FAFC', borderWidth: 1, borderColor: colors.border,
    borderRadius: 20, paddingHorizontal: 14, paddingVertical: 10, fontSize: 15, color: colors.text,
  },
  enviar: {
    width: 44, height: 44, borderRadius: 22, backgroundColor: colors.primary,
    alignItems: 'center', justifyContent: 'center', marginLeft: 8,
  },
  enviarDesativado: { opacity: 0.4 },

  encerrado: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'center', backgroundColor: colors.white,
    padding: 14, borderTopWidth: 1, borderTopColor: colors.border,
  },
  encerradoTexto: { fontSize: 13, color: '#718096', marginLeft: 6 },
});
