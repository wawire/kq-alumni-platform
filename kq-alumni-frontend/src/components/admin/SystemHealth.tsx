'use client';

/**
 * System Health Component
 * Displays status of critical system components
 */

import { useState, useEffect } from 'react';
import {
  Database,
  Mail,
  Server,
  CheckCircle,
  XCircle,
  AlertCircle,
  RefreshCw,
  Activity,
  ChevronDown,
  ChevronUp,
} from 'lucide-react';

interface HealthStatus {
  status: 'healthy' | 'degraded' | 'down';
  message?: string;
  responseTime?: number;
  lastChecked: Date;
}

interface SystemHealthData {
  database: HealthStatus;
  erpApi: HealthStatus;
  emailService: HealthStatus;
  overallStatus: 'healthy' | 'degraded' | 'down';
}

export function SystemHealth() {
  const [health, setHealth] = useState<SystemHealthData>({
    database: {
      status: 'healthy',
      message: 'All connections active',
      responseTime: 12,
      lastChecked: new Date(),
    },
    erpApi: {
      status: 'healthy',
      message: 'Mock mode enabled',
      responseTime: 45,
      lastChecked: new Date(),
    },
    emailService: {
      status: 'healthy',
      message: 'SMTP server responding',
      responseTime: 23,
      lastChecked: new Date(),
    },
    overallStatus: 'healthy',
  });
  const [isExpanded, setIsExpanded] = useState(false);
  const [isRefreshing, setIsRefreshing] = useState(false);

  // Mock health check - in production this would call a real API endpoint
  const checkHealth = async () => {
    setIsRefreshing(true);

    // Simulate API call
    await new Promise((resolve) => setTimeout(resolve, 500));

    // Update with fresh data
    setHealth({
      database: {
        status: 'healthy',
        message: 'All connections active',
        responseTime: Math.floor(Math.random() * 20) + 10,
        lastChecked: new Date(),
      },
      erpApi: {
        status: 'healthy',
        message: 'Mock mode enabled',
        responseTime: Math.floor(Math.random() * 50) + 30,
        lastChecked: new Date(),
      },
      emailService: {
        status: 'healthy',
        message: 'SMTP server responding',
        responseTime: Math.floor(Math.random() * 30) + 15,
        lastChecked: new Date(),
      },
      overallStatus: 'healthy',
    });

    setIsRefreshing(false);
  };

  // Auto-refresh every 60 seconds
  useEffect(() => {
    const interval = setInterval(checkHealth, 60000);
    return () => clearInterval(interval);
  }, []);

  const getStatusColor = (status: HealthStatus['status']) => {
    switch (status) {
      case 'healthy':
        return 'text-green-600 bg-green-50 border-green-200';
      case 'degraded':
        return 'text-yellow-600 bg-yellow-50 border-yellow-200';
      case 'down':
        return 'text-red-600 bg-red-50 border-red-200';
    }
  };

  const getStatusIcon = (status: HealthStatus['status']) => {
    switch (status) {
      case 'healthy':
        return <CheckCircle className="w-5 h-5 text-green-600" />;
      case 'degraded':
        return <AlertCircle className="w-5 h-5 text-yellow-600" />;
      case 'down':
        return <XCircle className="w-5 h-5 text-red-600" />;
    }
  };

  const getOverallStatusColor = () => {
    switch (health.overallStatus) {
      case 'healthy':
        return 'bg-green-100 border-green-300';
      case 'degraded':
        return 'bg-yellow-100 border-yellow-300';
      case 'down':
        return 'bg-red-100 border-red-300';
    }
  };

  return (
    <div className={`bg-white rounded-lg border-2 ${getOverallStatusColor()} overflow-hidden mb-8`}>
      {/* Compact Header */}
      <button
        onClick={() => setIsExpanded(!isExpanded)}
        className="w-full px-6 py-4 flex items-center justify-between hover:bg-opacity-50 transition-colors"
      >
        <div className="flex items-center gap-3">
          <Activity className="w-5 h-5 text-kq-red" />
          <div className="text-left">
            <h3 className="text-lg font-cabrito font-bold text-kq-dark">
              System Health
            </h3>
            <p className="text-sm text-gray-600">
              All systems operational
            </p>
          </div>
        </div>

        <div className="flex items-center gap-4">
          {/* Quick Status Indicators */}
          <div className="flex items-center gap-2">
            {getStatusIcon(health.database.status)}
            {getStatusIcon(health.erpApi.status)}
            {getStatusIcon(health.emailService.status)}
          </div>

          <button
            onClick={(e) => {
              e.stopPropagation();
              checkHealth();
            }}
            disabled={isRefreshing}
            className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <RefreshCw className={`w-4 h-4 text-gray-600 ${isRefreshing ? 'animate-spin' : ''}`} />
          </button>

          {isExpanded ? (
            <ChevronUp className="w-5 h-5 text-gray-400" />
          ) : (
            <ChevronDown className="w-5 h-5 text-gray-400" />
          )}
        </div>
      </button>

      {/* Detailed Health Status */}
      {isExpanded && (
        <div className="border-t border-gray-200 p-6">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {/* Database Health */}
            <div className={`rounded-lg border p-4 ${getStatusColor(health.database.status)}`}>
              <div className="flex items-start justify-between mb-3">
                <div className="flex items-center gap-2">
                  <Database className="w-5 h-5" />
                  <h4 className="font-bold">Database</h4>
                </div>
                {getStatusIcon(health.database.status)}
              </div>
              <p className="text-sm mb-2">{health.database.message}</p>
              <div className="flex items-center justify-between text-xs">
                <span>Response: {health.database.responseTime}ms</span>
                <span>{health.database.lastChecked.toLocaleTimeString()}</span>
              </div>
            </div>

            {/* ERP API Health */}
            <div className={`rounded-lg border p-4 ${getStatusColor(health.erpApi.status)}`}>
              <div className="flex items-start justify-between mb-3">
                <div className="flex items-center gap-2">
                  <Server className="w-5 h-5" />
                  <h4 className="font-bold">ERP API</h4>
                </div>
                {getStatusIcon(health.erpApi.status)}
              </div>
              <p className="text-sm mb-2">{health.erpApi.message}</p>
              <div className="flex items-center justify-between text-xs">
                <span>Response: {health.erpApi.responseTime}ms</span>
                <span>{health.erpApi.lastChecked.toLocaleTimeString()}</span>
              </div>
            </div>

            {/* Email Service Health */}
            <div className={`rounded-lg border p-4 ${getStatusColor(health.emailService.status)}`}>
              <div className="flex items-start justify-between mb-3">
                <div className="flex items-center gap-2">
                  <Mail className="w-5 h-5" />
                  <h4 className="font-bold">Email Service</h4>
                </div>
                {getStatusIcon(health.emailService.status)}
              </div>
              <p className="text-sm mb-2">{health.emailService.message}</p>
              <div className="flex items-center justify-between text-xs">
                <span>Response: {health.emailService.responseTime}ms</span>
                <span>{health.emailService.lastChecked.toLocaleTimeString()}</span>
              </div>
            </div>
          </div>

          {/* Additional Info */}
          <div className="mt-4 pt-4 border-t border-gray-200">
            <div className="flex items-center justify-between text-sm text-gray-600">
              <div className="flex items-center gap-2">
                <Activity className="w-4 h-4" />
                <span>Auto-refresh every 60 seconds</span>
              </div>
              <span className="text-xs">
                Last checked: {health.database.lastChecked.toLocaleString()}
              </span>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
