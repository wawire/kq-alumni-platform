"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { ArrowLeftIcon, EnvelopeIcon } from "@heroicons/react/24/outline";
import { toast } from "sonner";
import { Button } from "@/components/ui";
import { env } from "@/lib/env";

export default function ResendVerificationPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!email || !email.includes("@")) {
      toast.error("Please enter a valid email address");
      return;
    }

    setIsLoading(true);

    try {
      const response = await fetch(`${env.apiUrl}/api/v1/registrations/resend-verification`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ email: email.toLowerCase().trim() }),
      });

      if (response.ok) {
        toast.success("Verification email sent!", {
          description: "Please check your inbox and spam folder.",
        });
        setEmail("");
      } else {
        const error = await response.json();
        toast.error("Failed to send verification email", {
          description: error.detail || "Please try again later.",
        });
      }
    } catch (error) {
      console.error("Error resending verification email:", error);
      toast.error("Network error", {
        description: "Unable to connect to the server. Please try again.",
      });
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-kq-blue-50 to-white flex items-center justify-center px-4">
      <div className="max-w-md w-full">
        {/* Back Button */}
        <button
          onClick={() => router.back()}
          className="mb-6 flex items-center gap-2 text-kq-blue hover:text-kq-dark transition-colors"
        >
          <ArrowLeftIcon className="w-5 h-5" />
          <span>Go Back</span>
        </button>

        {/* Card */}
        <div className="bg-white rounded-2xl shadow-xl p-8">
          {/* Icon */}
          <div className="flex justify-center mb-6">
            <div className="w-16 h-16 bg-kq-blue-50 rounded-full flex items-center justify-center">
              <EnvelopeIcon className="w-8 h-8 text-kq-blue" />
            </div>
          </div>

          {/* Header */}
          <h1 className="text-3xl font-cabrito font-bold text-kq-dark text-center mb-2">
            Resend Verification Email
          </h1>
          <p className="text-gray-600 text-center mb-8">
            Enter your email address and we'll send you a new verification link.
          </p>

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-2">
                Email Address
              </label>
              <input
                id="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="your.email@example.com"
                required
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-kq-blue focus:border-transparent transition-all"
              />
            </div>

            <Button
              type="submit"
              variant="primary"
              size="lg"
              fullWidth
              disabled={isLoading}
            >
              {isLoading ? (
                <span className="flex items-center justify-center gap-2">
                  <span className="inline-block w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                  Sending...
                </span>
              ) : (
                "Resend Verification Email"
              )}
            </Button>
          </form>

          {/* Help Text */}
          <div className="mt-6 pt-6 border-t border-gray-200">
            <p className="text-sm text-gray-600 text-center">
              <strong>Note:</strong> Verification emails can only be resent for approved registrations.
              If you just registered, please wait for your registration to be approved first.
            </p>
          </div>
        </div>

        {/* Footer Note */}
        <p className="text-center text-sm text-gray-500 mt-6">
          Need help? Contact us at{" "}
          <a
            href="mailto:support@kqalumni.org"
            className="text-kq-blue hover:underline"
          >
            support@kqalumni.org
          </a>
        </p>
      </div>
    </div>
  );
}
