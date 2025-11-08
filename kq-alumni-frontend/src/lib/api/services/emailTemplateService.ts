/**
 * Email Template API Service
 * React Query hooks for email template management (SuperAdmin only)
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import axios from 'axios';
import { env } from '@/lib/env';

// ============================================
// Types
// ============================================

export interface EmailTemplate {
  id: number;
  templateKey: string;
  name: string;
  description?: string;
  subject: string;
  htmlBody: string;
  availableVariables?: string;
  isActive: boolean;
  isSystemDefault: boolean;
  createdAt: string;
  updatedAt: string;
  createdBy?: string;
  updatedBy?: string;
}

export interface CreateTemplateRequest {
  templateKey: string;
  name: string;
  description?: string;
  subject: string;
  htmlBody: string;
  availableVariables?: string;
  isActive: boolean;
}

export interface UpdateTemplateRequest {
  name: string;
  description?: string;
  subject: string;
  htmlBody: string;
  availableVariables?: string;
  isActive: boolean;
  updatedBy?: string;
}

export interface PreviewRequest {
  subject: string;
  htmlBody: string;
  variables: Record<string, string>;
}

export interface PreviewResponse {
  subject: string;
  htmlBody: string;
}

export interface SendTestEmailRequest {
  templateId: number;
  toEmail: string;
  variables: Record<string, string>;
}

// ============================================
// API Configuration
// ============================================

const API_BASE_URL = env.apiUrl;

// Create axios instance with auth token interceptor
const templateApi = axios.create({
  baseURL: `${API_BASE_URL}/api/v1`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add JWT token to requests
templateApi.interceptors.request.use((config) => {
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

export const emailTemplateQueryKeys = {
  all: ['email-templates'] as const,
  list: (activeOnly?: boolean) =>
    [...emailTemplateQueryKeys.all, 'list', { activeOnly }] as const,
  detail: (id: number) =>
    [...emailTemplateQueryKeys.all, 'detail', id] as const,
  byKey: (key: string) =>
    [...emailTemplateQueryKeys.all, 'by-key', key] as const,
};

// ============================================
// Query Hooks
// ============================================

/**
 * Get all email templates
 */
export function useEmailTemplates(activeOnly = false) {
  return useQuery({
    queryKey: emailTemplateQueryKeys.list(activeOnly),
    queryFn: async () => {
      const response = await templateApi.get<EmailTemplate[]>('/emailtemplates', {
        params: { activeOnly },
      });
      return response.data;
    },
  });
}

/**
 * Get email template by ID
 */
export function useEmailTemplate(id: number) {
  return useQuery({
    queryKey: emailTemplateQueryKeys.detail(id),
    queryFn: async () => {
      const response = await templateApi.get<EmailTemplate>(`/emailtemplates/${id}`);
      return response.data;
    },
    enabled: !!id,
  });
}

/**
 * Get email template by key
 */
export function useEmailTemplateByKey(key: string) {
  return useQuery({
    queryKey: emailTemplateQueryKeys.byKey(key),
    queryFn: async () => {
      const response = await templateApi.get<EmailTemplate>(`/emailtemplates/by-key/${key}`);
      return response.data;
    },
    enabled: !!key,
  });
}

// ============================================
// Mutation Hooks
// ============================================

/**
 * Create new email template
 */
export function useCreateEmailTemplate() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateTemplateRequest) => {
      const response = await templateApi.post<EmailTemplate>('/emailtemplates', data);
      return response.data;
    },
    onSuccess: () => {
      // Invalidate all template lists
      queryClient.invalidateQueries({ queryKey: emailTemplateQueryKeys.all });
    },
  });
}

/**
 * Update email template
 */
export function useUpdateEmailTemplate() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, data }: { id: number; data: UpdateTemplateRequest }) => {
      const response = await templateApi.put<EmailTemplate>(`/emailtemplates/${id}`, data);
      return response.data;
    },
    onSuccess: (_, variables) => {
      // Invalidate all lists and the specific template
      queryClient.invalidateQueries({ queryKey: emailTemplateQueryKeys.all });
      queryClient.invalidateQueries({ queryKey: emailTemplateQueryKeys.detail(variables.id) });
    },
  });
}

/**
 * Delete email template (only non-system templates)
 */
export function useDeleteEmailTemplate() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: number) => {
      await templateApi.delete(`/emailtemplates/${id}`);
    },
    onSuccess: () => {
      // Invalidate all template lists
      queryClient.invalidateQueries({ queryKey: emailTemplateQueryKeys.all });
    },
  });
}

/**
 * Preview email template with variables
 */
export function usePreviewEmailTemplate() {
  return useMutation({
    mutationFn: async (data: PreviewRequest) => {
      const response = await templateApi.post<PreviewResponse>('/emailtemplates/preview', data);
      return response.data;
    },
  });
}

/**
 * Send test email
 * Note: This endpoint needs to be added to the backend
 */
export function useSendTestEmail() {
  return useMutation({
    mutationFn: async (data: SendTestEmailRequest) => {
      const response = await templateApi.post('/emailtemplates/test-email', data);
      return response.data;
    },
  });
}
