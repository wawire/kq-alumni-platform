"use client";

import { useEffect, useMemo, useState } from "react";
import { ArrowRightIcon, ExclamationCircleIcon, CheckCircleIcon, ClockIcon } from "@heroicons/react/24/solid";
import { zodResolver } from "@hookform/resolvers/zod";
import { City, Country, ICity, ICountry } from "country-state-city";
import { FormProvider, useForm } from "react-hook-form";
import PhoneInput, {
  CountryData as PhoneCountryData,
} from "react-phone-input-2";
import "react-phone-input-2/lib/style.css";
import { SingleValue } from "react-select";
import { z } from "zod";

import { FormField, FormSelect } from "@/components/forms";
import { Button } from "@/components/ui";
import { useDebounce } from "@/hooks/useDebounce";
import { useDuplicateCheck } from "@/hooks/useDuplicateCheck";
import ProgressIndicator from "../ProgressIndicator";
import type { RegistrationFormData } from "../RegistrationForm";

// =====================================================
// VALIDATION SCHEMA - CONDITIONAL VALIDATION
// Full Name only required after ID verification
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
    .optional()
    .transform((val) => val?.trim() || undefined),
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
    data.currentCountryCode || "KE",
  );
  const [phoneValue, setPhoneValue] = useState<string>(data.mobileNumber || "");

  // ID Verification State
  const [verificationStatus, setVerificationStatus] = useState<VerificationStatus>('idle');
  const [erpData, setErpData] = useState<ErpVerificationData | null>(null);
  const [verificationError, setVerificationError] = useState<string>("");

  const methods = useForm<PersonalInfoFormData>({
    resolver: zodResolver(personalInfoSchema),
    defaultValues: {
      staffNumber: data.staffNumber || "",
      idNumber: data.idNumber || "",
      passportNumber: data.passportNumber || "",
      fullName: data.fullName || "",
      email: data.email || "",
      mobileCountryCode: data.mobileCountryCode || "+254",
      mobileNumber: data.mobileNumber || "",
      currentCountry: data.currentCountry || "Kenya",
      currentCountryCode: data.currentCountryCode || "KE",
      currentCity: data.currentCity || "",
      cityCustom: data.cityCustom || "",
    },
  });

  const {
    handleSubmit,
    setValue,
    watch,
  } = methods;

  // Watch fields for duplicate checking and verification
  const idNumberValue = watch("idNumber");
  const emailValue = watch("email");

  // Debounce values to avoid too many API calls
  const debouncedIdNumber = useDebounce(idNumberValue, 1000); // 1 second delay for ID verification
  const debouncedEmail = useDebounce(emailValue, 800);

  // Duplicate checking hooks
  const emailCheck = useDuplicateCheck("email");

  // Trigger duplicate checks when debounced values change
  useMemo(() => {
    if (debouncedEmail && debouncedEmail.length > 0) {
      emailCheck.checkValue(debouncedEmail);
    } else {
      emailCheck.reset();
    }
  }, [debouncedEmail]); // eslint-disable-line react-hooks/exhaustive-deps

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

      const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5166';
      const response = await fetch(`${apiUrl}/api/v1/registrations/verify-id/${encodeURIComponent(idOrPassport)}`);

      if (!response.ok) {
        throw new Error('Verification failed');
      }

      const result = await response.json();

      if (result.isAlreadyRegistered) {
        setVerificationStatus('already_registered');
        setVerificationError(result.message || 'This ID/Passport is already registered');
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
          setValue("fullName", result.fullName);
        }
        if (result.staffNumber) {
          setValue("staffNumber", result.staffNumber);
        }

        setVerificationError("");
      } else {
        setVerificationStatus('failed');
        setVerificationError(result.message || 'ID/Passport not found in our records');
        setErpData(null);
      }
    } catch (error) {
      setVerificationStatus('failed');
      setVerificationError('Unable to verify ID/Passport. Please try again.');
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
      setValue("currentCountry", country.name);
      setValue("currentCountryCode", country.isoCode);
      setValue("currentCity", "");

      // Also update mobile country code to match selected country
      if (country.phonecode) {
        setValue("mobileCountryCode", `+${country.phonecode}`);
      }
    }
  };

  const handlePhoneChange = (
    value: string,
    country: PhoneCountryData,
  ): void => {
    setPhoneValue(value);
    setValue("mobileNumber", value);
    setValue("mobileCountryCode", `+${country.dialCode}`);

    const phoneCountry = Country.getAllCountries().find(
      (c: ICountry) =>
        c.isoCode.toLowerCase() === country.countryCode.toLowerCase(),
    );

    if (phoneCountry) {
      setSelectedCountryCode(phoneCountry.isoCode);
      setValue("currentCountry", phoneCountry.name);
      setValue("currentCountryCode", phoneCountry.isoCode);
      // Reset city when country changes via phone
      setValue("currentCity", "");
    }
  };

  const onSubmit = (formData: PersonalInfoFormData): void => {
    // Don't submit if duplicates found or ID not verified
    if (emailCheck.isDuplicate) {
      return;
    }

    if (verificationStatus !== 'verified') {
      setVerificationError('Please wait for ID verification to complete');
      return;
    }

    // Include ERP data in submission
    onNext({
      ...formData,
      staffNumber: erpData?.staffNumber || formData.staffNumber,
      fullName: erpData?.fullName || formData.fullName,
    });
  };

  // Helper to get the duplicate check icon
  const getDuplicateIcon = (check: typeof emailCheck) => {
    if (check.isChecking) {
      return <div className="animate-spin h-5 w-5 border-2 border-gray-300 border-t-kq-red rounded-full" />;
    }
    if (check.isDuplicate) {
      return <ExclamationCircleIcon className="h-5 w-5 text-red-600" />;
    }
    if (check.error) {
      return null;
    }
    // Only show checkmark if we actually checked and it's not a duplicate
    if (watch("email")) {
      return <CheckCircleIcon className="h-5 w-5 text-green-600" />;
    }
    return null;
  };

  // Helper to get verification status icon
  const getVerificationIcon = () => {
    if (verificationStatus === 'verifying') {
      return <div className="animate-spin h-5 w-5 border-2 border-gray-300 border-t-kq-red rounded-full" />;
    }
    if (verificationStatus === 'verified') {
      return <CheckCircleIcon className="h-5 w-5 text-green-600" />;
    }
    if (verificationStatus === 'failed' || verificationStatus === 'already_registered') {
      return <ExclamationCircleIcon className="h-5 w-5 text-red-600" />;
    }
    return null;
  };

  return (
    <FormProvider {...methods}>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-8">
      <div>
        {/* Progress Indicator - Above Header */}
        <ProgressIndicator currentStep={1} totalSteps={3} />

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
                setValue("idNumber", cleaned);
              }}
              style={{ textTransform: "uppercase" }}
              className="uppercase"
              rightIcon={getVerificationIcon()}
            />
            {verificationStatus === 'verifying' && (
              <p className="mt-2 text-sm text-blue-600 flex items-center gap-2">
                <span className="inline-block w-4 h-4 border-2 border-blue-600 border-t-transparent rounded-full animate-spin" />
                Verifying with our records...
              </p>
            )}
          </div>

          {/* Full Name */}
          <div>
            <div className="flex items-center gap-2">
              <FormField
                name="fullName"
                label="Full Name"
                type="text"
                placeholder="As per company records"
                variant="underline"
                disabled
                className="bg-gray-50"
              />
              {verificationStatus === 'verified' && erpData?.fullName && (
                <CheckCircleIcon className="w-5 h-5 text-green-600 flex-shrink-0 mt-7" />
              )}
            </div>
            <p className="mt-2 text-xs text-gray-500">
              Auto-filled from company records
            </p>
          </div>
        </div>

        {/* Row 2: Staff Number & Email - Side by Side */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
          {/* Staff Number */}
          <div>
            <div className="flex items-center gap-2">
              <FormField
                name="staffNumber"
                label="Staff Number"
                type="text"
                placeholder="e.g., 0012345"
                variant="underline"
                disabled
                className="bg-gray-50"
              />
              {verificationStatus === 'verified' && erpData?.staffNumber && (
                <CheckCircleIcon className="w-5 h-5 text-green-600 flex-shrink-0 mt-7" />
              )}
            </div>
            <p className="mt-2 text-xs text-gray-500">
              Auto-filled from company records
            </p>
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
              rightIcon={getDuplicateIcon(emailCheck)}
            />
            {emailCheck.isDuplicate && (
              <p className="mt-2 text-sm text-kq-red">
                {emailCheck.error || "This email is already registered"}
              </p>
            )}
          </div>
        </div>

        {/* Mobile Number */}
        <div className="mb-8">
          <label className="block text-sm font-medium text-gray-700 mb-3">
            Mobile Number
          </label>
          <PhoneInput
            country={"ke"}
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
          disabled={verificationStatus !== 'verified' || emailCheck.isDuplicate}
        >
          {verificationStatus === 'idle' && 'Enter ID Number'}
          {verificationStatus === 'verifying' && (
            <span className="flex items-center justify-center gap-2">
              <span className="inline-block w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
              Verifying...
            </span>
          )}
          {verificationStatus === 'failed' && 'Verification Failed'}
          {verificationStatus === 'already_registered' && 'Already Registered'}
          {verificationStatus === 'verified' && emailCheck.isDuplicate && 'Email Already Used'}
          {verificationStatus === 'verified' && !emailCheck.isDuplicate && 'Continue'}
        </Button>
      </div>
    </form>
    </FormProvider>
  );
}
