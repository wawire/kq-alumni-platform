/** @type {import('next').NextConfig} */

const nextConfig = {
  // Output mode for standalone server (REQUIRED for dynamic routes like /verify/[token])
  output: 'standalone',

  // React settings
  reactStrictMode: true,

  // Transpile specific packages that have ES module issues
  transpilePackages: ['react-phone-input-2', 'country-state-city'],

  // Image optimization
  images: {
    remotePatterns: [
      {
        protocol: 'https',
        hostname: '**',
      },
    ],
    unoptimized: false,
    formats: ['image/avif', 'image/webp'],
  },

  // Compression
  compress: true,

  // TypeScript configuration
  typescript: {
    tsconfigPath: './tsconfig.json',
  },

  // ESLint
  eslint: {
    dirs: ['src'],
  },

  // Environment variables for IIS deployment
  env: {
    NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5295',
    NEXT_PUBLIC_API_TIMEOUT: process.env.NEXT_PUBLIC_API_TIMEOUT || '30000',
    NEXT_PUBLIC_ENV: process.env.NODE_ENV || 'development',
  },

  // Security and performance headers
  async headers() {
    return [
      {
        source: '/:path*',
        headers: [
          {
            key: 'X-Content-Type-Options',
            value: 'nosniff',
          },
          {
            key: 'X-Frame-Options',
            value: 'SAMEORIGIN',
          },
          {
            key: 'X-XSS-Protection',
            value: '1; mode=block',
          },
          {
            key: 'Referrer-Policy',
            value: 'strict-origin-when-cross-origin',
          },
          {
            key: 'Permissions-Policy',
            value: 'geolocation=(), microphone=(), camera=()',
          },
          {
            key: 'Strict-Transport-Security',
            value: 'max-age=31536000; includeSubDomains',
          },
        ],
      },
    ];
  },

  // Redirects (empty by default)
  async redirects() {
    return [];
  },

  // Rewrites for API proxying (if needed)
  async rewrites() {
    return {
      beforeFiles: [],
      afterFiles: [],
      fallback: [],
    };
  },

  // Webpack configuration with ES module fallbacks
  webpack: (config, { isServer }) => {
    // Handle ES modules and fallbacks for node-specific modules
    config.resolve.fallback = {
      ...config.resolve.fallback,
      fs: false,
      net: false,
      tls: false,
      crypto: false,
    };

    return config;
  },

  // Experimental features for performance
  experimental: {
    optimizePackageImports: ['lucide-react'],
  },

  // IIS Compatibility settings
  trailingSlash: false,
  poweredByHeader: false,
  productionBrowserSourceMaps: false,

  // Static generation timeout
  staticPageGenerationTimeout: 60,

  // Disable server components caching issues
  cacheMaxMemorySize: 52428800, // 50 MB
};

module.exports = nextConfig;
