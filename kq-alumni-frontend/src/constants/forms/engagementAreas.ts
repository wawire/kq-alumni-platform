/**
 * Alumni engagement preference options
 * These represent the different ways alumni can engage with the platform
 */
export const ENGAGEMENT_AREAS = [
  { value: 'MENTORSHIP', label: 'Mentorship' },
  { value: 'NETWORKING', label: 'Networking Events' },
  { value: 'JOB_OPPORTUNITIES', label: 'Job Opportunities / Talent Pool' },
  { value: 'VOLUNTEERING', label: 'Volunteering' },
  { value: 'REUNIONS', label: 'Reunions / Social Events' },
  {
    value: 'THOUGHT_LEADERSHIP',
    label: 'Thought Leadership (e.g., speaking at forums)',
  },
] as const;

export type EngagementAreaOption = typeof ENGAGEMENT_AREAS[number];
export type EngagementAreaValue = EngagementAreaOption['value'];
