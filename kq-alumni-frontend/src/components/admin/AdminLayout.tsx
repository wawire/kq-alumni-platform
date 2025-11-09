'use client';

/**
 * Admin Layout
 * Protected layout wrapper for admin dashboard pages
 * Handles authentication check and navigation
 */

import { useEffect, useState } from 'react';
import {
  AlertCircle,
  ArrowRight,
  Bell,
  ChevronDown,
  Clock,
  FileText,
  LayoutDashboard,
  LogOut,
  Mail,
  Menu,
  Settings,
  User as UserIcon,
  Users,
  X,
} from 'lucide-react';
import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';

import { Button } from '@/components/ui/button/Button';
import {
  getStoredAdminUser,
  isAdminAuthenticated,
  useAdminLogout,
  useDashboardStats,
} from '@/lib/api/services/adminService';
import { useAdminAuthActions, useAdminUser } from '@/store/adminStore';

// ============================================
// Types
// ============================================

interface AdminLayoutProps {
  children: React.ReactNode;
}

interface NavItem {
  name: string;
  href: string;
  icon: React.ComponentType<{ className?: string }>;
}

// ============================================
// Navigation Items
// ============================================

const navigation: NavItem[] = [
  { name: 'Dashboard', href: '/admin/dashboard', icon: LayoutDashboard },
  { name: 'Registrations', href: '/admin/registrations', icon: Users },
  { name: 'Requiring Review', href: '/admin/registrations/review', icon: FileText },
  { name: 'Email Templates', href: '/admin/email-templates', icon: Mail },
];

// ============================================
// Component
// ============================================

