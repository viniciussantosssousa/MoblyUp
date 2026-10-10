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
  sectionTitle: {
    fontSize: 16,
    fontWeight: 'bold',
    color: colors.primary,
    marginBottom: 16,
  },
  propostaCard: {
    backgroundColor: colors.white,
    borderRadius: 12,
    padding: 16,
    marginBottom: 16,
    borderWidth: 1,
    borderColor: colors.border,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.05,
    shadowRadius: 4,
    elevation: 2,
  },
  montadorInfoRow: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 12,
  },
  montadorNome: {
    fontSize: 16,
    fontWeight: 'bold',
    color: colors.primary,
    marginLeft: 8,
    flex: 1,
  },
  avaliacaoContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: '#FFFBEB',
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 12,
  },
  avaliacaoTexto: {
    fontSize: 12,
    fontWeight: 'bold',
    color: '#D69E2E',
    marginLeft: 4,
  },
  valorProposta: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#38A169',
    textAlign: 'center',
    marginVertical: 12,
  },
  acceptButton: {
    backgroundColor: colors.primary,
    borderRadius: 8,
    paddingVertical: 12,
    alignItems: 'center',
    marginTop: 8,
  },
  acceptButtonText: {
    color: colors.white,
    fontSize: 14,
    fontWeight: 'bold',
  },
  pagamentoInfo: {
    textAlign: 'center',
    color: '#718096',
    fontSize: 12,
    marginBottom: 8,
  },
  rejectButton: {
    borderWidth: 1,
    borderColor: '#E53E3E',
    borderRadius: 8,
    paddingVertical: 12,
    alignItems: 'center',
    marginTop: 8,
  },
  rejectButtonText: {
    color: '#E53E3E',
    fontSize: 14,
    fontWeight: 'bold',
  },
  refreshButton: {
    flexDirection: 'row',
    alignSelf: 'center',
    alignItems: 'center',
    marginTop: 16,
    padding: 8,
  },
  refreshButtonText: {
    color: colors.primary,
    fontWeight: 'bold',
    marginLeft: 6,
  },
  emptyText: {
    textAlign: 'center',
    color: '#718096',
    marginTop: 40,
    fontSize: 14,
  },
  loader: {
    marginTop: 40,
  }
});