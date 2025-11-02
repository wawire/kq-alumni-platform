'use client';

/**
 * Dashboard Charts Component
 * Visualizes registration statistics and trends
 */

import { useState } from 'react';
import {
  PieChart,
  Pie,
  Cell,
  ResponsiveContainer,
  Legend,
  Tooltip,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
} from 'recharts';
import { ChevronDown, ChevronUp, TrendingUp, PieChart as PieChartIcon } from 'lucide-react';
import type { DashboardStats } from '@/types/admin';

interface DashboardChartsProps {
  stats: DashboardStats;
}

const COLORS = {
  pending: '#EAB308', // yellow
  approved: '#22C55E', // green
  rejected: '#EF4444', // red
  requiresReview: '#F97316', // orange
};

export function DashboardCharts({ stats }: DashboardChartsProps) {
  const [isExpanded, setIsExpanded] = useState(false);

  // Prepare data for Status Distribution Pie Chart
  const statusDistributionData = [
    { name: 'Pending Approval', value: stats.pendingApproval, color: COLORS.pending },
    { name: 'Approved', value: stats.approved, color: COLORS.approved },
    { name: 'Rejected', value: stats.rejected, color: COLORS.rejected },
    { name: 'Requires Review', value: stats.requiringManualReview, color: COLORS.requiresReview },
  ].filter((item) => item.value > 0); // Only show non-zero values

  // Prepare data for Email Verification Bar Chart
  const emailVerificationData = [
    { name: 'Verified', count: stats.emailVerified, fill: '#8B5CF6' }, // purple
    { name: 'Not Verified', count: stats.emailNotVerified, fill: '#3B82F6' }, // blue
  ];

  return (
    <div className="bg-white rounded-lg border border-gray-200 overflow-hidden mb-8">
      {/* Collapsible Header */}
      <button
        onClick={() => setIsExpanded(!isExpanded)}
        className="w-full px-6 py-4 flex items-center justify-between hover:bg-gray-50 transition-colors"
      >
        <div className="flex items-center gap-3">
          <div className="bg-kq-red bg-opacity-10 rounded-lg p-2">
            <TrendingUp className="w-5 h-5 text-kq-red" />
          </div>
          <div className="text-left">
            <h3 className="text-lg font-cabrito font-bold text-kq-dark">
              Analytics & Trends
            </h3>
            <p className="text-sm text-gray-600">
              Visual insights into registration data
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <span className="text-sm text-gray-500">
            {isExpanded ? 'Hide Charts' : 'Show Charts'}
          </span>
          {isExpanded ? (
            <ChevronUp className="w-5 h-5 text-gray-400" />
          ) : (
            <ChevronDown className="w-5 h-5 text-gray-400" />
          )}
        </div>
      </button>

      {/* Charts Content */}
      {isExpanded && (
        <div className="p-6 pt-0 border-t border-gray-100">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
            {/* Status Distribution Pie Chart */}
            <div className="bg-gray-50 rounded-lg p-6">
              <div className="flex items-center gap-2 mb-4">
                <PieChartIcon className="w-5 h-5 text-kq-red" />
                <h4 className="font-bold text-kq-dark">Registration Status Distribution</h4>
              </div>
              <ResponsiveContainer width="100%" height={300}>
                <PieChart>
                  <Pie
                    data={statusDistributionData}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    label={(entry: any) =>
                      `${entry.name}: ${((entry.percent || 0) * 100).toFixed(0)}%`
                    }
                    outerRadius={80}
                    fill="#8884d8"
                    dataKey="value"
                  >
                    {statusDistributionData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Pie>
                  <Tooltip
                    formatter={(value: number) => [value, 'Count']}
                    contentStyle={{
                      backgroundColor: 'white',
                      border: '1px solid #E5E7EB',
                      borderRadius: '8px',
                    }}
                  />
                  <Legend
                    verticalAlign="bottom"
                    height={36}
                    iconType="circle"
                  />
                </PieChart>
              </ResponsiveContainer>
            </div>

            {/* Email Verification Bar Chart */}
            <div className="bg-gray-50 rounded-lg p-6">
              <div className="flex items-center gap-2 mb-4">
                <TrendingUp className="w-5 h-5 text-kq-red" />
                <h4 className="font-bold text-kq-dark">Email Verification Status</h4>
              </div>
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={emailVerificationData}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#E5E7EB" />
                  <XAxis dataKey="name" tick={{ fill: '#6B7280' }} />
                  <YAxis tick={{ fill: '#6B7280' }} />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: 'white',
                      border: '1px solid #E5E7EB',
                      borderRadius: '8px',
                    }}
                  />
                  <Bar dataKey="count" radius={[8, 8, 0, 0]}>
                    {emailVerificationData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.fill} />
                    ))}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
              <div className="mt-4 grid grid-cols-2 gap-4">
                <div className="text-center p-3 bg-white rounded-lg border border-purple-200">
                  <p className="text-2xl font-bold text-purple-600">
                    {stats.emailVerified}
                  </p>
                  <p className="text-xs text-gray-600 mt-1">Verified</p>
                </div>
                <div className="text-center p-3 bg-white rounded-lg border border-blue-200">
                  <p className="text-2xl font-bold text-blue-600">
                    {stats.emailNotVerified}
                  </p>
                  <p className="text-xs text-gray-600 mt-1">Not Verified</p>
                </div>
              </div>
            </div>
          </div>

          {/* Summary Stats Below Charts */}
          <div className="mt-6 grid grid-cols-2 md:grid-cols-4 gap-4">
            <div className="bg-white rounded-lg border border-gray-200 p-4 text-center">
              <p className="text-sm text-gray-600 mb-1">Total Registrations</p>
              <p className="text-2xl font-bold text-kq-dark">
                {stats.totalRegistrations.toLocaleString()}
              </p>
            </div>
            <div className="bg-white rounded-lg border border-gray-200 p-4 text-center">
              <p className="text-sm text-gray-600 mb-1">Active Members</p>
              <p className="text-2xl font-bold text-green-600">
                {stats.active.toLocaleString()}
              </p>
            </div>
            <div className="bg-white rounded-lg border border-yellow-200 p-4 text-center">
              <p className="text-sm text-gray-600 mb-1">In Progress</p>
              <p className="text-2xl font-bold text-yellow-600">
                {stats.pendingApproval.toLocaleString()}
              </p>
            </div>
            <div className="bg-white rounded-lg border border-orange-200 p-4 text-center">
              <p className="text-sm text-gray-600 mb-1">Needs Action</p>
              <p className="text-2xl font-bold text-orange-600">
                {stats.requiringManualReview.toLocaleString()}
              </p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