export function AdminLayout({ children }: AdminLayoutProps) {
  const router = useRouter();
  const pathname = usePathname();
  const user = useAdminUser();
  const { setUser, setToken, setAuthenticated } = useAdminAuthActions();
  const logout = useAdminLogout();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false);
  const [isNotificationOpen, setIsNotificationOpen] = useState(false);
  const [isCheckingAuth, setIsCheckingAuth] = useState(true);
  const { data: stats } = useDashboardStats();

  // Check authentication on mount
  useEffect(() => {
    const authenticated = isAdminAuthenticated();
    const storedUser = getStoredAdminUser();

    if (!authenticated || !storedUser) {
      // Redirect to login if not authenticated
      router.push('/admin/login');
      return;
    }

    if (!user) {
      // Restore user from localStorage
      setUser({
        userId: storedUser.userId,
        username: storedUser.username,
        email: storedUser.email,
        fullName: storedUser.fullName,
        role: storedUser.role,
      });
      setToken(storedUser.token);
      setAuthenticated(true);
    }

    // Mark auth check as complete
    setIsCheckingAuth(false);
  }, [user, setUser, setToken, setAuthenticated, router]);

  // Show loading state while checking authentication
  if (isCheckingAuth) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-kq-red mb-4"></div>
          <p className="text-gray-600 font-roboto">Loading...</p>
        </div>
      </div>
    );
  }

  // Don't render layout if not authenticated
  if (!user) {
    return null;
  }

  const handleLogout = () => {
    logout();
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Mobile Menu Backdrop */}
      {isMobileMenuOpen && (
        <div
          className="fixed inset-0 z-40 bg-black bg-opacity-50 lg:hidden"
          onClick={() => setIsMobileMenuOpen(false)}
        />
      )}

      {/* Sidebar */}
      <aside
        className={`
          fixed top-0 left-0 z-50 h-full w-64
          bg-navy-900 text-white
          transform transition-transform duration-300 ease-in-out
          lg:translate-x-0
          ${isMobileMenuOpen ? 'translate-x-0' : '-translate-x-full'}
        `}
      >
        {/* Logo */}
        <div className="flex items-center justify-between h-20 px-6 border-b border-navy-800">
          <div className="flex items-center gap-3">
            {/* <Image
              src="/assets/logos/logo-kq.svg"
              alt="Kenya Airways"
              width={48}
              height={48}
              className="w-12 h-12"
            /> */}
            <div>
              <span className="font-cabrito font-bold text-lg block text-white">Admin Portal</span>
              <span className="text-xs text-gray-400">Alumni Management</span>
            </div>
          </div>
          <button
            onClick={() => setIsMobileMenuOpen(false)}
            className="lg:hidden text-gray-400 hover:text-white"
          >
            <X className="w-6 h-6" />
          </button>
        </div>

        {/* Navigation */}
        <nav className="mt-6 px-3">
          {navigation.map((item) => {
            const Icon = item.icon;
            // Fix: Use exact match to prevent "/admin/registrations" staying active on "/admin/registrations/review"
            const isActive = pathname === item.href;

            return (
              <Link
                key={item.name}
                href={item.href}
                onClick={() => setIsMobileMenuOpen(false)}
                className={`
                  flex items-center gap-3 px-3 py-3 mb-1
                  rounded-tl-sm rounded-tr-lg rounded-br-sm rounded-bl-lg
                  transition-colors
                  ${
                    isActive
                      ? 'bg-kq-red text-white hover:text-white hover:bg-kq-red'
                      : 'text-gray-300 hover:bg-navy-800 hover:text-white'
                  }
                `}
              >
                <Icon className="w-5 h-5" />
                <span className="font-medium">{item.name}</span>
              </Link>
            );
          })}
        </nav>

        {/* User Info */}
        <div className="absolute bottom-0 left-0 right-0 p-4 border-t border-navy-800">
          {/* User Profile */}
          <div className="flex items-center gap-3 mb-3">
            <div className="w-10 h-10 rounded-full bg-kq-red flex items-center justify-center">
              <span className="text-sm font-bold text-white">
                {user.fullName
                  .split(' ')
                  .map((n) => n[0])
                  .join('')
                  .substring(0, 2)}
              </span>
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-white truncate">{user.fullName}</p>
              <p className="text-xs text-gray-400 truncate">{user.role}</p>
            </div>
          </div>
          <Button
            variant="outline"
            size="sm"
            onClick={handleLogout}
            leftIcon={<LogOut className="w-4 h-4" />}
            className="w-full border-navy-800 text-gray-300 hover:bg-navy-800 hover:text-white"
          >
            Logout
          </Button>
        </div>
      </aside>

      {/* Main Content */}
      <div className="lg:pl-64">
        {/* Top Bar */}
        <header className="sticky top-0 z-30 h-16 bg-white border-b border-gray-200 shadow-sm">
          <div className="h-full px-4 lg:px-6 flex items-center justify-between">
            {/* Mobile Menu Button + Page Title */}
            <div className="flex items-center gap-4">
              <button
                onClick={() => setIsMobileMenuOpen(true)}
                className="lg:hidden text-gray-600 hover:text-kq-dark"
              >
                <Menu className="w-6 h-6" />
              </button>
              <h1 className="text-lg lg:text-xl font-cabrito font-bold text-kq-dark">
                {navigation.find((item) => pathname === item.href)?.name || 'Dashboard'}
              </h1>
            </div>

            {/* Right Side: Notification + User */}
            <div className="flex items-center gap-3">
              {/* Notification Bell */}
              {stats && (stats.pendingApproval > 0 || stats.requiringManualReview > 0) && (
                <div className="relative">
                  <button
                    onClick={() => setIsNotificationOpen(!isNotificationOpen)}
                    className="relative p-2 hover:bg-gray-100 rounded-lg transition-colors"
                  >
                    <Bell className="w-5 h-5 text-gray-600" />
                    <span className="absolute top-1 right-1 w-4 h-4 bg-kq-red text-white text-xs font-bold rounded-full flex items-center justify-center">
                      {stats.pendingApproval + stats.requiringManualReview}
                    </span>
                  </button>

                  {/* Notification Dropdown */}
                  {isNotificationOpen && (
                    <>
                      <div
                        className="fixed inset-0 z-40"
                        onClick={() => setIsNotificationOpen(false)}
                      />
                      <div className="absolute right-0 mt-2 w-80 bg-white rounded-lg shadow-lg border border-gray-200 z-50">
                        <div className="p-4 border-b border-gray-200">
                          <h3 className="font-semibold text-kq-dark">Notifications</h3>
                          <p className="text-xs text-gray-500 mt-0.5">
                            {stats.pendingApproval + stats.requiringManualReview} pending task
                            {stats.pendingApproval + stats.requiringManualReview !== 1 ? 's' : ''}
                          </p>
                        </div>

                        <div className="max-h-96 overflow-y-auto">
                          {stats.requiringManualReview > 0 && (
                            <Link
                              href="/admin/registrations/review"
                              onClick={() => setIsNotificationOpen(false)}
                            >
                              <div className="p-4 hover:bg-orange-50 border-b border-gray-100 cursor-pointer">
                                <div className="flex items-start gap-3">
                                  <div className="w-8 h-8 bg-orange-100 rounded-full flex items-center justify-center flex-shrink-0">
                                    <AlertCircle className="w-4 h-4 text-orange-600" />
                                  </div>
                                  <div className="flex-1 min-w-0">
                                    <p className="text-sm font-medium text-kq-dark">
                                      Manual Review Required
                                    </p>
                                    <p className="text-xs text-gray-600 mt-0.5">
                                      {stats.requiringManualReview} registration
                                      {stats.requiringManualReview !== 1 ? 's' : ''} need
                                      {stats.requiringManualReview === 1 ? 's' : ''} your attention
                                    </p>
                                  </div>
                                  <ArrowRight className="w-4 h-4 text-gray-400 flex-shrink-0" />
                                </div>
                              </div>
                            </Link>
                          )}

                          {stats.pendingApproval > 0 && (
                            <Link
                              href="/admin/registrations?status=Pending"
                              onClick={() => setIsNotificationOpen(false)}
                            >
                              <div className="p-4 hover:bg-yellow-50 border-b border-gray-100 cursor-pointer">
                                <div className="flex items-start gap-3">
                                  <div className="w-8 h-8 bg-yellow-100 rounded-full flex items-center justify-center flex-shrink-0">
                                    <Clock className="w-4 h-4 text-yellow-600" />
                                  </div>
                                  <div className="flex-1 min-w-0">
                                    <p className="text-sm font-medium text-kq-dark">
                                      Pending Approval
                                    </p>
                                    <p className="text-xs text-gray-600 mt-0.5">
                                      {stats.pendingApproval} registration
                                      {stats.pendingApproval !== 1 ? 's' : ''} being processed
                                    </p>
                                  </div>
                                  <ArrowRight className="w-4 h-4 text-gray-400 flex-shrink-0" />
                                </div>
                              </div>
                            </Link>
                          )}
                        </div>

                        <div className="p-3 border-t border-gray-200 bg-gray-50">
                          <Link
                            href="/admin/registrations"
                            onClick={() => setIsNotificationOpen(false)}
                          >
                            <button className="text-xs text-kq-red hover:underline font-medium">
                              View All Registrations →
                            </button>
                          </Link>
                        </div>
                      </div>
                    </>
                  )}
                </div>
              )}

              {/* User Dropdown Menu */}
              <div className="relative">
                <button
                  onClick={() => setIsUserMenuOpen(!isUserMenuOpen)}
                  className="hidden sm:flex items-center gap-2 px-3 py-1.5 bg-gray-50 hover:bg-gray-100 rounded-lg transition-colors"
                >
                  <div className="w-8 h-8 rounded-full bg-kq-red flex items-center justify-center">
                    <span className="text-xs font-bold text-white">
                      {user.fullName
                        .split(' ')
                        .map((n) => n[0])
                        .join('')
                        .substring(0, 2)}
                    </span>
                  </div>
                  <div className="hidden md:block text-left">
                    <p className="text-sm font-medium text-kq-dark leading-tight">
                      {user.fullName}
                    </p>
                    <p className="text-xs text-gray-500">{user.role}</p>
                  </div>
                  <ChevronDown className="w-4 h-4 text-gray-500" />
                </button>

                {/* Dropdown Menu */}
                {isUserMenuOpen && (
                  <>
                    <div className="fixed inset-0 z-40" onClick={() => setIsUserMenuOpen(false)} />
                    <div className="absolute right-0 mt-2 w-56 bg-white rounded-lg shadow-lg border border-gray-200 py-1 z-50">
                      <div className="px-4 py-3 border-b border-gray-200">
                        <p className="text-sm font-medium text-kq-dark">{user.fullName}</p>
                        <p className="text-xs text-gray-500 mt-0.5">{user.email}</p>
                        <p className="text-xs text-kq-red mt-1 font-medium">{user.role}</p>
                      </div>

                      <Link href="/admin/profile">
                        <button
                          onClick={() => setIsUserMenuOpen(false)}
                          className="w-full px-4 py-2.5 text-left text-sm text-gray-700 hover:bg-gray-50 flex items-center gap-2"
                        >
                          <UserIcon className="w-4 h-4" />
                          My Profile
                        </button>
                      </Link>

                      <Link href="/admin/settings">
                        <button
                          onClick={() => setIsUserMenuOpen(false)}
                          className="w-full px-4 py-2.5 text-left text-sm text-gray-700 hover:bg-gray-50 flex items-center gap-2"
                        >
                          <Settings className="w-4 h-4" />
                          Settings
                        </button>
                      </Link>

                      <div className="border-t border-gray-200 mt-1 pt-1">
                        <button
                          onClick={handleLogout}
                          className="w-full px-4 py-2.5 text-left text-sm text-red-600 hover:bg-red-50 flex items-center gap-2"
                        >
                          <LogOut className="w-4 h-4" />
                          Logout
                        </button>
                      </div>
                    </div>
                  </>
                )}
              </div>
            </div>
          </div>
        </header>

        {/* Page Content */}
        <main className="p-4 lg:p-6 xl:p-8 min-h-screen bg-gray-50">{children}</main>
      </div>
    </div>
  );
}
