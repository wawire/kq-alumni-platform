/**
 * FormTextarea Component
 *
 * A complete form textarea field that combines Label, textarea, and error/description messages.
 * Integrates seamlessly with react-hook-form.
 *
 * @example
 * ```tsx
 * import { FormTextarea } from '@/components/forms';
 *
 * // Inside a form with FormProvider
 * <FormTextarea
 *   name="professionalCertifications"
 *   label="Professional Certifications"
 *   placeholder="e.g., PMP, CPA, ACCA"
 *   rows={4}
 *   maxLength={1000}
 * />
 * ```
 */

import { forwardRef } from 'react';
import { useFormContext, Controller } from 'react-hook-form';
import { Label } from '@/components/ui/label';
import { cn } from '@/lib/utils';

export interface FormTextareaProps
  extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
  /**
   * Field name (matches react-hook-form field name)
   */
  name: string;

  /**
   * Label text
   */
  label: string;

  /**
   * Optional description text shown below the textarea
   */
  description?: string;

  /**
   * Mark field as required (shows asterisk)
   */
  required?: boolean;

  /**
   * Additional container class name
   */
  containerClassName?: string;

  /**
   * Number of visible text rows
   */
  rows?: number;

  /**
   * Maximum character length
   */
  maxLength?: number;

  /**
   * Show character counter
   */
  showCounter?: boolean;
}

const Textarea = forwardRef<HTMLTextAreaElement, React.TextareaHTMLAttributes<HTMLTextAreaElement> & { error?: boolean }>(
  ({ className, error, ...props }, ref) => {
    return (
      <textarea
        className={cn(
          'w-full px-4 py-3 rounded-lg transition-all resize-none',
          'text-gray-900 placeholder-gray-400',
          'focus:outline-none focus:ring-2 focus:ring-offset-1',
          error
            ? 'border-2 border-red-300 focus:border-red-500 focus:ring-red-200'
            : 'border-2 border-gray-300 focus:border-kq-red focus:ring-kq-red/20',
          'disabled:bg-gray-100 disabled:text-gray-500 disabled:cursor-not-allowed',
          className
        )}
        ref={ref}
        {...props}
      />
    );
  }
);

Textarea.displayName = 'Textarea';

export const FormTextarea = ({
  name,
  label,
  description,
  required,
  containerClassName,
  rows = 4,
  maxLength,
  showCounter = true,
  className,
  disabled,
  ...textareaProps
}: FormTextareaProps) => {
  const {
    control,
    formState: { errors },
    watch,
  } = useFormContext();

  const error = errors[name];
  const errorMessage = error?.message as string | undefined;
  const currentValue = watch(name) || '';
  const charCount = currentValue.length;

  return (
    <Controller
      name={name}
      control={control}
      render={({ field }) => (
        <div className={cn('space-y-2', containerClassName)}>
          {/* Label */}
          <Label htmlFor={name} required={required} disabled={disabled}>
            {label}
          </Label>

          {/* Textarea */}
          <Textarea
            {...field}
            {...textareaProps}
            id={name}
            rows={rows}
            maxLength={maxLength}
            error={Boolean(error)}
            disabled={disabled}
            className={className}
            aria-invalid={Boolean(error)}
            aria-describedby={
              error
                ? `${name}-error`
                : description
                ? `${name}-description`
                : undefined
            }
          />

          {/* Character Counter */}
          {showCounter && maxLength && (
            <div className="flex justify-end">
              <p className={cn(
                'text-xs',
                charCount > maxLength * 0.9 ? 'text-orange-600' : 'text-gray-500',
                charCount >= maxLength ? 'text-red-600 font-semibold' : ''
              )}>
                {charCount} / {maxLength}
              </p>
            </div>
          )}

          {/* Description (shown when no error) */}
          {description && !error && (
            <p id={`${name}-description`} className="text-sm text-gray-500">
              {description}
            </p>
          )}

          {/* Error Message */}
          {error && (
            <p
              id={`${name}-error`}
              className="text-sm text-kq-red"
              role="alert"
            >
              {errorMessage}
            </p>
          )}
        </div>
      )}
    />
  );
};

FormTextarea.displayName = 'FormTextarea';
