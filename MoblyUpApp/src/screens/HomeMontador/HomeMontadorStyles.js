import { StyleSheet, Platform } from 'react-native';
import { colors } from '../../theme/colors';

export const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#F5F7FA',
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    backgroundColor: colors.primary,
    paddingHorizontal: 24,
    paddingTop: Platform.OS === 'ios' ? 60 : 40,
    paddingBottom: 20,
    borderBottomWidth: 1,
    borderBottomColor: colors.primary,
  },
  greeting: {
    fontSize: 24,
    fontWeight: 'bold',
    color: colors.white,
  },
  subGreeting: {
    fontSize: 14,
    color: '#E2E8F0',
    marginTop: 4,
  },
  content: {
    padding: 24,
    paddingBottom: 40,
  },
  sectionHeader: {
    marginBottom: 16,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: colors.primary,
  },
  orderCard: {
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
  orderHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 12,
  },
  orderTitle: {
    fontSize: 16,
    fontWeight: 'bold',
    color: colors.primary,
    flex: 1,
  },
  distanceBadge: {
    backgroundColor: '#EBF8FF',
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 4,
  },
  distanceText: {
    color: '#3182CE',
    fontSize: 12,
    fontWeight: 'bold',
  },
  orderDetailRow: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 8,
  },
  orderDetailText: {
    fontSize: 14,
    color: '#718096',
    marginLeft: 8,
  },
  actionButton: {
    backgroundColor: colors.primary,
    borderRadius: 8,
    paddingVertical: 12,
    alignItems: 'center',
    marginTop: 12,
  },
  actionButtonText: {
    color: colors.white,
    fontSize: 14,
    fontWeight: 'bold',
  },
  cardActions: {
    flexDirection: 'row',
    marginTop: 4,
  },
  cardActionLeft: {
    flex: 1,
    marginRight: 8,
  },
  cardActionRight: {
    flex: 2,
  },
  declineButton: {
    borderWidth: 1,
    borderColor: '#E53E3E',
    borderRadius: 8,
    paddingVertical: 12,
    alignItems: 'center',
    marginTop: 12,
  },
  declineButtonText: {
    color: '#E53E3E',
    fontSize: 14,
    fontWeight: 'bold',
  },
  buttonDisabled: {
    opacity: 0.5,
  },
  badgeLiberado: {
    backgroundColor: '#F0FFF4',
  },
  badgeLiberadoText: {
    color: '#38A169',
  },
  aguardandoText: {
    fontSize: 13,
    color: '#D69E2E',
    marginTop: 8,
    fontStyle: 'italic',
  },
  emptyText: {
    textAlign: 'center',
    color: '#718096',
    marginTop: 20,
    fontSize: 14,
  },
  loader: {
    marginTop: 20,
  }
});