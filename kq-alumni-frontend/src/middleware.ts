/**
 * Next.js Middleware
 * Runs before every request to protected routes
 *
 * IMPORTANT: This middleware protects /admin routes by checking for authentication
 * before the page even renders, providing defense-in-depth security.
 */

import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

// ============================================
// Middleware Function
// ============================================

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  // ========================================
  // Admin Route Protection
  // ========================================

  if (pathname.startsWith('/admin')) {
    // Allow login page (public)
    if (pathname === '/admin/login') {
      // If already authenticated, redirect to dashboard
      const adminToken = request.cookies.get('admin_token')?.value;
      if (adminToken) {
        return NextResponse.redirect(new URL('/admin/dashboard', request.url));
      }
      return NextResponse.next();
    }

    // For all other /admin routes, check authentication
    const adminToken = request.cookies.get('admin_token')?.value;

    if (!adminToken) {
      // Not authenticated - redirect to login
      const loginUrl = new URL('/admin/login', request.url);
      // Preserve the intended destination for post-login redirect
      loginUrl.searchParams.set('redirect', pathname);
      return NextResponse.redirect(loginUrl);
    }
  }

  // Allow request to continue
  return NextResponse.next();
}

// ============================================
// Middleware Configuration
// ============================================

export const config = {
  matcher: [
    /*
     * Match all admin routes except static files and API routes
     * This includes:
     * - /admin/dashboard
     * - /admin/registrations
     * - /admin/settings
     * - /admin/profile
     * - etc.
     */
    '/admin/:path*',
  ],
};
