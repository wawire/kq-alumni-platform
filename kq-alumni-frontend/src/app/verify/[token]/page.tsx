"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { CheckCircleIcon, XCircleIcon } from "@heroicons/react/24/solid";
import axios, { AxiosError } from "axios";

import { API_BASE_URL } from "@/config/api";
import type { VerificationResponse, ErrorResponse } from "@/types";

type VerificationStatus = "verifying" | "success" | "error" | "expired";

export default function VerifyEmailPage() {
  const params = useParams();
  const router = useRouter();
  const [status, setStatus] = useState<VerificationStatus>("verifying");
  const [data, setData] = useState<VerificationResponse | null>(null);
  const [errorMessage, setErrorMessage] = useState("");
  const [countdown, setCountdown] = useState(5);

  useEffect(() => {
    let timer: NodeJS.Timeout;

    const verifyEmail = async () => {
      try {
        const token = params.token as string;

        // 📡 Request verification from API
        const response = await axios.get<VerificationResponse>(
          `${API_BASE_URL}/api/v1/verify/${token}`,
        );

        setStatus("success");
        setData(response.data);

        // ⏳ Start countdown after successful verification
        let count = 5;
        timer = setInterval(() => {
          count -= 1;
          setCountdown(count);

          if (count === 0) {
            clearInterval(timer);
            router.push("/");
          }
        }, 1000);
      } catch (err) {
        const error = err as AxiosError<ErrorResponse>;
        const statusCode = error.response?.status;
        const detail = error.response?.data?.detail || "";

        if (statusCode === 400) {
          if (detail.toLowerCase().includes("expired")) {
            setStatus("expired");
            setErrorMessage("This verification link has expired.");
          } else {
            setStatus("error");
            setErrorMessage(detail || "Verification failed.");
          }
        } else if (statusCode === 404) {
          setStatus("error");
          setErrorMessage("Verification token not found.");
        } else if (error.code === "ERR_NETWORK") {
          setStatus("error");
          setErrorMessage(
            "Cannot connect to server. Please check your internet connection.",
          );
        } else {
          setStatus("error");
          setErrorMessage("An unexpected error occurred. Please try again.");
        }
      }
    };

    if (params.token) {
      void verifyEmail();
    }

    // 🧹 Cleanup interval on unmount
    return () => {
      if (timer) {
        clearInterval(timer);
      }
    };
  }, [params.token, router]);

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 flex items-center justify-center p-8">
      <div className="max-w-2xl w-full bg-white rounded-2xl shadow-2xl p-12">
        {/* VERIFYING STATE */}
        {status === "verifying" && (
          <div className="text-center">
            <div className="flex justify-center mb-6">
              <div className="w-24 h-24 border-8 border-gray-200 border-t-kq-red rounded-full animate-spin"></div>
            </div>
            <h2 className="text-3xl font-cabrito font-bold text-gray-900 mb-4">
              Verifying Your Email...
            </h2>
            <p className="text-gray-600 text-lg">
              Please wait while we verify your account
            </p>
          </div>
        )}

        {/* SUCCESS STATE */}
        {status === "success" && (
          <div className="text-center">
            <div className="flex justify-center mb-6">
              <div className="w-24 h-24 bg-green-100 rounded-full flex items-center justify-center animate-bounce">
                <CheckCircleIcon className="w-16 h-16 text-green-600" />
              </div>
            </div>

            <h1 className="text-4xl font-cabrito font-bold text-gray-900 mb-4">
              Email Verified Successfully! 🎉
            </h1>

            {data?.fullName && (
              <p className="text-xl text-gray-600 mb-8">
                Welcome, {data.fullName}!
              </p>
            )}

            <div className="bg-green-50 border-l-4 border-green-400 p-6 rounded-lg mb-8 text-left">
              <h3 className="font-cabrito font-bold text-lg text-gray-900 mb-2">
                ✅ Your Account is Now Active
              </h3>
              <ul className="space-y-2 text-gray-700">
                <li className="flex items-start gap-2">
                  <span className="text-green-600 mt-1">•</span>
                  <span>You can now access all alumni benefits</span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="text-green-600 mt-1">•</span>
                  <span>Connect with fellow alumni worldwide</span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="text-green-600 mt-1">•</span>
                  <span>
                    Explore upcoming events and networking opportunities
                  </span>
                </li>
              </ul>
            </div>

            <div className="mb-6">
              <p className="text-lg text-gray-600 mb-2">
                Redirecting you to the homepage in{" "}
                <span className="font-bold text-kq-red text-2xl">
                  {countdown}
                </span>{" "}
                seconds...
              </p>
              <div className="w-full bg-gray-200 rounded-full h-2 overflow-hidden">
                <div
                  className="bg-kq-red h-2 rounded-full transition-all duration-1000 ease-linear"
                  style={{ width: `${((5 - countdown) / 5) * 100}%` }}
                ></div>
              </div>
            </div>

            <button
              onClick={() => router.push("/")}
              className="bg-kq-red hover:bg-kq-red-dark text-white font-cabrito font-bold text-lg px-8 py-4 rounded-lg transition-all duration-300 shadow-lg hover:shadow-xl transform hover:scale-[1.02]"
            >
              Go to Homepage Now
            </button>
          </div>
        )}

        {/* ERROR / EXPIRED STATE */}
        {(status === "error" || status === "expired") && (
          <div className="text-center">
            <div className="flex justify-center mb-6">
              <div className="w-24 h-24 bg-red-100 rounded-full flex items-center justify-center">
                <XCircleIcon className="w-16 h-16 text-red-600" />
              </div>
            </div>

            <h1 className="text-4xl font-cabrito font-bold text-gray-900 mb-4">
              {status === "expired"
                ? "Link Expired ⏰"
                : "Verification Failed ❌"}
            </h1>

            <p className="text-lg text-gray-600 mb-8">{errorMessage}</p>

            <div className="bg-yellow-50 border-l-4 border-yellow-400 p-6 rounded-lg mb-8 text-left">
              <h3 className="font-cabrito font-bold text-lg text-gray-900 mb-2">
                What to do next:
              </h3>
              <ul className="space-y-2 text-gray-700">
                <li className="flex items-start gap-2">
                  <span className="text-yellow-600 mt-1">•</span>
                  <span>
                    Check if you used the most recent verification email
                  </span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="text-yellow-600 mt-1">•</span>
                  <span>Make sure you copied the entire verification link</span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="text-yellow-600 mt-1">•</span>
                  <span>Contact our support team for assistance</span>
                </li>
              </ul>
            </div>

            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <a
                href="mailto:KQ.Alumni@kenya-airways.com"
                className="bg-kq-red hover:bg-kq-red-dark text-white font-cabrito font-bold text-lg px-8 py-4 rounded-lg transition-all duration-300 shadow-lg hover:shadow-xl"
              >
                Contact Support
              </a>
              <button
                onClick={() => router.push("/register")}
                className="bg-white hover:bg-gray-50 text-gray-700 border-2 border-gray-300 font-cabrito font-bold text-lg px-8 py-4 rounded-lg transition-all duration-300"
              >
                Back to Registration
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
