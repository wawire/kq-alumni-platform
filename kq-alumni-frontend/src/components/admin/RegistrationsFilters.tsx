'use client';

/**
 * Advanced Registrations Filters Component
 * Comprehensive filtering options for registration list
 */

import { useState, useEffect } from 'react';
import {
  Search,
  Filter,
  X,
  ChevronDown,
  ChevronUp,
  Calendar,
  RefreshCw,
  Building2,
  Globe2,
  MapPin,
  Briefcase,
  CheckCircle,
} from 'lucide-react';
import type { RegistrationStatus, RegistrationFilters } from '@/types/admin';

interface RegistrationsFiltersProps {
  filters: RegistrationFilters;
  onFiltersChange: (filters: RegistrationFilters) => void;
  totalCount?: number;
}

export function RegistrationsFilters({
  filters,
  onFiltersChange,
  totalCount,
}: RegistrationsFiltersProps) {
  const [showAdvanced, setShowAdvanced] = useState(false);
  const [localSearch, setLocalSearch] = useState(filters.searchQuery || '');

  // Automatic search with debouncing (800ms delay)
  useEffect(() => {
    const timer = setTimeout(() => {
      if (localSearch !== filters.searchQuery) {
        onFiltersChange({ ...filters, searchQuery: localSearch || undefined, pageNumber: 1 });
      }
    }, 800);

    return () => clearTimeout(timer);
  }, [localSearch]); // eslint-disable-line react-hooks/exhaustive-deps

  const statuses: Array<{ label: string; value: RegistrationStatus | undefined }> = [
    { label: 'All', value: undefined },
    { label: 'Pending', value: 'Pending' },
    { label: 'Approved', value: 'Approved' },
    { label: 'Active', value: 'Active' },
    { label: 'Rejected', value: 'Rejected' },
  ];

  const handleStatusChange = (status: RegistrationStatus | undefined) => {
    onFiltersChange({ ...filters, status, pageNumber: 1 });
  };

  const handleClearSearch = () => {
    setLocalSearch('');
    onFiltersChange({ ...filters, searchQuery: undefined, pageNumber: 1 });
  };

  const handleDateChange = (field: 'dateFrom' | 'dateTo' | 'exitDateFrom' | 'exitDateTo', value: string) => {
    onFiltersChange({ ...filters, [field]: value || undefined, pageNumber: 1 });
  };

  const handleTextChange = (field: 'department' | 'country' | 'city' | 'industry', value: string) => {
    onFiltersChange({ ...filters, [field]: value || undefined, pageNumber: 1 });
  };

  const handleToggleFilter = (field: 'emailVerified' | 'requiresManualReview' | 'erpValidated') => {
    const currentValue = filters[field];
    const newValue = currentValue === undefined ? true : currentValue === true ? false : undefined;
    onFiltersChange({ ...filters, [field]: newValue, pageNumber: 1 });
  };

  const handleYearChange = (year: number | undefined) => {
    onFiltersChange({ ...filters, registrationYear: year, pageNumber: 1 });
  };

  const handleClearFilters = () => {
    setLocalSearch('');
    onFiltersChange({
      pageNumber: 1,
      pageSize: filters.pageSize,
    });
  };

  const hasActiveFilters =
    filters.status ||
    filters.searchQuery ||
    filters.dateFrom ||
    filters.dateTo ||
    filters.emailVerified !== undefined ||
    filters.requiresManualReview !== undefined ||
    filters.department ||
    filters.exitDateFrom ||
    filters.exitDateTo ||
    filters.country ||
    filters.city ||
    filters.industry ||
    filters.erpValidated !== undefined ||
    filters.registrationYear;

  const activeFilterCount = [
    filters.status,
    filters.searchQuery,
    filters.dateFrom,
    filters.dateTo,
    filters.emailVerified !== undefined,
    filters.requiresManualReview !== undefined,
    filters.department,
    filters.exitDateFrom,
    filters.exitDateTo,
    filters.country,
    filters.city,
    filters.industry,
    filters.erpValidated !== undefined,
    filters.registrationYear,
  ].filter(Boolean).length;

  // Quick filter presets
  const currentYear = new Date().getFullYear();
  const quickFilters = [
    { label: `${currentYear} Registrations`, action: () => handleYearChange(currentYear) },
    { label: `${currentYear - 1} Registrations`, action: () => handleYearChange(currentYear - 1) },
    {
      label: 'Pending Review',
      action: () => onFiltersChange({ ...filters, status: 'Pending', requiresManualReview: true, pageNumber: 1 }),
    },
    {
      label: 'Recently Approved',
      action: () =>
        onFiltersChange({
          ...filters,
          status: 'Approved',
          dateFrom: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
          pageNumber: 1,
        }),
    },
  ];

  return (
    <div className="bg-white rounded-lg border border-gray-200 overflow-hidden mb-6">
      {/* Quick Filter Presets */}
      <div className="bg-gray-50 px-4 py-3 border-b border-gray-200">
        <div className="flex items-center gap-2 flex-wrap">
          <span className="text-sm font-medium text-gray-600">Quick Filters:</span>
          {quickFilters.map((preset, index) => (
            <button
              key={index}
              onClick={preset.action}
              className="px-3 py-1 text-xs font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-100 transition-colors"
            >
              {preset.label}
            </button>
          ))}
        </div>
      </div>

      {/* Main Filters Bar */}
      <div className="p-4">
        <div className="flex flex-col lg:flex-row gap-4">
          {/* Search - Automatic with Debouncing */}
          <div className="flex-1">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
              <input
                type="text"
                placeholder="Search by reg number, name, email, staff number, or ID (automatic)..."
                value={localSearch}
                onChange={(e) => setLocalSearch(e.target.value)}
                className="w-full pl-10 pr-10 py-2.5 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-kq-red focus:border-transparent"
              />
              {localSearch && (
                <button
                  type="button"
                  onClick={handleClearSearch}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 transition-colors"
                >
                  <X className="w-5 h-5" />
                </button>
              )}
            </div>
          </div>

          {/* Advanced Filters Toggle */}
          <div className="flex gap-2">
            <button
              onClick={() => setShowAdvanced(!showAdvanced)}
              className={`
                flex items-center gap-2 px-4 py-2.5 rounded-lg border transition-colors
                ${showAdvanced
                  ? 'bg-kq-red text-white border-kq-red'
                  : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-50'
                }
              `}
            >
              <Filter className="w-4 h-4" />
              <span className="font-medium">Filters</span>
              {activeFilterCount > 0 && (
                <span className={`
                  px-2 py-0.5 rounded-full text-xs font-bold
                  ${showAdvanced ? 'bg-white text-kq-red' : 'bg-kq-red text-white'}
                `}>
                  {activeFilterCount}
                </span>
              )}
              {showAdvanced ? (
                <ChevronUp className="w-4 h-4" />
              ) : (
                <ChevronDown className="w-4 h-4" />
              )}
            </button>

            {hasActiveFilters && (
              <button
                onClick={handleClearFilters}
                className="flex items-center gap-2 px-4 py-2.5 rounded-lg border border-gray-300 text-gray-700 hover:bg-gray-50 transition-colors"
              >
                <RefreshCw className="w-4 h-4" />
                <span className="font-medium hidden sm:inline">Clear</span>
              </button>
            )}
          </div>
        </div>

        {/* Results Count */}
        {totalCount !== undefined && (
          <div className="mt-3 text-sm text-gray-600">
            {totalCount} registration{totalCount !== 1 ? 's' : ''} found
            {hasActiveFilters && ' (filtered)'}
          </div>
        )}
      </div>

      {/* Advanced Filters Panel */}
      {showAdvanced && (
        <div className="border-t border-gray-200 p-4 bg-gray-50">
          <div className="space-y-6">
            {/* Row 1: Status & Basic Filters */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              {/* Status Filter */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Status
                </label>
                <div className="flex flex-wrap gap-2">
                  {statuses.map((statusOption) => (
                    <button
                      key={statusOption.label}
                      onClick={() => handleStatusChange(statusOption.value)}
                      className={`
                        px-3 py-1.5 rounded-lg text-sm font-medium transition-colors border
                        ${filters.status === statusOption.value
                          ? 'bg-kq-red text-white border-kq-red'
                          : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-100'
                        }
                      `}
                    >
                      {statusOption.label}
                    </button>
                  ))}
                </div>
              </div>

              {/* Email Verified Filter */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Email Verification
                </label>
                <div className="flex gap-2">
                  <button
                    onClick={() => handleToggleFilter('emailVerified')}
                    className={`
                      flex-1 px-3 py-2 rounded-lg text-sm font-medium transition-colors border
                      ${filters.emailVerified === true
                        ? 'bg-green-100 text-green-800 border-green-300'
                        : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-100'
                      }
                    `}
                  >
                    Verified
                  </button>
                  <button
                    onClick={() => onFiltersChange({ ...filters, emailVerified: filters.emailVerified === false ? undefined : false, pageNumber: 1 })}
                    className={`
                      flex-1 px-3 py-2 rounded-lg text-sm font-medium transition-colors border
                      ${filters.emailVerified === false
                        ? 'bg-blue-100 text-blue-800 border-blue-300'
                        : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-100'
                      }
                    `}
                  >
                    Not Verified
                  </button>
                </div>
              </div>

              {/* Manual Review Filter */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Manual Review
                </label>
                <div className="flex gap-2">
                  <button
                    onClick={() => handleToggleFilter('requiresManualReview')}
                    className={`
                      flex-1 px-3 py-2 rounded-lg text-sm font-medium transition-colors border
                      ${filters.requiresManualReview === true
                        ? 'bg-orange-100 text-orange-800 border-orange-300'
                        : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-100'
                      }
                    `}
                  >
                    Requires Review
                  </button>
                  <button
                    onClick={() => onFiltersChange({ ...filters, requiresManualReview: filters.requiresManualReview === false ? undefined : false, pageNumber: 1 })}
                    className={`
                      flex-1 px-3 py-2 rounded-lg text-sm font-medium transition-colors border
                      ${filters.requiresManualReview === false
                        ? 'bg-gray-100 text-gray-800 border-gray-300'
                        : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-100'
                      }
                    `}
                  >
                    No Review
                  </button>
                </div>
              </div>

              {/* ERP Validated Filter */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  <CheckCircle className="w-4 h-4 inline-block mr-1 -mt-0.5" />
                  ERP Validation
                </label>
                <div className="flex gap-2">
                  <button
                    onClick={() => handleToggleFilter('erpValidated')}
                    className={`
                      flex-1 px-3 py-2 rounded-lg text-sm font-medium transition-colors border
                      ${filters.erpValidated === true
                        ? 'bg-green-100 text-green-800 border-green-300'
                        : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-100'
                      }
                    `}
                  >
                    Validated
                  </button>
                  <button
                    onClick={() => onFiltersChange({ ...filters, erpValidated: filters.erpValidated === false ? undefined : false, pageNumber: 1 })}
                    className={`
                      flex-1 px-3 py-2 rounded-lg text-sm font-medium transition-colors border
                      ${filters.erpValidated === false
                        ? 'bg-red-100 text-red-800 border-red-300'
                        : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-100'
                      }
                    `}
                  >
                    Not Validated
                  </button>
                </div>
              </div>
            </div>

            {/* Row 2: Date Filters */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              {/* Registration Date From */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  <Calendar className="w-4 h-4 inline-block mr-1 -mt-0.5" />
                  Created From
                </label>
                <input
                  type="date"
                  value={filters.dateFrom || ''}
                  onChange={(e) => handleDateChange('dateFrom', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-kq-red focus:border-transparent"
                />
              </div>

              {/* Registration Date To */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  <Calendar className="w-4 h-4 inline-block mr-1 -mt-0.5" />
                  Created To
                </label>
                <input
                  type="date"
                  value={filters.dateTo || ''}
                  onChange={(e) => handleDateChange('dateTo', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-kq-red focus:border-transparent"
                />
              </div>

              {/* Exit Date From */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  <Calendar className="w-4 h-4 inline-block mr-1 -mt-0.5" />
                  Exit Date From
                </label>
                <input
                  type="date"
                  value={filters.exitDateFrom || ''}
                  onChange={(e) => handleDateChange('exitDateFrom', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-kq-red focus:border-transparent"
                />
              </div>

              {/* Exit Date To */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  <Calendar className="w-4 h-4 inline-block mr-1 -mt-0.5" />
                  Exit Date To
                </label>
                <input
                  type="date"
                  value={filters.exitDateTo || ''}
                  onChange={(e) => handleDateChange('exitDateTo', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-kq-red focus:border-transparent"
                />
              </div>
            </div>

            {/* Row 3: Location & Organization Filters */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              {/* Department */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  <Building2 className="w-4 h-4 inline-block mr-1 -mt-0.5" />
                  Department
                </label>
                <input
                  type="text"
                  placeholder="Enter department..."
                  value={filters.department || ''}
                  onChange={(e) => handleTextChange('department', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-kq-red focus:border-transparent"
                />
              </div>

              {/* Country */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  <Globe2 className="w-4 h-4 inline-block mr-1 -mt-0.5" />
                  Country
                </label>
                <input
                  type="text"
                  placeholder="Enter country..."
                  value={filters.country || ''}
                  onChange={(e) => handleTextChange('country', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-kq-red focus:border-transparent"
                />
              </div>

              {/* City */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  <MapPin className="w-4 h-4 inline-block mr-1 -mt-0.5" />
                  City
                </label>
                <input
                  type="text"
                  placeholder="Enter city..."
                  value={filters.city || ''}
                  onChange={(e) => handleTextChange('city', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-kq-red focus:border-transparent"
                />
              </div>

              {/* Industry */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  <Briefcase className="w-4 h-4 inline-block mr-1 -mt-0.5" />
                  Industry
                </label>
                <input
                  type="text"
                  placeholder="Enter industry..."
                  value={filters.industry || ''}
                  onChange={(e) => handleTextChange('industry', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-kq-red focus:border-transparent"
                />
              </div>
            </div>

            {/* Row 4: Other Options */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              {/* Registration Year */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Registration Year
                </label>
                <select
                  value={filters.registrationYear || ''}
                  onChange={(e) => handleYearChange(e.target.value ? Number(e.target.value) : undefined)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-kq-red focus:border-transparent"
                >
                  <option value="">All Years</option>
                  {Array.from({ length: 5 }, (_, i) => currentYear - i).map((year) => (
                    <option key={year} value={year}>
                      {year}
                    </option>
                  ))}
                </select>
              </div>

              {/* Page Size */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Results Per Page
                </label>
                <select
                  value={filters.pageSize}
                  onChange={(e) =>
                    onFiltersChange({ ...filters, pageSize: Number(e.target.value), pageNumber: 1 })
                  }
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-kq-red focus:border-transparent"
                >
                  <option value={10}>10</option>
                  <option value={20}>20</option>
                  <option value={50}>50</option>
                  <option value={100}>100</option>
                </select>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
