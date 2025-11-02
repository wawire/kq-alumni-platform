/**
 * FormSelect Component
 *
 * A complete form select field that combines Label, react-select, and error/description messages.
 * Integrates seamlessly with react-hook-form.
 *
 * @example
 * ```tsx
 * import { FormSelect } from '@/components/forms/FormSelect';
 * import { INDUSTRIES } from '@/constants/forms';
 *
 * // Inside a form with FormProvider
 * <FormSelect
 *   name="industry"
 *   label="Industry"
 *   options={INDUSTRIES}
 *   placeholder="Select your industry"
 *   required
 * />
 * ```
 */

import { useFormContext, Controller } from 'react-hook-form';
import Select, { Props as ReactSelectProps } from 'react-select';
import CreatableSelect from 'react-select/creatable';
import { Label } from '@/components/ui/label';
import { createSelectStyles } from '@/lib/forms';
import { cn } from '@/lib/utils';

interface SelectOption {
  value: string;
  label: string;
}

export interface FormSelectProps<OptionType extends SelectOption = SelectOption>
  extends Omit<ReactSelectProps<OptionType, false>, 'name'> {
  /**
   * Field name (matches react-hook-form field name)
   */
  name: string;

  /**
   * Label text
   */
  label: string;

  /**
   * Select options
   */
  options: readonly OptionType[];

  /**
   * Optional description text shown below the select
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
   * Enable creatable mode (allows creating new options)
   */
  isCreatable?: boolean;

  /**
   * Format the label for creating new options
   */
  formatCreateLabel?: (inputValue: string) => string;
}

export const FormSelect = <OptionType extends SelectOption = SelectOption>({
  name,
  label,
  options,
  description,
  required,
  containerClassName,
  placeholder = 'Select...',
  isDisabled,
  isClearable = false,
  isSearchable = true,
  isCreatable = false,
  formatCreateLabel,
  ...selectProps
}: FormSelectProps<OptionType>) => {
  const {
    control,
    formState: { errors },
  } = useFormContext();

  const error = errors[name];
  const errorMessage = error?.message as string | undefined;

  // Get select styles with error state
  const selectStyles = createSelectStyles<OptionType>(Boolean(error));

  // Choose the appropriate Select component
  const SelectComponent = isCreatable ? CreatableSelect : Select;

  return (
    <Controller
      name={name}
      control={control}
      render={({ field }) => (
        <div className={cn('space-y-2', containerClassName)}>
          {/* Label */}
          <Label htmlFor={name} required={required} disabled={isDisabled}>
            {label}
          </Label>

          {/* React Select / Creatable Select */}
          <SelectComponent<OptionType>
            {...field}
            {...selectProps}
            inputId={name}
            instanceId={name}
            options={options}
            placeholder={placeholder}
            isClearable={isClearable}
            isSearchable={isSearchable}
            isDisabled={isDisabled}
            styles={selectStyles}
            classNamePrefix="react-select"
            onChange={(option) => field.onChange(option?.value || '')}
            value={
              options.find((opt) => opt.value === field.value) ||
              (field.value ? { value: field.value, label: field.value } as OptionType : null)
            }
            formatCreateLabel={formatCreateLabel}
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

FormSelect.displayName = 'FormSelect';
