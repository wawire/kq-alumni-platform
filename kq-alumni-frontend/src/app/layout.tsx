import type { Metadata } from 'next';

import { ConditionalHeader } from '@/components/layout/ConditionalHeader';
import { QueryProvider } from '@/components/providers';
import '../styles/globals.css';

export const metadata: Metadata = {
  title: 'Kenya Airways Alumni Association',
  description: 'Connect, grow, and stay part of the Kenya Airways family',

  // Favicons and Icons
  icons: {
    icon: [
      { url: '/favicon.ico', sizes: 'any' },
      { url: '/favicon-16x16.png', sizes: '16x16', type: 'image/png' },
      { url: '/favicon-32x32.png', sizes: '32x32', type: 'image/png' },
    ],
    apple: [
      { url: '/apple-touch-icon.png', sizes: '180x180', type: 'image/png' },
    ],
    other: [
      { url: '/android-chrome-192x192.png', sizes: '192x192', type: 'image/png' },
      { url: '/android-chrome-512x512.png', sizes: '512x512', type: 'image/png' },
    ],
  },

  // Web App Manifest
  manifest: '/site.webmanifest',

  // Theme Colors
  themeColor: '#DC143C',

  // Open Graph
  openGraph: {
    type: 'website',
    locale: 'en_US',
    url: 'https://kqalumni.kenya-airways.com',
    siteName: 'Kenya Airways Alumni Association',
    title: 'Kenya Airways Alumni Association',
    description: 'Connect, grow, and stay part of the Kenya Airways family',
    images: [
      {
        url: '/assets/logos/logo-kq.svg',
        width: 1200,
        height: 630,
        alt: 'Kenya Airways Alumni Association',
      },
    ],
  },

  // Twitter Card
  twitter: {
    card: 'summary_large_image',
    title: 'Kenya Airways Alumni Association',
    description: 'Connect, grow, and stay part of the Kenya Airways family',
    images: ['/assets/logos/logo-kq.svg'],
  },

  // Additional Meta Tags
  other: {
    'msapplication-TileColor': '#DC143C',
    'msapplication-config': '/browserconfig.xml',
  },
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body>
        <QueryProvider>
          <ConditionalHeader />
          {children}
        </QueryProvider>
      </body>
    </html>
  );
}
