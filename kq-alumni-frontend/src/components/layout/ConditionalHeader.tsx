'use client';

/**
 * Conditional Header
 * Shows the main site header only on non-admin pages
 */

import { usePathname } from 'next/navigation';
import Header from './Header';

export function ConditionalHeader() {
  const pathname = usePathname();

  // Hide header on admin routes
  const isAdminRoute = pathname?.startsWith('/admin');

  if (isAdminRoute) {
    return null;
  }

  return <Header />;
}
