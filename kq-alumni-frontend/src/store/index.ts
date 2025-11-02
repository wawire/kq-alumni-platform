/**
 * Zustand Store
 *
 * Combined store with all slices and middleware.
 * Includes devtools for debugging and persistence.
 *
 * @example
 * ```tsx
 * import { useStore } from '@/store';
 *
 * function MyComponent() {
 *   const { formData, setFormData, showToast } = useStore();
 *
 *   return <div>...</div>;
 * }
 * ```
 */

import { create } from 'zustand';
import { devtools, persist } from 'zustand/middleware';
import {
  createRegistrationSlice,
  type RegistrationSlice,
} from './slices/registrationSlice';
import { createUISlice, type UISlice } from './slices/uiSlice';

/**
 * Combined store state type
 */
export type StoreState = RegistrationSlice & UISlice;

/**
 * Main application store
 * Combines all slices with devtools and persistence
 */
export const useStore = create<StoreState>()(
  devtools(
    persist(
      (...args) => ({
        ...createRegistrationSlice(...args),
        ...createUISlice(...args),
      }),
      {
        name: 'kq-alumni-store',
        // Only persist registration data, not UI state
        partialize: (state) => ({
          formData: state.formData,
          registrationId: state.registrationId,
          currentStep: state.currentStep,
        }),
      }
    ),
    {
      name: 'KQ Alumni Store',
      enabled: process.env.NODE_ENV === 'development',
    }
  )
);

/**
 * Typed selectors for better performance
 * Use these to avoid re-renders when unrelated state changes
 */

// Registration Selectors
export const useRegistrationFormData = () =>
  useStore((state) => state.formData);
export const useRegistrationId = () => useStore((state) => state.registrationId);
export const useRegistrationStatus = () => useStore((state) => state.status);
export const useCurrentStep = () => useStore((state) => state.currentStep);

// UI Selectors
export const useIsLoading = () => useStore((state) => state.isLoading);
export const useToasts = () => useStore((state) => state.toasts);
export const useIsModalOpen = () => useStore((state) => state.isModalOpen);
export const useModalContent = () => useStore((state) => state.modalContent);
export const useIsSidebarOpen = () => useStore((state) => state.isSidebarOpen);

// Action Selectors
export const useRegistrationActions = () =>
  useStore((state) => ({
    setFormData: state.setFormData,
    updateFormData: state.updateFormData,
    setRegistrationId: state.setRegistrationId,
    setStatus: state.setStatus,
    setCurrentStep: state.setCurrentStep,
    nextStep: state.nextStep,
    previousStep: state.previousStep,
    clearRegistration: state.clearRegistration,
    saveToLocalStorage: state.saveToLocalStorage,
    loadFromLocalStorage: state.loadFromLocalStorage,
  }));

export const useUIActions = () =>
  useStore((state) => ({
    setLoading: state.setLoading,
    showToast: state.showToast,
    removeToast: state.removeToast,
    clearToasts: state.clearToasts,
    openModal: state.openModal,
    closeModal: state.closeModal,
    toggleSidebar: state.toggleSidebar,
    setSidebarOpen: state.setSidebarOpen,
  }));

// Re-export types for convenience
export type {
  RegistrationSlice,
  RegistrationFormData,
} from './slices/registrationSlice';
export { RegistrationStatus } from './slices/registrationSlice';
export type { UISlice, Toast, ToastType } from './slices/uiSlice';
