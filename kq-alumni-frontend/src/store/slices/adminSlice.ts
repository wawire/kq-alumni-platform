/**
 * Admin Slice
 * State management for admin dashboard
 */

import { StateCreator } from 'zustand';
import type { AdminUser, RegistrationFilters } from '@/types/admin';

// ============================================
// Types
// ============================================

export interface AdminSlice {
  // Authentication state
  user: AdminUser | null;
  isAuthenticated: boolean;
  token: string | null;

  // Registration filters
  registrationFilters: RegistrationFilters;

  // UI state
  selectedRegistrationId: string | null;
  isApprovalModalOpen: boolean;
  isRejectionModalOpen: boolean;

  // Actions
  setUser: (user: AdminUser | null) => void;
  setToken: (token: string | null) => void;
  setAuthenticated: (isAuthenticated: boolean) => void;
  logout: () => void;

  // Filter actions
  setRegistrationFilters: (filters: RegistrationFilters) => void;
  resetFilters: () => void;

  // Modal actions
  selectRegistration: (id: string | null) => void;
  openApprovalModal: (id: string) => void;
  closeApprovalModal: () => void;
  openRejectionModal: (id: string) => void;
  closeRejectionModal: () => void;
}

// ============================================
// Initial State
// ============================================

const initialState = {
  user: null,
  isAuthenticated: false,
  token: null,
  registrationFilters: {
    pageNumber: 1,
    pageSize: 20,
  },
  selectedRegistrationId: null,
  isApprovalModalOpen: false,
  isRejectionModalOpen: false,
};

// ============================================
// Slice Creator
// ============================================

export const createAdminSlice: StateCreator<AdminSlice> = (set) => ({
  ...initialState,

  // Authentication actions
  setUser: (user: AdminUser | null) =>
    set({
      user,
      isAuthenticated: Boolean(user),
    }),

  setToken: (token: string | null) =>
    set({
      token,
      isAuthenticated: Boolean(token),
    }),

  setAuthenticated: (isAuthenticated: boolean) =>
    set({
      isAuthenticated,
    }),

  logout: () => {
    // Clear localStorage
    if (typeof window !== 'undefined') {
      localStorage.removeItem('admin_token');
      localStorage.removeItem('admin_user');
    }

    // Reset state
    set({
      user: null,
      isAuthenticated: false,
      token: null,
      registrationFilters: initialState.registrationFilters,
      selectedRegistrationId: null,
      isApprovalModalOpen: false,
      isRejectionModalOpen: false,
    });
  },

  // Filter actions
  setRegistrationFilters: (filters: Partial<RegistrationFilters>) =>
    set((state) => ({
      registrationFilters: {
        ...state.registrationFilters,
        ...filters,
      },
    })),

  resetFilters: () =>
    set({
      registrationFilters: initialState.registrationFilters,
    }),

  // Modal actions
  selectRegistration: (id: string | null) =>
    set({
      selectedRegistrationId: id,
    }),

  openApprovalModal: (id: string) =>
    set({
      selectedRegistrationId: id,
      isApprovalModalOpen: true,
      isRejectionModalOpen: false,
    }),

  closeApprovalModal: () =>
    set({
      isApprovalModalOpen: false,
      selectedRegistrationId: null,
    }),

  openRejectionModal: (id: string) =>
    set({
      selectedRegistrationId: id,
      isRejectionModalOpen: true,
      isApprovalModalOpen: false,
    }),

  closeRejectionModal: () =>
    set({
      isRejectionModalOpen: false,
      selectedRegistrationId: null,
    }),
});
