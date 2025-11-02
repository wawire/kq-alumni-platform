/**
 * FormField Component
 *
 * A complete form field that combines Label, Input, and error/description messages.
 * Integrates seamlessly with react-hook-form.
 *
 * @example
 * ```tsx
 * import { FormField } from '@/components/forms/FormField';
 * import { useFormContext } from 'react-hook-form';
 *
 * // Inside a form with FormProvider
 * <FormField
 *   name="email"
 *   label="Email Address"
 *   placeholder="you@example.com"
 *   required
 * />
 * ```
 */

import { useFormContext, Controller } from 'react-hook-form';
import { Input, type InputProps } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { cn } from '@/lib/utils';

export interface FormFieldProps extends Omit<InputProps, 'name'> {
  /**
   * Field name (matches react-hook-form field name)
   */
  name: string;

  /**
   * Label text
   */
  label: string;

  /**
   * Optional description text shown below the input
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
   * Icon to display on the left side of input
   */
  leftIcon?: React.ReactNode;

  /**
   * Icon to display on the right side of input
   */
  rightIcon?: React.ReactNode;
}

export const FormField = ({
  name,
  label,
  description,
  required,
  containerClassName,
  className,
  leftIcon,
  rightIcon,
  disabled,
  ...inputProps
}: FormFieldProps) => {
  const {
    control,
    formState: { errors },
  } = useFormContext();

  const error = errors[name];
  const errorMessage = error?.message as string | undefined;

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

          {/* Input */}
          <Input
            {...field}
            {...inputProps}
            id={name}
            error={Boolean(error)}
            disabled={disabled}
            leftIcon={leftIcon}
            rightIcon={rightIcon}
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

          {/* Description (shown when no error) */}
          {description && !error && (
            <p
              id={`${name}-description`}
              className="text-sm text-gray-500"
            >
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

FormField.displayName = 'FormField';
