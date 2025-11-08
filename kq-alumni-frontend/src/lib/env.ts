/**
 * Environment Variable Loader
 * Safely reads and validates required environment variables at runtime.
 */

const getEnv = (key: string, required = false, fallback?: string): string => {
  const value = process.env[key];

  if (!value && required) {
    const message = `Missing required environment variable: ${key}`;

    // Server-only: stop the build/startup if critical env var is missing
    if (typeof window === 'undefined') {
      throw new Error(`[ERROR] ${message}`);
    }

    // Client: log warning but continue
    console.warn(`[WARNING] ${message}`);
    return fallback ?? '';
  }

  return value ?? fallback ?? '';
};

/**
 * Centralized, type-safe environment definition
 */
export const env = {
  // API Configuration
  apiUrl: getEnv('NEXT_PUBLIC_API_URL', true, 'http://localhost:5000'),
  apiTimeout: parseInt(getEnv('NEXT_PUBLIC_API_TIMEOUT', false, '30000')),

  // Environment Metadata
  environment: getEnv('NEXT_PUBLIC_ENV', false, 'development'),

  // Application Info
  appName: getEnv('NEXT_PUBLIC_APP_NAME', false, 'KQ Alumni Association'),
  appVersion: getEnv('NEXT_PUBLIC_APP_VERSION', false, '1.0.0'),

  // Feature Flags
  enableAnalytics: getEnv('NEXT_PUBLIC_ENABLE_ANALYTICS', false, 'false') === 'true',
  enableDebugMode: getEnv('NEXT_PUBLIC_ENABLE_DEBUG_MODE', false, 'true') === 'true',

  // Contact Info
  supportEmail: getEnv('NEXT_PUBLIC_SUPPORT_EMAIL', false, 'KQ.Alumni@kenya-airways.com'),

  // Logging Level
  logLevel: getEnv('NEXT_PUBLIC_LOG_LEVEL', false, 'debug'),
} as const;

export type Environment = typeof env;

// Log summary on server only (one-time)
if (typeof window === 'undefined') {
  /* eslint-disable no-console */
  console.log('[ENV] Environment initialized:');
  console.log(`  API URL: ${env.apiUrl}`);
  console.log(`  Timeout: ${env.apiTimeout}ms`);
  console.log(`  Environment: ${env.environment}`);
  /* eslint-enable no-console */
}
