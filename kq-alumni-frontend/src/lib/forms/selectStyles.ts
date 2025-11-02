/**
 * Shared react-select styles for the KQ Alumni Platform
 * Provides consistent styling across all select components
 *
 * Brand colors:
 * - KQ Red: #DC143C
 * - KQ Red Dark: #B01030
 */

import { StylesConfig } from 'react-select';

/**
 * Base select styles with KQ Alumni Platform branding
 * Can be used with any react-select component
 *
 * @template OptionType - The type of option object (must have value and label)
 * @param hasError - Whether the select has a validation error
 * @returns StylesConfig object for react-select
 *
 * @example
 * ```tsx
 * import Select from 'react-select';
 * import { createSelectStyles } from '@/lib/forms/selectStyles';
 *
 * const options = [{ value: '1', label: 'Option 1' }];
 * const styles = createSelectStyles<typeof options[number]>();
 *
 * <Select options={options} styles={styles} />
 * ```
 */
export const createSelectStyles = <OptionType,>(
  hasError: boolean = false
): StylesConfig<OptionType, false> => ({
  control: (base, state) => ({
    ...base,
    border: 0,
    borderBottom: hasError
      ? '2px solid #DC143C'
      : '2px solid #d1d5db',
    borderRadius: 0,
    boxShadow: 'none',
    padding: '8px 0',
    backgroundColor: 'transparent',
    minHeight: '48px',
    '&:hover': {
      borderBottom: '2px solid #DC143C',
    },
    ...(state.isFocused && {
      borderBottom: hasError ? '2px solid #DC143C' : '2px solid #DC143C',
    }),
  }),

  valueContainer: (base) => ({
    ...base,
    padding: '2px 4px',
  }),

  input: (base) => ({
    ...base,
    margin: 0,
    padding: 0,
    color: '#111827',
  }),

  placeholder: (base) => ({
    ...base,
    color: '#9ca3af',
  }),

  singleValue: (base) => ({
    ...base,
    color: '#111827',
  }),

  menu: (base) => ({
    ...base,
    borderRadius: '8px',
    boxShadow:
      '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
    zIndex: 9999,
    marginTop: '4px',
    backgroundColor: 'white',
  }),

  menuList: (base) => ({
    ...base,
    padding: '4px',
  }),

  option: (base, state) => ({
    ...base,
    backgroundColor: state.isSelected
      ? '#FECACA' // Light red when selected
      : state.isFocused
      ? '#FEE2E2' // Lighter red on hover
      : 'white',
    color: '#111827',
    cursor: 'pointer',
    borderRadius: '4px',
    padding: '10px 12px',
    '&:active': {
      backgroundColor: '#FECACA',
    },
  }),

  indicatorSeparator: () => ({
    display: 'none',
  }),

  dropdownIndicator: (base, state) => ({
    ...base,
    color: '#6b7280',
    transition: 'all 0.2s',
    '&:hover': {
      color: '#DC143C',
    },
    ...(state.isFocused && {
      color: '#DC143C',
    }),
  }),

  clearIndicator: (base) => ({
    ...base,
    color: '#6b7280',
    cursor: 'pointer',
    '&:hover': {
      color: '#DC143C',
    },
  }),

  multiValue: (base) => ({
    ...base,
    backgroundColor: '#FEE2E2',
    borderRadius: '4px',
  }),

  multiValueLabel: (base) => ({
    ...base,
    color: '#991B1B',
  }),

  multiValueRemove: (base) => ({
    ...base,
    color: '#991B1B',
    cursor: 'pointer',
    '&:hover': {
      backgroundColor: '#DC143C',
      color: 'white',
    },
  }),

  noOptionsMessage: (base) => ({
    ...base,
    color: '#6b7280',
    padding: '12px',
  }),

  loadingMessage: (base) => ({
    ...base,
    color: '#6b7280',
    padding: '12px',
  }),
});

/**
 * Multi-select styles variant
 * Optimized for components that allow multiple selections
 */
export const createMultiSelectStyles = <OptionType,>(
  hasError: boolean = false
): StylesConfig<OptionType, true> => {
  const baseStyles = createSelectStyles<OptionType>(hasError);

  return {
    ...baseStyles,
    control: (base, state) => ({
      ...base,
      border: 0,
      borderBottom: hasError ? '2px solid #DC143C' : '2px solid #d1d5db',
      borderRadius: 0,
      boxShadow: 'none',
      padding: '8px 0',
      backgroundColor: 'transparent',
      minHeight: '48px',
      '&:hover': {
        borderBottom: '2px solid #DC143C',
      },
      ...(state.isFocused && {
        borderBottom: hasError ? '2px solid #DC143C' : '2px solid #DC143C',
      }),
    }),
  } as StylesConfig<OptionType, true>;
};

/**
 * Creatable select styles variant
 * For react-select/creatable components
 */
export const createCreatableSelectStyles = <OptionType,>(
  hasError: boolean = false
): StylesConfig<OptionType, false> => createSelectStyles<OptionType>(hasError);
