import type { Metadata } from 'next';
import { QueryProvider } from '@/components/providers';
import { ConditionalHeader } from '@/components/layout/ConditionalHeader';
import '../styles/globals.css';

export const metadata: Metadata = {
  title: 'Kenya Airways Alumni Association',
  description: 'Connect, grow, and stay part of the Kenya Airways family',
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
