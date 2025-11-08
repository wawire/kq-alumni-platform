'use client';

/**
 * Email Template Management Page (SuperAdmin only)
 * Manage email templates with live preview and variable substitution
 */

import { useState } from 'react';
import {
  Mail,
  Edit2,
  Eye,
  Plus,
  Save,
  X,
  Send,
  Copy,
  CheckCircle2,
  AlertCircle,
  FileText,
} from 'lucide-react';
import { AdminLayout } from '@/components/admin/AdminLayout';
import { Button } from '@/components/ui/button/Button';
import {
  useEmailTemplates,
  useUpdateEmailTemplate,
  usePreviewEmailTemplate,
  type EmailTemplate,
  type UpdateTemplateRequest,
} from '@/lib/api/services/emailTemplateService';
import { toast } from 'sonner';

// ============================================
// Variable Helper Component
// ============================================

interface VariableHelperProps {
  variables: string[];
  onInsert: (variable: string) => void;
}

function VariableHelper({ variables, onInsert }: VariableHelperProps) {
  const [copiedVar, setCopiedVar] = useState<string | null>(null);

  const handleCopy = (variable: string) => {
    const varText = `{{${variable}}}`;
    navigator.clipboard.writeText(varText);
    setCopiedVar(variable);
    toast.success(`Copied {{${variable}}} to clipboard`);
    setTimeout(() => setCopiedVar(null), 2000);
  };

  return (
    <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
      <h4 className="text-sm font-semibold text-blue-900 mb-3 flex items-center gap-2">
        <FileText className="w-4 h-4" />
        Available Variables
      </h4>
      <div className="flex flex-wrap gap-2">
        {variables.map((variable) => (
          <button
            key={variable}
            onClick={() => handleCopy(variable)}
            className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-white border border-blue-300 rounded-md text-sm font-mono text-blue-700 hover:bg-blue-100 hover:border-blue-400 transition-colors"
          >
            {copiedVar === variable ? (
              <CheckCircle2 className="w-3.5 h-3.5 text-green-600" />
            ) : (
              <Copy className="w-3.5 h-3.5" />
            )}
            {`{{${variable}}}`}
          </button>
        ))}
      </div>
      <p className="text-xs text-blue-600 mt-3">
        Click to copy. Variables will be replaced with actual values when emails are sent.
      </p>
    </div>
  );
}

// ============================================
// Template Editor Modal Component
// ============================================

interface TemplateEditorModalProps {
  template: EmailTemplate;
  onClose: () => void;
  onSave: () => void;
}

