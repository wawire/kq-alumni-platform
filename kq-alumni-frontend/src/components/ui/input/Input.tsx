/**
 * Input Component
 *
 * A flexible text input component with validation states and icon support.
 * Matches the KQ Alumni Platform design with bottom border styling.
 *
 * @example
 * ```tsx
 * import { Input } from '@/components/ui/input';
 *
 * // Basic input
 * <Input placeholder="Enter your name" />
 *
 * // With error state
 * <Input error placeholder="Email" />
 *
 * // With icon
 * <Input leftIcon={<SearchIcon />} placeholder="Search..." />
 * ```
 */

import { forwardRef } from 'react';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '@/lib/utils';

const inputVariants = cva(
  // Base styles
  'w-full bg-transparent border-0 border-b-2 transition-all focus:outline-none disabled:cursor-not-allowed disabled:opacity-50',
  {
    variants: {
      variant: {
        // Bottom border style (KQ Alumni default)
        underline:
          'border-gray-300 focus:border-kq-red text-gray-900 placeholder-gray-400',

        // Outlined input
        outline:
          'border-2 border-gray-300 rounded-lg focus:border-kq-red text-gray-900 placeholder-gray-400',

        // Filled background
        filled:
          'bg-gray-100 border-gray-300 rounded-lg focus:border-kq-red focus:bg-white text-gray-900 placeholder-gray-400',
      },

      size: {
        sm: 'px-1 py-2 text-sm',
        md: 'px-1 py-3 text-base',
        lg: 'px-1 py-4 text-lg',
      },

      hasError: {
        true: 'border-kq-red focus:border-kq-red',
        false: '',
      },
    },
    defaultVariants: {
      variant: 'underline',
      size: 'md',
      hasError: false,
    },
  }
);

export interface InputProps
  extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'size'>,
    VariantProps<typeof inputVariants> {
  /**
   * Show error state styling
   */
  error?: boolean;

  /**
   * Icon to display on the left side
   */
  leftIcon?: React.ReactNode;

  /**
   * Icon to display on the right side
   */
  rightIcon?: React.ReactNode;

  /**
   * Additional wrapper class name
   */
  wrapperClassName?: string;
}

const Input = forwardRef<HTMLInputElement, InputProps>(
  (
    {
      className,
      variant,
      size,
      error,
      leftIcon,
      rightIcon,
      wrapperClassName,
      disabled,
      type = 'text',
      ...props
    },
    ref
  ) => {
    const hasError = error;

    // If there are icons, wrap in a container
    if (leftIcon || rightIcon) {
      return (
        <div className={cn('relative flex items-center', wrapperClassName)}>
          {leftIcon && (
            <div className="absolute left-3 flex items-center text-gray-500">
              {leftIcon}
            </div>
          )}

          <input
            type={type}
            className={cn(
              inputVariants({ variant, size, hasError, className }),
              leftIcon && 'pl-10',
              rightIcon && 'pr-10'
            )}
            ref={ref}
            disabled={disabled}
            aria-invalid={hasError}
            {...props}
          />

          {rightIcon && (
            <div className="absolute right-3 flex items-center text-gray-500">
              {rightIcon}
            </div>
          )}
        </div>
      );
    }

    // Simple input without icons
    return (
      <input
        type={type}
        className={cn(inputVariants({ variant, size, hasError, className }))}
        ref={ref}
        disabled={disabled}
        aria-invalid={hasError}
        {...props}
      />
    );
  }
);

Input.displayName = 'Input';

export { Input, inputVariants };
