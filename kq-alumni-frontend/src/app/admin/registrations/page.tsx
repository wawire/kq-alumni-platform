'use client';

/**
 * Admin Registrations List Page
 * View and manage all alumni registrations with filtering and pagination
 */

import { useState, Suspense } from 'react';
import {
  Search,
  ChevronLeft,
  ChevronRight,
  ChevronUp,
  ChevronDown,
  Eye,
  CheckCircle2,
  XCircle,
  Clock,
  AlertCircle,
  Mail,
  Check,
  X,
} from 'lucide-react';
import Link from 'next/link';
import { useSearchParams } from 'next/navigation';

import { AdminLayout } from '@/components/admin/AdminLayout';
import { BulkActions } from '@/components/admin/BulkActions';
import { RegistrationsFilters } from '@/components/admin/RegistrationsFilters';
import { RegistrationPreviewModal } from '@/components/admin/RegistrationPreviewModal';
import { Button } from '@/components/ui/button/Button';
import { useAdminRegistrations, useApproveRegistration, useRejectRegistration } from '@/lib/api/services/adminService';
import type { RegistrationStatus, RegistrationFilters, SortableColumn, SortOrder, AdminRegistration } from '@/types/admin';

// ============================================
// Sortable Header Component
// ============================================

interface SortableHeaderProps {
  label: string;
  column: SortableColumn;
  currentSort?: SortableColumn;
  currentOrder?: SortOrder;
  onSort: (column: SortableColumn) => void;
}

function SortableHeader({ label, column, currentSort, currentOrder, onSort }: SortableHeaderProps) {
  const isActive = currentSort === column;

  return (
    <th className="px-6 py-3 text-left">
      <button
        onClick={() => onSort(column)}
        className="flex items-center gap-2 text-xs font-medium text-gray-500 uppercase tracking-wider hover:text-gray-700 transition-colors group"
      >
        {label}
        <div className="flex flex-col">
          <ChevronUp
            className={`w-3 h-3 -mb-1 transition-colors ${
              isActive && currentOrder === 'asc'
                ? 'text-kq-red'
                : 'text-gray-300 group-hover:text-gray-400'
            }`}
          />
          <ChevronDown
            className={`w-3 h-3 -mt-1 transition-colors ${
              isActive && currentOrder === 'desc'
                ? 'text-kq-red'
                : 'text-gray-300 group-hover:text-gray-400'
            }`}
          />
        </div>
      </button>
    </th>
  );
}

// ============================================
// Status Badge Component
// ============================================

interface StatusBadgeProps {
  status: RegistrationStatus;
  requiresManualReview?: boolean;
}

