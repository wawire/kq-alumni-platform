/**
 * useDuplicateCheck Hook
 *
 * Checks for duplicate values in registration (staff number, email, etc.)
 * Uses debouncing to avoid excessive API calls.
 *
 * @example
 * ```tsx
 * const {
 *   isDuplicate,
 *   isChecking,
 *   error,
 *   checkValue
 * } = useDuplicateCheck('staffNumber', 500);
 *
 * // In your input onChange
 * const handleChange = (value: string) => {
 *   setValue(value);
 *   checkValue(value);
 * };
 *
 * // Show error if duplicate
 * {isDuplicate && <p>This staff number is already registered</p>}
 * ```
 */

import { useState, useCallback } from 'react';
import { API_BASE_URL } from '@/config/api';
import type { DuplicateField } from '@/types';

interface UseDuplicateCheckResult {
  /**
   * Whether the value is a duplicate
   */
  isDuplicate: boolean;

  /**
   * Whether a check is in progress
   */
  isChecking: boolean;

  /**
   * Error message if check failed
   */
  error: string | null;

  /**
   * Function to trigger duplicate check
   */
  checkValue: (value: string) => Promise<void>;

  /**
   * Reset the check state
   */
  reset: () => void;
}

/**
 * Checks for duplicate registration values
 * @param field - Field to check (staffNumber, email, mobile, linkedin)
 * @returns Duplicate check state and functions
 */
export function useDuplicateCheck(
  field: DuplicateField
): UseDuplicateCheckResult {
  const [isChecking, setIsChecking] = useState(false);
  const [isDuplicate, setIsDuplicate] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const checkValue = useCallback(
    async (value: string) => {
      // Reset state
      setError(null);
      setIsDuplicate(false);

      // Skip empty values
      if (!value || value.trim() === '') {
        return;
      }

      setIsChecking(true);

      try {
        // Convert camelCase field name to kebab-case for API route
        const fieldMap: Record<DuplicateField, string> = {
          staffNumber: 'staff-number',
          email: 'email',
          mobile: 'mobile',
          linkedin: 'linkedin',
        };
        const apiField = fieldMap[field];

        // Build check endpoint based on field
        const endpoint = `${API_BASE_URL}/api/v1/registrations/check/${apiField}/${encodeURIComponent(
          value
        )}`;

        const response = await fetch(endpoint, {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
          },
        });

        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();

        // API returns { exists: boolean }
        setIsDuplicate(data.exists || false);

        if (data.exists) {
          // Set user-friendly error message based on field
          const fieldNames: Record<DuplicateField, string> = {
            staffNumber: 'Staff number',
            email: 'Email address',
            mobile: 'Mobile number',
            linkedin: 'LinkedIn profile',
          };

          setError(
            `This ${fieldNames[field].toLowerCase()} is already registered`
          );
        }
      } catch (err) {
        if (process.env.NODE_ENV === 'development') {
          console.error('Error checking for duplicates:', err);
        }
        setError('Unable to verify. Please try again.');
        setIsDuplicate(false);
      } finally {
        setIsChecking(false);
      }
    },
    [field]
  );

  const reset = useCallback(() => {
    setIsDuplicate(false);
    setIsChecking(false);
    setError(null);
  }, []);

  return {
    isDuplicate,
    isChecking,
    error,
    checkValue,
    reset,
  };
}
