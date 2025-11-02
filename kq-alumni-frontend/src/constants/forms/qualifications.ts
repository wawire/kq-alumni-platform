/**
 * Educational qualification options for the KQ Alumni Platform
 * These values map to the backend QualificationLevel enum
 */
export const QUALIFICATIONS = [
  { value: 'PHD', label: 'Doctorate (PhD)' },
  { value: 'MASTERS', label: "Master's Degree" },
  { value: 'BACHELORS', label: "Bachelor's Degree" },
  { value: 'HND', label: 'Higher National Diploma (HND)' },
  { value: 'DIPLOMA', label: 'Diploma' },
  { value: 'CERTIFICATE', label: 'Certificate' },
  { value: 'ADVANCED_CERT', label: 'Advanced Certificate' },
  { value: 'PROFESSIONAL', label: 'Professional Course' },
] as const;

export type QualificationOption = typeof QUALIFICATIONS[number];
export type QualificationValue = QualificationOption['value'];