function StatusBadge({ status, requiresManualReview }: StatusBadgeProps) {
  const getStatusColor = () => {
    if (requiresManualReview) {
      return 'bg-orange-100 text-orange-800 border-orange-300';
    }

    switch (status) {
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

  const getStatusIcon = () => {
    if (requiresManualReview) {
      return AlertCircle;
    }
    switch (status) {
      case 'Pending':
        return Clock;
      case 'Approved':
        return CheckCircle2;
      case 'Rejected':
        return XCircle;
      case 'Active':
        return Mail;
      default:
        return Clock;
    }
  };

  const Icon = getStatusIcon();
  const label = requiresManualReview ? 'Requires Review' : status;

  return (
    <span
      className={`
        inline-flex items-center gap-1 px-2.5 py-0.5
        text-xs font-medium rounded-full border
        ${getStatusColor()}
      `}
    >
      <Icon className="w-3.5 h-3.5" />
      {label}
    </span>
  );
}

// ============================================
// Main Component
// ============================================

function RegistrationsPageContent() {
  const searchParams = useSearchParams();
  const initialStatus = (searchParams?.get('status') as RegistrationStatus) || undefined;

  const [filters, setFilters] = useState<RegistrationFilters>({
    status: initialStatus,
    pageNumber: 1,
    pageSize: 20,
  });
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [actionModal, setActionModal] = useState<{
    type: 'approve' | 'reject' | null;
    registrationId: string | null;
    registrationName: string | null;
  }>({ type: null, registrationId: null, registrationName: null });
  const [rejectReason, setRejectReason] = useState('');
  const [previewRegistration, setPreviewRegistration] = useState<AdminRegistration | null>(null);

  const { data, isLoading, error } = useAdminRegistrations(filters);
  const approveMutation = useApproveRegistration();
  const rejectMutation = useRejectRegistration();

  const handlePageChange = (page: number) => {
    setFilters((prev) => ({ ...prev, pageNumber: page }));
    setSelectedIds([]); // Clear selection on page change
  };

  const handleSort = (column: SortableColumn) => {
    setFilters((prev) => {
      // If clicking the same column, toggle the order
      if (prev.sortBy === column) {
        return {
          ...prev,
          sortOrder: prev.sortOrder === 'asc' ? 'desc' : 'asc',
          pageNumber: 1, // Reset to first page when sorting changes
        };
      }
      // If clicking a new column, set it as sortBy with ascending order
      return {
        ...prev,
        sortBy: column,
        sortOrder: 'asc',
        pageNumber: 1,
      };
    });
  };

  const handleSelectAll = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.checked && data) {
      setSelectedIds(data.data.map((reg) => reg.id));
    } else {
      setSelectedIds([]);
    }
  };

  const handleSelectOne = (id: string, checked: boolean) => {
    if (checked) {
      setSelectedIds((prev) => [...prev, id]);
    } else {
      setSelectedIds((prev) => prev.filter((selectedId) => selectedId !== id));
    }
  };

  const handleBulkApprove = async (ids: string[]) => {
    for (const id of ids) {
      await approveMutation.mutateAsync({ id, data: { notes: 'Bulk approved' } });
    }
  };

  const handleBulkReject = async (ids: string[], reason: string) => {
    for (const id of ids) {
      await rejectMutation.mutateAsync({ id, data: { reason } });
    }
  };

  const handleExport = (ids: string[]) => {
    if (!data) {
      return;
    }

    const selectedRegistrations = data.data.filter((reg) => ids.includes(reg.id));
    const csvContent = [
      ['Staff Number', 'Full Name', 'Email', 'Status', 'Created Date'].join(','),
      ...selectedRegistrations.map((reg) =>
        [
          reg.staffNumber,
          `"${reg.fullName}"`,
          reg.email,
          reg.registrationStatus,
          new Date(reg.createdAt).toLocaleDateString(),
        ].join(',')
      ),
    ].join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `registrations-${new Date().toISOString().split('T')[0]}.csv`;
    a.click();
    window.URL.revokeObjectURL(url);
  };

  const handleQuickApprove = async () => {
    if (!actionModal.registrationId) return;

    try {
      await approveMutation.mutateAsync({
        id: actionModal.registrationId,
        data: { notes: 'Quick approved from table' },
      });
      setActionModal({ type: null, registrationId: null, registrationName: null });
    } catch (error) {
      console.error('Failed to approve registration:', error);
    }
  };

  const handleQuickReject = async () => {
    if (!actionModal.registrationId || !rejectReason.trim()) return;

    try {
      await rejectMutation.mutateAsync({
        id: actionModal.registrationId,
        data: { reason: rejectReason },
      });
      setActionModal({ type: null, registrationId: null, registrationName: null });
      setRejectReason('');
    } catch (error) {
      console.error('Failed to reject registration:', error);
    }
  };

  const isAllSelected = data && selectedIds.length === data.data.length && data.data.length > 0;

  return (
    <AdminLayout>
      <div className="max-w-7xl mx-auto">
        {/* Bulk Actions Toolbar */}
        <BulkActions
          selectedIds={selectedIds}
          onClearSelection={() => setSelectedIds([])}
          onBulkApprove={handleBulkApprove}
          onBulkReject={handleBulkReject}
          onExport={handleExport}
        />
        {/* Page Header */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-6">
          <div>
            <h2 className="text-2xl font-cabrito font-bold text-kq-dark mb-2">
              Registrations
            </h2>
            <p className="text-gray-600">
              Manage and review all alumni registrations
            </p>
          </div>
          <Link href="/admin/registrations/review">
            <Button variant="primary" size="md" leftIcon={<AlertCircle className="w-4 h-4" />}>
              Requiring Review
            </Button>
          </Link>
        </div>

        {/* Advanced Filters */}
        <RegistrationsFilters
          filters={filters}
          onFiltersChange={setFilters}
          totalCount={data?.pagination.totalCount}
        />

        {/* Loading State */}
        {isLoading && (
          <div className="bg-white rounded-tl-sm rounded-tr-2xl rounded-br-sm rounded-bl-2xl border border-gray-200 p-8">
            <div className="animate-pulse space-y-4">
              {[...Array(5)].map((_, i) => (
                <div key={i} className="flex items-center gap-4">
                  <div className="h-4 bg-gray-200 rounded w-1/4" />
                  <div className="h-4 bg-gray-200 rounded w-1/3" />
                  <div className="h-4 bg-gray-200 rounded w-1/5" />
                  <div className="h-4 bg-gray-200 rounded w-1/6" />
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Error State */}
        {error && (
          <div className="bg-red-50 border-l-4 border-red-500 p-4 rounded-r-md">
            <div className="flex items-start">
              <AlertCircle className="w-5 h-5 text-red-500 mt-0.5 mr-3" />
              <div>
                <p className="text-sm font-medium text-red-800">Failed to load registrations</p>
                <p className="text-sm text-red-700 mt-1">
                  {(error as Error)?.message || 'Please try again later'}
                </p>
              </div>
            </div>
          </div>
        )}

        {/* Registrations Table (Desktop) */}
        {data && !isLoading && (
          <>
            {/* Desktop Table View - Hidden on mobile */}
            <div className="hidden md:block bg-white rounded-tl-sm rounded-tr-2xl rounded-br-sm rounded-bl-2xl border border-gray-200 overflow-hidden">
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="bg-gray-50 border-b border-gray-200">
                    <tr>
                      <th className="px-6 py-3 w-12">
                        <input
                          type="checkbox"
                          checked={isAllSelected}
                          onChange={handleSelectAll}
                          className="w-4 h-4 text-kq-red border-gray-300 rounded focus:ring-kq-red"
                        />
                      </th>
                      <SortableHeader
                        label="Staff #"
                        column="staffNumber"
                        currentSort={filters.sortBy}
                        currentOrder={filters.sortOrder}
                        onSort={handleSort}
                      />
                      <SortableHeader
                        label="Alumni Details"
                        column="fullName"
                        currentSort={filters.sortBy}
                        currentOrder={filters.sortOrder}
                        onSort={handleSort}
                      />
                      <SortableHeader
                        label="Contact"
                        column="email"
                        currentSort={filters.sortBy}
                        currentOrder={filters.sortOrder}
                        onSort={handleSort}
                      />
                      <SortableHeader
                        label="Status"
                        column="registrationStatus"
                        currentSort={filters.sortBy}
                        currentOrder={filters.sortOrder}
                        onSort={handleSort}
                      />
                      <SortableHeader
                        label="Registered"
                        column="createdAt"
                        currentSort={filters.sortBy}
                        currentOrder={filters.sortOrder}
                        onSort={handleSort}
                      />
                      <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Actions
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {data.data.length === 0 ? (
                      <tr>
                        <td colSpan={7} className="px-6 py-12 text-center">
                          <div className="flex flex-col items-center justify-center text-gray-500">
                            <Search className="w-12 h-12 mb-3 opacity-50" />
                            <p className="text-lg font-medium mb-1">
                              {filters.searchQuery
                                ? `No registrations found for "${filters.searchQuery}"`
                                : 'No registrations found'}
                            </p>
                            <p className="text-sm">Try adjusting your filters</p>
                          </div>
                        </td>
                      </tr>
                    ) : (
                      data.data.map((registration) => (
                        <tr key={registration.id} className="hover:bg-gray-50 transition-colors">
                          <td className="px-6 py-4 whitespace-nowrap">
                            <input
                              type="checkbox"
                              checked={selectedIds.includes(registration.id)}
                              onChange={(e) => handleSelectOne(registration.id, e.target.checked)}
                              className="w-4 h-4 text-kq-red border-gray-300 rounded focus:ring-kq-red"
                            />
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="flex flex-col">
                              <span className="text-sm font-bold text-kq-red">
                                {registration.registrationNumber || 'N/A'}
                              </span>
                              <span className="text-xs text-gray-500">
                                {registration.staffNumber || 'No Staff #'}
                              </span>
                            </div>
                          </td>
                          <td className="px-6 py-4">
                            <div className="flex flex-col">
                              <span className="text-sm font-medium text-gray-900">
                                {registration.fullName}
                              </span>
                              <span className="text-xs text-gray-500">
                                ID: {registration.idNumber || registration.passportNumber || 'N/A'}
                              </span>
                            </div>
                          </td>
                          <td className="px-6 py-4">
                            <div className="flex flex-col">
                              <span className="text-sm text-gray-900">
                                {registration.email}
                              </span>
                              {registration.mobileNumber && (
                                <span className="text-xs text-gray-500">
                                  {registration.mobileCountryCode} {registration.mobileNumber}
                                </span>
                              )}
                            </div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <StatusBadge
                              status={registration.registrationStatus}
                              requiresManualReview={registration.requiresManualReview}
                            />
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="flex flex-col">
                              <span className="text-sm text-gray-900">
                                {new Date(registration.createdAt).toLocaleDateString()}
                              </span>
                              <span className="text-xs text-gray-500">
                                {new Date(registration.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                              </span>
                            </div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-right">
                            <div className="flex items-center justify-end gap-2">
                              {registration.registrationStatus === 'Pending' && (
                                <>
                                  <Button
                                    variant="ghost"
                                    size="sm"
                                    onClick={() =>
                                      setActionModal({
                                        type: 'approve',
                                        registrationId: registration.id,
                                        registrationName: registration.fullName,
                                      })
                                    }
                                    className="text-green-600 hover:text-green-700 hover:bg-green-50"
                                    leftIcon={<Check className="w-4 h-4" />}
                                  >
                                    Approve
                                  </Button>
                                  <Button
                                    variant="ghost"
                                    size="sm"
                                    onClick={() =>
                                      setActionModal({
                                        type: 'reject',
                                        registrationId: registration.id,
                                        registrationName: registration.fullName,
                                      })
                                    }
                                    className="text-red-600 hover:text-red-700 hover:bg-red-50"
                                    leftIcon={<X className="w-4 h-4" />}
                                  >
                                    Reject
                                  </Button>
                                </>
                              )}
                              <Button
                                variant="ghost"
                                size="sm"
                                onClick={() => setPreviewRegistration(registration)}
                                leftIcon={<Eye className="w-4 h-4" />}
                              >
                                Quick View
                              </Button>
                            </div>
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>
            </div>

            {/* Mobile Card View - Visible on mobile only */}
            <div className="md:hidden space-y-4">
              {data.data.length === 0 ? (
                <div className="bg-white rounded-lg border border-gray-200 p-8">
                  <div className="flex flex-col items-center justify-center text-gray-500">
                    <Search className="w-12 h-12 mb-3 opacity-50" />
                    <p className="text-lg font-medium mb-1">
                      {filters.searchQuery
                        ? `No registrations found for "${filters.searchQuery}"`
                        : 'No registrations found'}
                    </p>
                    <p className="text-sm">Try adjusting your filters</p>
                  </div>
                </div>
              ) : (
                data.data.map((registration) => (
                  <div
                    key={registration.id}
                    className="bg-white rounded-lg border border-gray-200 p-4 space-y-3"
                  >
                    {/* Card Header: Checkbox + Registration Number + Status */}
                    <div className="flex items-start justify-between gap-3">
                      <div className="flex items-start gap-3 flex-1">
                        <input
                          type="checkbox"
                          checked={selectedIds.includes(registration.id)}
                          onChange={(e) => handleSelectOne(registration.id, e.target.checked)}
                          className="w-4 h-4 text-kq-red border-gray-300 rounded focus:ring-kq-red mt-1"
                        />
                        <div className="flex-1">
                          <div className="text-sm font-bold text-kq-red">
                            {registration.registrationNumber || 'N/A'}
                          </div>
                          <div className="text-xs text-gray-500">
                            {registration.staffNumber || 'No Staff #'}
                          </div>
                        </div>
                      </div>
                      <StatusBadge
                        status={registration.registrationStatus}
                        requiresManualReview={registration.requiresManualReview}
                      />
                    </div>

                    {/* Alumni Details */}
                    <div className="border-t border-gray-100 pt-3">
                      <div className="text-sm font-medium text-gray-900 mb-1">
                        {registration.fullName}
                      </div>
                      <div className="text-xs text-gray-500">
                        ID: {registration.idNumber || registration.passportNumber || 'N/A'}
                      </div>
                    </div>

                    {/* Contact Information */}
                    <div className="text-sm space-y-1">
                      <div className="flex items-center gap-2">
                        <Mail className="w-4 h-4 text-gray-400" />
                        <span className="text-gray-900">{registration.email}</span>
                      </div>
                      {registration.mobileNumber && (
                        <div className="text-xs text-gray-500 ml-6">
                          {registration.mobileCountryCode} {registration.mobileNumber}
                        </div>
                      )}
                    </div>

                    {/* Registration Date */}
                    <div className="flex items-center gap-2 text-xs text-gray-500">
                      <Clock className="w-4 h-4" />
                      <span>
                        {new Date(registration.createdAt).toLocaleDateString()} at{' '}
                        {new Date(registration.createdAt).toLocaleTimeString([], {
                          hour: '2-digit',
                          minute: '2-digit',
                        })}
                      </span>
                    </div>

                    {/* Actions */}
                    <div className="flex flex-col gap-2 pt-3 border-t border-gray-100">
                      {registration.registrationStatus === 'Pending' && (
                        <div className="flex gap-2">
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() =>
                              setActionModal({
                                type: 'approve',
                                registrationId: registration.id,
                                registrationName: registration.fullName,
                              })
                            }
                            className="flex-1 text-green-600 hover:text-green-700 hover:bg-green-50"
                            leftIcon={<Check className="w-4 h-4" />}
                          >
                            Approve
                          </Button>
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() =>
                              setActionModal({
                                type: 'reject',
                                registrationId: registration.id,
                                registrationName: registration.fullName,
                              })
                            }
                            className="flex-1 text-red-600 hover:text-red-700 hover:bg-red-50"
                            leftIcon={<X className="w-4 h-4" />}
                          >
                            Reject
                          </Button>
                        </div>
                      )}
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => setPreviewRegistration(registration)}
                        leftIcon={<Eye className="w-4 h-4" />}
                        className="w-full"
                      >
                        Quick View
                      </Button>
                    </div>
                  </div>
                ))
              )}
            </div>

            {/* Pagination */}
            {data.pagination.totalPages > 1 && (
              <div className="flex items-center justify-between mt-6">
                <div className="text-sm text-gray-700">
                  Showing{' '}
                  <span className="font-medium">
                    {(data.pagination.currentPage - 1) * data.pagination.pageSize + 1}
                  </span>{' '}
                  to{' '}
                  <span className="font-medium">
                    {Math.min(
                      data.pagination.currentPage * data.pagination.pageSize,
                      data.pagination.totalCount
                    )}
                  </span>{' '}
                  of <span className="font-medium">{data.pagination.totalCount}</span> results
                </div>
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => handlePageChange(data.pagination.currentPage - 1)}
                    disabled={data.pagination.currentPage === 1}
                    leftIcon={<ChevronLeft className="w-4 h-4" />}
                  >
                    Previous
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => handlePageChange(data.pagination.currentPage + 1)}
                    disabled={data.pagination.currentPage === data.pagination.totalPages}
                    rightIcon={<ChevronRight className="w-4 h-4" />}
                  >
                    Next
                  </Button>
                </div>
              </div>
            )}
          </>
        )}

        {/* Approve Confirmation Modal */}
        {actionModal.type === 'approve' && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
              <h3 className="text-lg font-bold text-gray-900 mb-2">
                Approve Registration
              </h3>
              <p className="text-sm text-gray-600 mb-4">
                Are you sure you want to approve the registration for{' '}
                <span className="font-medium">{actionModal.registrationName}</span>?
              </p>
              <div className="flex justify-end gap-3">
                <Button
                  variant="outline"
                  size="md"
                  onClick={() => setActionModal({ type: null, registrationId: null, registrationName: null })}
                  disabled={approveMutation.isPending}
                >
                  Cancel
                </Button>
                <Button
                  variant="primary"
                  size="md"
                  onClick={handleQuickApprove}
                  disabled={approveMutation.isPending}
                  leftIcon={approveMutation.isPending ? undefined : <CheckCircle2 className="w-4 h-4" />}
                >
                  {approveMutation.isPending ? 'Approving...' : 'Approve'}
                </Button>
              </div>
            </div>
          </div>
        )}

        {/* Reject Confirmation Modal */}
        {actionModal.type === 'reject' && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
              <h3 className="text-lg font-bold text-gray-900 mb-2">
                Reject Registration
              </h3>
              <p className="text-sm text-gray-600 mb-4">
                Please provide a reason for rejecting the registration for{' '}
                <span className="font-medium">{actionModal.registrationName}</span>:
              </p>
              <textarea
                value={rejectReason}
                onChange={(e) => setRejectReason(e.target.value)}
                placeholder="Enter rejection reason..."
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-kq-red focus:border-transparent mb-4"
                rows={4}
              />
              <div className="flex justify-end gap-3">
                <Button
                  variant="outline"
                  size="md"
                  onClick={() => {
                    setActionModal({ type: null, registrationId: null, registrationName: null });
                    setRejectReason('');
                  }}
                  disabled={rejectMutation.isPending}
                >
                  Cancel
                </Button>
                <Button
                  variant="danger"
                  size="md"
                  onClick={handleQuickReject}
                  disabled={rejectMutation.isPending || !rejectReason.trim()}
                  leftIcon={rejectMutation.isPending ? undefined : <XCircle className="w-4 h-4" />}
                >
                  {rejectMutation.isPending ? 'Rejecting...' : 'Reject'}
                </Button>
              </div>
            </div>
          </div>
        )}

        {/* Registration Preview Modal */}
        {previewRegistration && (
          <RegistrationPreviewModal
            registration={previewRegistration}
            onClose={() => setPreviewRegistration(null)}
          />
        )}
      </div>
    </AdminLayout>
  );
}

// ============================================
// Page Wrapper with Suspense
// ============================================

export default function AdminRegistrationsPage() {
  return (
    <Suspense
      fallback={
        <AdminLayout>
          <div className="max-w-7xl mx-auto">
            <div className="bg-white rounded-tl-sm rounded-tr-2xl rounded-br-sm rounded-bl-2xl border border-gray-200 p-8">
              <div className="animate-pulse space-y-4">
                {[...Array(5)].map((_, i) => (
                  <div key={i} className="flex items-center gap-4">
                    <div className="h-4 bg-gray-200 rounded w-1/4" />
                    <div className="h-4 bg-gray-200 rounded w-1/3" />
                    <div className="h-4 bg-gray-200 rounded w-1/5" />
                    <div className="h-4 bg-gray-200 rounded w-1/6" />
                  </div>
                ))}
              </div>
            </div>
          </div>
        </AdminLayout>
      }
    >
      <RegistrationsPageContent />
    </Suspense>
  );
}
