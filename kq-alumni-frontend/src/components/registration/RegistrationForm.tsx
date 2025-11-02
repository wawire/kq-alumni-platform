"use client";

import React, { useEffect } from "react";
import {
  BriefcaseIcon,
  HeartIcon,
  UserGroupIcon,
} from "@heroicons/react/24/outline";
import { toast, Toaster } from "sonner";

import { useSubmitRegistration } from "@/lib/api/services/registrationService";
import { useRegistrationFormData, useRegistrationActions, useRegistrationStatus, useRegistrationId, useCurrentStep, RegistrationStatus } from "@/store";
import EmploymentStep from "./steps/EmploymentStep";
import EngagementStep from "./steps/EngagementStep";
import PersonalInfoStep from "./steps/PersonalInfoStep";
import SuccessScreen from "./SuccessScreen";

export type FormStep = "personal" | "employment" | "engagement" | "success";

export interface RegistrationFormData {
  staffNumber: string;
  fullName: string;
  email: string;
  mobileCountryCode: string;
  mobileNumber: string;
  currentCountry: string;
  currentCountryCode: string;
  currentCity: string;
  cityCustom?: string;
  currentEmployer?: string;
  currentJobTitle?: string;
  industry?: string;
  linkedInProfile?: string;
  qualificationsAttained: string[];
  professionalCertifications?: string;
  engagementPreferences: string[];
  consentGiven: boolean;
}

export default function RegistrationForm() {
  // Zustand store
  const formData = useRegistrationFormData();
  const currentStep = useCurrentStep();
  const registrationId = useRegistrationId();
  const status = useRegistrationStatus();
  const { updateFormData, nextStep, previousStep, setRegistrationId, setStatus, loadFromLocalStorage } = useRegistrationActions();

  // React Query mutation
  const submitMutation = useSubmitRegistration();

  // Load persisted data on mount
  useEffect(() => {
    loadFromLocalStorage();
  }, [loadFromLocalStorage]);

  const handleNext = (data: Partial<RegistrationFormData>): void => {
    updateFormData(data);
    nextStep();
  };

  const handleBack = (): void => {
    previousStep();
  };

  const handleSubmit = async (
    data: Partial<RegistrationFormData>,
  ): Promise<void> => {
    const finalData = { ...formData, ...data } as RegistrationFormData;

    toast.loading("Submitting your registration...", {
      id: "registration-loading",
    });

    submitMutation.mutate(finalData, {
      onSuccess: (response) => {
        toast.dismiss("registration-loading");
        toast.success("Registration successful! 🎉", {
          description: "Welcome to the KQ Alumni family!",
          duration: 4000,
        });

        setRegistrationId(response.id);
        setStatus(RegistrationStatus.SUCCESS);
      },
      onError: (error: Error) => {
        toast.dismiss("registration-loading");

        toast.error(error.message || "Registration failed. Please try again.", {
          duration: 6000,
          style: { fontWeight: "600" },
        });
      },
    });
  };

  // Show success screen when status is success
  if (status === RegistrationStatus.SUCCESS) {
    return (
      <SuccessScreen
        registrationId={registrationId || ""}
        email={formData.email || ""}
        fullName={formData.fullName || ""}
      />
    );
  }

  return (
    <>
      <Toaster
        position="top-right"
        expand={false}
        richColors
        closeButton
        toastOptions={{
          style: {
            background: "white",
            border: "1px solid #e5e7eb",
            padding: "16px",
            fontSize: "14px",
          },
          className: "toast-custom",
          duration: 5000,
        }}
      />

      <div className="min-h-screen flex">
        {/* LEFT PANEL */}
        <div className="hidden lg:flex lg:w-2/5 bg-gradient-to-br from-navy-900 via-navy-800 to-navy-900 text-white relative overflow-hidden">
          <div className="absolute inset-0 opacity-10">
            <div
              className="absolute inset-0"
              style={{
                backgroundImage: `url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23ffffff' fill-opacity='0.4'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")`,
              }}
            />
          </div>

          <div className="relative z-10 flex flex-col justify-center p-12 w-full">
            <div className="space-y-8">
              <div>
                <h1 className="text-4xl font-cabrito font-bold mb-4">
                  KQ Alumni Association
                </h1>
                <p className="text-lg text-gray-300 font-roboto mb-8">
                  Join a network of exceptional professionals who have been part
                  of the Kenya Airways family.
                </p>
              </div>

              <div className="space-y-8">
                <FeatureItem
                  icon={<UserGroupIcon className="w-6 h-6 text-white" />}
                  title="Network & Connect"
                  description="Build meaningful relationships with fellow alumni"
                />
                <FeatureItem
                  icon={<BriefcaseIcon className="w-6 h-6 text-white" />}
                  title="Career Growth"
                  description="Access mentorship and job opportunities"
                />
                <FeatureItem
                  icon={<HeartIcon className="w-6 h-6 text-white" />}
                  title="Give Back"
                  description="Contribute through mentorship and volunteering"
                />
              </div>
            </div>

            <div className="absolute left-12 right-12 bottom-6">
              <p className="text-gray-400 text-sm font-roboto pr-11">
                Your information is protected under the Data Protection Act and
                will only be used for alumni association purposes.
              </p>
            </div>
          </div>
        </div>

        {/* RIGHT PANEL (form) */}
        <div className="flex-1 bg-gray-50 flex flex-col justify-center items-center min-h-screen">
          <div className="max-w-2xl w-full px-8 py-12">
            {currentStep === 0 && (
              <PersonalInfoStep data={formData} onNext={handleNext} />
            )}
            {currentStep === 1 && (
              <EmploymentStep
                data={formData}
                onNext={handleNext}
                onBack={handleBack}
              />
            )}
            {currentStep === 2 && (
              <EngagementStep
                data={formData}
                onSubmit={handleSubmit}
                onBack={handleBack}
                isSubmitting={submitMutation.isPending}
              />
            )}
          </div>
        </div>
      </div>
    </>
  );
}

interface FeatureItemProps {
  icon: React.ReactNode;
  title: string;
  description: string;
}

function FeatureItem({ icon, title, description }: FeatureItemProps) {
  return (
    <div className="flex items-start gap-4">
      <div className="flex-shrink-0 w-12 h-12 bg-white/10 rounded-lg flex items-center justify-center">
        {icon}
      </div>
      <div>
        <h3 className="font-cabrito font-semibold text-lg mb-1">{title}</h3>
        <p className="text-gray-400 text-sm font-roboto">{description}</p>
      </div>
    </div>
  );
}
