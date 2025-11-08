import { CheckCircleIcon } from '@heroicons/react/24/solid';
import { useRouter } from 'next/navigation';
import { useRegistrationActions } from '@/store';

interface Props {
  registrationId: string;
  email: string;
  fullName: string;
}

export default function SuccessScreen({ registrationId, email, fullName }: Props) {
  const { clearRegistration } = useRegistrationActions();
  const router = useRouter();

  const handleBackHome = () => {
    clearRegistration();
    router.push('/');
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-green-50 to-white flex items-center justify-center p-8">
      <div className="max-w-2xl w-full bg-white rounded-2xl shadow-2xl p-12 text-center">
        {/* Success Icon */}
        <div className="flex justify-center mb-6">
          <div className="w-24 h-24 bg-green-100 rounded-full flex items-center justify-center animate-bounce">
            <CheckCircleIcon className="w-16 h-16 text-green-600" />
          </div>
        </div>

        {/* Success Message */}
        <h1 className="text-4xl font-cabrito font-bold text-kq-dark mb-4">
          Registration Successful!
        </h1>
        <p className="text-xl text-gray-600 mb-8 font-roboto">
          Welcome to the KQ Alumni Association, {fullName}!
        </p>

        {/* Info Box */}
        <div className="bg-blue-50 border-l-4 border-blue-400 p-6 rounded-lg mb-8 text-left">
          <h3 className="font-cabrito font-bold text-lg text-gray-900 mb-2">What Happens Next?</h3>
          <ul className="space-y-2 text-gray-700 font-roboto list-disc list-inside">
            <li>
              A confirmation email has been sent to <strong>{email}</strong>
            </li>
            <li>
              Your registration is being verified against our records
            </li>
            <li>
              You&apos;ll receive a welcome email once approved (usually within 24-48 hours)
            </li>
            <li>
              Check your spam folder if you don&apos;t see our email
            </li>
          </ul>
        </div>

        {/* Registration ID */}
        <div className="mb-8">
          <p className="text-sm text-gray-500 mb-2">Your Registration ID:</p>
          <p className="font-mono text-lg font-bold text-gray-900 bg-gray-100 px-4 py-2 rounded">
            {registrationId}
          </p>
        </div>

        {/* Single Action Button */}
        <button
          onClick={handleBackHome}
          className="bg-kq-red hover:bg-kq-red-dark text-white font-cabrito font-bold text-lg px-12 py-4 rounded-lg transition-all duration-300 shadow-lg hover:shadow-xl"
        >
          Back Home
        </button>
      </div>
    </div>
  );
}
