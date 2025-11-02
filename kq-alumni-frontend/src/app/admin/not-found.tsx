'use client';

/**
 * Admin 404 Not Found Page
 * Custom error page for admin section
 */

import Link from 'next/link';
import { AdminLayout } from '@/components/admin/AdminLayout';
import { Home, Search, ArrowLeft } from 'lucide-react';
import { Button } from '@/components/ui/button/Button';

export default function AdminNotFound() {
  return (
    <AdminLayout>
      <div className="min-h-[60vh] flex items-center justify-center">
        <div className="text-center max-w-md">
          {/* 404 Illustration */}
          <div className="mb-8">
            <div className="inline-flex items-center justify-center w-32 h-32 bg-gray-100 rounded-full mb-4">
              <Search className="w-16 h-16 text-gray-400" />
            </div>
            <h1 className="text-6xl font-cabrito font-bold text-kq-dark mb-2">404</h1>
            <h2 className="text-2xl font-semibold text-gray-700 mb-2">Page Not Found</h2>
            <p className="text-gray-600">
              The admin page you're looking for doesn't exist or has been moved.
            </p>
          </div>

          {/* Action Buttons */}
          <div className="flex flex-col sm:flex-row gap-3 justify-center">
            <Link href="/admin/dashboard">
              <Button variant="primary" size="md" leftIcon={<Home className="w-4 h-4" />}>
                Go to Dashboard
              </Button>
            </Link>
            <button onClick={() => window.history.back()}>
              <Button variant="outline" size="md" leftIcon={<ArrowLeft className="w-4 h-4" />}>
                Go Back
              </Button>
            </button>
          </div>

          {/* Quick Links */}
          <div className="mt-8 pt-8 border-t border-gray-200">
            <p className="text-sm text-gray-600 mb-3">Quick Links:</p>
            <div className="flex flex-wrap gap-2 justify-center">
              <Link href="/admin/registrations">
                <button className="px-3 py-1.5 text-sm text-kq-red hover:bg-kq-red hover:text-white border border-kq-red rounded-lg transition-colors">
                  All Registrations
                </button>
              </Link>
              <Link href="/admin/registrations/review">
                <button className="px-3 py-1.5 text-sm text-kq-red hover:bg-kq-red hover:text-white border border-kq-red rounded-lg transition-colors">
                  Requiring Review
                </button>
              </Link>
              <Link href="/admin/settings">
                <button className="px-3 py-1.5 text-sm text-kq-red hover:bg-kq-red hover:text-white border border-kq-red rounded-lg transition-colors">
                  Settings
                </button>
              </Link>
            </div>
          </div>
        </div>
      </div>
    </AdminLayout>
  );
}
