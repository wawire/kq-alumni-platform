/**
 * Label Component
 *
 * An accessible label component for form inputs.
 * Automatically handles required field indicators.
 *
 * @example
 * ```tsx
 * import { Label } from '@/components/ui/label';
 *
 * // Basic label
 * <Label htmlFor="email">Email Address</Label>
 *
 * // With required indicator
 * <Label htmlFor="name" required>Full Name</Label>
 * ```
 */

import { forwardRef } from 'react';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '@/lib/utils';

const labelVariants = cva(
  'block font-medium text-gray-700 transition-colors',
  {
    variants: {
      size: {
        sm: 'text-xs mb-1',
        md: 'text-sm mb-3',
        lg: 'text-base mb-4',
      },

      disabled: {
        true: 'opacity-50 cursor-not-allowed',
        false: 'cursor-pointer',
      },
    },
    defaultVariants: {
      size: 'md',
      disabled: false,
    },
  }
);

export interface LabelProps
  extends React.LabelHTMLAttributes<HTMLLabelElement>,
    VariantProps<typeof labelVariants> {
  /**
   * Show required indicator (red asterisk)
   */
  required?: boolean;

  /**
   * Apply disabled styling
   */
  disabled?: boolean;
}

const Label = forwardRef<HTMLLabelElement, LabelProps>(
  (
    {
      className,
      size,
      required,
      disabled,
      children,
      ...props
    },
    ref
  ) => {
    return (
      <label
        className={cn(labelVariants({ size, disabled, className }))}
        ref={ref}
        {...props}
      >
        <span className="flex items-center gap-1">
          {children}
          {required && (
            <span className="text-kq-red" aria-label="required">
              *
            </span>
          )}
        </span>
      </label>
    );
  }
);

Label.displayName = 'Label';

export { Label, labelVariants };
