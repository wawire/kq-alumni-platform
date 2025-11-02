/**
 * Registration API Service
 *
 * React Query hooks for registration-related API calls.
 * Provides automatic caching, retries, and state management.
 *
 * @example
 * ```tsx
 * import { useSubmitRegistration } from '@/lib/api/services/registrationService';
 *
 * function RegistrationForm() {
 *   const { mutate, isPending, isError } = useSubmitRegistration();
 *
 *   const handleSubmit = (data) => {
 *     mutate(data, {
 *       onSuccess: () => alert('Success!'),
 *       onError: (error) => alert(error.message),
 *     });
 *   };
 * }
 * ```
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import axios from 'axios';
import { queryKeys } from '../queryClient';
import { API_BASE_URL } from '@/config/api';
import type {
  RegistrationFormData,
  RegistrationResponse,
  RegistrationStatusResponse,
} from '@/types';

/**
 * Submit registration mutation
 */
export const useSubmitRegistration = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: RegistrationFormData): Promise<RegistrationResponse> => {
      try {
        const response = await axios.post(
          `${API_BASE_URL}/api/v1/registrations`,
          data
        );
        return response.data;
      } catch (error) {
        if (axios.isAxiosError(error)) {
          // Network error
          if (!error.response) {
            throw new Error('Network error. Please check your connection and try again.');
          }
          // Server error with response
          const message = error.response.data?.message || error.response.data?.detail || 'Registration failed. Please try again.';
          throw new Error(message);
        }
        throw error;
      }
    },
    onSuccess: (data) => {
      // Invalidate relevant queries
      queryClient.invalidateQueries({
        queryKey: queryKeys.registration.all,
      });

      // Optionally cache the registration detail
      if (data.id) {
        queryClient.setQueryData(
          queryKeys.registration.detail(data.id),
          data
        );
      }
    },
  });
};

/**
 * Get registration status by email
 */
export const useRegistrationStatus = (email: string, enabled: boolean = true) => {
  return useQuery({
    queryKey: queryKeys.registration.status(email),
    queryFn: async (): Promise<RegistrationStatusResponse> => {
      try {
        const response = await axios.get(
          `${API_BASE_URL}/api/v1/registrations/status?email=${encodeURIComponent(email)}`
        );
        return response.data;
      } catch (error) {
        if (axios.isAxiosError(error)) {
          if (!error.response) {
            throw new Error('Unable to check registration status. Please check your connection.');
          }
          const message = error.response.data?.message || error.response.data?.detail || 'Failed to fetch registration status.';
          throw new Error(message);
        }
        throw error;
      }
    },
    enabled: Boolean(email) && enabled,
    staleTime: 30000, // 30 seconds
    retry: 3,
    retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000), // Exponential backoff
  });
};

/**
 * Check for duplicate values
 */
export const useDuplicateCheck = (
  field: string,
  value: string,
  enabled: boolean = false
) => {
  return useQuery({
    queryKey: queryKeys.registration.duplicate(field, value),
    queryFn: async (): Promise<{ exists: boolean }> => {
      try {
        const response = await axios.get(
          `${API_BASE_URL}/api/v1/registrations/check/${field}/${encodeURIComponent(value)}`
        );
        return response.data;
      } catch (error) {
        if (axios.isAxiosError(error)) {
          if (!error.response) {
            throw new Error('Unable to check for duplicates. Please check your connection.');
          }
          // Return false on error to allow user to continue
          return { exists: false };
        }
        throw error;
      }
    },
    enabled: Boolean(value) && enabled,
    staleTime: 60000, // 1 minute
    retry: false, // Don't retry duplicate checks
  });
};

/**
 * Verify email with token
 */
export const useVerifyEmail = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (token: string): Promise<{ success: boolean; message: string }> => {
      try {
        const response = await axios.get(`${API_BASE_URL}/api/v1/verify/${token}`);
        return response.data;
      } catch (error) {
        if (axios.isAxiosError(error)) {
          if (!error.response) {
            throw new Error('Unable to verify email. Please check your connection and try again.');
          }
          const message = error.response.data?.message || error.response.data?.detail || 'Email verification failed.';
          throw new Error(message);
        }
        throw error;
      }
    },
    onSuccess: () => {
      // Invalidate registration queries
      queryClient.invalidateQueries({
        queryKey: queryKeys.registration.all,
      });
    },
  });
};
