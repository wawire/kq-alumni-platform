"use client";

import React from "react";
import { CheckCircleIcon, PencilIcon } from "@heroicons/react/24/outline";
import { RegistrationFormData } from "@/components/registration/RegistrationForm";

interface ReviewStepProps {
  data: Partial<RegistrationFormData>;
  onSubmit: (data: Partial<RegistrationFormData>) => Promise<void>;
  onEdit: (step: number) => void;
  isSubmitting: boolean;
}

export default function ReviewStep({
  data,
  onSubmit,
  onEdit,
  isSubmitting,
}: ReviewStepProps) {
  const handleSubmit = async () => {
    await onSubmit(data);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="text-center mb-8">
        <h2 className="text-3xl font-cabrito font-bold text-navy-900 mb-2">
          Review Your Information
        </h2>
        <p className="text-gray-600 font-roboto">
          Please verify all details before submitting your registration
        </p>
      </div>

      {/* Personal Information Section */}
      <ReviewSection
        title="Personal Information"
        onEdit={() => onEdit(0)}
        items={[
          { label: "Staff Number", value: data.staffNumber },
          { label: "Full Name", value: data.fullName },
          { label: "Email", value: data.email },
          {
            label: "Mobile Number",
            value: data.mobileNumber
              ? `${data.mobileCountryCode} ${data.mobileNumber}`
              : undefined,
          },
          {
            label: "ID/Passport Number",
            value: data.idNumber || data.passportNumber,
          },
          {
            label: "Current Location",
            value: data.currentCity && data.currentCountry
              ? `${data.currentCity}, ${data.currentCountry}`
              : undefined,
          },
        ]}
      />

      {/* Employment Section */}
      <ReviewSection
        title="Employment & Professional Information"
        onEdit={() => onEdit(1)}
        items={[
          { label: "Current Employer", value: data.currentEmployer },
          { label: "Current Job Title", value: data.currentJobTitle },
          { label: "Industry", value: data.industry },
          { label: "LinkedIn Profile", value: data.linkedInProfile },
          {
            label: "Qualifications",
            value: data.qualificationsAttained?.join(", "),
          },
          {
            label: "Professional Certifications",
            value: data.professionalCertifications,
          },
        ]}
      />

      {/* Engagement Section */}
      <ReviewSection
        title="Engagement Preferences"
        onEdit={() => onEdit(2)}
        items={[
          {
            label: "Areas of Interest",
            value: data.engagementPreferences?.join(", "),
          },
          {
            label: "Consent",
            value: data.consentGiven ? "✓ Agreed to terms and conditions" : "✗ Not agreed",
          },
        ]}
      />

      {/* Consent Reminder */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 flex items-start gap-3">
        <CheckCircleIcon className="w-5 h-5 text-blue-600 flex-shrink-0 mt-0.5" />
        <div className="text-sm text-blue-900">
          <p className="font-medium mb-1">Data Protection Notice</p>
          <p className="text-blue-700">
            Your information will be processed in accordance with the Data
            Protection Act and used exclusively for KQ Alumni Association purposes.
          </p>
        </div>
      </div>

      {/* Action Buttons */}
      <div className="flex gap-4 pt-6">
        <button
          type="button"
          onClick={() => onEdit(2)}
          disabled={isSubmitting}
          className="flex-1 px-6 py-3 border-2 border-navy-800 text-navy-800 rounded-lg font-roboto font-medium hover:bg-navy-50 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
        >
          Back to Edit
        </button>
        <button
          type="button"
          onClick={handleSubmit}
          disabled={isSubmitting || !data.consentGiven}
          className="flex-1 px-6 py-3 bg-navy-800 text-white rounded-lg font-roboto font-medium hover:bg-navy-900 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
        >
          {isSubmitting ? (
            <>
              <svg
                className="animate-spin h-5 w-5 text-white"
                xmlns="http://www.w3.org/2000/svg"
                fill="none"
                viewBox="0 0 24 24"
              >
                <circle
                  className="opacity-25"
                  cx="12"
                  cy="12"
                  r="10"
                  stroke="currentColor"
                  strokeWidth="4"
                />
                <path
                  className="opacity-75"
                  fill="currentColor"
                  d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                />
              </svg>
              Submitting...
            </>
          ) : (
            "Submit Registration"
          )}
        </button>
      </div>
    </div>
  );
}

interface ReviewSectionProps {
  title: string;
  items: Array<{ label: string; value?: string }>;
  onEdit: () => void;
}

function ReviewSection({ title, items, onEdit }: ReviewSectionProps) {
  return (
    <div className="bg-white border border-gray-200 rounded-lg p-6">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-cabrito font-semibold text-navy-900">
          {title}
        </h3>
        <button
          type="button"
          onClick={onEdit}
          className="flex items-center gap-2 text-sm text-navy-600 hover:text-navy-800 font-roboto font-medium transition-colors"
          aria-label={`Edit ${title}`}
        >
          <PencilIcon className="w-4 h-4" />
          Edit
        </button>
      </div>

      <div className="space-y-3">
        {items.map((item, index) => (
          item.value && (
            <div key={index} className="flex flex-col sm:flex-row sm:items-start gap-1 sm:gap-4">
              <dt className="font-roboto font-medium text-sm text-gray-600 sm:w-1/3">
                {item.label}
              </dt>
              <dd className="font-roboto text-sm text-gray-900 sm:w-2/3 break-words">
                {item.value}
              </dd>
            </div>
          )
        ))}
      </div>
    </div>
  );
}
