/**
 * Navigation constants for the header component
 * Centralizes all navigation links for easy maintenance
 */

export interface NavLink {
  href: string;
  label: string;
  external?: boolean;
}

export interface QuickLink extends NavLink {
  icon?: string;
}

/**
 * Main navigation links displayed in the header
 * NOTE: Pages commented out are not yet implemented
 */
export const MAIN_NAV_LINKS: NavLink[] = [
  // Temporarily hidden - pages not yet implemented
  // {
  //   href: '/about',
  //   label: 'About the Alumni Network',
  // },
  // {
  //   href: '/benefits',
  //   label: 'Member Benefits',
  // },
  // {
  //   href: '/events',
  //   label: 'Events & Reunions',
  // },
  // {
  //   href: '/news',
  //   label: 'News & Updates',
  // },
];

/**
 * Quick access links displayed in the header top bar
 */
export const QUICK_LINKS: QuickLink[] = [
  // Temporarily hidden - page not yet implemented
  // {
  //   href: '/login',
  //   label: 'Member Portal',
  // },
  {
    href: 'mailto:KQ.Alumni@kenya-airways.com',
    label: 'KQ.Alumni@kenya-airways.com',
    external: true,
  },
];

/**
 * Mobile-specific quick links (simplified for mobile menu)
 */
export const MOBILE_QUICK_LINKS: NavLink[] = [
  // Temporarily hidden - page not yet implemented
  // {
  //   href: '/login',
  //   label: 'Member Portal',
  // },
  // Temporarily hidden - page not yet implemented
  // {
  //   href: '/events',
  //   label: 'Events & Reunions',
  // },
  {
    href: 'mailto:KQ.Alumni@kenya-airways.com',
    label: 'Contact Us',
    external: true,
  },
];
