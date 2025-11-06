/**
 * API Configuration
 * Centralized configuration for backend communication.
 */
import { env } from '@/lib/env';

/**
 * Base API configuration
 */
export const API_BASE_URL = env.apiUrl;
export const API_TIMEOUT = env.apiTimeout;

/**
 * REST Endpoints
 */
export const API_ENDPOINTS = {
  REGISTER: '/api/v1/registrations',
  VERIFY_EMAIL: (token: string) => `/api/v1/verification/${token}`,
  GET_REGISTRATION: (id: string) => `/api/v1/registrations/${id}`,
  GET_REGISTRATION_STATUS: (id: string) => `/api/v1/registrations/${id}/status`,
  CHECK_STAFF_NUMBER: (staffNumber: string) =>
    `/api/v1/registrations/check/staff-number/${staffNumber}`,
  CHECK_EMAIL: (email: string) => `/api/v1/registrations/check/email/${email}`,
} as const;

/**
 * Standard HTTP Status Codes
 */
export const API_STATUS = {
  OK: 200,
  CREATED: 201,
  BAD_REQUEST: 400,
  UNAUTHORIZED: 401,
  FORBIDDEN: 403,
  NOT_FOUND: 404,
  CONFLICT: 409,
  SERVER_ERROR: 500,
} as const;
