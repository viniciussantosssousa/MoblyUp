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
    backgroundColor: colors.white,
    paddingHorizontal: 24,
    paddingTop: Platform.OS === 'ios' ? 60 : 40,
    paddingBottom: 20,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  greeting: {
    fontSize: 24,
    fontWeight: 'bold',
    color: colors.primary,
  },
  subGreeting: {
    fontSize: 14,
    color: '#718096',
    marginTop: 4,
  },
  content: {
    padding: 24,
    paddingBottom: 40,
  },
  ctaCard: {
    backgroundColor: colors.primary,
    borderRadius: 16,
    padding: 24,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginBottom: 20,
    shadowColor: colors.primary,
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.3,
    shadowRadius: 8,
    elevation: 5,
  },
  ctaTextContainer: {
    flex: 1,
    paddingRight: 16,
  },
  ctaTitle: {
    color: colors.white,
    fontSize: 20,
    fontWeight: 'bold',
    marginBottom: 8,
  },
  ctaSubtitle: {
    color: colors.white,
    fontSize: 14,
    opacity: 0.8,
  },
  supportCard: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: colors.white,
    padding: 16,
    borderRadius: 12,
    marginBottom: 32,
    borderWidth: 1,
    borderColor: colors.border,
  },
  supportTextContainer: {
    marginLeft: 12,
  },
  supportTitle: {
    fontSize: 16,
    fontWeight: 'bold',
    color: colors.primary,
  },
  supportSubtitle: {
    fontSize: 12,
    color: '#718096',
    marginTop: 2,
  },
  sectionHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 16,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: colors.primary,
  },
  seeAll: {
    fontSize: 14,
    color: '#3182CE',
    fontWeight: '600',
  },
  orderCard: {
    backgroundColor: colors.white,
    borderRadius: 12,
    padding: 16,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: colors.border,
  },
  orderHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: 8,
  },
  orderTitle: {
    fontSize: 16,
    fontWeight: 'bold',
    color: colors.primary,
  },
  orderDate: {
    fontSize: 12,
    color: '#A0AEC0',
  },
  orderAddress: {
    fontSize: 14,
    color: '#718096',
    marginBottom: 12,
  },
  statusContainerPending: {
    alignSelf: 'flex-start',
    backgroundColor: '#FFFBEB',
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 6,
    borderWidth: 1,
    borderColor: '#F6E05E',
  },
  statusTextPending: {
    color: '#D69E2E',
    fontSize: 12,
    fontWeight: 'bold',
  },
  statusContainerDone: {
    alignSelf: 'flex-start',
    backgroundColor: '#F0FFF4',
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 6,
    borderWidth: 1,
    borderColor: '#9AE6B4',
  },
  statusTextDone: {
    color: '#38A169',
    fontSize: 12,
    fontWeight: 'bold',
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