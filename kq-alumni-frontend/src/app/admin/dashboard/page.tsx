'use client';

/**
 * Admin Dashboard Page
 * Overview page with statistics and quick actions
 */

import { useState, useEffect } from 'react';
import Link from 'next/link';
import {
  Users,
  UserCheck,
  UserX,
  Clock,
  AlertCircle,
  CheckCircle2,
  Mail,
  MailCheck,
  ArrowRight,
  Activity,
  Calendar,
  ExternalLink,
  RefreshCw,
} from 'lucide-react';
import { AdminLayout } from '@/components/admin/AdminLayout';
import { DashboardCharts } from '@/components/admin/DashboardCharts';
import { SystemHealth } from '@/components/admin/SystemHealth';
import { useDashboardStats, useAdminRegistrations } from '@/lib/api/services/adminService';
import { Button } from '@/components/ui/button/Button';

// ============================================
// Stats Card Component
// ============================================

interface StatsCardProps {
  title: string;
  value: number;
  icon: React.ComponentType<{ className?: string }>;
  color: 'blue' | 'green' | 'red' | 'yellow' | 'purple' | 'orange';
  description?: string;
  trend?: {
    value: number;
    isPositive: boolean;
  };
}

const colorClasses = {
  blue: {
    bg: 'bg-blue-50',
    icon: 'text-blue-600',
    border: 'border-blue-200',
  },
  green: {
    bg: 'bg-green-50',
    icon: 'text-green-600',
    border: 'border-green-200',
  },
  red: {
    bg: 'bg-red-50',
    icon: 'text-red-600',
    border: 'border-red-200',
  },
  yellow: {
    bg: 'bg-yellow-50',
    icon: 'text-yellow-600',
    border: 'border-yellow-200',
  },
  purple: {
    bg: 'bg-purple-50',
    icon: 'text-purple-600',
    border: 'border-purple-200',
  },
  orange: {
    bg: 'bg-orange-50',
    icon: 'text-orange-600',
    border: 'border-orange-200',
  },
};

function StatsCard({ title, value, icon: Icon, color, description }: StatsCardProps) {
  const colors = colorClasses[color];

  return (
    <div className="bg-white rounded-lg border border-gray-200 p-6 hover:shadow-lg transition-shadow">
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <p className="text-sm font-medium text-gray-600 mb-1">{title}</p>
          <p className="text-3xl font-cabrito font-bold text-kq-dark mb-2">
            {value.toLocaleString()}
          </p>
          {description && (
            <p className="text-xs text-gray-500">{description}</p>
          )}
        </div>
        <div
          className={`
            ${colors.bg} ${colors.icon}
            w-12 h-12 rounded-lg flex items-center justify-center
            border ${colors.border}
          `}
        >
          <Icon className="w-6 h-6" />
        </div>
      </div>
    </div>
  );
}

// ============================================
// Main Component
// ============================================

