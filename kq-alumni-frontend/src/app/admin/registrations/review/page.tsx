'use client';

/**
 * Requiring Manual Review Page
 * Lists registrations flagged for HR review after failed ERP validation
 */

import { AlertCircle, Search, Eye, Calendar, User } from 'lucide-react';
import Link from 'next/link';

import { AdminLayout } from '@/components/admin/AdminLayout';
import { Button } from '@/components/ui/button/Button';
import { useRequiringReviewRegistrations } from '@/lib/api/services/adminService';

// ============================================
// Main Component
// ============================================

export default function RequiringReviewPage() {
  const { data: registrations, isLoading, error } = useRequiringReviewRegistrations();

  return (
    <AdminLayout>
      <div className="max-w-7xl mx-auto">
        {/* Page Header */}
        <div className="mb-6">
          <div className="flex items-center gap-3 mb-2">
            <div className="bg-orange-100 p-2 rounded-lg">
              <AlertCircle className="w-6 h-6 text-orange-600" />
            </div>
            <div>
              <h2 className="text-2xl font-cabrito font-bold text-kq-dark">
                Requiring Manual Review
              </h2>
              <p className="text-gray-600">
                {registrations ? `${registrations.length} registration${registrations.length !== 1 ? 's' : ''} need your attention` : 'Loading...'}
              </p>
            </div>
          </div>
        </div>

        {/* Info Banner */}
        <div className="bg-orange-50 border-l-4 border-orange-500 p-4 rounded-r-md mb-6">
          <div className="flex items-start">
            <AlertCircle className="w-5 h-5 text-orange-600 mt-0.5 mr-3 flex-shrink-0" />
            <div className="flex-1">
              <p className="text-sm font-medium text-orange-800 mb-1">
                Action Required
              </p>
              <p className="text-sm text-orange-700">
                These registrations failed automatic ERP validation after multiple attempts.
                Please review each case and approve or reject manually.
              </p>
            </div>
          </div>
        </div>

        {/* Loading State */}
        {isLoading && (
          <div className="space-y-4">
            {[...Array(3)].map((_, i) => (
              <div
                key={i}
                className="bg-white rounded-tl-sm rounded-tr-2xl rounded-br-sm rounded-bl-2xl border border-gray-200 p-6 animate-pulse"
              >
                <div className="flex items-start gap-4">
                  <div className="flex-1 space-y-3">
                    <div className="h-4 bg-gray-200 rounded w-3/4" />
                    <div className="h-3 bg-gray-200 rounded w-1/2" />
                    <div className="h-3 bg-gray-200 rounded w-2/3" />
                  </div>
                  <div className="h-10 w-24 bg-gray-200 rounded" />
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Error State */}
        {error && (
          <div className="bg-red-50 border-l-4 border-red-500 p-4 rounded-r-md">
            <div className="flex items-start">
              <AlertCircle className="w-5 h-5 text-red-500 mt-0.5 mr-3" />
              <div>
                <p className="text-sm font-medium text-red-800">
                  Failed to load registrations
                </p>
                <p className="text-sm text-red-700 mt-1">
                  {(error as Error)?.message || 'Please try again later'}
                </p>
              </div>
            </div>
          </div>
        )}

        {/* Empty State */}
        {registrations && registrations.length === 0 && !isLoading && (
          <div className="bg-white rounded-tl-sm rounded-tr-2xl rounded-br-sm rounded-bl-2xl border border-gray-200 p-12">
            <div className="flex flex-col items-center justify-center text-gray-500">
              <div className="bg-green-100 p-4 rounded-full mb-4">
                <Search className="w-12 h-12 text-green-600" />
              </div>
              <p className="text-lg font-medium text-kq-dark mb-2">
                No Registrations Requiring Review
              </p>
              <p className="text-sm text-gray-600 text-center max-w-md">
                Great job! All registrations have been processed. New cases requiring manual
                review will appear here automatically.
              </p>
              <Link href="/admin/registrations" className="mt-6">
                <Button variant="outline" size="md">
                  View All Registrations
                </Button>
              </Link>
            </div>
          </div>
        )}

        {/* Registrations List */}
        {registrations && registrations.length > 0 && !isLoading && (
          <div className="space-y-4">
            {registrations.map((registration) => (
              <div
                key={registration.id}
                className="bg-white rounded-tl-sm rounded-tr-2xl rounded-br-sm rounded-bl-2xl border-2 border-orange-200 hover:border-orange-300 transition-colors p-6"
              >
                <div className="flex items-start justify-between gap-6">
                  {/* Registration Info */}
                  <div className="flex-1">
                    <div className="flex items-start gap-4 mb-4">
                      <div className="bg-orange-100 p-3 rounded-lg">
                        <User className="w-6 h-6 text-orange-600" />
                      </div>
                      <div className="flex-1">
                        <h3 className="text-lg font-bold text-kq-dark mb-1">
                          {registration.fullName}
                        </h3>
                        <div className="flex flex-wrap items-center gap-4 text-sm text-gray-600">
                          <span className="flex items-center gap-1">
                            <span className="font-medium text-kq-dark">Staff #:</span>
                            {registration.staffNumber}
                          </span>
                          <span className="flex items-center gap-1">
                            <span className="font-medium text-kq-dark">Email:</span>
                            {registration.email}
                          </span>
                        </div>
                      </div>
                    </div>

                    {/* Metadata */}
                    <div className="flex flex-wrap items-center gap-4 text-xs text-gray-500">
                      <span className="flex items-center gap-1">
                        <Calendar className="w-3.5 h-3.5" />
                        Registered: {new Date(registration.createdAt).toLocaleDateString()}
                      </span>
                      {registration.erpValidationAttempts && (
                        <span className="flex items-center gap-1">
                          <AlertCircle className="w-3.5 h-3.5" />
                          {registration.erpValidationAttempts} validation attempts
                        </span>
                      )}
                    </div>
                  </div>

                  {/* Action Button */}
                  <div className="flex-shrink-0">
                    <Link href={`/admin/registrations/${registration.id}`}>
                      <Button
                        variant="primary"
                        size="md"
                        leftIcon={<Eye className="w-4 h-4" />}
                      >
                        Review Now
                      </Button>
                    </Link>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </AdminLayout>
  );
}
