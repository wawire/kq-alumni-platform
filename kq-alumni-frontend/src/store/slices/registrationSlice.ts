/**
 * Registration Slice (Zustand)
 *
 * Manages registration form state across multiple steps.
 * Persists data to localStorage for recovery if user leaves.
 *
 * @example
 * ```tsx
 * import { useStore } from '@/store';
 *
 * const { formData, setFormData, clearFormData } = useStore();
 *
 * // Update form data
 * setFormData({ staffNumber: '0012345', fullName: 'John Doe' });
 *
 * // Clear on submission
 * clearFormData();
 * ```
 */

import { StateCreator } from 'zustand';

export enum RegistrationStatus {
  IDLE = 'idle',
  PENDING = 'pending',
  APPROVED = 'approved',
  ACTIVE = 'active',
  REJECTED = 'rejected',
  SUCCESS = 'success', // Form successfully submitted, showing success screen
}

export interface RegistrationFormData {
  // Personal Info
  staffNumber?: string;
  idNumber?: string;
  passportNumber?: string;
  fullName?: string;
  email?: string;
  mobileCountryCode?: string;
  mobileNumber?: string;
  currentCountry?: string;
  currentCountryCode?: string;
  currentCity?: string;
  cityCustom?: string;

  // Employment
  currentEmployer?: string;
  currentJobTitle?: string;
  industry?: string;
  linkedInProfile?: string;

  // Education
  qualificationsAttained?: string[];
  professionalCertifications?: string;

  // Engagement
  engagementPreferences?: string[];
  consentGiven?: boolean;
}

export interface RegistrationSlice {
  // State
  formData: RegistrationFormData;
  registrationId: string | null;
  status: RegistrationStatus;
  currentStep: number;

  // Actions
  setFormData: (data: Partial<RegistrationFormData>) => void;
  updateFormData: (data: Partial<RegistrationFormData>) => void;
  setRegistrationId: (id: string) => void;
  setStatus: (status: RegistrationStatus) => void;
  setCurrentStep: (step: number) => void;
  nextStep: () => void;
  previousStep: () => void;
  clearRegistration: () => void;

  // Persistence
  saveToLocalStorage: () => void;
  loadFromLocalStorage: () => void;
}

const STORAGE_KEY = 'kq-alumni-registration';
const TOTAL_STEPS = 3;

export const createRegistrationSlice: StateCreator<
  RegistrationSlice,
  [],
  [],
  RegistrationSlice
> = (set, get) => ({
  // Initial State
  formData: {},
  registrationId: null,
  status: RegistrationStatus.IDLE,
  currentStep: 0,

  // Actions
  setFormData: (data: Partial<RegistrationFormData>) => {
    set({ formData: data });
    get().saveToLocalStorage();
  },

  updateFormData: (data: Partial<RegistrationFormData>) => {
    set((state) => ({
      formData: { ...state.formData, ...data },
    }));
    get().saveToLocalStorage();
  },

  setRegistrationId: (id: string | null) => {
    set({ registrationId: id });
    get().saveToLocalStorage();
  },

  setStatus: (status: RegistrationStatus) => {
    set({ status });
  },

  setCurrentStep: (step: number) => {
    if (step >= 0 && step < TOTAL_STEPS) {
      set({ currentStep: step });
      get().saveToLocalStorage();
    }
  },

  nextStep: () => {
    const { currentStep } = get();
    if (currentStep < TOTAL_STEPS - 1) {
      set({ currentStep: currentStep + 1 });
      get().saveToLocalStorage();
    }
  },

  previousStep: () => {
    const { currentStep } = get();
    if (currentStep > 0) {
      set({ currentStep: currentStep - 1 });
      get().saveToLocalStorage();
    }
  },

  clearRegistration: () => {
    set({
      formData: {},
      registrationId: null,
      status: RegistrationStatus.IDLE,
      currentStep: 0,
    });

    // Clear from localStorage
    if (typeof window !== 'undefined') {
      window.localStorage.removeItem(STORAGE_KEY);
    }
  },

  // Persistence
  saveToLocalStorage: () => {
    if (typeof window === 'undefined') {
      return;
    }

    const { formData, registrationId, currentStep } = get();

    try {
      window.localStorage.setItem(
        STORAGE_KEY,
        JSON.stringify({
          formData,
          registrationId,
          currentStep,
          savedAt: new Date().toISOString(),
        })
      );
    } catch (error) {
      if (process.env.NODE_ENV === 'development') {
        console.error('Error saving registration to localStorage:', error);
      }
    }
  },

  loadFromLocalStorage: () => {
    if (typeof window === 'undefined') {
      return;
    }

    try {
      const stored = window.localStorage.getItem(STORAGE_KEY);

      if (stored) {
        const { formData, registrationId, currentStep } = JSON.parse(stored);

        set({
          formData: formData || {},
          registrationId: registrationId || null,
          currentStep: currentStep || 0,
        });
      }
    } catch (error) {
      if (process.env.NODE_ENV === 'development') {
        console.error('Error loading registration from localStorage:', error);
      }
    }
  },
});
