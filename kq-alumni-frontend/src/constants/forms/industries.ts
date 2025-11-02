/**
 * Industry options for the KQ Alumni Platform
 * Used in employment information section
 */

export interface IndustryOption {
  value: string;
  label: string;
}

export const INDUSTRIES: IndustryOption[] = [
  { value: 'Aviation', label: 'Aviation' },
  { value: 'Aerospace', label: 'Aerospace' },
  { value: 'Technology', label: 'Technology' },
  { value: 'Information Technology', label: 'Information Technology' },
  { value: 'Finance', label: 'Finance & Banking' },
  { value: 'Healthcare', label: 'Healthcare & Medical' },
  { value: 'Education', label: 'Education & Training' },
  { value: 'Manufacturing', label: 'Manufacturing' },
  { value: 'Retail', label: 'Retail & E-commerce' },
  { value: 'Hospitality', label: 'Hospitality & Tourism' },
  { value: 'Real Estate', label: 'Real Estate & Construction' },
  { value: 'Media', label: 'Media & Entertainment' },
  { value: 'Telecommunications', label: 'Telecommunications' },
  { value: 'Energy', label: 'Energy & Utilities' },
  { value: 'Agriculture', label: 'Agriculture & Food' },
  { value: 'Consulting', label: 'Consulting & Professional Services' },
  { value: 'Legal', label: 'Legal Services' },
  { value: 'Marketing', label: 'Marketing & Advertising' },
  { value: 'Logistics', label: 'Logistics & Supply Chain' },
  { value: 'Non-Profit', label: 'Non-Profit & NGO' },
  { value: 'Government', label: 'Government & Public Sector' },
  { value: 'Insurance', label: 'Insurance' },
  { value: 'Automotive', label: 'Automotive' },
  { value: 'Other', label: 'Other' },
];

export type IndustryValue = IndustryOption['value'];
