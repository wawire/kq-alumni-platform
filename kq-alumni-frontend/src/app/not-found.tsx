'use client';

/**
 * Main Site 404 Not Found Page
 * Custom error page for public pages
 */

import { Home, Search, ArrowLeft, Mail } from 'lucide-react';
import Link from 'next/link';

export default function NotFound() {
  return (
    <div className="min-h-screen bg-gray-50">
      {/* Main Content */}
      <main className="flex items-center justify-center px-4 py-12" style={{ minHeight: '80vh' }}>
        <div className="text-center max-w-2xl">
          {/* 404 Illustration */}
          <div className="mb-8">
            <div className="inline-flex items-center justify-center w-32 h-32 bg-kq-red bg-opacity-10 rounded-full mb-6">
              <Search className="w-16 h-16 text-kq-red" />
            </div>
            <h1 className="text-7xl font-cabrito font-bold text-kq-dark mb-3">404</h1>
            <h2 className="text-3xl font-semibold text-gray-800 mb-3">Page Not Found</h2>
            <p className="text-lg text-gray-600 mb-8">
              Oops! The page you&apos;re looking for seems to have taken an unexpected flight path.
            </p>
          </div>

          {/* Action Buttons */}
          <div className="flex flex-col sm:flex-row gap-4 justify-center mb-12">
            <Link href="/">
              <button className="inline-flex items-center gap-2 px-6 py-3 bg-kq-red text-white font-medium rounded-lg hover:bg-opacity-90 transition-all shadow-lg hover:shadow-xl">
                <Home className="w-5 h-5" />
                Return Home
              </button>
            </Link>
            <button
              onClick={() => window.history.back()}
              className="inline-flex items-center gap-2 px-6 py-3 bg-white text-kq-dark font-medium rounded-lg border-2 border-gray-300 hover:border-kq-red hover:text-kq-red transition-all"
            >
              <ArrowLeft className="w-5 h-5" />
              Go Back
            </button>
          </div>

          {/* Quick Links */}
          <div className="bg-white rounded-lg border border-gray-200 p-6 shadow-sm">
            <p className="text-sm font-semibold text-gray-700 mb-4">Quick Links:</p>
            <div className="flex justify-center">
              <Link href="/register">
                <button className="px-6 py-2.5 text-sm text-gray-700 hover:text-kq-red hover:bg-gray-50 border border-gray-200 rounded-lg transition-colors">
                  Register Now
                </button>
              </Link>
            </div>
          </div>

          {/* Support Contact */}
          <div className="mt-8 text-center">
            <p className="text-sm text-gray-600">
              Need help? Contact us at{' '}
              <a href="mailto:KQ.Alumni@kenya-airways.com" className="text-kq-red hover:underline font-medium">
                <Mail className="w-4 h-4 inline-block -mt-0.5" />{' '}
                KQ.Alumni@kenya-airways.com
              </a>
            </p>
          </div>
        </div>
      </main>
    </div>
  );
}
