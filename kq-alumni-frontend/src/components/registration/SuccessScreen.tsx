import { CheckCircleIcon } from '@heroicons/react/24/solid';
import Link from 'next/link';

interface Props {
  registrationId: string;
  email: string;
  fullName: string;
}

export default function SuccessScreen({ registrationId, email, fullName }: Props) {
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
          <ul className="space-y-2 text-gray-700 font-roboto">
            <li className="flex items-start gap-2">
              <span className="text-blue-600 mt-1">✓</span>
              <span>
                A confirmation email has been sent to <strong>{email}</strong>
              </span>
            </li>
            <li className="flex items-start gap-2">
              <span className="text-blue-600 mt-1">✓</span>
              <span>Your registration is being verified against our records</span>
            </li>
            <li className="flex items-start gap-2">
              <span className="text-blue-600 mt-1">✓</span>
              <span>
                You&apos;ll receive a welcome email once approved (usually within 24-48 hours)
              </span>
            </li>
            <li className="flex items-start gap-2">
              <span className="text-blue-600 mt-1">✓</span>
              <span>Check your spam folder if you don&apos;t see our email</span>
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

        {/* Action Buttons */}
        <div className="flex flex-col sm:flex-row gap-4 justify-center">
          <Link
            href="/"
            className="bg-kq-red hover:bg-kq-red-dark text-white font-cabrito font-bold text-lg px-8 py-4 rounded-lg transition-all duration-300 shadow-lg hover:shadow-xl"
          >
            Return to Home
          </Link>

          <a
            href="mailto:KQ.Alumni@kenya-airways.com"
            className="bg-white hover:bg-gray-50 text-gray-700 border-2 border-gray-300 font-cabrito font-bold text-lg px-8 py-4 rounded-lg transition-all duration-300"
          >
            Contact Support
          </a>
        </div>
      </div>
    </div>
  );
}
