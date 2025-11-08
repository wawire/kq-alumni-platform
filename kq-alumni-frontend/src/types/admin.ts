/**
 * Admin Types
 * Type definitions for admin dashboard features
 */

// ============================================
// Admin User Types
// ============================================

export interface AdminUser {
  userId: number;
  username: string;
  email: string;
  fullName: string;
  role: AdminRole;
}

export type AdminRole = 'SuperAdmin' | 'HRManager' | 'HROfficer';

// ============================================
// Authentication Types
// ============================================

export interface AdminLoginRequest {
  username: string;
  password: string;
}

export interface AdminLoginResponse {
  token: string;
  expiresAt: string;
  userId: number;
  username: string;
  fullName: string;
  role: AdminRole;
  email: string;
}

export interface CreateAdminUserRequest {
  username: string;
  email: string;
  password: string;
  fullName: string;
  role: AdminRole;
}

// ============================================
// Registration Management Types
// ============================================

export interface AdminRegistration {
  id: string;
  staffNumber?: string;
  idNumber?: string;
  passportNumber?: string;
  fullName: string;
  email: string;
  mobileCountryCode?: string;
  mobileNumber?: string;
  linkedInProfile?: string;
  currentCountry: string;
  currentCountryCode: string;
  currentCity: string;
  qualificationsAttained: string;
  engagementPreferences: string;
  registrationStatus: RegistrationStatus;

  // ERP Validation
  erpValidated: boolean;
  erpValidatedAt?: string;
  erpStaffName?: string;
  erpDepartment?: string;
  erpExitDate?: string;
  erpValidationAttempts?: number;
  lastErpValidationAttempt?: string;

  // Manual Review
  requiresManualReview: boolean;
  manualReviewReason?: string;
  manuallyReviewed: boolean;
  reviewedBy?: string;
  reviewedAt?: string;
  reviewNotes?: string;

  // Email Verification
  emailVerified: boolean;
  emailVerifiedAt?: string;

  // Approval/Rejection
  approvedAt?: string;
  rejectedAt?: string;
  rejectionReason?: string;

  // Timestamps
  createdAt: string;
  updatedAt: string;
  updatedBy?: string;
}

export type RegistrationStatus = 'Pending' | 'Approved' | 'Rejected' | 'Active';

export interface PaginatedRegistrations {
  data: AdminRegistration[];
  pagination: {
    currentPage: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
  };
}

export type SortableColumn = 'fullName' | 'createdAt' | 'registrationStatus' | 'staffNumber' | 'email';
export type SortOrder = 'asc' | 'desc';

export interface RegistrationFilters {
  status?: RegistrationStatus;
  requiresManualReview?: boolean;
  searchQuery?: string;
  dateFrom?: string;
  dateTo?: string;
  emailVerified?: boolean;
  sortBy?: SortableColumn;
  sortOrder?: SortOrder;
  pageNumber?: number;
  pageSize?: number;
}

// ============================================
// Dashboard Stats Types
// ============================================

export interface DashboardStats {
  totalRegistrations: number;
  pendingApproval: number;
  requiringManualReview: number;
  approved: number;
  rejected: number;
  active: number;
  emailVerified: number;
  emailNotVerified: number;
}

// ============================================
// Audit Log Types
// ============================================

export interface AuditLog {
  id: number;
  registrationId: string;
  action: string;
  performedBy: string;
  adminUserId?: number;
  notes?: string;
  rejectionReason?: string;
  timestamp: string;
  ipAddress?: string;
  previousStatus?: string;
  newStatus?: string;
  isAutomated: boolean;
  adminUser?: {
    username: string;
    fullName: string;
    role: AdminRole;
  };
}

// ============================================
// Action Request Types
// ============================================

export interface ApproveRegistrationRequest {
  notes?: string;
}

export interface RejectRegistrationRequest {
  reason: string;
  notes?: string;
}

// ============================================
// API Response Types
// ============================================

export interface ApiError {
  type: string;
  title: string;
  status: number;
  detail: string;
}

export interface ActionResponse {
  message: string;
}
