'use client';

/**
 * Audit Log Timeline Component
 * Displays a visual timeline of all actions performed on a registration
 * Provides compliance tracking and accountability
 */

import {
  CheckCircle2,
  XCircle,
  Edit,
  Trash2,
  RefreshCw,
  User,
  Clock,
  AlertCircle,
  FileText,
  Shield,
} from 'lucide-react';
import type { AuditLog } from '@/types/admin';

interface AuditLogTimelineProps {
  logs: AuditLog[];
  showAll?: boolean;
  maxItems?: number;
}

export function AuditLogTimeline({ logs, showAll = false, maxItems = 10 }: AuditLogTimelineProps) {
  if (!logs || logs.length === 0) {
    return (
      <div className="bg-gray-50 rounded-lg border border-gray-200 p-8 text-center">
        <Clock className="w-12 h-12 text-gray-400 mx-auto mb-3" />
        <p className="text-gray-600 font-medium">No activity yet</p>
        <p className="text-sm text-gray-500 mt-1">
          Actions performed on this registration will appear here
        </p>
      </div>
    );
  }

  const displayLogs = showAll ? logs : logs.slice(0, maxItems);
  const hasMore = !showAll && logs.length > maxItems;

  const getActionIcon = (action: string) => {
    const actionLower = action.toLowerCase();
    if (actionLower.includes('approv')) {
      return <CheckCircle2 className="w-5 h-5" />;
    }
    if (actionLower.includes('reject')) {
      return <XCircle className="w-5 h-5" />;
    }
    if (actionLower.includes('update') || actionLower.includes('override')) {
      return <RefreshCw className="w-5 h-5" />;
    }
    if (actionLower.includes('delet')) {
      return <Trash2 className="w-5 h-5" />;
    }
    return <Edit className="w-5 h-5" />;
  };

  const getActionColor = (action: string, isAutomated: boolean) => {
    const actionLower = action.toLowerCase();

    if (actionLower.includes('approv')) {
      return {
        bg: 'bg-green-50',
        border: 'border-green-200',
        icon: 'text-green-600',
        dot: 'bg-green-500',
        line: 'border-green-200',
      };
    }
    if (actionLower.includes('reject')) {
      return {
        bg: 'bg-red-50',
        border: 'border-red-200',
        icon: 'text-red-600',
        dot: 'bg-red-500',
        line: 'border-red-200',
      };
    }
    if (isAutomated) {
      return {
        bg: 'bg-blue-50',
        border: 'border-blue-200',
        icon: 'text-blue-600',
        dot: 'bg-blue-500',
        line: 'border-blue-200',
      };
    }
    return {
      bg: 'bg-gray-50',
      border: 'border-gray-200',
      icon: 'text-gray-600',
      dot: 'bg-gray-500',
      line: 'border-gray-200',
    };
  };

  const formatActionName = (action: string) => {
    // Convert camelCase or PascalCase to readable format
    return action.replace(/([A-Z])/g, ' $1').trim();
  };

  return (
    <div className="space-y-4">
      {displayLogs.map((log, index) => {
        const colors = getActionColor(log.action, log.isAutomated);
        const isLastItem = index === displayLogs.length - 1;

        return (
          <div key={log.id} className="relative">
            {/* Timeline Line */}
            {!isLastItem && (
              <div
                className={`absolute left-5 top-12 w-0.5 h-full ${colors.line} border-l-2 border-dashed`}
                style={{ zIndex: 0 }}
              />
            )}

            {/* Log Entry */}
            <div className="relative flex gap-4">
              {/* Timeline Dot */}
              <div className="relative flex-shrink-0" style={{ zIndex: 1 }}>
                <div className={`w-10 h-10 rounded-full ${colors.bg} ${colors.border} border-2 flex items-center justify-center ${colors.icon}`}>
                  {getActionIcon(log.action)}
                </div>
                {log.isAutomated && (
                  <div className="absolute -bottom-1 -right-1 w-5 h-5 bg-blue-500 rounded-full flex items-center justify-center border-2 border-white">
                    <Shield className="w-3 h-3 text-white" />
                  </div>
                )}
              </div>

              {/* Log Content */}
              <div className={`flex-1 min-w-0 ${colors.bg} ${colors.border} border rounded-lg p-4 mb-2`}>
                {/* Header */}
                <div className="flex items-start justify-between mb-2">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-1 flex-wrap">
                      <h4 className="font-bold text-kq-dark">
                        {formatActionName(log.action)}
                      </h4>
                      {log.isAutomated && (
                        <span className="px-2 py-0.5 bg-blue-100 text-blue-700 text-xs font-medium rounded-full">
                          Automated
                        </span>
                      )}
                    </div>
                    <div className="flex items-center gap-3 text-sm text-gray-600 flex-wrap">
                      <div className="flex items-center gap-1">
                        <User className="w-4 h-4" />
                        <span className="font-medium">{log.performedBy}</span>
                      </div>
                      <div className="flex items-center gap-1">
                        <Clock className="w-4 h-4" />
                        <span>
                          {new Date(log.timestamp).toLocaleDateString('en-US', {
                            month: 'short',
                            day: 'numeric',
                            year: 'numeric',
                          })}
                          {' at '}
                          {new Date(log.timestamp).toLocaleTimeString('en-US', {
                            hour: '2-digit',
                            minute: '2-digit',
                          })}
                        </span>
                      </div>
                    </div>
                  </div>
                </div>

                {/* Status Change */}
                {(log.previousStatus || log.newStatus) && (
                  <div className="mb-3 p-3 bg-white bg-opacity-50 rounded border border-gray-200">
                    <div className="flex items-center gap-2 text-sm flex-wrap">
                      <span className="text-gray-600">Status changed:</span>
                      {log.previousStatus && (
                        <span className="px-2 py-1 bg-gray-100 text-gray-700 rounded font-medium">
                          {log.previousStatus}
                        </span>
                      )}
                      {log.previousStatus && log.newStatus && (
                        <span className="text-gray-400">→</span>
                      )}
                      {log.newStatus && (
                        <span className={`px-2 py-1 rounded font-medium ${
                          log.newStatus === 'Approved'
                            ? 'bg-green-100 text-green-700'
                            : log.newStatus === 'Rejected'
                            ? 'bg-red-100 text-red-700'
                            : 'bg-gray-100 text-gray-700'
                        }`}>
                          {log.newStatus}
                        </span>
                      )}
                    </div>
                  </div>
                )}

                {/* Rejection Reason */}
                {log.rejectionReason && (
                  <div className="mb-3 p-3 bg-red-50 bg-opacity-50 rounded border border-red-200">
                    <div className="flex items-start gap-2">
                      <AlertCircle className="w-4 h-4 text-red-600 mt-0.5 flex-shrink-0" />
                      <div className="flex-1 min-w-0">
                        <p className="text-xs font-medium text-red-900 uppercase tracking-wide mb-1">
                          Rejection Reason
                        </p>
                        <p className="text-sm text-red-800 break-words">
                          {log.rejectionReason}
                        </p>
                      </div>
                    </div>
                  </div>
                )}

                {/* Notes */}
                {log.notes && (
                  <div className="mb-2 p-3 bg-white bg-opacity-50 rounded border border-gray-200">
                    <div className="flex items-start gap-2">
                      <FileText className="w-4 h-4 text-gray-600 mt-0.5 flex-shrink-0" />
                      <div className="flex-1 min-w-0">
                        <p className="text-xs font-medium text-gray-600 uppercase tracking-wide mb-1">
                          Notes
                        </p>
                        <p className="text-sm text-gray-800 break-words">
                          {log.notes}
                        </p>
                      </div>
                    </div>
                  </div>
                )}

                {/* Admin User Details */}
                {log.adminUser && (
                  <div className="text-xs text-gray-500 flex items-center gap-3 mt-2 pt-2 border-t border-gray-200 flex-wrap">
                    <span>
                      <span className="font-medium">{log.adminUser.fullName}</span>
                      {' '}({log.adminUser.role})
                    </span>
                    {log.ipAddress && (
                      <span className="text-gray-400">
                        IP: {log.ipAddress}
                      </span>
                    )}
                  </div>
                )}
              </div>
            </div>
          </div>
        );
      })}

      {/* Show More Indicator */}
      {hasMore && (
        <div className="text-center py-3">
          <p className="text-sm text-gray-500">
            {logs.length - maxItems} more {logs.length - maxItems === 1 ? 'entry' : 'entries'} not shown
          </p>
        </div>
      )}
    </div>
  );
}