export default function AdminDashboardPage() {
  const { data: stats, isLoading, error, refetch } = useDashboardStats();
  const { data: recentRegistrations, refetch: refetchRegistrations } = useAdminRegistrations({ pageNumber: 1, pageSize: 5 });
  const [lastUpdated, setLastUpdated] = useState<Date>(new Date());
  const [isRefreshing, setIsRefreshing] = useState(false);

  // Auto-refresh every 30 seconds
  useEffect(() => {
    const interval = setInterval(() => {
      refetch();
      refetchRegistrations();
      setLastUpdated(new Date());
    }, 30000); // 30 seconds

    return () => clearInterval(interval);
  }, [refetch, refetchRegistrations]);

  // Manual refresh
  const handleManualRefresh = async () => {
    setIsRefreshing(true);
    await Promise.all([refetch(), refetchRegistrations()]);
    setLastUpdated(new Date());
    setTimeout(() => setIsRefreshing(false), 500);
  };

  return (
    <AdminLayout>
      <div className="max-w-7xl mx-auto">
        {/* Auto-refresh indicator */}
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-2 text-sm text-gray-500">
            <Clock className="w-4 h-4" />
            <span>
              Last updated:{' '}
              {lastUpdated.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', second: '2-digit' })}
            </span>
          </div>
          <button
            onClick={handleManualRefresh}
            disabled={isRefreshing}
            className="flex items-center gap-2 px-3 py-1.5 text-sm text-gray-600 hover:text-kq-red hover:bg-gray-100 rounded-lg transition-colors disabled:opacity-50"
          >
            <RefreshCw className={`w-4 h-4 ${isRefreshing ? 'animate-spin' : ''}`} />
            <span>Refresh</span>
          </button>
        </div>

        {/* System Health */}
        <SystemHealth />

        {/* Loading State */}
        {isLoading && (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {[...Array(8)].map((_, i) => (
              <div
                key={i}
                className="bg-white rounded-tl-sm rounded-tr-2xl rounded-br-sm rounded-bl-2xl border border-gray-200 p-6 animate-pulse"
              >
                <div className="h-4 bg-gray-200 rounded w-24 mb-4" />
                <div className="h-8 bg-gray-200 rounded w-16 mb-2" />
                <div className="h-3 bg-gray-200 rounded w-32" />
              </div>
            ))}
          </div>
        )}

        {/* Error State */}
        {error && (
          <div className="bg-red-50 border-l-4 border-red-500 p-4 rounded-r-md">
            <div className="flex items-start">
              <AlertCircle className="w-5 h-5 text-red-500 mt-0.5 mr-3" />
              <div>
                <p className="text-sm font-medium text-red-800">
                  Failed to load dashboard statistics
                </p>
                <p className="text-sm text-red-700 mt-1">
                  {(error as any)?.response?.data?.detail || (error as any)?.message || 'Please try again later'}
                </p>
              </div>
            </div>
          </div>
        )}

        {/* Stats Grid */}
        {stats && !isLoading && (
          <>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
              <StatsCard
                title="Total Registrations"
                value={stats.totalRegistrations}
                icon={Users}
                color="blue"
                description="All time registrations"
              />
              <StatsCard
                title="Pending Approval"
                value={stats.pendingApproval}
                icon={Clock}
                color="yellow"
                description="Awaiting ERP validation"
              />
              <StatsCard
                title="Requires Review"
                value={stats.requiringManualReview}
                icon={AlertCircle}
                color="orange"
                description="Flagged for HR review"
              />
              <StatsCard
                title="Approved"
                value={stats.approved}
                icon={UserCheck}
                color="green"
                description="Approved registrations"
              />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
              <StatsCard
                title="Active Members"
                value={stats.active}
                icon={CheckCircle2}
                color="green"
                description="Email verified"
              />
              <StatsCard
                title="Email Verified"
                value={stats.emailVerified}
                icon={MailCheck}
                color="purple"
                description="Completed verification"
              />
              <StatsCard
                title="Not Verified"
                value={stats.emailNotVerified}
                icon={Mail}
                color="blue"
                description="Pending verification"
              />
              <StatsCard
                title="Rejected"
                value={stats.rejected}
                icon={UserX}
                color="red"
                description="Declined applications"
              />
            </div>
          </>
        )}

        {/* Analytics Charts */}
        {stats && !isLoading && (
          <DashboardCharts stats={stats} />
        )}

        {/* Quick Actions */}
        {stats && !isLoading && (
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
            {/* Requiring Review Card */}
            {stats.requiringManualReview > 0 && (
              <div className="bg-white rounded-lg border-2 border-orange-200 p-6">
                <div className="flex items-start mb-4">
                  <div className="bg-orange-100 rounded-lg p-3 mr-4">
                    <AlertCircle className="w-6 h-6 text-orange-600" />
                  </div>
                  <div>
                    <h3 className="text-lg font-bold text-kq-dark mb-1">
                      Action Required
                    </h3>
                    <p className="text-sm text-gray-600">
                      {stats.requiringManualReview} registration
                      {stats.requiringManualReview !== 1 ? 's' : ''} need your review
                    </p>
                  </div>
                </div>
                <Link href="/admin/registrations/review">
                  <Button variant="primary" size="md" rightIcon={<ArrowRight className="w-4 h-4" />}>
                    Review Now
                  </Button>
                </Link>
              </div>
            )}

            {/* Pending Approval Card */}
            {stats.pendingApproval > 0 && (
              <div className="bg-white rounded-lg border-2 border-yellow-200 p-6">
                <div className="flex items-start mb-4">
                  <div className="bg-yellow-100 rounded-lg p-3 mr-4">
                    <Clock className="w-6 h-6 text-yellow-600" />
                  </div>
                  <div>
                    <h3 className="text-lg font-bold text-kq-dark mb-1">
                      In Progress
                    </h3>
                    <p className="text-sm text-gray-600">
                      {stats.pendingApproval} registration
                      {stats.pendingApproval !== 1 ? 's' : ''} being processed automatically
                    </p>
                  </div>
                </div>
                <Link href="/admin/registrations?status=Pending">
                  <Button variant="outline" size="md" rightIcon={<ArrowRight className="w-4 h-4" />}>
                    View Pending
                  </Button>
                </Link>
              </div>
            )}
          </div>
        )}

        {/* Recent Activity */}
        {recentRegistrations && recentRegistrations.data && recentRegistrations.data.length > 0 && (
          <div>
            <div className="flex items-center justify-between mb-4">
              <div className="flex items-center gap-2">
                <Activity className="w-5 h-5 text-kq-red" />
                <h3 className="text-lg font-cabrito font-bold text-kq-dark">
                  Recent Registrations
                </h3>
              </div>
              <Link href="/admin/registrations">
                <Button variant="ghost" size="sm" rightIcon={<ExternalLink className="w-4 h-4" />}>
                  View All
                </Button>
              </Link>
            </div>

            <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
              <div className="divide-y divide-gray-200">
                {recentRegistrations.data.map((registration) => (
                  <Link
                    key={registration.id}
                    href={`/admin/registrations/${registration.id}`}
                    className="block hover:bg-gray-50 transition-colors p-4"
                  >
                    <div className="flex items-center justify-between">
                      <div className="flex-1">
                        <div className="flex items-center gap-3 mb-2">
                          <div className="w-10 h-10 rounded-full bg-navy-900 flex items-center justify-center">
                            <span className="text-sm font-bold text-white">
                              {registration.fullName
                                .split(' ')
                                .map((n) => n[0])
                                .join('')
                                .substring(0, 2)}
                            </span>
                          </div>
                          <div className="flex-1">
                            <p className="font-medium text-kq-dark">
                              {registration.fullName}
                            </p>
                            <p className="text-sm text-gray-600">
                              {registration.staffNumber} • {registration.email}
                            </p>
                          </div>
                        </div>
                        <div className="flex items-center gap-4 text-xs text-gray-500 ml-13">
                          <div className="flex items-center gap-1">
                            <Calendar className="w-3 h-3" />
                            {new Date(registration.createdAt).toLocaleDateString('en-US', {
                              month: 'short',
                              day: 'numeric',
                              year: 'numeric',
                            })}
                          </div>
                          {registration.requiresManualReview && (
                            <span className="px-2 py-0.5 bg-orange-100 text-orange-700 rounded-full font-medium">
                              Requires Review
                            </span>
                          )}
                        </div>
                      </div>
                      <div className="ml-4">
                        <span
                          className={`
                            px-3 py-1 rounded-full text-xs font-medium
                            ${
                              registration.registrationStatus === 'Approved'
                                ? 'bg-green-100 text-green-700'
                                : registration.registrationStatus === 'Rejected'
                                ? 'bg-red-100 text-red-700'
                                : registration.registrationStatus === 'Pending'
                                ? 'bg-yellow-100 text-yellow-700'
                                : 'bg-blue-100 text-blue-700'
                            }
                          `}
                        >
                          {registration.registrationStatus}
                        </span>
                      </div>
                    </div>
                  </Link>
                ))}
              </div>
            </div>
          </div>
        )}
      </div>
    </AdminLayout>
  );
}