function TemplateEditorModal({ template, onClose, onSave }: TemplateEditorModalProps) {
  const [subject, setSubject] = useState(template.subject);
  const [htmlBody, setHtmlBody] = useState(template.htmlBody);
  const [description, setDescription] = useState(template.description || '');
  const [isActive, setIsActive] = useState(template.isActive);
  const [showPreview, setShowPreview] = useState(false);
  const [previewHtml, setPreviewHtml] = useState('');
  const [previewSubject, setPreviewSubject] = useState('');

  const updateMutation = useUpdateEmailTemplate();
  const previewMutation = usePreviewEmailTemplate();

  // Parse available variables from the template
  const availableVariables = template.availableVariables
    ? template.availableVariables.split(',').map((v) => v.trim().replace(/[{}]/g, ''))
    : [];

  // Sample data for preview
  const getSampleVariables = (): Record<string, string> => {
    const samples: Record<string, string> = {
      alumniName: 'John Doe',
      registrationId: '123e4567-e89b-12d3-a456-426614174000',
      registrationNumber: 'KQA-2025-ABCDE',
      currentDate: new Date().toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
      }),
      staffNumber: '12345',
      verificationLink: 'https://alumni.kenya-airways.com/verify?token=sample-token',
      rejectionReason: 'Unable to verify employment records',
    };
    return samples;
  };

  const handlePreview = async () => {
    try {
      const result = await previewMutation.mutateAsync({
        subject,
        htmlBody,
        variables: getSampleVariables(),
      });
      setPreviewSubject(result.subject);
      setPreviewHtml(result.htmlBody);
      setShowPreview(true);
    } catch (error) {
      toast.error('Failed to generate preview');
    }
  };

  const handleSave = async () => {
    if (!subject.trim() || !htmlBody.trim()) {
      toast.error('Subject and HTML body are required');
      return;
    }

    try {
      const updateData: UpdateTemplateRequest = {
        name: template.name,
        description,
        subject,
        htmlBody,
        availableVariables: template.availableVariables,
        isActive,
        updatedBy: 'Admin', // This should come from auth context
      };

      await updateMutation.mutateAsync({
        id: template.id,
        data: updateData,
      });

      toast.success('Template updated successfully');
      onSave();
      onClose();
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to update template');
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-7xl max-h-[90vh] overflow-hidden flex flex-col">
        {/* Header */}
        <div className="px-6 py-4 border-b border-gray-200 flex items-center justify-between">
          <div>
            <h2 className="text-xl font-semibold text-gray-900">
              Edit Template: {template.name}
            </h2>
            <p className="text-sm text-gray-500 mt-1">
              Template Key: <span className="font-mono">{template.templateKey}</span>
              {template.isSystemDefault && (
                <span className="ml-2 px-2 py-0.5 bg-purple-100 text-purple-700 text-xs rounded-full">
                  System Default
                </span>
              )}
            </p>
          </div>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition-colors"
          >
            <X className="w-6 h-6" />
          </button>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Editor Panel */}
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Description (Optional)
                </label>
                <input
                  type="text"
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-kq-red focus:border-transparent"
                  placeholder="Brief description of this template"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Email Subject *
                </label>
                <input
                  type="text"
                  value={subject}
                  onChange={(e) => setSubject(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-kq-red focus:border-transparent font-mono text-sm"
                  placeholder="Email subject with {{variables}}"
                />
              </div>

              <VariableHelper
                variables={availableVariables}
                onInsert={(variable) => {
                  // Insert at cursor position in the active field
                  // For simplicity, just copy to clipboard
                }}
              />

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  HTML Body *
                </label>
                <textarea
                  value={htmlBody}
                  onChange={(e) => setHtmlBody(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-kq-red focus:border-transparent font-mono text-sm"
                  rows={20}
                  placeholder="HTML email body with {{variables}}"
                />
              </div>

              <div className="flex items-center gap-2">
                <input
                  type="checkbox"
                  id="isActive"
                  checked={isActive}
                  onChange={(e) => setIsActive(e.target.checked)}
                  className="w-4 h-4 text-kq-red border-gray-300 rounded focus:ring-kq-red"
                />
                <label htmlFor="isActive" className="text-sm text-gray-700">
                  Active (template will be used for emails)
                </label>
              </div>
            </div>

            {/* Preview Panel */}
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <h3 className="text-lg font-semibold text-gray-900">Live Preview</h3>
                <Button
                  onClick={handlePreview}
                  disabled={previewMutation.isPending}
                  variant="secondary"
                  size="sm"
                >
                  <Eye className="w-4 h-4 mr-2" />
                  {previewMutation.isPending ? 'Generating...' : 'Generate Preview'}
                </Button>
              </div>

              {showPreview ? (
                <div className="border border-gray-300 rounded-lg overflow-hidden">
                  {/* Preview Subject */}
                  <div className="bg-gray-100 px-4 py-3 border-b border-gray-300">
                    <p className="text-xs text-gray-500 mb-1">Subject:</p>
                    <p className="text-sm font-medium text-gray-900">{previewSubject}</p>
                  </div>

                  {/* Preview Body */}
                  <div className="p-4 bg-white max-h-[600px] overflow-y-auto">
                    <div dangerouslySetInnerHTML={{ __html: previewHtml }} />
                  </div>
                </div>
              ) : (
                <div className="border-2 border-dashed border-gray-300 rounded-lg p-8 text-center">
                  <Eye className="w-12 h-12 text-gray-400 mx-auto mb-3" />
                  <p className="text-gray-500">
                    Click &quot;Generate Preview&quot; to see how the email will look
                  </p>
                  <p className="text-sm text-gray-400 mt-2">
                    Preview uses sample data for all variables
                  </p>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="px-6 py-4 border-t border-gray-200 flex items-center justify-between bg-gray-50">
          <div className="flex items-center gap-2 text-sm text-gray-600">
            <AlertCircle className="w-4 h-4" />
            <span>Changes will affect all future emails sent with this template</span>
          </div>
          <div className="flex items-center gap-3">
            <Button onClick={onClose} variant="secondary">
              Cancel
            </Button>
            <Button
              onClick={handleSave}
              disabled={updateMutation.isPending}
              variant="primary"
            >
              <Save className="w-4 h-4 mr-2" />
              {updateMutation.isPending ? 'Saving...' : 'Save Template'}
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}

// ============================================
// Template Card Component
// ============================================

interface TemplateCardProps {
  template: EmailTemplate;
  onEdit: (template: EmailTemplate) => void;
}

function TemplateCard({ template, onEdit }: TemplateCardProps) {
  return (
    <div className="bg-white border border-gray-200 rounded-lg p-6 hover:shadow-md transition-shadow">
      <div className="flex items-start justify-between mb-4">
        <div className="flex-1">
          <div className="flex items-center gap-3 mb-2">
            <h3 className="text-lg font-semibold text-gray-900">{template.name}</h3>
            {template.isSystemDefault && (
              <span className="px-2.5 py-0.5 bg-purple-100 text-purple-700 text-xs font-medium rounded-full">
                System Default
              </span>
            )}
            {template.isActive ? (
              <span className="px-2.5 py-0.5 bg-green-100 text-green-700 text-xs font-medium rounded-full flex items-center gap-1">
                <CheckCircle2 className="w-3 h-3" />
                Active
              </span>
            ) : (
              <span className="px-2.5 py-0.5 bg-gray-100 text-gray-700 text-xs font-medium rounded-full">
                Inactive
              </span>
            )}
          </div>
          <p className="text-sm font-mono text-gray-500 mb-2">{template.templateKey}</p>
          {template.description && (
            <p className="text-sm text-gray-600 mb-3">{template.description}</p>
          )}
        </div>
      </div>

      <div className="space-y-3 mb-4">
        <div>
          <p className="text-xs text-gray-500 mb-1">Subject:</p>
          <p className="text-sm text-gray-900 font-medium">{template.subject}</p>
        </div>

        {template.availableVariables && (
          <div>
            <p className="text-xs text-gray-500 mb-1">Available Variables:</p>
            <div className="flex flex-wrap gap-1">
              {template.availableVariables.split(',').map((variable, index) => (
                <span
                  key={index}
                  className="px-2 py-0.5 bg-blue-50 text-blue-700 text-xs font-mono rounded"
                >
                  {variable.trim()}
                </span>
              ))}
            </div>
          </div>
        )}
      </div>

      <div className="flex items-center justify-between pt-4 border-t border-gray-200">
        <div className="text-xs text-gray-500">
          Updated: {new Date(template.updatedAt).toLocaleDateString()}
          {template.updatedBy && ` by ${template.updatedBy}`}
        </div>
        <Button onClick={() => onEdit(template)} variant="secondary" size="sm">
          <Edit2 className="w-4 h-4 mr-2" />
          Edit Template
        </Button>
      </div>
    </div>
  );
}

// ============================================
// Main Page Component
// ============================================

export default function EmailTemplatesPage() {
  const [editingTemplate, setEditingTemplate] = useState<EmailTemplate | null>(null);
  const [showActiveOnly, setShowActiveOnly] = useState(false);

  const { data: templates, isLoading, error, refetch } = useEmailTemplates(showActiveOnly);

  const handleEdit = (template: EmailTemplate) => {
    setEditingTemplate(template);
  };

  const handleCloseEditor = () => {
    setEditingTemplate(null);
  };

  const handleSaveComplete = () => {
    refetch();
  };

  return (
    <AdminLayout>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-3">
              <Mail className="w-7 h-7 text-kq-red" />
              Email Templates
            </h1>
            <p className="text-gray-600 mt-1">
              Manage and customize email templates sent to alumni
            </p>
          </div>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="bg-white border border-gray-200 rounded-lg p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Total Templates</p>
                <p className="text-3xl font-bold text-gray-900 mt-1">
                  {templates?.length || 0}
                </p>
              </div>
              <FileText className="w-10 h-10 text-blue-500" />
            </div>
          </div>

          <div className="bg-white border border-gray-200 rounded-lg p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Active Templates</p>
                <p className="text-3xl font-bold text-green-600 mt-1">
                  {templates?.filter((t) => t.isActive).length || 0}
                </p>
              </div>
              <CheckCircle2 className="w-10 h-10 text-green-500" />
            </div>
          </div>

          <div className="bg-white border border-gray-200 rounded-lg p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">System Defaults</p>
                <p className="text-3xl font-bold text-purple-600 mt-1">
                  {templates?.filter((t) => t.isSystemDefault).length || 0}
                </p>
              </div>
              <Mail className="w-10 h-10 text-purple-500" />
            </div>
          </div>
        </div>

        {/* Filters */}
        <div className="bg-white border border-gray-200 rounded-lg p-4">
          <div className="flex items-center gap-3">
            <input
              type="checkbox"
              id="showActiveOnly"
              checked={showActiveOnly}
              onChange={(e) => setShowActiveOnly(e.target.checked)}
              className="w-4 h-4 text-kq-red border-gray-300 rounded focus:ring-kq-red"
            />
            <label htmlFor="showActiveOnly" className="text-sm text-gray-700">
              Show active templates only
            </label>
          </div>
        </div>

        {/* Loading State */}
        {isLoading && (
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-kq-red mx-auto"></div>
            <p className="text-gray-600 mt-4">Loading templates...</p>
          </div>
        )}

        {/* Error State */}
        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-6 text-center">
            <AlertCircle className="w-12 h-12 text-red-500 mx-auto mb-3" />
            <p className="text-red-700 font-medium">Failed to load email templates</p>
            <p className="text-red-600 text-sm mt-1">
              {(error as any)?.response?.data?.message || 'Please try again later'}
            </p>
            <Button onClick={() => refetch()} variant="secondary" size="sm" className="mt-4">
              Retry
            </Button>
          </div>
        )}

        {/* Templates Grid */}
        {!isLoading && !error && templates && (
          <>
            {templates.length === 0 ? (
              <div className="bg-white border-2 border-dashed border-gray-300 rounded-lg p-12 text-center">
                <Mail className="w-16 h-16 text-gray-400 mx-auto mb-4" />
                <p className="text-gray-600 font-medium">No email templates found</p>
                <p className="text-gray-500 text-sm mt-1">
                  {showActiveOnly
                    ? 'Try disabling the active filter'
                    : 'Email templates will appear here'}
                </p>
              </div>
            ) : (
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {templates.map((template) => (
                  <TemplateCard
                    key={template.id}
                    template={template}
                    onEdit={handleEdit}
                  />
                ))}
              </div>
            )}
          </>
        )}
      </div>

      {/* Editor Modal */}
      {editingTemplate && (
        <TemplateEditorModal
          template={editingTemplate}
          onClose={handleCloseEditor}
          onSave={handleSaveComplete}
        />
      )}
    </AdminLayout>
  );
}
