/**
 * Admin API Service
 * React Query hooks for admin dashboard operations
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import axios from 'axios';
import { env } from '@/lib/env';
import type {
  AdminLoginRequest,
  AdminLoginResponse,
  AdminRegistration,
  PaginatedRegistrations,
  RegistrationFilters,
  DashboardStats,
  AuditLog,
  ApproveRegistrationRequest,
  RejectRegistrationRequest,
  ActionResponse,
  CreateAdminUserRequest,
} from '@/types/admin';

// ============================================
// API Configuration
// ============================================

const API_BASE_URL = env.apiUrl;

// Create axios instance with auth token interceptor
const adminApi = axios.create({
  baseURL: `${API_BASE_URL}/api/v1`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add JWT token to requests
adminApi.interceptors.request.use((config) => {
  if (typeof window !== 'undefined') {
    const token = localStorage.getItem('admin_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
  }
  return config;
});

// ============================================
// Query Keys Factory
// ============================================

export const adminQueryKeys = {
  auth: {
    all: ['admin', 'auth'] as const,
    currentUser: () => [...adminQueryKeys.auth.all, 'current'] as const,
  },
  registrations: {
    all: ['admin', 'registrations'] as const,
    list: (filters?: RegistrationFilters) =>
      [...adminQueryKeys.registrations.all, 'list', filters] as const,
    detail: (id: string) =>
      [...adminQueryKeys.registrations.all, 'detail', id] as const,
    requireingReview: () =>
      [...adminQueryKeys.registrations.all, 'requiring-review'] as const,
    auditLogs: (id: string) =>
      [...adminQueryKeys.registrations.all, 'audit-logs', id] as const,
  },
  dashboard: {
    all: ['admin', 'dashboard'] as const,
    stats: () => [...adminQueryKeys.dashboard.all, 'stats'] as const,
  },
};

// ============================================
// Authentication Hooks
// ============================================

/**
 * Admin login mutation
 */
export function useAdminLogin() {
  return useMutation({
    mutationFn: async (credentials: AdminLoginRequest) => {
      const response = await adminApi.post<AdminLoginResponse>(
        '/admin/login',
        credentials
      );
      return response.data;
    },
    onSuccess: (data) => {
      // Store token in localStorage and cookie (for middleware)
      if (typeof window !== 'undefined') {
        localStorage.setItem('admin_token', data.token);
        localStorage.setItem('admin_user', JSON.stringify(data));

        // Set cookie for middleware (expires in 8 hours, same as token)
        const expiresDate = new Date(data.expiresAt);
        document.cookie = `admin_token=${data.token}; path=/; expires=${expiresDate.toUTCString()}; SameSite=Lax; Secure`;
      }
    },
  });
}

/**
 * Get current admin user
 */
export function useCurrentAdminUser() {
  return useQuery({
    queryKey: adminQueryKeys.auth.currentUser(),
    queryFn: async () => {
      const response = await adminApi.get<AdminLoginResponse>('/admin/me');
      return response.data;
    },
    staleTime: 5 * 60 * 1000, // 5 minutes
    retry: false,
  });
}

/**
 * Admin logout
 */
export function useAdminLogout() {
  const queryClient = useQueryClient();

  return () => {
    // Clear token and user data
    if (typeof window !== 'undefined') {
      localStorage.removeItem('admin_token');
      localStorage.removeItem('admin_user');
      // Also clear cookie used by middleware
      document.cookie = 'admin_token=; path=/; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
    }

    // Clear all queries
    queryClient.clear();

    // Redirect to login
    window.location.href = '/admin/login';
  };
}

/**
 * Create new admin user (SuperAdmin only)
 */
export function useCreateAdminUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateAdminUserRequest) => {
      const response = await adminApi.post<ActionResponse>(
        '/admin/users',
        data
      );
      return response.data;
    },
    onSuccess: () => {
      // Invalidate admin users list (if we implement it)
      queryClient.invalidateQueries({ queryKey: adminQueryKeys.auth.all });
    },
  });
}

// ============================================
// Registration Management Hooks
// ============================================

/**
 * Get paginated registrations list with filters
 */
