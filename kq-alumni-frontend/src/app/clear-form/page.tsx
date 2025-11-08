"use client";

import { useState } from "react";

export default function ClearFormData() {
  const [cleared, setCleared] = useState(false);

  const handleClear = () => {
    if (typeof window !== "undefined") {
      localStorage.removeItem("kq-alumni-registration");
      setCleared(true);
      console.log("Registration form data cleared from localStorage");
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <div className="max-w-md w-full bg-white rounded-lg shadow-lg p-8">
        <h1 className="text-2xl font-bold text-gray-900 mb-4">
          🧹 Clear Registration Form Data
        </h1>

        <p className="text-gray-600 mb-4">
          This will clear any saved registration data from your browser's storage.
          Use this if you're experiencing issues with outdated form data.
        </p>

        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
          <p className="text-sm font-medium text-blue-900 mb-2">
            What gets cleared:
          </p>
          <ul className="text-sm text-blue-800 space-y-1">
            <li>• Personal information (name, email, phone)</li>
            <li>• Location data (country, city)</li>
            <li>• Employment details</li>
            <li>• Engagement preferences</li>
          </ul>
        </div>

        {!cleared ? (
          <button
            onClick={handleClear}
            className="w-full bg-red-600 hover:bg-red-700 text-white font-medium py-3 px-4 rounded-lg transition-colors"
          >
            Clear Form Data
          </button>
        ) : (
          <div className="bg-green-50 border border-green-200 rounded-lg p-4">
            <p className="text-green-900 font-medium mb-2">
              ✓ Form data cleared successfully!
            </p>
            <p className="text-sm text-green-700 mb-4">
              You can now return to the registration form and start fresh.
            </p>
            <a
              href="/register"
              className="inline-block w-full bg-green-600 hover:bg-green-700 text-white font-medium py-3 px-4 rounded-lg text-center transition-colors"
            >
              Go to Registration Form
            </a>
          </div>
        )}

        {!cleared && (
          <div className="mt-4 text-center">
            <a
              href="/register"
              className="text-sm text-gray-600 hover:text-gray-900"
            >
              ← Back to registration
            </a>
          </div>
        )}
      </div>
    </div>
  );
}
