'use client';

/**
 * Admin Login Page
 * Authentication page for HR staff to access the admin dashboard
 */

import { useState } from 'react';
import { zodResolver } from '@hookform/resolvers/zod';
import { Shield, AlertCircle } from 'lucide-react';
import { useRouter } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { z } from 'zod';

import { Button } from '@/components/ui/button/Button';
import { useAdminLogin } from '@/lib/api/services/adminService';
import { useAdminAuthActions } from '@/store/adminStore';
import type { AdminLoginRequest } from '@/types/admin';

// ============================================
// Validation Schema
// ============================================

const loginSchema = z.object({
  username: z.string().min(3, 'Username must be at least 3 characters'),
  password: z.string().min(8, 'Password must be at least 8 characters'),
});

type LoginFormData = z.infer<typeof loginSchema>;

// ============================================
// Component
// ============================================

export default function AdminLoginPage() {
  const router = useRouter();
  const { mutate: login, isPending, error } = useAdminLogin();
  const { setUser, setToken, setAuthenticated } = useAdminAuthActions();
  const [showPassword, setShowPassword] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = (data: LoginFormData) => {
    login(data as AdminLoginRequest, {
      onSuccess: (response) => {
        // Store user data in state
        setUser({
          userId: response.userId,
          username: response.username,
          email: response.email,
          fullName: response.fullName,
          role: response.role,
        });
        setToken(response.token);
        setAuthenticated(true);

        // Redirect to dashboard
        router.push('/admin/dashboard');
      },
    });
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-navy-900 via-navy-800 to-navy-900 px-4 py-12">
      <div className="w-full max-w-md">
        {/* Header */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-kq-red rounded-tl-sm rounded-tr-2xl rounded-br-sm rounded-bl-2xl mb-4">
            <Shield className="w-8 h-8 text-white" />
          </div>
          <h1 className="text-3xl font-cabrito font-bold text-white mb-2">
            Admin Login
          </h1>
          <p className="text-gray-400 font-roboto">
            Kenya Airways Alumni Association
          </p>
        </div>

        {/* Login Card */}
        <div className="bg-white rounded-tl-sm rounded-tr-2xl rounded-br-sm rounded-bl-2xl shadow-2xl p-8">
          {/* Error Alert */}
          {error && (
            <div className="mb-6 p-4 bg-red-50 border-l-4 border-red-500 rounded-r-md">
              <div className="flex items-start">
                <AlertCircle className="w-5 h-5 text-red-500 mt-0.5 mr-3 flex-shrink-0" />
                <div>
                  <p className="text-sm font-medium text-red-800">
                    Login Failed
                  </p>
                  <p className="text-sm text-red-700 mt-1">
                    {(error as Error)?.message ||
                      'Invalid username or password'}
                  </p>
                </div>
              </div>
            </div>
          )}

          {/* Login Form */}
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            {/* Username Field */}
            <div>
              <label
                htmlFor="username"
                className="block text-sm font-medium text-kq-dark mb-2"
              >
                Username
              </label>
              <input
                {...register('username')}
                id="username"
                type="text"
                autoComplete="username"
                placeholder="Enter your username"
                className={`
                  w-full px-4 py-3
                  border-b-2
                  ${errors.username ? 'border-red-500' : 'border-gray-300'}
                  bg-transparent
                  focus:outline-none
                  focus:border-kq-red
                  transition-colors
                  text-kq-dark
                  placeholder:text-gray-400
                `}
              />
              {errors.username && (
                <p className="mt-2 text-sm text-red-600">
                  {errors.username.message}
                </p>
              )}
            </div>

            {/* Password Field */}
            <div>
              <label
                htmlFor="password"
                className="block text-sm font-medium text-kq-dark mb-2"
              >
                Password
              </label>
              <div className="relative">
                <input
                  {...register('password')}
                  id="password"
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="current-password"
                  placeholder="Enter your password"
                  className={`
                    w-full px-4 py-3 pr-12
                    border-b-2
                    ${errors.password ? 'border-red-500' : 'border-gray-300'}
                    bg-transparent
                    focus:outline-none
                    focus:border-kq-red
                    transition-colors
                    text-kq-dark
                    placeholder:text-gray-400
                  `}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-0 top-1/2 -translate-y-1/2 px-3 text-gray-500 hover:text-kq-dark transition-colors"
                >
                  {showPassword ? (
                    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                    </svg>
                  ) : (
                    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21" />
                    </svg>
                  )}
                </button>
              </div>
              {errors.password && (
                <p className="mt-2 text-sm text-red-600">
                  {errors.password.message}
                </p>
              )}
            </div>

            {/* Submit Button */}
            <Button
              type="submit"
              variant="primary"
              size="lg"
              className="w-full"
              isLoading={isPending}
              loadingText="Signing in..."
            >
              Sign In
            </Button>
          </form>

          {/* Help Text */}
          <div className="mt-6 pt-6 border-t border-gray-200">
            <p className="text-sm text-center text-gray-600">
              For access issues, contact{' '}
              <a
                href="mailto:KQ.Alumni@kenya-airways.com"
                className="text-kq-red hover:text-kq-red-dark font-medium transition-colors"
              >
                KQ.Alumni@kenya-airways.com
              </a>
            </p>
          </div>
        </div>

        {/* Footer */}
        <p className="text-center text-sm text-gray-400 mt-6">
          Kenya Airways Alumni Association © {new Date().getFullYear()}
        </p>
      </div>
    </div>
  );
}