export function useAdminRegistrations(filters?: RegistrationFilters) {
  return useQuery({
    queryKey: adminQueryKeys.registrations.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams();
      if (filters?.status) {
        params.append('status', filters.status);
      }
      if (filters?.requiresManualReview !== undefined) {
        params.append(
          'requiresManualReview',
          filters.requiresManualReview.toString()
        );
      }
      if (filters?.searchQuery) {
        params.append('searchQuery', filters.searchQuery);
      }
      if (filters?.dateFrom) {
        params.append('dateFrom', filters.dateFrom);
      }
      if (filters?.dateTo) {
        params.append('dateTo', filters.dateTo);
      }
      if (filters?.emailVerified !== undefined) {
        params.append('emailVerified', filters.emailVerified.toString());
      }
      if (filters?.sortBy) {
        params.append('sortBy', filters.sortBy);
      }
      if (filters?.sortOrder) {
        params.append('sortOrder', filters.sortOrder);
      }
      if (filters?.pageNumber) {
        params.append('pageNumber', filters.pageNumber.toString());
      }
      if (filters?.pageSize) {
        params.append('pageSize', filters.pageSize.toString());
      }

      const response = await adminApi.get<PaginatedRegistrations>(
        `/admin/registrations?${params.toString()}`
      );
      return response.data;
    },
    staleTime: 30 * 1000, // 30 seconds
  });
}

/**
 * Get registrations requiring manual review
 */
export function useRequiringReviewRegistrations() {
  return useQuery({
    queryKey: adminQueryKeys.registrations.requireingReview(),
    queryFn: async () => {
      const response = await adminApi.get<{ data: AdminRegistration[] }>(
        '/admin/registrations/requiring-review'
      );
      return response.data.data;
    },
    staleTime: 30 * 1000, // 30 seconds
  });
}

/**
 * Get registration detail by ID
 */
export function useRegistrationDetail(id: string) {
  return useQuery({
    queryKey: adminQueryKeys.registrations.detail(id),
    queryFn: async () => {
      const response = await adminApi.get<AdminRegistration>(
        `/admin/registrations/${id}`
      );
      return response.data;
    },
    enabled: Boolean(id),
    staleTime: 60 * 1000, // 1 minute
  });
}

/**
 * Approve registration mutation
 */
export function useApproveRegistration() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      id,
      data,
    }: {
      id: string;
      data: ApproveRegistrationRequest;
    }) => {
      const response = await adminApi.post<ActionResponse>(
        `/admin/registrations/${id}/approve`,
        data
      );
      return response.data;
    },
    onSuccess: (_, variables) => {
      // Invalidate queries to refresh data
      queryClient.invalidateQueries({
        queryKey: adminQueryKeys.registrations.all,
      });
      queryClient.invalidateQueries({
        queryKey: adminQueryKeys.dashboard.stats(),
      });
      queryClient.invalidateQueries({
        queryKey: adminQueryKeys.registrations.detail(variables.id),
      });
    },
  });
}

/**
 * Reject registration mutation
 */
export function useRejectRegistration() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      id,
      data,
    }: {
      id: string;
      data: RejectRegistrationRequest;
    }) => {
      const response = await adminApi.post<ActionResponse>(
        `/admin/registrations/${id}/reject`,
        data
      );
      return response.data;
    },
    onSuccess: (_, variables) => {
      // Invalidate queries to refresh data
      queryClient.invalidateQueries({
        queryKey: adminQueryKeys.registrations.all,
      });
      queryClient.invalidateQueries({
        queryKey: adminQueryKeys.dashboard.stats(),
      });
      queryClient.invalidateQueries({
        queryKey: adminQueryKeys.registrations.detail(variables.id),
      });
    },
  });
}

/**
 * Get audit logs for a registration
 */
export function useRegistrationAuditLogs(id: string) {
  return useQuery({
    queryKey: adminQueryKeys.registrations.auditLogs(id),
    queryFn: async () => {
      const response = await adminApi.get<{ data: AuditLog[] }>(
        `/admin/registrations/${id}/audit-logs`
      );
      return response.data.data;
    },
    enabled: Boolean(id),
    staleTime: 60 * 1000, // 1 minute
  });
}

// ============================================
// Dashboard Hooks
// ============================================

/**
 * Get dashboard statistics
 */
export function useDashboardStats() {
  return useQuery({
    queryKey: adminQueryKeys.dashboard.stats(),
    queryFn: async () => {
      const response = await adminApi.get<DashboardStats>(
        '/admin/dashboard/stats'
      );
      return response.data;
    },
    staleTime: 30 * 1000, // 30 seconds
    refetchInterval: 60 * 1000, // Auto-refetch every minute
  });
}

// ============================================
// Helper Functions
// ============================================

/**
 * Check if user is authenticated
 */
export function isAdminAuthenticated(): boolean {
  if (typeof window === 'undefined') {
    return false;
  }
  const token = localStorage.getItem('admin_token');
  return Boolean(token);
}

/**
 * Get stored admin user data
 */
export function getStoredAdminUser(): AdminLoginResponse | null {
  if (typeof window === 'undefined') {
    return null;
  }
  const userStr = localStorage.getItem('admin_user');
  if (!userStr) {
    return null;
  }
  try {
    return JSON.parse(userStr);
  } catch {
    return null;
  }
}
