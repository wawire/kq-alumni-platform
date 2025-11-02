/**
 * Utility for merging Tailwind CSS classes
 * Combines clsx for conditional classes and tailwind-merge for deduplication
 *
 * @example
 * ```tsx
 * cn('px-4 py-2', isActive && 'bg-red-500', className)
 * // Result: 'px-4 py-2 bg-red-500 custom-class'
 * ```
 */

import { type ClassValue, clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}
