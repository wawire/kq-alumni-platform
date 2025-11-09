"use client";

import { useEffect, useMemo, useState } from "react";
import { ClockIcon } from "@heroicons/react/24/solid";
import { zodResolver } from "@hookform/resolvers/zod";
import { City, Country, ICity, ICountry } from "country-state-city";
import { FormProvider, useForm } from "react-hook-form";
import PhoneInput, {
  CountryData as PhoneCountryData,
} from "react-phone-input-2";
import "react-phone-input-2/lib/style.css";
import { SingleValue } from "react-select";
import { toast } from "sonner";
import { z } from "zod";

import { FormField, FormSelect } from "@/components/forms";
import { Button } from "@/components/ui";
import { useDebounce } from "@/hooks/useDebounce";
import { useDuplicateCheck } from "@/hooks/useDuplicateCheck";
import { env } from "@/lib/env";
import ProgressIndicator from "../ProgressIndicator";
import type { RegistrationFormData } from "../RegistrationForm";

// =====================================================
// VALIDATION SCHEMA
// Full Name is required and auto-populated from ERP verification
// Mobile Number is optional
// =====================================================
const personalInfoSchema = z.object({
  staffNumber: z
    .string()
    .optional()
    .transform((val) => val?.trim().toUpperCase() || undefined),
  idNumber: z
    .string()
    .min(1, "ID Number or Passport Number is required")
    .transform((val) => val?.trim().toUpperCase() || ""),
  passportNumber: z
    .string()
    .optional()
    .transform((val) => val?.trim().toUpperCase() || undefined),
  fullName: z
    .string()
    .min(1, "Full name is required")
    .transform((val) => val?.trim() || ""),
  email: z
    .string()
    .min(1, "Email address is required")
    .email("Invalid email format")
    .max(255, "Email address too long")
    .transform((val) => val.toLowerCase().trim()),
  mobileCountryCode: z.string().optional(),
  mobileNumber: z.string().optional(),
  currentCountry: z.string().min(1, "Country is required"),
  currentCountryCode: z.string().min(1, "Country code is required"),
  currentCity: z.string().min(1, "City is required"),
  cityCustom: z.string().optional(),
});

type PersonalInfoFormData = z.infer<typeof personalInfoSchema>;

// =====================================================
// TYPESCRIPT INTERFACES
// =====================================================
interface Props {
  data: Partial<RegistrationFormData>;
  onNext: (data: Partial<RegistrationFormData>) => void;
}

interface CountryOption {
  value: string;
  label: string;
  flag: string;
}

interface CityOption {
  value: string;
  label: string;
}

// =====================================================
// VERIFICATION STATE TYPES
// =====================================================
type VerificationStatus = 'idle' | 'verifying' | 'verified' | 'failed' | 'already_registered';

interface ErpVerificationData {
  staffNumber?: string;
  fullName?: string;
  department?: string;
  exitDate?: string;
}

