'use client';

/**
 * Admin Registration Detail Page
 * View and manage individual registration
 */

import { useState } from 'react';
import {
  ArrowLeft,
  Mail,
  Phone,
  Briefcase,
  MapPin,
  Globe,
  Linkedin,
  CheckCircle2,
  XCircle,
  AlertCircle,
  User,
} from 'lucide-react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { toast, Toaster } from 'sonner';

import { AdminLayout } from '@/components/admin/AdminLayout';
import { AuditLogTimeline } from '@/components/admin/AuditLogTimeline';
import { Button } from '@/components/ui/button/Button';
import { ConfirmationModal } from '@/components/ui/modal/ConfirmationModal';
import {
  useRegistrationDetail,
  useApproveRegistration,
  useRejectRegistration,
  useRegistrationAuditLogs,
} from '@/lib/api/services/adminService';

interface Props {
  params: { id: string };
}

export default function RegistrationDetailPage({ params }: Props) {
  const { id } = params;
  const router = useRouter();
  const [rejectReason, setRejectReason] = useState('');
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [showApproveModal, setShowApproveModal] = useState(false);

  const { data: registration, isLoading, error } = useRegistrationDetail(id);
  const { data: auditLogs } = useRegistrationAuditLogs(id);
  const approveMutation = useApproveRegistration();
  const rejectMutation = useRejectRegistration();

  const handleApproveClick = () => {
    setShowApproveModal(true);
  };

  const handleApproveConfirm = () => {
    if (!registration) {
      return;
    }
    setShowApproveModal(false);
    toast.loading('Approving registration...', { id: 'approve-toast' });
    approveMutation.mutate(
      { id, data: {} },
      {
        onSuccess: () => {
          toast.success('Registration approved successfully!', {
            id: 'approve-toast',
            description: 'Approval email has been sent to the applicant.',
            duration: 4000,
          });
          setTimeout(() => {
            router.push('/admin/registrations');
          }, 1000);
        },
        onError: (error: Error) => {
          toast.error('Failed to approve registration', {
            id: 'approve-toast',
            description: error?.message || 'Please try again.',
            duration: 5000,
          });
        },
      }
    );
  };

  const handleReject = () => {
    if (!registration || !rejectReason.trim()) {
      return;
    }
    toast.loading('Rejecting registration...', { id: 'reject-toast' });
    rejectMutation.mutate(
      {
        id,
        data: { reason: rejectReason },
      },
      {
        onSuccess: () => {
          toast.success('Registration rejected', {
            id: 'reject-toast',
            description: 'Rejection notification has been sent to the applicant.',
            duration: 4000,
          });
          setShowRejectModal(false);
          setTimeout(() => {
            router.push('/admin/registrations');
          }, 1000);
        },
        onError: (error: Error) => {
          toast.error('Failed to reject registration', {
            id: 'reject-toast',
            description: error?.message || 'Please try again.',
            duration: 5000,
          });
        },
      }
    );
  };

  if (isLoading) {
    return (
      <AdminLayout>
        <div className="max-w-5xl mx-auto">
          <div className="animate-pulse space-y-4">
            <div className="h-8 bg-gray-200 rounded w-1/4" />
            <div className="h-64 bg-gray-200 rounded" />
          </div>
        </div>
      </AdminLayout>
    );
  }

  if (error || !registration) {
    return (
      <AdminLayout>
        <div className="max-w-5xl mx-auto">
          <div className="bg-red-50 border-l-4 border-red-500 p-4 rounded-r-md">
            <div className="flex items-start">
              <AlertCircle className="w-5 h-5 text-red-500 mt-0.5 mr-3" />
              <div>
                <p className="text-sm font-medium text-red-800">
                  Failed to load registration details
                </p>
                <p className="text-sm text-red-700 mt-1">
                  {(error as Error)?.message || 'Registration not found'}
                </p>
              </div>
            </div>
          </div>
          <Link href="/admin/registrations">
            <Button variant="outline" size="md" leftIcon={<ArrowLeft className="w-4 h-4" />} className="mt-4">
              Back to Registrations
            </Button>
          </Link>
        </div>
      </AdminLayout>
    );
  }

  const statusColor =
    registration.registrationStatus === 'Approved'
      ? 'bg-green-100 text-green-700 border-green-200'
      : registration.registrationStatus === 'Rejected'
      ? 'bg-red-100 text-red-700 border-red-200'
      : registration.registrationStatus === 'Pending'
      ? 'bg-yellow-100 text-yellow-700 border-yellow-200'
      : 'bg-blue-100 text-blue-700 border-blue-200';

  return (
    <AdminLayout>
      <Toaster position="top-right" richColors />
      <div className="max-w-5xl mx-auto">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <Link href="/admin/registrations">
            <Button variant="ghost" size="sm" leftIcon={<ArrowLeft className="w-4 h-4" />}>
              Back
            </Button>
          </Link>

          {/* Action Buttons */}
          {registration.registrationStatus === 'Pending' && (
            <div className="flex gap-3">
              <Button
                variant="outline"
                size="md"
                onClick={() => setShowRejectModal(true)}
                leftIcon={<XCircle className="w-4 h-4" />}
                disabled={rejectMutation.isPending}
              >
                Reject
              </Button>
              <Button
                variant="primary"
                size="md"
                onClick={handleApproveClick}
                leftIcon={<CheckCircle2 className="w-4 h-4" />}
                disabled={approveMutation.isPending}
              >
                {approveMutation.isPending ? 'Approving...' : 'Approve'}
              </Button>
            </div>
          )}
        </div>

        {/* Status Banner */}
        {registration.requiresManualReview && registration.registrationStatus === 'Pending' && (
          <div className="bg-orange-50 border-l-4 border-orange-500 p-4 rounded-r-md mb-6">
            <div className="flex items-start">
              <AlertCircle className="w-5 h-5 text-orange-500 mt-0.5 mr-3" />
              <div>
                <p className="text-sm font-medium text-orange-800">Manual Review Required</p>
                <p className="text-sm text-orange-700 mt-1">
                  {registration.manualReviewReason || 'This registration requires manual approval'}
                </p>
              </div>
            </div>
          </div>
        )}

        {/* Main Content */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Left Column - Main Info */}
          <div className="lg:col-span-2 space-y-6">
            {/* Personal Information */}
            <div className="bg-white rounded-lg border border-gray-200 p-6">
              <div className="flex items-center gap-3 mb-4">
                <div className="w-16 h-16 rounded-full bg-navy-900 flex items-center justify-center">
                  <span className="text-xl font-bold text-white">
                    {registration.fullName
                      .split(' ')
                      .map((n) => n[0])
                      .join('')
                      .substring(0, 2)}
                  </span>
                </div>
                <div>
                  <h2 className="text-2xl font-cabrito font-bold text-kq-dark">
                    {registration.fullName}
                  </h2>
                  {registration.staffNumber && (
                    <p className="text-gray-600">Staff No: {registration.staffNumber}</p>
                  )}
                  {!registration.staffNumber && (registration.idNumber || registration.passportNumber) && (
                    <p className="text-gray-600">
                      {registration.idNumber ? `ID: ${registration.idNumber}` : `Passport: ${registration.passportNumber}`}
                    </p>
                  )}
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="flex items-start gap-3">
                  <Mail className="w-5 h-5 text-gray-400 mt-0.5" />
                  <div>
                    <p className="text-sm text-gray-600">Email</p>
                    <p className="font-medium text-kq-dark">{registration.email}</p>
                  </div>
                </div>

                <div className="flex items-start gap-3">
                  <Phone className="w-5 h-5 text-gray-400 mt-0.5" />
                  <div>
                    <p className="text-sm text-gray-600">Mobile</p>
                    <p className="font-medium text-kq-dark">{registration.mobileNumber}</p>
                  </div>
                </div>

                {registration.idNumber && (
                  <div className="flex items-start gap-3">
                    <User className="w-5 h-5 text-gray-400 mt-0.5" />
                    <div>
                      <p className="text-sm text-gray-600">National ID Number</p>
                      <p className="font-medium text-kq-dark">{registration.idNumber}</p>
                    </div>
                  </div>
                )}

                {registration.passportNumber && (
                  <div className="flex items-start gap-3">
                    <User className="w-5 h-5 text-gray-400 mt-0.5" />
                    <div>
                      <p className="text-sm text-gray-600">Passport Number</p>
                      <p className="font-medium text-kq-dark">{registration.passportNumber}</p>
                    </div>
                  </div>
                )}

                {registration.staffNumber && (
                  <div className="flex items-start gap-3">
                    <User className="w-5 h-5 text-gray-400 mt-0.5" />
                    <div>
                      <p className="text-sm text-gray-600">Staff Number</p>
                      <p className="font-medium text-kq-dark">{registration.staffNumber}</p>
                    </div>
                  </div>
                )}
              </div>
            </div>

            {/* ERP Validation Details */}
            <div className="bg-white rounded-lg border border-gray-200 p-6">
              <h3 className="text-lg font-semibold text-kq-dark mb-4 flex items-center gap-2">
                <Briefcase className="w-5 h-5" />
                ERP Validation Details
              </h3>

              <div className="space-y-3">
                <div>
                  <p className="text-sm text-gray-600">ERP Staff Name</p>
                  <p className="font-medium text-kq-dark">{registration.erpStaffName || 'N/A'}</p>
                </div>

                <div>
                  <p className="text-sm text-gray-600">Department</p>
                  <p className="font-medium text-kq-dark">{registration.erpDepartment || 'N/A'}</p>
                </div>

                <div>
                  <p className="text-sm text-gray-600">Exit Date</p>
                  <p className="font-medium text-kq-dark">
                    {registration.erpExitDate
                      ? new Date(registration.erpExitDate).toLocaleDateString()
                      : 'N/A'}
                  </p>
                </div>

                <div>
                  <p className="text-sm text-gray-600">Validation Status</p>
                  <p className="font-medium text-kq-dark">
                    {registration.erpValidated ? (
                      <span className="text-green-600">✓ Validated</span>
                    ) : (
                      <span className="text-yellow-600">Pending Validation</span>
                    )}
                  </p>
                </div>

                {registration.erpValidatedAt && (
                  <div>
                    <p className="text-sm text-gray-600">Validated At</p>
                    <p className="font-medium text-kq-dark">
                      {new Date(registration.erpValidatedAt).toLocaleString()}
                    </p>
                  </div>
                )}
              </div>
            </div>

            {/* Location */}
            <div className="bg-white rounded-lg border border-gray-200 p-6">
              <h3 className="text-lg font-semibold text-kq-dark mb-4 flex items-center gap-2">
                <MapPin className="w-5 h-5" />
                Location
              </h3>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm text-gray-600">Country</p>
                  <p className="font-medium text-kq-dark">{registration.currentCountry}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600">City</p>
                  <p className="font-medium text-kq-dark">{registration.currentCity}</p>
                </div>
              </div>
            </div>

            {/* Social Links */}
            {registration.linkedInProfile && (
              <div className="bg-white rounded-lg border border-gray-200 p-6">
                <h3 className="text-lg font-semibold text-kq-dark mb-4 flex items-center gap-2">
                  <Globe className="w-5 h-5" />
                  Social Links
                </h3>

                <div className="flex items-center gap-2">
                  <Linkedin className="w-4 h-4 text-blue-600" />
                  <a
                    href={registration.linkedInProfile}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-blue-600 hover:underline"
                  >
                    LinkedIn Profile
                  </a>
                </div>
              </div>
            )}
          </div>

          {/* Right Column - Status & History */}
          <div className="space-y-6">
            {/* Status Card */}
            <div className="bg-white rounded-lg border border-gray-200 p-6">
              <h3 className="text-lg font-semibold text-kq-dark mb-4">Status</h3>

              <div className={`px-3 py-2 rounded-lg border text-center font-medium ${statusColor}`}>
                {registration.registrationStatus}
              </div>

              <div className="mt-4 space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-gray-600">Email Verified</span>
                  <span className="font-medium">
                    {registration.emailVerified ? '✓ Yes' : '✗ No'}
                  </span>
                </div>

                <div className="flex justify-between">
                  <span className="text-gray-600">Created</span>
                  <span className="font-medium">
                    {new Date(registration.createdAt).toLocaleDateString()}
                  </span>
                </div>

                {registration.approvedAt && (
                  <div className="flex justify-between">
                    <span className="text-gray-600">Approved</span>
                    <span className="font-medium">
                      {new Date(registration.approvedAt).toLocaleDateString()}
                    </span>
                  </div>
                )}
              </div>
            </div>

            {/* Audit Log Timeline */}
            <div className="bg-white rounded-lg border border-gray-200 p-6">
              <h3 className="text-lg font-cabrito font-bold text-kq-dark mb-6 flex items-center gap-2">
                <Clock className="w-5 h-5 text-kq-red" />
                Activity Timeline
              </h3>
              <AuditLogTimeline logs={auditLogs || []} maxItems={10} />
            </div>
          </div>
        </div>

        {/* Approve Confirmation Modal */}
        <ConfirmationModal
          isOpen={showApproveModal}
          onClose={() => setShowApproveModal(false)}
          onConfirm={handleApproveConfirm}
          title="Approve Registration"
          message={`Are you sure you want to approve ${registration?.fullName || 'this'} registration? An approval email will be sent to the applicant.`}
          confirmText="Yes, Approve"
          cancelText="Cancel"
          type="success"
          isLoading={approveMutation.isPending}
        />

        {/* Reject Modal */}
        {showRejectModal && (
          <>
            <div
              className="fixed inset-0 bg-black bg-opacity-50 z-40"
              onClick={() => setShowRejectModal(false)}
            />
            <div className="fixed inset-0 flex items-center justify-center z-50 p-4">
              <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
                <h3 className="text-lg font-bold text-kq-dark mb-4">Reject Registration</h3>

                <p className="text-sm text-gray-600 mb-4">
                  Please provide a reason for rejecting this registration:
                </p>

                <textarea
                  className="w-full border border-gray-300 rounded-lg p-3 text-sm focus:outline-none focus:ring-2 focus:ring-kq-red"
                  rows={4}
                  value={rejectReason}
                  onChange={(e) => setRejectReason(e.target.value)}
                  placeholder="Enter rejection reason..."
                />

                <div className="flex gap-3 mt-6">
                  <Button
                    variant="outline"
                    size="md"
                    onClick={() => setShowRejectModal(false)}
                    className="flex-1"
                  >
                    Cancel
                  </Button>
                  <Button
                    variant="primary"
                    size="md"
                    onClick={handleReject}
                    disabled={!rejectReason.trim() || rejectMutation.isPending}
                    className="flex-1 bg-red-600 hover:bg-red-700"
                  >
                    {rejectMutation.isPending ? 'Rejecting...' : 'Reject'}
                  </Button>
                </div>
              </div>
            </div>
          </>
        )}
      </div>
    </AdminLayout>
  );
}
