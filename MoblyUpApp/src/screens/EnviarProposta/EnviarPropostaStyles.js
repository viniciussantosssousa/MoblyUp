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
  detailsCard: {
    backgroundColor: colors.white,
    borderRadius: 12,
    padding: 20,
    marginBottom: 24,
    borderWidth: 1,
    borderColor: colors.border,
  },
  infoRow: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    marginBottom: 12,
  },
  infoTextContainer: {
    marginLeft: 12,
    flex: 1,
  },
  infoLabel: {
    fontSize: 12,
    color: '#A0AEC0',
    marginBottom: 2,
  },
  infoValue: {
    fontSize: 14,
    fontWeight: '600',
    color: colors.text,
  },
  imagePreview: {
    width: '100%',
    height: 200,
    borderRadius: 8,
    marginTop: 12,
    borderWidth: 1,
    borderColor: colors.border,
  },
  noImageText: {
    fontSize: 12,
    color: '#A0AEC0',
    fontStyle: 'italic',
    marginTop: 12,
  },
  inputContainer: {
    marginBottom: 24,
  },
  inputLabel: {
    fontSize: 14,
    fontWeight: '600',
    color: colors.primary,
    marginBottom: 8,
  },
  inputWrapper: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: colors.white,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 8,
    paddingHorizontal: 16,
  },
  currencySymbol: {
    fontSize: 18,
    color: colors.text,
    marginRight: 8,
    fontWeight: 'bold',
  },
  input: {
    flex: 1,
    paddingVertical: 16,
    fontSize: 18,
    color: colors.text,
    fontWeight: 'bold',
  },
  summaryCard: {
    backgroundColor: '#F0FFF4',
    borderRadius: 8,
    padding: 16,
    marginBottom: 24,
    borderWidth: 1,
    borderColor: '#9AE6B4',
  },
  summaryRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: 8,
  },
  summaryLabel: {
    fontSize: 14,
    color: '#4A5568',
  },
  summaryValue: {
    fontSize: 14,
    color: '#E53E3E',
    fontWeight: '600',
  },
  divider: {
    height: 1,
    backgroundColor: '#9AE6B4',
    marginVertical: 8,
  },
  netValueLabel: {
    fontSize: 16,
    fontWeight: 'bold',
    color: '#276749',
  },
  netValueAmount: {
    fontSize: 20,
    fontWeight: 'bold',
    color: '#276749',
  },
  submitButton: {
    backgroundColor: colors.primary,
    borderRadius: 8,
    paddingVertical: 16,
    alignItems: 'center',
  },
  submitButtonText: {
    color: colors.white,
    fontSize: 16,
    fontWeight: 'bold',
  },
  declineButton: {
    alignItems: 'center',
    paddingVertical: 16,
    marginTop: 8,
  },
  declineButtonText: {
    color: '#E53E3E',
    fontSize: 15,
    fontWeight: 'bold',
  }
});