// =====================================================
// MAIN COMPONENT
// =====================================================
export default function PersonalInfoStep({ data, onNext }: Props) {
  const [selectedCountryCode, setSelectedCountryCode] = useState<string>(
    data.currentCountryCode || "", // No default - user must select their location
  );
  const [phoneValue, setPhoneValue] = useState<string>(data.mobileNumber || "");

  // ID Verification State
  const [verificationStatus, setVerificationStatus] = useState<VerificationStatus>('idle');
  const [erpData, setErpData] = useState<ErpVerificationData | null>(null);
  const [verificationError, setVerificationError] = useState<string>("");
  const [allowManualMode, setAllowManualMode] = useState<boolean>(false); // Fallback mode when ERP is unavailable

  const methods = useForm<PersonalInfoFormData>({
    resolver: zodResolver(personalInfoSchema),
    defaultValues: {
      staffNumber: data.staffNumber || "",
      idNumber: data.idNumber || "",
      passportNumber: data.passportNumber || "",
      fullName: data.fullName || "",
      email: data.email || "",
      mobileCountryCode: data.mobileCountryCode || "",
      mobileNumber: data.mobileNumber || "",
      currentCountry: data.currentCountry || "",
      currentCountryCode: data.currentCountryCode || "",
      currentCity: data.currentCity || "",
      cityCustom: data.cityCustom || "",
    },
  });

  const {
    handleSubmit,
    setValue,
    watch,
    formState: { errors },
  } = methods;

  // Watch fields for duplicate checking and verification
  const idNumberValue = watch("idNumber");
  const emailValue = watch("email");
  const currentCountryCodeValue = watch("currentCountryCode");

  // Debounce values to avoid too many API calls
  const debouncedIdNumber = useDebounce(idNumberValue, 1000); // 1 second delay for ID verification
  const debouncedEmail = useDebounce(emailValue, 800);

  // Duplicate checking hooks
  const emailCheck = useDuplicateCheck("email");

  // Trigger duplicate checks when debounced values change
  useEffect(() => {
    if (debouncedEmail && debouncedEmail.length > 0) {
      emailCheck.checkValue(debouncedEmail);
    } else {
      emailCheck.reset();
    }
  }, [debouncedEmail]); // eslint-disable-line react-hooks/exhaustive-deps

  // Show toast when email duplicate is detected
  useEffect(() => {
    if (emailCheck.isDuplicate) {
      toast.error('Email Already Registered', {
        description: emailCheck.error || 'This email is already registered',
        duration: 5000,
      });
    }
  }, [emailCheck.isDuplicate, emailCheck.error]);

  // Sync selectedCountryCode with form value
  useEffect(() => {
    if (currentCountryCodeValue && currentCountryCodeValue !== selectedCountryCode) {
      setSelectedCountryCode(currentCountryCodeValue);
    }
  }, [currentCountryCodeValue, selectedCountryCode]);

  // =====================================================
  // ID VERIFICATION LOGIC
  // =====================================================
  const verifyIdWithERP = async (idOrPassport: string) => {
    if (!idOrPassport || idOrPassport.length < 5) {
      setVerificationStatus('idle');
      setErpData(null);
      setVerificationError("");
      return;
    }

    try {
      setVerificationStatus('verifying');
      setVerificationError("");

      const apiUrl = env.apiUrl;
      const url = `${apiUrl}/api/v1/registrations/verify-id/${encodeURIComponent(idOrPassport)}`;

      const response = await fetch(url);

      if (!response.ok) {
        throw new Error('Verification failed');
      }

      const result = await response.json();

      if (result.isAlreadyRegistered) {
        setVerificationStatus('already_registered');
        toast.error('Already Registered', {
          description: result.message || 'This ID/Passport is already registered',
          duration: 5000,
        });
        setErpData(null);
        return;
      }

      if (result.isVerified) {
        setVerificationStatus('verified');
        setErpData({
          staffNumber: result.staffNumber,
          fullName: result.fullName,
          department: result.department,
          exitDate: result.exitDate,
        });

        // Auto-populate fields from ERP
        if (result.fullName) {
          setValue("fullName", result.fullName, { shouldValidate: true });
        }
        if (result.staffNumber) {
          setValue("staffNumber", result.staffNumber, { shouldValidate: true });
        }

        setVerificationError("");
      } else {
        setVerificationStatus('failed');
        // Show toast instead of inline error to reduce form clutter
        toast.error('ID/Passport Not Found', {
          description: result.message || 'ID/Passport not found in our records. Please verify and contact HR if this error persists.',
          duration: 5000,
        });
        setErpData(null);
      }
    } catch (error) {
      setVerificationStatus('failed');
      // Show toast for system errors
      toast.error('Verification Error', {
        description: 'Unable to verify ID/Passport. This may be due to a system issue. You can continue with manual review.',
        duration: 5000,
      });
      setErpData(null);
    }
  };

  // Trigger verification when debounced ID changes
  useEffect(() => {
    if (debouncedIdNumber) {
      verifyIdWithERP(debouncedIdNumber);
    } else {
      setVerificationStatus('idle');
      setErpData(null);
      setVerificationError("");
    }
  }, [debouncedIdNumber]); // eslint-disable-line react-hooks/exhaustive-deps

  const countryOptions = useMemo((): CountryOption[] => {
    const allCountries = Country.getAllCountries();
    return allCountries.map((country: ICountry) => ({
      value: country.isoCode,
      label: country.name,
      flag: country.flag,
    }));
  }, []);

  const cityOptions = useMemo((): CityOption[] => {
    if (!selectedCountryCode) {
      return [];
    }
    const cities = City.getCitiesOfCountry(selectedCountryCode);
    if (!cities || cities.length === 0) {
      return [];
    }
    return cities.map((city: ICity) => ({
      value: city.name,
      label: city.name,
    }));
  }, [selectedCountryCode]);

  const handleCountryChange = (option: SingleValue<CountryOption>): void => {
    if (!option) {
      return;
    }

    const country = Country.getCountryByCode(option.value);
    if (country) {
      setSelectedCountryCode(country.isoCode);
      // FormSelect already sets currentCountryCode with validation
      // We just need to set the related fields
      setValue("currentCountry", country.name, { shouldValidate: true });
      setValue("currentCity", "", { shouldValidate: true });

      // Note: Mobile country code is NOT updated here
      // Phone number country code is independent from current location
    }
  };

  const handlePhoneChange = (
    value: string,
    country: PhoneCountryData,
  ): void => {
    setPhoneValue(value);

    // Remove country dial code from the phone number to avoid duplication
    // PhoneInput returns the full number (e.g., "254712345678")
    // We need to store just the local number (e.g., "712345678")
    const dialCode = country.dialCode || "";
    const localNumber = value.startsWith(dialCode) ? value.substring(dialCode.length) : value;

    setValue("mobileNumber", localNumber, { shouldValidate: true });
    setValue("mobileCountryCode", `+${dialCode}`, { shouldValidate: true });

    // Note: Current location is NOT updated here
    // You can live in Japan and have a Kenyan phone number
  };

  const onSubmit = (formData: PersonalInfoFormData): void => {
    // Don't submit if duplicates found
    if (emailCheck.isDuplicate) {
      return;
    }

    // Check verification status - allow manual mode as fallback
    if (verificationStatus !== 'verified' && !allowManualMode) {
      setVerificationError('Please wait for ID verification to complete or click "Continue with Manual Review"');
      return;
    }

    // v2.2.0 OPTIMIZATION: Include ERP validation data to skip redundant backend verification
    if (verificationStatus === 'verified' && erpData) {
      // AUTO PATH: ERP verified during registration
      onNext({
        ...formData,
        staffNumber: erpData.staffNumber || formData.staffNumber,
        fullName: erpData.fullName || formData.fullName,
        // Include ERP validation data (eliminates redundant backend ERP call)
        erpValidated: true,
        erpStaffName: erpData.fullName,
        erpDepartment: erpData.department,
        erpExitDate: erpData.exitDate,
      });
    } else if (allowManualMode) {
      // MANUAL PATH: ERP verification failed/unavailable
      onNext({
        ...formData,
        erpValidated: false,
        requiresManualReview: true,
        manualReviewReason: 'ERP verification unavailable - submitted via manual mode',
      });
    } else {
      // Fallback (shouldn't reach here due to validation checks above)
      setVerificationError('Unable to proceed. Please try again.');
    }
  };

  // Check if form has any validation errors
  const hasErrors = Object.keys(errors).length > 0;
  const canProceed = (verificationStatus === 'verified' || allowManualMode) && !emailCheck.isDuplicate && !hasErrors;

  return (
    <FormProvider {...methods}>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-8">
      <div>
        {/* Progress Indicator - Above Header */}
        <ProgressIndicator currentStep={1} totalSteps={4} />

        {/* Header */}
        <div className="mb-6">
          <h2 className="text-3xl font-cabrito font-bold text-kq-dark mb-2">
            Personal Information & Contact Information
          </h2>
          <div className="flex items-center gap-2 text-sm text-gray-600">
            <ClockIcon className="w-4 h-4" />
            <span>About 5 minutes</span>
          </div>
        </div>

        {/* Row 1: ID Number / Passport & Full Name - Side by Side */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
          {/* ID Number / Passport Number */}
          <div>
            <FormField
              name="idNumber"
              label="ID Number / Passport No"
              type="text"
              placeholder="e.g., 12345678 or A1234567"
              required
              variant="underline"
              onChange={(e) => {
                const cleaned = e.target.value.toUpperCase().replace(/[^A-Z0-9]/g, '');
                setValue("idNumber", cleaned, { shouldValidate: true });
              }}
              style={{ textTransform: "uppercase" }}
              className="uppercase"
            />
            {errors.idNumber && (
              <p className="mt-2 text-sm text-kq-red">
                {errors.idNumber.message}
              </p>
            )}
            {!errors.idNumber && verificationStatus === 'verifying' && (
              <p className="mt-2 text-sm text-blue-600 flex items-center gap-2">
                <span className="inline-block w-4 h-4 border-2 border-blue-600 border-t-transparent rounded-full animate-spin" />
                Verifying with our records...
              </p>
            )}
            {!errors.idNumber && verificationStatus === 'failed' && !allowManualMode && (
              <div className="mt-3 p-3 bg-yellow-50 border border-yellow-200 rounded-lg">
                <p className="text-sm text-yellow-800 mb-2">
                  <strong>Can't verify automatically?</strong> You can continue with manual review.
                  Our HR team will verify your information manually.
                </p>
                <button
                  type="button"
                  onClick={() => {
                    setAllowManualMode(true);
                    setVerificationError('');
                  }}
                  className="text-sm font-medium text-yellow-700 hover:text-yellow-900 underline"
                >
                  Continue with Manual Review
                </button>
              </div>
            )}
          </div>

          {/* Full Name */}
          <div>
            <FormField
              name="fullName"
              label="Full Name"
              type="text"
              placeholder={allowManualMode ? "Enter your full name" : "As per company records"}
              variant="underline"
              disabled={!allowManualMode}
              required
              className={allowManualMode ? "" : "bg-gray-50"}
            />
            {errors.fullName && (
              <p className="mt-2 text-sm text-kq-red">
                {errors.fullName.message}
              </p>
            )}
          </div>
        </div>

        {/* Row 2: Staff Number & Email - Side by Side */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
          {/* Staff Number */}
          <div>
            <FormField
              name="staffNumber"
              label="Staff Number"
              type="text"
              placeholder={allowManualMode ? "e.g., 0012345 (if known)" : "e.g., 0012345"}
              variant="underline"
              disabled={!allowManualMode}
              className={allowManualMode ? "" : "bg-gray-50"}
            />
          </div>

          {/* Email */}
          <div>
            <FormField
              name="email"
              label="Email Address"
              type="email"
              placeholder="your.email@example.com"
              required
              variant="underline"
            />
            {errors.email && (
              <p className="mt-2 text-sm text-kq-red">
                {errors.email.message}
              </p>
            )}
          </div>
        </div>

        {/* Mobile Number */}
        <div className="mb-8">
          <label className="block text-sm font-medium text-gray-700 mb-1.5">
            Mobile Number <span className="text-gray-500 font-normal">(Optional)</span>
          </label>
          <PhoneInput
            country={""}
            value={phoneValue}
            onChange={handlePhoneChange}
            inputStyle={{
              width: "100%",
              border: 0,
              borderBottom: "2px solid #d1d5db",
              borderRadius: 0,
              padding: "12px 4px 12px 48px",
              fontSize: "16px",
              backgroundColor: "transparent",
              color: "#111827",
            }}
            buttonStyle={{
              border: 0,
              borderBottom: "2px solid #d1d5db",
              borderRadius: 0,
              backgroundColor: "transparent",
            }}
            dropdownStyle={{
              borderRadius: "8px",
              boxShadow: "0 4px 6px -1px rgba(0, 0, 0, 0.1)",
            }}
            containerClass="phone-input-container"
            enableSearch
            searchStyle={{
              width: "90%",
              padding: "8px",
              border: "1px solid #d1d5db",
              borderRadius: "4px",
            }}
            searchPlaceholder="Search country"
          />
        </div>

        {/* Country & City */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
          <FormSelect<CountryOption>
            name="currentCountryCode"
            label="Country"
            options={countryOptions}
            placeholder="Select country"
            required
            isSearchable
            onChange={(option) => handleCountryChange(option)}
            formatOptionLabel={(option: CountryOption) => (
              <div className="flex items-center gap-2">
                <span className="text-xl">{option.flag}</span>
                <span>{option.label}</span>
              </div>
            )}
          />

          <FormSelect<CityOption>
            name="currentCity"
            label="City"
            options={cityOptions}
            placeholder={
              selectedCountryCode ? "Select city" : "Select country first"
            }
            required
            isSearchable
            isClearable
            isDisabled={!selectedCountryCode || cityOptions.length === 0}
            noOptionsMessage={() => "No cities available"}
          />
        </div>
      </div>

      <div className="pt-8">
        <Button
          type="submit"
          variant="primary"
          size="lg"
          fullWidth
          disabled={!canProceed}
        >
          {verificationStatus === 'idle' && 'Enter ID Number to Start'}
          {verificationStatus === 'verifying' && (
            <span className="flex items-center justify-center gap-2">
              <span className="inline-block w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
              Verifying ID...
            </span>
          )}
          {verificationStatus === 'failed' && !allowManualMode && 'Enable Manual Review to Continue'}
          {verificationStatus === 'already_registered' && 'Already Registered'}
          {verificationStatus === 'verified' && emailCheck.isDuplicate && 'Email Already Used'}
          {((verificationStatus === 'verified' || allowManualMode) && !emailCheck.isDuplicate && hasErrors) && 'Please Complete All Required Fields'}
          {canProceed && (allowManualMode ? 'Continue with Manual Review' : 'Continue')}
        </Button>
        {verificationStatus === 'verified' && hasErrors && (
          <p className="mt-3 text-sm text-center text-gray-600">
            Please fill in all required fields above to continue
          </p>
        )}
      </div>
    </form>
    </FormProvider>
  );
}
