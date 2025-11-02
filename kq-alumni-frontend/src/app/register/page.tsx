import ErrorBoundary from '@/components/ErrorBoundary';
import RegistrationForm from '@/components/registration/RegistrationForm';

export default function RegisterPage() {
  return (
    <ErrorBoundary>
      <RegistrationForm />
    </ErrorBoundary>
  );
}
