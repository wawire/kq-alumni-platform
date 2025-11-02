/**
 * Admin Store
 *
 * Separate store for admin dashboard to keep admin state isolated
 * from main user-facing application state.
 */

import { create } from 'zustand';
import { devtools } from 'zustand/middleware';
import { createAdminSlice, type AdminSlice } from './slices/adminSlice';

/**
 * Admin store state type
 */
export type AdminStoreState = AdminSlice;

/**
 * Admin application store
 * Isolated from main user store for security
 */
export const useAdminStore = create<AdminStoreState>()(
  devtools(
    (...args) => ({
      ...createAdminSlice(...args),
    }),
    {
      name: 'KQ Alumni Admin Store',
      enabled: process.env.NODE_ENV === 'development',
    }
  )
);

/**
 * Typed selectors for better performance
 */

// Authentication Selectors
export const useAdminUser = () => useAdminStore((state) => state.user);
export const useIsAdminAuthenticated = () =>
  useAdminStore((state) => state.isAuthenticated);
export const useAdminToken = () => useAdminStore((state) => state.token);

// Filter Selectors
export const useRegistrationFilters = () =>
  useAdminStore((state) => state.registrationFilters);

// Modal Selectors
export const useSelectedRegistrationId = () =>
  useAdminStore((state) => state.selectedRegistrationId);
export const useIsApprovalModalOpen = () =>
  useAdminStore((state) => state.isApprovalModalOpen);
export const useIsRejectionModalOpen = () =>
  useAdminStore((state) => state.isRejectionModalOpen);

// Action Selectors
export const useAdminAuthActions = () =>
  useAdminStore((state) => ({
    setUser: state.setUser,
    setToken: state.setToken,
    setAuthenticated: state.setAuthenticated,
    logout: state.logout,
  }));

export const useAdminFilterActions = () =>
  useAdminStore((state) => ({
    setRegistrationFilters: state.setRegistrationFilters,
    resetFilters: state.resetFilters,
  }));

export const useAdminModalActions = () =>
  useAdminStore((state) => ({
    selectRegistration: state.selectRegistration,
    openApprovalModal: state.openApprovalModal,
    closeApprovalModal: state.closeApprovalModal,
    openRejectionModal: state.openRejectionModal,
    closeRejectionModal: state.closeRejectionModal,
  }));

// Re-export types for convenience
export type { AdminSlice } from './slices/adminSlice';
