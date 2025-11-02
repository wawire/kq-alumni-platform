"use client";

import { useMemo, useState } from "react";
import { ArrowRightIcon, ExclamationCircleIcon, CheckCircleIcon } from "@heroicons/react/24/solid";
import { zodResolver } from "@hookform/resolvers/zod";
import { City, Country, ICity, ICountry } from "country-state-city";
import { FormProvider, useForm } from "react-hook-form";
import PhoneInput, {
  CountryData as PhoneCountryData,
} from "react-phone-input-2";
import "react-phone-input-2/lib/style.css";
import { SingleValue } from "react-select";
import { z } from "zod";
import { Button } from "@/components/ui";
import { FormField, FormSelect } from "@/components/forms";
import { useDebounce } from "@/hooks/useDebounce";
import { useDuplicateCheck } from "@/hooks/useDuplicateCheck";
import type { RegistrationFormData } from "../RegistrationForm";

// =====================================================
// VALIDATION SCHEMA - EXACT MATCH WITH BACKEND
// =====================================================
const personalInfoSchema = z.object({
  staffNumber: z
    .string()
    .min(1, "Staff number is required")
    .length(7, "Staff number must be exactly 7 characters")
    .regex(
      /^(00\d{5}|00[CA]\d{4})$/,
      "Invalid staff number format. Use: 00XXXXX (permanent), 00CXXXX (contract), or 00AXXXX (intern)",
    )
    .refine((val) => val === val.toUpperCase(), {
      message: "Staff number must be uppercase",
    })
    .transform((val) => val.toUpperCase().trim()),
  fullName: z
    .string()
    .min(2, "Full name must be at least 2 characters")
    .max(200, "Full name cannot exceed 200 characters")
    .regex(
      /^[a-zA-Z\s\-']+$/,
      "Only letters, spaces, hyphens, and apostrophes allowed",
    )
    .transform((val) => val.trim()),
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
// MAIN COMPONENT
// =====================================================
export default function PersonalInfoStep({ data, onNext }: Props) {
  const [selectedCountryCode, setSelectedCountryCode] = useState<string>(
    data.currentCountryCode || "KE",
  );
  const [phoneValue, setPhoneValue] = useState<string>(data.mobileNumber || "");

  const methods = useForm<PersonalInfoFormData>({
    resolver: zodResolver(personalInfoSchema),
    defaultValues: {
      staffNumber: data.staffNumber || "",
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

  // Watch fields for duplicate checking
  const emailValue = watch("email");
  const staffNumberValue = watch("staffNumber");

  // Debounce values to avoid too many API calls
  const debouncedEmail = useDebounce(emailValue, 800);
  const debouncedStaffNumber = useDebounce(staffNumberValue, 800);

  // Duplicate checking hooks
  const emailCheck = useDuplicateCheck("email");
  const staffNumberCheck = useDuplicateCheck("staffNumber");

  // Trigger duplicate checks when debounced values change
  useMemo(() => {
    if (debouncedEmail && debouncedEmail.length > 0) {
      emailCheck.checkValue(debouncedEmail);
    } else {
      emailCheck.reset();
    }
  }, [debouncedEmail]); // eslint-disable-line react-hooks/exhaustive-deps

  useMemo(() => {
    if (debouncedStaffNumber && debouncedStaffNumber.length === 7) {
      staffNumberCheck.checkValue(debouncedStaffNumber);
    } else {
      staffNumberCheck.reset();
    }
  }, [debouncedStaffNumber]); // eslint-disable-line react-hooks/exhaustive-deps

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
    }
  };

  const onSubmit = (formData: PersonalInfoFormData): void => {
    // Don't submit if duplicates found
    if (emailCheck.isDuplicate || staffNumberCheck.isDuplicate) {
      return;
    }
    onNext(formData);
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
    if (watch("email") || watch("staffNumber")) {
      return <CheckCircleIcon className="h-5 w-5 text-green-600" />;
    }
    return null;
  };

  return (
    <FormProvider {...methods}>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-8">
      <div>
        <h2 className="text-3xl font-cabrito font-bold text-kq-dark mb-8">
          Personal Information
        </h2>

        {/* Full Name & Staff Number */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
          <FormField
            name="fullName"
            label="Full Name"
            type="text"
            placeholder="As per company records"
            required
            variant="underline"
          />

          <FormField
            name="staffNumber"
            label="Staff Number"
            type="text"
            placeholder="00A7122"
            maxLength={7}
            required
            variant="underline"
            description={
              staffNumberCheck.isDuplicate
                ? staffNumberCheck.error || "This staff number is already registered"
                : undefined // Hide format text until error occurs
            }
            rightIcon={getDuplicateIcon(staffNumberCheck)}
            onChange={(e) => {
              const uppercased = e.target.value.toUpperCase();
              setValue("staffNumber", uppercased);
            }}
            style={{ textTransform: "uppercase" }}
            className="uppercase"
          />
        </div>

        <h3 className="text-2xl font-cabrito font-bold text-kq-dark mt-12 mb-8">
          Contact Information
        </h3>

        {/* Email */}
        <div className="mb-8">
          <FormField
            name="email"
            label="Email Address"
            type="email"
            placeholder="your.email@example.com"
            required
            variant="underline"
            description={
              emailCheck.isDuplicate
                ? emailCheck.error || "This email is already registered"
                : undefined
            }
            rightIcon={getDuplicateIcon(emailCheck)}
          />
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
          rightIcon={<ArrowRightIcon className="w-5 h-5" />}
        >
          Next Step
        </Button>
      </div>
    </form>
    </FormProvider>
  );
}
