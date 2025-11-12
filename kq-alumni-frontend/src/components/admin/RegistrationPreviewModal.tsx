'use client';

/**
 * Registration Preview Modal
 * Quick view modal for registration details without leaving the page
 */

import { X, Mail, MapPin, Calendar, CheckCircle2, XCircle, AlertCircle, User, Building } from 'lucide-react';
import type { AdminRegistration } from '@/types/admin';
import { Button } from '@/components/ui/button/Button';

interface RegistrationPreviewModalProps {
  registration: AdminRegistration;
  onClose: () => void;
}

export function RegistrationPreviewModal({ registration, onClose }: RegistrationPreviewModalProps) {
  const getStatusColor = () => {
    switch (registration.registrationStatus) {
      case 'Pending':
        return 'bg-yellow-100 text-yellow-800 border-yellow-300';
      case 'Approved':
        return 'bg-green-100 text-green-800 border-green-300';
      case 'Rejected':
        return 'bg-red-100 text-red-800 border-red-300';
      case 'Active':
        return 'bg-blue-100 text-blue-800 border-blue-300';
      default:
        return 'bg-gray-100 text-gray-800 border-gray-300';
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg max-w-3xl w-full max-h-[90vh] overflow-hidden flex flex-col">
        {/* Header */}
        <div className="flex items-start justify-between p-6 border-b border-gray-200">
          <div className="flex-1">
            <h2 className="text-2xl font-cabrito font-bold text-kq-dark mb-2">
              Registration Details
            </h2>
            <div className="flex items-center gap-3 flex-wrap">
              <span className="text-sm font-bold text-kq-red">
                {registration.staffNumber || `ID: ${registration.id.substring(0, 8)}...`}
              </span>
              <span className={`inline-flex items-center gap-1 px-2.5 py-0.5 text-xs font-medium rounded-full border ${getStatusColor()}`}>
                {registration.registrationStatus}
              </span>
              {registration.requiresManualReview && (
                <span className="inline-flex items-center gap-1 px-2.5 py-0.5 text-xs font-medium rounded-full border bg-orange-100 text-orange-800 border-orange-300">
                  <AlertCircle className="w-3.5 h-3.5" />
                  Requires Review
                </span>
              )}
              {registration.emailVerified && (
                <span className="inline-flex items-center gap-1 px-2.5 py-0.5 text-xs font-medium rounded-full border bg-green-100 text-green-800 border-green-300">
                  <CheckCircle2 className="w-3.5 h-3.5" />
                  Email Verified
                </span>
              )}
            </div>
          </div>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition-colors p-1 rounded-lg hover:bg-gray-100"
          >
            <X className="w-6 h-6" />
          </button>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-6 space-y-6">
          {/* Personal Information */}
          <section>
            <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <User className="w-5 h-5 text-kq-red" />
              Personal Information
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 bg-gray-50 rounded-lg p-4">
              <div>
                <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">Full Name</label>
                <p className="text-sm font-medium text-gray-900 mt-1">{registration.fullName}</p>
              </div>
              <div>
                <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">Staff Number</label>
                <p className="text-sm text-gray-900 mt-1">{registration.staffNumber || 'N/A'}</p>
              </div>
              <div>
                <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">ID Number</label>
                <p className="text-sm text-gray-900 mt-1">{registration.idNumber || 'N/A'}</p>
              </div>
              <div>
                <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">Passport Number</label>
                <p className="text-sm text-gray-900 mt-1">{registration.passportNumber || 'N/A'}</p>
              </div>
            </div>
          </section>

          {/* Contact Information */}
          <section>
            <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <Mail className="w-5 h-5 text-kq-red" />
              Contact Information
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 bg-gray-50 rounded-lg p-4">
              <div className="md:col-span-2">
                <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">Email Address</label>
                <p className="text-sm text-gray-900 mt-1">{registration.email}</p>
              </div>
              {registration.mobileNumber && (
                <div>
                  <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">Mobile Number</label>
                  <p className="text-sm text-gray-900 mt-1">
                    {registration.mobileCountryCode} {registration.mobileNumber}
                  </p>
                </div>
              )}
              {registration.linkedInProfile && (
                <div className="md:col-span-2">
                  <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">LinkedIn Profile</label>
                  <a
                    href={registration.linkedInProfile}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-sm text-blue-600 hover:text-blue-800 mt-1 block"
                  >
                    {registration.linkedInProfile}
                  </a>
                </div>
              )}
            </div>
          </section>

          {/* Location Information */}
          <section>
            <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <MapPin className="w-5 h-5 text-kq-red" />
              Location
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 bg-gray-50 rounded-lg p-4">
              <div>
                <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">Country</label>
                <p className="text-sm text-gray-900 mt-1">
                  {registration.currentCountry} ({registration.currentCountryCode})
                </p>
              </div>
              <div>
                <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">City</label>
                <p className="text-sm text-gray-900 mt-1">{registration.currentCity}</p>
              </div>
            </div>
          </section>

          {/* Professional Information */}
          <section>
            <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <Building className="w-5 h-5 text-kq-red" />
              Professional Information
            </h3>
            <div className="bg-gray-50 rounded-lg p-4 space-y-4">
              <div>
                <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">Qualifications Attained</label>
                <p className="text-sm text-gray-900 mt-1 whitespace-pre-wrap">{registration.qualificationsAttained}</p>
              </div>
              <div>
                <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">Engagement Preferences</label>
                <p className="text-sm text-gray-900 mt-1 whitespace-pre-wrap">{registration.engagementPreferences}</p>
              </div>
            </div>
          </section>

          {/* ERP Validation */}
          {registration.erpValidated && (
            <section>
              <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
                <CheckCircle2 className="w-5 h-5 text-green-600" />
                ERP Validation
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4 bg-green-50 rounded-lg p-4 border border-green-200">
                <div>
                  <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">Validated</label>
                  <p className="text-sm text-gray-900 mt-1">Yes</p>
                </div>
                {registration.erpValidatedAt && (
                  <div>
                    <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">Validated At</label>
                    <p className="text-sm text-gray-900 mt-1">
                      {new Date(registration.erpValidatedAt).toLocaleString()}
                    </p>
                  </div>
                )}
                {registration.erpStaffName && (
                  <div>
                    <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">ERP Staff Name</label>
                    <p className="text-sm text-gray-900 mt-1">{registration.erpStaffName}</p>
                  </div>
                )}
                {registration.erpDepartment && (
                  <div>
                    <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">Department</label>
                    <p className="text-sm text-gray-900 mt-1">{registration.erpDepartment}</p>
                  </div>
                )}
                {registration.erpExitDate && (
                  <div>
                    <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">Exit Date</label>
                    <p className="text-sm text-gray-900 mt-1">
                      {new Date(registration.erpExitDate).toLocaleDateString()}
                    </p>
                  </div>
                )}
              </div>
            </section>
          )}

          {/* Manual Review */}
          {registration.requiresManualReview && (
            <section>
              <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
                <AlertCircle className="w-5 h-5 text-orange-600" />
                Manual Review
              </h3>
              <div className="bg-orange-50 rounded-lg p-4 border border-orange-200">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">Status</label>
                    <p className="text-sm text-gray-900 mt-1">
                      {registration.manuallyReviewed ? 'Reviewed' : 'Pending Review'}
                    </p>
                  </div>
                  {registration.manualReviewReason && (
                    <div className="md:col-span-2">
                      <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">Reason</label>
                      <p className="text-sm text-gray-900 mt-1">{registration.manualReviewReason}</p>
                    </div>
                  )}
                  {registration.reviewNotes && (
                    <div className="md:col-span-2">
                      <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">Review Notes</label>
                      <p className="text-sm text-gray-900 mt-1">{registration.reviewNotes}</p>
                    </div>
                  )}
                </div>
              </div>
            </section>
          )}

          {/* Rejection Information */}
          {registration.registrationStatus === 'Rejected' && registration.rejectionReason && (
            <section>
              <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
                <XCircle className="w-5 h-5 text-red-600" />
                Rejection Information
              </h3>
              <div className="bg-red-50 rounded-lg p-4 border border-red-200">
                <div>
                  <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">Reason</label>
                  <p className="text-sm text-gray-900 mt-1">{registration.rejectionReason}</p>
                </div>
                {registration.rejectedAt && (
                  <div className="mt-3">
                    <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">Rejected At</label>
                    <p className="text-sm text-gray-900 mt-1">
                      {new Date(registration.rejectedAt).toLocaleString()}
                    </p>
                  </div>
                )}
              </div>
            </section>
          )}

          {/* Timestamps */}
          <section>
            <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <Calendar className="w-5 h-5 text-kq-red" />
              Timeline
            </h3>
            <div className="bg-gray-50 rounded-lg p-4 space-y-3">
              <div>
                <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">Created</label>
                <p className="text-sm text-gray-900 mt-1">
                  {new Date(registration.createdAt).toLocaleString()}
                </p>
              </div>
              <div>
                <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">Last Updated</label>
                <p className="text-sm text-gray-900 mt-1">
                  {new Date(registration.updatedAt).toLocaleString()}
                  {registration.updatedBy && ` by ${registration.updatedBy}`}
                </p>
              </div>
              {registration.approvedAt && (
                <div>
                  <label className="text-xs font-medium text-gray-500 uppercase tracking-wider">Approved</label>
                  <p className="text-sm text-gray-900 mt-1">
                    {new Date(registration.approvedAt).toLocaleString()}
                  </p>
                </div>
              )}
            </div>
          </section>
        </div>

        {/* Footer */}
        <div className="flex justify-end gap-3 p-6 border-t border-gray-200 bg-gray-50">
          <Button
            variant="outline"
            size="md"
            onClick={onClose}
          >
            Close
          </Button>
          <a href={`/admin/registrations/${registration.id}`}>
            <Button
              variant="primary"
              size="md"
            >
              View Full Details
            </Button>
          </a>
        </div>
      </div>
    </div>
  );
}
