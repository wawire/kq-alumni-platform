"use client";

import { useState } from "react";
import { ArrowLeftIcon, ArrowRightIcon } from "@heroicons/react/24/solid";
import { zodResolver } from "@hookform/resolvers/zod";
import { FormProvider, useForm } from "react-hook-form";
import { z } from "zod";

import { FormField, FormSelect, FormTextarea } from "@/components/forms";
import { Button } from "@/components/ui";
import { QUALIFICATIONS, INDUSTRIES, type IndustryOption } from "@/constants/forms";
import type { RegistrationFormData } from "../RegistrationForm";

const employmentSchema = z.object({
  currentEmployer: z
    .string()
    .max(200, "Employer name too long (max 200 characters)")
    .optional(),
  currentJobTitle: z
    .string()
    .max(200, "Job title too long (max 200 characters)")
    .optional(),
  industry: z
    .string()
    .max(100, "Industry name too long (max 100 characters)")
    .optional(),
  linkedInProfile: z
    .string()
    .max(500, "LinkedIn URL too long (max 500 characters)")
    .regex(
      /^$|^https?:\/\/(www\.)?linkedin\.com\/.*$/,
      "Please provide a valid LinkedIn profile URL",
    )
    .optional()
    .or(z.literal("")),
  qualificationsAttained: z
    .array(z.string())
    .min(1, "Please select at least one qualification"),
  professionalCertifications: z
    .string()
    .max(1000, "Text too long (max 1000 characters)")
    .optional(),
});

type EmploymentFormData = z.infer<typeof employmentSchema>;

interface Props {
  data: Partial<RegistrationFormData>;
  onNext: (data: Partial<RegistrationFormData>) => void;
  onBack: () => void;
}

export default function EmploymentStep({ data, onNext, onBack }: Props) {
  const [selectedQualifications, setSelectedQualifications] = useState<
    string[]
  >(data.qualificationsAttained || []);

  const methods = useForm<EmploymentFormData>({
    resolver: zodResolver(employmentSchema),
    defaultValues: {
      currentEmployer: data.currentEmployer || "",
      currentJobTitle: data.currentJobTitle || "",
      industry: data.industry || "",
      linkedInProfile: data.linkedInProfile || "",
      qualificationsAttained: data.qualificationsAttained || [],
      professionalCertifications: data.professionalCertifications || "",
    },
  });

  const {
    handleSubmit,
    formState: { errors },
    setValue,
  } = methods;

  const handleQualificationToggle = (value: string): void => {
    const newQualifications = selectedQualifications.includes(value)
      ? selectedQualifications.filter((q) => q !== value)
      : [...selectedQualifications, value];

    setSelectedQualifications(newQualifications);
    setValue("qualificationsAttained", newQualifications);
  };

  const onSubmit = (formData: EmploymentFormData): void => {
    onNext(formData);
  };

  return (
    <FormProvider {...methods}>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-8">
      <div>
        <h2 className="text-3xl font-cabrito font-bold text-kq-dark mb-8">
          Employment Information
        </h2>

        {/* Employer and Job Title */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
          <FormField
            name="currentEmployer"
            label="Current Employer"
            type="text"
            placeholder="e.g., Acme Corporation"
            variant="underline"
          />

          <FormField
            name="currentJobTitle"
            label="Current Job Title"
            type="text"
            placeholder="e.g., Senior Operations Manager"
            variant="underline"
          />
        </div>

        {/* Industry Field */}
        <div className="mb-8">
          <FormSelect<IndustryOption>
            name="industry"
            label="Industry / Field of Work"
            options={INDUSTRIES}
            placeholder="Select or type your industry"
            isSearchable
            isClearable
            isCreatable
            formatCreateLabel={(inputValue: string) => `Add "${inputValue}"`}
          />
        </div>

        {/* LinkedIn */}
        <div className="mb-8">
          <FormField
            name="linkedInProfile"
            label="LinkedIn Profile"
            type="url"
            placeholder="https://www.linkedin.com/in/yourprofile"
            variant="underline"
          />
        </div>

        {/* Education */}
        <h3 className="text-2xl font-cabrito font-bold text-kq-dark mt-12 mb-8">
          Education & Professional Development
        </h3>

        {/* Qualifications */}
        <div className="mb-8">
          <label className="block text-sm font-medium text-gray-700 mb-4">
            Qualifications Attained <span className="text-kq-red">*</span>
          </label>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
            {QUALIFICATIONS.map((qual) => (
              <label
                key={qual.value}
                className="flex items-center gap-3 cursor-pointer hover:bg-gray-50 p-3 rounded transition-colors"
              >
                <input
                  type="checkbox"
                  value={qual.value}
                  checked={selectedQualifications.includes(qual.value)}
                  onChange={() => handleQualificationToggle(qual.value)}
                  className="w-5 h-5 text-kq-red border-gray-300 rounded focus:ring-0 focus:ring-offset-0 cursor-pointer"
                />
                <span className="text-gray-900 text-sm">{qual.label}</span>
              </label>
            ))}
          </div>
          {errors.qualificationsAttained && (
            <p className="mt-2 text-sm text-kq-red">
              {errors.qualificationsAttained.message}
            </p>
          )}
        </div>

        {/* Certifications */}
        <div className="mb-8">
          <FormTextarea
            name="professionalCertifications"
            label="Professional Certifications"
            placeholder="e.g., PMP, CPA, ACCA, AWS Certified, Six Sigma"
            rows={4}
            maxLength={1000}
            showCounter={true}
          />
        </div>
      </div>

      {/* Navigation */}
      <div className="flex gap-4 pt-8">
        <Button
          type="button"
          onClick={onBack}
          variant="secondary"
          size="lg"
          className="flex-1"
          leftIcon={<ArrowLeftIcon className="w-5 h-5" />}
        >
          Back
        </Button>
        <Button
          type="submit"
          variant="primary"
          size="lg"
          className="flex-1"
          rightIcon={<ArrowRightIcon className="w-5 h-5" />}
        >
          Next Step
        </Button>
      </div>
    </form>
    </FormProvider>
  );
}
