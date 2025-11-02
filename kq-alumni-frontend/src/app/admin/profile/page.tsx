'use client';

/**
 * Admin Profile Page
 * View admin user profile information
 */

import { AdminLayout } from '@/components/admin/AdminLayout';
import { useAdminUser } from '@/store/adminStore';
import { Mail, Shield, Calendar, User } from 'lucide-react';

export default function AdminProfilePage() {
  const user = useAdminUser();

  if (!user) {
    return null;
  }

  return (
    <AdminLayout>
      <div className="max-w-4xl mx-auto">
        <h2 className="text-2xl font-cabrito font-bold text-kq-dark mb-6">My Profile</h2>

        {/* Profile Card */}
        <div className="bg-white rounded-lg border border-gray-200 p-6 mb-6">
          <div className="flex items-center gap-4 mb-6">
            <div className="w-20 h-20 rounded-full bg-kq-red flex items-center justify-center">
              <span className="text-2xl font-bold text-white">
                {user.fullName.split(' ').map((n) => n[0]).join('').substring(0, 2)}
              </span>
            </div>
            <div>
              <h3 className="text-xl font-bold text-kq-dark">{user.fullName}</h3>
              <p className="text-gray-600">{user.role}</p>
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="flex items-start gap-3">
              <User className="w-5 h-5 text-gray-400 mt-0.5" />
              <div>
                <p className="text-sm text-gray-600">Username</p>
                <p className="font-medium text-kq-dark">{user.username}</p>
              </div>
            </div>

            <div className="flex items-start gap-3">
              <Mail className="w-5 h-5 text-gray-400 mt-0.5" />
              <div>
                <p className="text-sm text-gray-600">Email</p>
                <p className="font-medium text-kq-dark">{user.email}</p>
              </div>
            </div>

            <div className="flex items-start gap-3">
              <Shield className="w-5 h-5 text-gray-400 mt-0.5" />
              <div>
                <p className="text-sm text-gray-600">Role</p>
                <p className="font-medium text-kq-dark">{user.role}</p>
              </div>
            </div>

            <div className="flex items-start gap-3">
              <Calendar className="w-5 h-5 text-gray-400 mt-0.5" />
              <div>
                <p className="text-sm text-gray-600">User ID</p>
                <p className="font-medium text-kq-dark">#{user.userId}</p>
              </div>
            </div>
          </div>
        </div>

        {/* Security Info */}
        <div className="bg-blue-50 border-l-4 border-blue-500 p-4 rounded-r-md">
          <p className="text-sm text-blue-800 font-medium">Security Reminder</p>
          <p className="text-sm text-blue-700 mt-1">
            For security reasons, please change your password regularly. You can update your password from the Settings page.
          </p>
        </div>
      </div>
    </AdminLayout>
  );
}
