/**
 * Registration Type Definitions
 *
 * Centralized type definitions for all registration-related data structures.
 * This is the single source of truth for registration types across the application.
 */

/**
 * Registration form data structure
 * Used for submitting new alumni registrations
 */
export interface RegistrationFormData {
  // Personal Information
  staffNumber: string;
  fullName: string;
  email: string;
  mobileCountryCode?: string;
  mobileNumber?: string;

  // Location Information
  currentCountry: string;
  currentCountryCode: string;
  currentCity: string;
  cityCustom?: string;

  // Employment Information
  currentEmployer?: string;
  currentJobTitle?: string;
  industry?: string;
  linkedInProfile?: string;

  // Qualifications
  qualificationsAttained: string[];
  professionalCertifications?: string;

  // Engagement Preferences
  engagementPreferences: string[];

  // Consent
  consentGiven: boolean;
}

/**
 * Registration API response
 * Returned when a new registration is successfully created
 */
export interface RegistrationResponse {
  id: string;
  registrationNumber?: string;
  staffNumber: string;
  fullName: string;
  email: string;
  mobile?: string;
  status: string;
  message: string;
  registeredAt?: string;
  createdAt: string;
}

/**
 * Registration status response
 * Used for checking the status of an existing registration
 */
export interface RegistrationStatusResponse {
  id: string;
  staffNumber: string;
  fullName: string;
  email: string;
  status: string;
  emailVerified: boolean;
  erpValidated: boolean;
  approvedAt?: string;
  rejectedAt?: string;
  rejectionReason?: string;
  createdAt: string;
  updatedAt: string;
}

/**
 * Email verification response
 * Returned when verifying an email with a token
 */
export interface VerificationResponse {
  success: boolean;
  message: string;
  email?: string;
  fullName?: string;
}

/**
 * Duplicate check response
 * Used for checking if a value already exists
 */
export interface DuplicateCheckResponse {
  exists: boolean;
  field?: string;
  value?: string;
}

/**
 * API validation error structure
 * Follows ASP.NET Core validation problem details format
 */
export interface ValidationError {
  type: string;
  title: string;
  status: number;
  errors?: Record<string, string[]>;
  detail?: string;
  traceId?: string;
}

/**
 * Generic error response
 */
export interface ErrorResponse {
  detail?: string;
  message?: string;
  status?: number;
}

/**
 * Registration status enum
 */
export enum RegistrationStatus {
  PENDING = 'Pending',
  EMAIL_VERIFIED = 'EmailVerified',
  ERP_VALIDATED = 'ErpValidated',
  APPROVED = 'Approved',
  REJECTED = 'Rejected',
}

/**
 * Duplicate field types for validation
 */
export type DuplicateField = 'staffNumber' | 'email' | 'mobile' | 'linkedin';
