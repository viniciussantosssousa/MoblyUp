import { StyleSheet, Platform } from 'react-native';
import { colors } from '../../theme/colors';

export const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#F5F7FA',
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: colors.white,
    paddingHorizontal: 24,
    paddingTop: Platform.OS === 'ios' ? 60 : 40,
    paddingBottom: 20,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  backButton: {
    marginRight: 16,
  },
  headerTitle: {
    fontSize: 20,
    fontWeight: 'bold',
    color: colors.primary,
  },
  content: {
    padding: 24,
    paddingBottom: 40,
  },
  card: {
    backgroundColor: colors.white,
    borderRadius: 12,
    padding: 16,
    marginBottom: 20,
    borderWidth: 1,
    borderColor: colors.border,
  },
  cardTitle: {
    fontSize: 16,
    fontWeight: 'bold',
    color: colors.primary,
    marginBottom: 4,
  },
  cardText: {
    fontSize: 14,
    color: '#718096',
    marginBottom: 8,
  },
  cardValor: {
    fontSize: 15,
    fontWeight: 'bold',
    color: '#276749',
  },
  instrucao: {
    fontSize: 14,
    color: '#4A5568',
    marginBottom: 16,
    lineHeight: 20,
  },
  cameraButton: {
    height: 200,
    borderRadius: 12,
    borderWidth: 2,
    borderStyle: 'dashed',
    borderColor: colors.border,
    backgroundColor: colors.white,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: 24,
  },
  cameraButtonText: {
    marginTop: 8,
    fontSize: 15,
    fontWeight: 'bold',
    color: colors.primary,
  },
  preview: {
    width: '100%',
    height: 240,
    borderRadius: 12,
    borderWidth: 1,
    borderColor: colors.border,
  },
  trocarFoto: {
    textAlign: 'center',
    color: colors.primary,
    fontWeight: 'bold',
    marginTop: 12,
    marginBottom: 24,
  },
  submitButton: {
    backgroundColor: colors.primary,
    borderRadius: 8,
    paddingVertical: 16,
    alignItems: 'center',
  },
  submitButtonDisabled: {
    opacity: 0.5,
  },
  submitButtonText: {
    color: colors.white,
    fontSize: 16,
    fontWeight: 'bold',
  },
});
