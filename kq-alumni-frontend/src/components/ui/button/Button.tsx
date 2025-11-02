/**
 * Button Component
 *
 * A flexible, accessible button component with multiple variants and sizes.
 * Built with class-variance-authority (CVA) for type-safe variant management.
 *
 * @example
 * ```tsx
 * import { Button } from '@/components/ui/button';
 *
 * // Primary button
 * <Button>Click me</Button>
 *
 * // Secondary with icon
 * <Button variant="secondary" leftIcon={<Icon />}>
 *   Save
 * </Button>
 *
 * // Loading state
 * <Button isLoading>Submitting...</Button>
 * ```
 */

import { forwardRef } from 'react';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '@/lib/utils';

const buttonVariants = cva(
  // Base styles - applied to all buttons
  'inline-flex items-center justify-center font-cabrito font-bold transition-all duration-300 focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:pointer-events-none disabled:opacity-50',
  {
    variants: {
      variant: {
        // KQ Red primary button
        primary:
          'bg-kq-red text-white hover:bg-kq-red-dark focus:ring-kq-red shadow-lg hover:shadow-xl transform hover:scale-[1.02]',

        // Secondary gray button
        secondary:
          'bg-gray-200 text-gray-700 hover:bg-gray-300 focus:ring-gray-400',

        // Outline button
        outline:
          'border-2 border-kq-red text-kq-red bg-transparent hover:bg-kq-red hover:text-white focus:ring-kq-red',

        // Ghost button (minimal)
        ghost:
          'bg-transparent text-kq-red hover:bg-red-50 focus:ring-kq-red',

        // Destructive/danger button
        destructive:
          'bg-red-600 text-white hover:bg-red-700 focus:ring-red-500 shadow-lg hover:shadow-xl',

        // Link style button
        link:
          'bg-transparent text-kq-red underline-offset-4 hover:underline focus:ring-0',
      },

      size: {
        sm: 'text-sm px-4 py-2 rounded-tl-sm rounded-tr-2xl rounded-br-sm rounded-bl-2xl',
        md: 'text-base px-6 py-3 rounded-tl-sm rounded-tr-2xl rounded-br-sm rounded-bl-2xl',
        lg: 'text-lg px-8 py-4 rounded-tl-sm rounded-tr-2xl rounded-br-sm rounded-bl-2xl',
        icon: 'p-2 rounded-tl-sm rounded-tr-2xl rounded-br-sm rounded-bl-2xl',
      },

      fullWidth: {
        true: 'w-full',
        false: '',
      },
    },
    defaultVariants: {
      variant: 'primary',
      size: 'md',
      fullWidth: false,
    },
  }
);

export interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  /**
   * Show loading spinner and disable button
   */
  isLoading?: boolean;

  /**
   * Icon to display on the left side of the button text
   */
  leftIcon?: React.ReactNode;

  /**
   * Icon to display on the right side of the button text
   */
  rightIcon?: React.ReactNode;

  /**
   * Custom loading text to show when isLoading is true
   */
  loadingText?: string;
}

/**
 * Loading Spinner Component
 */
const LoadingSpinner = ({ className }: { className?: string }) => (
  <svg
    className={cn('animate-spin', className)}
    xmlns="http://www.w3.org/2000/svg"
    fill="none"
    viewBox="0 0 24 24"
  >
    <circle
      className="opacity-25"
      cx="12"
      cy="12"
      r="10"
      stroke="currentColor"
      strokeWidth="4"
    />
    <path
      className="opacity-75"
      fill="currentColor"
      d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
    />
  </svg>
);

const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  (
    {
      className,
      variant,
      size,
      fullWidth,
      isLoading,
      leftIcon,
      rightIcon,
      loadingText,
      children,
      disabled,
      type = 'button',
      ...props
    },
    ref
  ) => {
    return (
      <button
        type={type}
        className={cn(buttonVariants({ variant, size, fullWidth, className }))}
        ref={ref}
        disabled={disabled || isLoading}
        aria-busy={isLoading}
        aria-disabled={disabled || isLoading}
        {...props}
      >
        {/* Loading state */}
        {isLoading && (
          <>
            <LoadingSpinner className="mr-2 h-4 w-4" />
            {loadingText && <span>{loadingText}</span>}
            {!loadingText && children}
          </>
        )}

        {/* Normal state */}
        {!isLoading && (
          <>
            {leftIcon && <span className="mr-2 flex-shrink-0">{leftIcon}</span>}
            {children}
            {rightIcon && <span className="ml-2 flex-shrink-0">{rightIcon}</span>}
          </>
        )}
      </button>
    );
  }
);

Button.displayName = 'Button';

export { Button, buttonVariants };
