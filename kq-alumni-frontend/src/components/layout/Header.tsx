'use client';

import { useEffect, useRef, useState } from 'react';
import { Mail, Menu, X } from 'lucide-react';
import Image from 'next/image';
import Link from 'next/link';
import { usePathname } from 'next/navigation';

import { MAIN_NAV_LINKS, MOBILE_QUICK_LINKS, QUICK_LINKS } from '@/constants';

export default function Header() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const pathname = usePathname();
  const mobileMenuRef = useRef<HTMLDivElement>(null);

  // Close mobile menu when route changes
  useEffect(() => {
    setMobileMenuOpen(false);
  }, [pathname]);

  // Handle escape key press
  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && mobileMenuOpen) {
        setMobileMenuOpen(false);
      }
    };

    document.addEventListener('keydown', handleEscape);
    return () => document.removeEventListener('keydown', handleEscape);
  }, [mobileMenuOpen]);

  // Handle click outside
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (
        mobileMenuOpen &&
        mobileMenuRef.current &&
        !mobileMenuRef.current.contains(e.target as Node)
      ) {
        setMobileMenuOpen(false);
      }
    };

    if (mobileMenuOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }

    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [mobileMenuOpen]);

  /**
   * Determines if a link is active based on current pathname
   */
  const isActiveLink = (href: string): boolean => {
    if (href === '/') {
      return pathname === '/';
    }
    return pathname.startsWith(href);
  };

  /**
   * Returns CSS classes for navigation links with active state
   */
  const getNavLinkClasses = (href: string, baseClasses: string): string => {
    const activeClasses = isActiveLink(href) ? 'text-kq-red font-semibold' : '';
    return `${baseClasses} ${activeClasses}`.trim();
  };

  return (
    <header className="sticky top-0 z-50 bg-white shadow-md border-b border-gray-200">
      {/* ===========================
          Desktop / Tablet Header
      ============================ */}
      <div className="hidden md:flex items-center h-[72px] xl:h-[80px]">
        {/* Left: Logo — perfectly flush */}
        <div className="flex-shrink-0 h-full">
          <Link href="/" className="block h-full w-auto" aria-label="Kenya Airways Home">
            <Image
              src="/assets/logos/logo-kq.svg"
              alt="Kenya Airways"
              width={260}
              height={80}
              priority
              className="h-full w-auto object-contain"
            />
          </Link>
        </div>

        {/* Right: Navigation */}
        <div className="flex flex-col justify-center w-full px-8 h-full">
          {/* Top Row: Quick Links */}
          <div className="flex gap-8 text-xs md:text-sm font-semibold justify-end">
            {QUICK_LINKS.map((link) => {
              const isEmail = link.href.startsWith('mailto:');
              const LinkComponent = isEmail ? 'a' : Link;

              return (
                <LinkComponent
                  key={link.href}
                  href={link.href}
                  className={getNavLinkClasses(
                    link.href,
                    `text-gray-700 hover:text-kq-red uppercase tracking-wide transition-colors ${
                      isEmail ? 'flex items-center gap-2' : ''
                    }`
                  )}
                >
                  {isEmail && <Mail size={16} className="stroke-current" aria-hidden="true" />}
                  <span>{link.label}</span>
                </LinkComponent>
              );
            })}
          </div>

          {/* Bottom Row: Main Nav */}
          <nav
            className="flex gap-8 text-sm md:text-base font-medium justify-end items-center mt-6"
            aria-label="Main navigation"
          >
            {MAIN_NAV_LINKS.map((link) => (
              <Link
                key={link.href}
                href={link.href}
                className={getNavLinkClasses(
                  link.href,
                  'text-gray-800 hover:text-kq-red transition-colors'
                )}
              >
                {link.label}
              </Link>
            ))}

            {/* Search button temporarily hidden - functionality not yet implemented */}
            {/* <button
              className="text-gray-800 hover:text-kq-red transition-colors"
              aria-label="Search"
              onClick={() => {
                // TODO: Implement search functionality
                if (process.env.NODE_ENV === 'development') {
                  // eslint-disable-next-line no-console
                  console.log('Search clicked');
                }
              }}
            >
              <Search size={22} className="stroke-current" aria-hidden="true" />
            </button> */}
          </nav>
        </div>
      </div>

      {/* ===========================
          Mobile Header
      ============================ */}
      <div className="flex md:hidden items-center justify-between h-[64px] sm:h-[68px] m-0 p-0">
        {/* Logo flush to left */}
        <Link href="/" className="block h-full" aria-label="Kenya Airways Home">
          <Image
            src="/assets/logos/logo-kq.svg"
            alt="Kenya Airways"
            width={140}
            height={64}
            priority
            className="h-full w-auto object-contain"
          />
        </Link>

        {/* Menu Toggle */}
        <button
          onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
          className="text-gray-700 hover:text-kq-red transition-colors p-2"
          aria-label={mobileMenuOpen ? 'Close menu' : 'Open menu'}
          aria-expanded={mobileMenuOpen}
          aria-controls="mobile-menu"
        >
          {mobileMenuOpen ? (
            <X size={24} aria-hidden="true" />
          ) : (
            <Menu size={24} aria-hidden="true" />
          )}
        </button>
      </div>

      {/* ===========================
          Mobile Menu
      ============================ */}
      {mobileMenuOpen && (
        <div
          id="mobile-menu"
          ref={mobileMenuRef}
          className="md:hidden border-t border-gray-200 bg-white"
          role="dialog"
          aria-modal="true"
        >
          <nav className="flex flex-col px-4 py-4 space-y-3" aria-label="Mobile navigation">
            <div className="border-b border-gray-200 pb-3 mb-2">
              <p className="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-3">
                Quick Links
              </p>
              {MOBILE_QUICK_LINKS.map((link) => {
                const isEmail = link.href.startsWith('mailto:');
                const LinkComponent = isEmail ? 'a' : Link;

                return (
                  <LinkComponent
                    key={link.href}
                    href={link.href}
                    className={getNavLinkClasses(
                      link.href,
                      'block text-sm text-gray-700 hover:text-kq-red transition-colors py-2'
                    )}
                    onClick={() => !isEmail && setMobileMenuOpen(false)}
                  >
                    {link.label}
                  </LinkComponent>
                );
              })}
            </div>

            {/* Main Nav Links */}
            {MAIN_NAV_LINKS.map((link) => (
              <Link
                key={link.href}
                href={link.href}
                className={getNavLinkClasses(
                  link.href,
                  'text-base text-gray-800 hover:text-kq-red transition-colors py-2 font-medium'
                )}
                onClick={() => setMobileMenuOpen(false)}
              >
                {link.label}
              </Link>
            ))}
          </nav>
        </div>
      )}
    </header>
  );
}
