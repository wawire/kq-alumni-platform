/**
 * Confirmation Modal Component
 * Reusable modal for confirming destructive or important actions
 */

import { AlertCircle, CheckCircle2, XCircle } from 'lucide-react';
import { Button } from '../button/Button';

interface ConfirmationModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  type?: 'danger' | 'warning' | 'success' | 'info';
  isLoading?: boolean;
}

export function ConfirmationModal({
  isOpen,
  onClose,
  onConfirm,
  title,
  message,
  confirmText = 'Confirm',
  cancelText = 'Cancel',
  type = 'info',
  isLoading = false,
}: ConfirmationModalProps) {
  if (!isOpen) return null;

  const iconColors = {
    danger: 'text-red-600',
    warning: 'text-yellow-600',
    success: 'text-green-600',
    info: 'text-blue-600',
  };

  const Icon = {
    danger: XCircle,
    warning: AlertCircle,
    success: CheckCircle2,
    info: AlertCircle,
  }[type];

  const confirmButtonColor = {
    danger: 'bg-red-600 hover:bg-red-700',
    warning: 'bg-yellow-600 hover:bg-yellow-700',
    success: 'bg-green-600 hover:bg-green-700',
    info: 'bg-blue-600 hover:bg-blue-700',
  }[type];

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black bg-opacity-50 z-40 transition-opacity"
        onClick={onClose}
      />

      {/* Modal */}
      <div className="fixed inset-0 flex items-center justify-center z-50 p-4">
        <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6 transform transition-all">
          {/* Icon */}
          <div className="flex items-center justify-center mb-4">
            <div className={`w-12 h-12 rounded-full bg-gray-100 flex items-center justify-center`}>
              <Icon className={`w-6 h-6 ${iconColors[type]}`} />
            </div>
          </div>

          {/* Content */}
          <div className="text-center mb-6">
            <h3 className="text-lg font-bold text-kq-dark mb-2">{title}</h3>
            <p className="text-sm text-gray-600">{message}</p>
          </div>

          {/* Actions */}
          <div className="flex gap-3">
            <Button
              variant="outline"
              size="md"
              onClick={onClose}
              disabled={isLoading}
              className="flex-1"
            >
              {cancelText}
            </Button>
            <Button
              variant="primary"
              size="md"
              onClick={onConfirm}
              disabled={isLoading}
              className={`flex-1 ${confirmButtonColor}`}
            >
              {isLoading ? 'Processing...' : confirmText}
            </Button>
          </div>
        </div>
      </div>
    </>
  );
}
