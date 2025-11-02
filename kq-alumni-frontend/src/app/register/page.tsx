import RegistrationForm from '@/components/registration/RegistrationForm';
import ErrorBoundary from '@/components/ErrorBoundary';

export default function RegisterPage() {
  return (
    <ErrorBoundary>
      <RegistrationForm />
    </ErrorBoundary>
  );
}
