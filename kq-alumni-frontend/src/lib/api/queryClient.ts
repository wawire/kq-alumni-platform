/**
 * React Query Configuration
 *
 * Configures QueryClient with default options for caching,
 * retries, and error handling.
 *
 * @example
 * ```tsx
 * import { QueryClientProvider } from '@tanstack/react-query';
 * import { queryClient } from '@/lib/api/queryClient';
 *
 * <QueryClientProvider client={queryClient}>
 *   <App />
 * </QueryClientProvider>
 * ```
 */

import { QueryClient } from '@tanstack/react-query';

/**
 * Default query options for all queries
 */
const defaultQueryOptions = {
  queries: {
    // Time before data is considered stale
    staleTime: 1000 * 60 * 5, // 5 minutes

    // Time before inactive queries are garbage collected
    gcTime: 1000 * 60 * 30, // 30 minutes (previously cacheTime)

    // Retry failed queries
    retry: 3,
    retryDelay: (attemptIndex: number) => Math.min(1000 * 2 ** attemptIndex, 30000),

    // Refetch options
    refetchOnWindowFocus: false,
    refetchOnMount: true,
    refetchOnReconnect: true,
  },
  mutations: {
    // Retry failed mutations
    retry: 1,
    retryDelay: 1000,
  },
};

/**
 * Global QueryClient instance
 */
export const queryClient = new QueryClient({
  defaultOptions: defaultQueryOptions,
});

/**
 * Query key factory for consistent key management
 */
export const queryKeys = {
  // Registration queries
  registration: {
    all: ['registration'] as const,
    detail: (id: string) => [...queryKeys.registration.all, 'detail', id] as const,
    status: (email: string) => [...queryKeys.registration.all, 'status', email] as const,
    duplicate: (field: string, value: string) =>
      [...queryKeys.registration.all, 'duplicate', field, value] as const,
  },

  // Verification queries
  verification: {
    all: ['verification'] as const,
    token: (token: string) => [...queryKeys.verification.all, 'token', token] as const,
  },

  // Add more query key groups as needed
  // user: { ... },
  // alumni: { ... },
};
