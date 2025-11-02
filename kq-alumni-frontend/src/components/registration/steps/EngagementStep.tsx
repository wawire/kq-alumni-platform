"use client";

import React, { useState } from "react";
import { ArrowLeftIcon, CheckCircleIcon } from "@heroicons/react/24/solid";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { Button } from "@/components/ui";
import { ENGAGEMENT_AREAS } from "@/constants/forms";
import type { RegistrationFormData } from "../RegistrationForm";

const engagementSchema = z.object({
  engagementPreferences: z
    .array(z.string())
    .min(1, "Please select at least one area of interest"),
  consentGiven: z
    .boolean()
    .refine((val) => val === true, "You must give consent to register"),
});

type EngagementFormData = z.infer<typeof engagementSchema>;

interface Props {
  data: Partial<RegistrationFormData>;
  onSubmit: (data: Partial<RegistrationFormData>) => void;
  onBack: () => void;
  isSubmitting?: boolean;
}

const EngagementStep: React.FC<Props> = ({
  data,
  onSubmit,
  onBack,
  isSubmitting = false,
}) => {
  const [selectedEngagements, setSelectedEngagements] = useState<string[]>(
    data.engagementPreferences || [],
  );
  const [consentChecked, setConsentChecked] = useState<boolean>(
    data.consentGiven || false,
  );

  const {
    handleSubmit,
    formState: { errors },
    setValue,
  } = useForm<EngagementFormData>({
    resolver: zodResolver(engagementSchema),
    defaultValues: {
      engagementPreferences: data.engagementPreferences || [],
      consentGiven: data.consentGiven || false,
    },
  });

  const handleEngagementToggle = (value: string): void => {
    const newEngagements = selectedEngagements.includes(value)
      ? selectedEngagements.filter((e) => e !== value)
      : [...selectedEngagements, value];

    setSelectedEngagements(newEngagements);
    setValue("engagementPreferences", newEngagements);
  };

  const handleConsentChange = (
    e: React.ChangeEvent<HTMLInputElement>,
  ): void => {
    const checked = e.target.checked;
    setConsentChecked(checked);
    setValue("consentGiven", checked);
  };

  const onSubmitForm = (formData: EngagementFormData): void => {
    onSubmit(formData);
  };

  return (
    <form onSubmit={handleSubmit(onSubmitForm)} className="space-y-8">
      <div>
        <h2 className="text-3xl font-cabrito font-bold text-kq-dark mb-8">
          Alumni Engagement
        </h2>

        {/* Engagement Areas */}
        <div className="mb-8">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Areas of Interest <span className="text-kq-red">*</span>
          </label>
          <p className="text-sm text-gray-600 mb-4">
            Select all areas that interest you
          </p>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {ENGAGEMENT_AREAS.map((option) => (
              <label
                key={option.value}
                className="flex items-center gap-3 cursor-pointer hover:bg-gray-50 p-3 rounded-lg transition-colors border border-transparent hover:border-gray-200"
              >
                <input
                  type="checkbox"
                  value={option.value}
                  checked={selectedEngagements.includes(option.value)}
                  onChange={() => handleEngagementToggle(option.value)}
                  className="w-5 h-5 text-kq-red border-gray-300 rounded focus:ring-0 focus:ring-offset-0 cursor-pointer flex-shrink-0"
                />
                <span className="text-gray-900 text-sm leading-tight">
                  {option.label}
                </span>
              </label>
            ))}
          </div>
          {errors.engagementPreferences && (
            <p className="mt-2 text-sm text-kq-red">
              {errors.engagementPreferences.message}
            </p>
          )}
        </div>

        {/* Consent */}
        <h3 className="text-2xl font-cabrito font-bold text-kq-dark mt-12 mb-8">
          Consent & Verification
        </h3>

        <div className="mb-8">
          <label className="flex items-start gap-4 cursor-pointer p-4 bg-gray-50 rounded-lg hover:bg-gray-100 transition-all">
            <input
              type="checkbox"
              checked={consentChecked}
              onChange={handleConsentChange}
              className="w-6 h-6 text-kq-red border-gray-300 rounded focus:ring-0 focus:ring-offset-0 cursor-pointer mt-1 flex-shrink-0"
            />
            <div className="flex-1">
              <span className="text-gray-900 leading-relaxed">
                I hereby consent to Kenya Airways verifying my staff records and
                to the storage and processing of my personal data for alumni
                engagement, in accordance with the Data Protection Act.
              </span>
              <span className="text-kq-red ml-1">*</span>
            </div>
          </label>
          {errors.consentGiven && (
            <p className="mt-2 text-sm text-kq-red">
              {errors.consentGiven.message}
            </p>
          )}
        </div>

        <div className="bg-blue-50 border-l-4 border-blue-400 p-4 rounded-lg">
          <p className="text-sm text-gray-700 leading-relaxed">
            <strong>What happens next:</strong> Upon submission, your
            registration will be verified against our records. You will receive
            a confirmation email once your registration is approved. Welcome to
            the KQ Alumni family!
          </p>
        </div>
      </div>

      {/* Buttons */}
      <div className="flex gap-4 pt-8">
        <Button
          type="button"
          onClick={onBack}
          disabled={isSubmitting}
          variant="secondary"
          size="lg"
          className="flex-1"
          leftIcon={<ArrowLeftIcon className="w-5 h-5" />}
        >
          Back
        </Button>
        <Button
          type="submit"
          disabled={isSubmitting}
          variant="primary"
          size="lg"
          className="flex-1"
          isLoading={isSubmitting}
          loadingText="Submitting..."
          rightIcon={!isSubmitting ? <CheckCircleIcon className="w-5 h-5" /> : undefined}
        >
          Submit Registration
        </Button>
      </div>
    </form>
  );
};

export default EngagementStep;
