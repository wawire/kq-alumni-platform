/**
 * UI Slice (Zustand)
 *
 * Manages global UI state (modals, toasts, loading indicators, etc.)
 *
 * @example
 * ```tsx
 * import { useStore } from '@/store';
 *
 * const { isLoading, setLoading } = useStore();
 *
 * // Show loading
 * setLoading(true);
 *
 * // Show toast
 * showToast('Success!', 'success');
 * ```
 */

import { StateCreator } from 'zustand';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface Toast {
  id: string;
  message: string;
  type: ToastType;
  duration?: number;
}

export interface UISlice {
  // Loading State
  isLoading: boolean;
  loadingMessage: string | null;

  // Toast Notifications
  toasts: Toast[];

  // Modal State
  isModalOpen: boolean;
  modalContent: React.ReactNode | null;

  // Sidebar State (for mobile)
  isSidebarOpen: boolean;

  // Actions - Loading
  setLoading: (loading: boolean, message?: string) => void;

  // Actions - Toasts
  showToast: (message: string, type?: ToastType, duration?: number) => void;
  removeToast: (id: string) => void;
  clearToasts: () => void;

  // Actions - Modal
  openModal: (content: React.ReactNode) => void;
  closeModal: () => void;

  // Actions - Sidebar
  toggleSidebar: () => void;
  setSidebarOpen: (open: boolean) => void;
}

export const createUISlice: StateCreator<UISlice, [], [], UISlice> = (
  set,
  get
) => ({
  // Initial State
  isLoading: false,
  loadingMessage: null,
  toasts: [],
  isModalOpen: false,
  modalContent: null,
  isSidebarOpen: false,

  // Loading Actions
  setLoading: (loading: boolean, message?: string) => {
    set({
      isLoading: loading,
      loadingMessage: message || null,
    });
  },

  // Toast Actions
  showToast: (message: string, type: ToastType = 'info', duration: number = 5000) => {
    const id = `toast-${Date.now()}-${Math.random()}`;

    const newToast: Toast = {
      id,
      message,
      type,
      duration,
    };

    set((state) => ({
      toasts: [...state.toasts, newToast],
    }));

    // Auto-remove toast after duration
    if (duration > 0) {
      setTimeout(() => {
        get().removeToast(id);
      }, duration);
    }
  },

  removeToast: (id: string) => {
    set((state) => ({
      toasts: state.toasts.filter((toast) => toast.id !== id),
    }));
  },

  clearToasts: () => {
    set({ toasts: [] });
  },

  // Modal Actions
  openModal: (content: React.ReactNode) => {
    set({
      isModalOpen: true,
      modalContent: content,
    });
  },

  closeModal: () => {
    set({
      isModalOpen: false,
      modalContent: null,
    });
  },

  // Sidebar Actions
  toggleSidebar: () => {
    set((state) => ({
      isSidebarOpen: !state.isSidebarOpen,
    }));
  },

  setSidebarOpen: (open: boolean) => {
    set({ isSidebarOpen: open });
  },
});
