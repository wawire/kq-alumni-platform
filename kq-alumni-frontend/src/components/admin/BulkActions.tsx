'use client';

/**
 * Bulk Actions Component
 * Provides bulk approve/reject functionality for registrations
 */

import { useState } from 'react';
import {
  CheckCircle,
  XCircle,
  Download,
  X,
  AlertTriangle,
} from 'lucide-react';
import { Button } from '@/components/ui/button/Button';

interface BulkActionsProps {
  selectedIds: string[];
  onClearSelection: () => void;
  onBulkApprove: (ids: string[]) => Promise<void>;
  onBulkReject: (ids: string[], reason: string) => Promise<void>;
  onExport: (ids: string[]) => void;
}

export function BulkActions({
  selectedIds,
  onClearSelection,
  onBulkApprove,
  onBulkReject,
  onExport,
}: BulkActionsProps) {
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [rejectReason, setRejectReason] = useState('');
  const [isProcessing, setIsProcessing] = useState(false);

  if (selectedIds.length === 0) {
    return null;
  }

  const handleBulkApprove = async () => {
    if (!confirm(`Are you sure you want to approve ${selectedIds.length} registration(s)?`)) {
      return;
    }

    setIsProcessing(true);
    try {
      await onBulkApprove(selectedIds);
      onClearSelection();
    } catch (error) {
      console.error('Bulk approve failed:', error);
      alert('Failed to approve registrations. Please try again.');
    } finally {
      setIsProcessing(false);
    }
  };

  const handleBulkReject = async () => {
    if (!rejectReason.trim()) {
      alert('Please provide a reason for rejection');
      return;
    }

    setIsProcessing(true);
    try {
      await onBulkReject(selectedIds, rejectReason);
      setShowRejectModal(false);
      setRejectReason('');
      onClearSelection();
    } catch (error) {
      console.error('Bulk reject failed:', error);
      alert('Failed to reject registrations. Please try again.');
    } finally {
      setIsProcessing(false);
    }
  };

  const handleExport = () => {
    onExport(selectedIds);
  };

  return (
    <>
      {/* Bulk Actions Toolbar */}
      <div className="fixed bottom-6 left-1/2 -translate-x-1/2 z-50 animate-slide-up">
        <div className="bg-kq-dark text-white rounded-lg shadow-2xl border border-gray-700 px-6 py-4 flex items-center gap-6">
          {/* Selection Info */}
          <div className="flex items-center gap-3">
            <div className="bg-kq-red bg-opacity-20 rounded-full w-10 h-10 flex items-center justify-center">
              <span className="text-lg font-bold">{selectedIds.length}</span>
            </div>
            <div>
              <p className="font-medium">
                {selectedIds.length} item{selectedIds.length !== 1 ? 's' : ''} selected
              </p>
              <button
                onClick={onClearSelection}
                className="text-xs text-gray-300 hover:text-white underline"
              >
                Clear selection
              </button>
            </div>
          </div>

          {/* Divider */}
          <div className="h-12 w-px bg-gray-600" />

          {/* Actions */}
          <div className="flex items-center gap-3">
            <Button
              variant="outline"
              size="sm"
              leftIcon={<CheckCircle className="w-4 h-4" />}
              onClick={handleBulkApprove}
              disabled={isProcessing}
              className="bg-green-600 hover:bg-green-700 text-white border-green-600 hover:border-green-700"
            >
              {isProcessing ? 'Processing...' : 'Approve'}
            </Button>

            <Button
              variant="outline"
              size="sm"
              leftIcon={<XCircle className="w-4 h-4" />}
              onClick={() => setShowRejectModal(true)}
              disabled={isProcessing}
              className="bg-red-600 hover:bg-red-700 text-white border-red-600 hover:border-red-700"
            >
              Reject
            </Button>

            <Button
              variant="outline"
              size="sm"
              leftIcon={<Download className="w-4 h-4" />}
              onClick={handleExport}
              disabled={isProcessing}
              className="bg-blue-600 hover:bg-blue-700 text-white border-blue-600 hover:border-blue-700"
            >
              Export
            </Button>
          </div>

          {/* Close Button */}
          <button
            onClick={onClearSelection}
            className="ml-2 text-gray-300 hover:text-white transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>
      </div>

      {/* Reject Modal */}
      {showRejectModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
            <div className="flex items-start gap-3 mb-4">
              <div className="bg-red-100 rounded-full p-2">
                <AlertTriangle className="w-6 h-6 text-red-600" />
              </div>
              <div className="flex-1">
                <h3 className="text-lg font-bold text-kq-dark mb-1">
                  Reject {selectedIds.length} Registration{selectedIds.length !== 1 ? 's' : ''}
                </h3>
                <p className="text-sm text-gray-600">
                  Please provide a reason for rejection. This will be recorded in the audit log.
                </p>
              </div>
            </div>

            <div className="mb-6">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Rejection Reason <span className="text-red-500">*</span>
              </label>
              <textarea
                value={rejectReason}
                onChange={(e) => setRejectReason(e.target.value)}
                placeholder="Enter reason for rejection..."
                rows={4}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-kq-red focus:border-transparent resize-none"
                disabled={isProcessing}
              />
            </div>

            <div className="flex gap-3 justify-end">
              <Button
                variant="outline"
                size="md"
                onClick={() => {
                  setShowRejectModal(false);
                  setRejectReason('');
                }}
                disabled={isProcessing}
              >
                Cancel
              </Button>
              <Button
                variant="primary"
                size="md"
                onClick={handleBulkReject}
                disabled={isProcessing || !rejectReason.trim()}
                className="bg-red-600 hover:bg-red-700"
              >
                {isProcessing ? 'Processing...' : 'Reject All'}
              </Button>
            </div>
          </div>
        </div>
      )}

      <style jsx>{`
        @keyframes slide-up {
          from {
            transform: translate(-50%, 100%);
            opacity: 0;
          }
          to {
            transform: translate(-50%, 0);
            opacity: 1;
          }
        }
        .animate-slide-up {
          animation: slide-up 0.3s ease-out;
        }
      `}</style>
    </>
  );
}
