import axios, { AxiosError, AxiosInstance } from "axios";

import { API_BASE_URL, API_TIMEOUT } from "@/config/api";
import type { RegistrationResponse, ValidationError } from "@/types";
import type { RegistrationFormData } from "../../components/registration/RegistrationForm";

// ======================================================
// LOGGER — Centralized logging (disable console in prod)
// ======================================================
const isDev = process.env.NODE_ENV !== "production";
const logger = {
  // eslint-disable-next-line no-console
  info: (...args: unknown[]) => isDev && console.info("[API]", ...args),
  // eslint-disable-next-line no-console
  error: (...args: unknown[]) => isDev && console.error("[API]", ...args),
};

logger.info("[CONFIG] API Configuration:", { baseURL: API_BASE_URL });

// ======================================================
// ERROR MESSAGES
// ======================================================
const ERROR_MESSAGES = {
  STAFF_NUMBER_REQUIRED: "Staff number is required",
  STAFF_NUMBER_FORMAT:
    "Invalid staff number format. Use 00XXXXX (permanent), 00CXXXX (contract), or 00AXXXX (intern)",
  STAFF_NUMBER_LENGTH: "Staff number must be exactly 7 characters",
  STAFF_NUMBER_NOT_FOUND:
    "Staff number not found in our records. Please contact HR",
  STAFF_NUMBER_DUPLICATE: "This staff number is already registered",
  FULL_NAME_REQUIRED: "Full name is required",
  FULL_NAME_TOO_SHORT: "Full name must be at least 2 characters",
  FULL_NAME_TOO_LONG: "Full name cannot exceed 200 characters",
  FULL_NAME_INVALID_CHARS:
    "Full name can only contain letters (including accented characters), spaces, hyphens, apostrophes, periods, commas, and backticks",
  FULL_NAME_MISMATCH: "Name does not match our records",
  EMAIL_REQUIRED: "Email address is required",
  EMAIL_INVALID: "Please enter a valid email address",
  EMAIL_TOO_LONG: "Email address is too long",
  EMAIL_DUPLICATE: "This email is already registered",
  EMAIL_DISPOSABLE: "Please use a permanent email address",
  PHONE_INVALID: "Invalid phone number format",
  PHONE_TOO_SHORT: "Phone number is too short (minimum 6 digits)",
  PHONE_TOO_LONG: "Phone number is too long (maximum 15 digits)",
  COUNTRY_REQUIRED: "Please select your country",
  CITY_REQUIRED: "Please select your city",
  QUALIFICATIONS_REQUIRED: "Please select at least one qualification",
  ENGAGEMENT_REQUIRED: "Please select at least one area of interest",
  CONSENT_REQUIRED: "You must give consent to register",
  RATE_LIMIT: "Too many registration attempts. Please try again in an hour",
  NETWORK_ERROR:
    "Cannot connect to the server. Please check if the backend is running",
  SERVER_ERROR: "Server error occurred. Please try again later",
  VALIDATION_ERROR: "Please check your form for errors",
  TIMEOUT_ERROR: "Request timeout. Please try again",
  UNKNOWN_ERROR: "An unexpected error occurred. Please try again",
};

// ======================================================
// AXIOS CLIENT
// ======================================================
const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: API_TIMEOUT,
  headers: {
    "Content-Type": "application/json",
    Accept: "application/json",
  },
  withCredentials: false,
});

// ======================================================
// REQUEST INTERCEPTOR
// ======================================================
apiClient.interceptors.request.use(
  (config) => {
    logger.info("[REQUEST] API Request:", {
      method: config.method?.toUpperCase(),
      url: `${config.baseURL}${config.url}`,
      timestamp: new Date().toISOString(),
    });
    return config;
  },
  (err) => {
    logger.error("[ERROR] Request Interceptor Error:", err);
    return Promise.reject(err);
  },
);

// ======================================================
// RESPONSE INTERCEPTOR
// ======================================================
apiClient.interceptors.response.use(
  (response) => {
    logger.info("[SUCCESS] API Response:", {
      status: response.status,
      url: response.config.url,
      timestamp: new Date().toISOString(),
    });
    return response;
  },
  (err: AxiosError<ValidationError>) => {
    logger.error("[ERROR] API Response Error:", {
      message: err.message,
      code: err.code,
      status: err.response?.status,
      url: err.config?.url,
      timestamp: new Date().toISOString(),
    });
    return Promise.reject(err);
  },
);

// ======================================================
// VALIDATION ERROR PARSER
// ======================================================
function parseValidationErrors(errors: Record<string, string[]>): string[] {
  const errorMessages: string[] = [];

  for (const [field, messages] of Object.entries(errors)) {
    const lowerField = field.toLowerCase();
    const firstMessage = messages[0]?.toLowerCase() || "";

    if (lowerField.includes("staffnumber")) {
      if (firstMessage.includes("required")) {
        errorMessages.push(ERROR_MESSAGES.STAFF_NUMBER_REQUIRED);
      } else if (
        firstMessage.includes("format") ||
        firstMessage.includes("invalid")
      ) {
        errorMessages.push(ERROR_MESSAGES.STAFF_NUMBER_FORMAT);
      } else if (
        firstMessage.includes("length") ||
        firstMessage.includes("7 character")
      ) {
        errorMessages.push(ERROR_MESSAGES.STAFF_NUMBER_LENGTH);
      } else if (
        firstMessage.includes("not found") ||
        firstMessage.includes("does not exist")
      ) {
        errorMessages.push(ERROR_MESSAGES.STAFF_NUMBER_NOT_FOUND);
      } else {
        errorMessages.push(`Staff Number: ${messages[0]}`);
      }
    } else if (lowerField.includes("email")) {
      if (firstMessage.includes("required")) {
        errorMessages.push(ERROR_MESSAGES.EMAIL_REQUIRED);
      } else if (
        firstMessage.includes("invalid") ||
        firstMessage.includes("format")
      ) {
        errorMessages.push(ERROR_MESSAGES.EMAIL_INVALID);
      } else if (firstMessage.includes("too long")) {
        errorMessages.push(ERROR_MESSAGES.EMAIL_TOO_LONG);
      } else if (firstMessage.includes("disposable")) {
        errorMessages.push(ERROR_MESSAGES.EMAIL_DISPOSABLE);
      } else {
        errorMessages.push(`Email: ${messages[0]}`);
      }
    } else if (lowerField.includes("fullname") || lowerField.includes("name")) {
      if (firstMessage.includes("required")) {
        errorMessages.push(ERROR_MESSAGES.FULL_NAME_REQUIRED);
      } else if (
        firstMessage.includes("mismatch") ||
        firstMessage.includes("match")
      ) {
        errorMessages.push(ERROR_MESSAGES.FULL_NAME_MISMATCH);
      } else if (
        firstMessage.includes("character") ||
        firstMessage.includes("invalid")
      ) {
        errorMessages.push(ERROR_MESSAGES.FULL_NAME_INVALID_CHARS);
      } else if (firstMessage.includes("too short")) {
        errorMessages.push(ERROR_MESSAGES.FULL_NAME_TOO_SHORT);
      } else if (firstMessage.includes("too long")) {
        errorMessages.push(ERROR_MESSAGES.FULL_NAME_TOO_LONG);
      } else {
        errorMessages.push(`Full Name: ${messages[0]}`);
      }
    } else if (lowerField.includes("country")) {
      errorMessages.push(ERROR_MESSAGES.COUNTRY_REQUIRED);
    } else if (lowerField.includes("city")) {
      errorMessages.push(ERROR_MESSAGES.CITY_REQUIRED);
    } else if (lowerField.includes("qualification")) {
      errorMessages.push(ERROR_MESSAGES.QUALIFICATIONS_REQUIRED);
    } else if (lowerField.includes("engagement")) {
      errorMessages.push(ERROR_MESSAGES.ENGAGEMENT_REQUIRED);
    } else if (lowerField.includes("consent")) {
      errorMessages.push(ERROR_MESSAGES.CONSENT_REQUIRED);
    } else if (lowerField.includes("phone") || lowerField.includes("mobile")) {
      if (firstMessage.includes("short")) {
        errorMessages.push(ERROR_MESSAGES.PHONE_TOO_SHORT);
      } else if (firstMessage.includes("long")) {
        errorMessages.push(ERROR_MESSAGES.PHONE_TOO_LONG);
      } else {
        errorMessages.push(ERROR_MESSAGES.PHONE_INVALID);
      }
    } else {
      errorMessages.push(`${field}: ${messages[0]}`);
    }
  }

  return errorMessages;
}

// ======================================================
// MAIN ERROR HANDLER
// ======================================================
export function handleApiError(err: unknown): string[] {
  logger.error("[DEBUG] Handling API Error:", err);

  if (!axios.isAxiosError(err)) {
    return [ERROR_MESSAGES.UNKNOWN_ERROR];
  }

  const axiosError = err as AxiosError<ValidationError>;

  if (!axiosError.response) {
    if (axiosError.code === "ECONNABORTED") {
      return [ERROR_MESSAGES.TIMEOUT_ERROR];
    }
    if (
      axiosError.code === "ERR_NETWORK" ||
      axiosError.code === "ECONNREFUSED"
    ) {
      return [ERROR_MESSAGES.NETWORK_ERROR];
    }
    return [ERROR_MESSAGES.NETWORK_ERROR];
  }

  const { status, data } = axiosError.response;

  if (status === 400) {
    if (data.errors && Object.keys(data.errors).length > 0) {
      return parseValidationErrors(data.errors);
    }
    return [data.detail || ERROR_MESSAGES.VALIDATION_ERROR];
  }

  if (status === 409) {
    const detail = data.detail?.toLowerCase() || "";
    if (detail.includes("staff")) {
      return [ERROR_MESSAGES.STAFF_NUMBER_DUPLICATE];
    }
    if (detail.includes("email")) {
      return [ERROR_MESSAGES.EMAIL_DUPLICATE];
    }
    return [data.detail || "Duplicate registration"];
  }

  if (status === 429) {
    return [ERROR_MESSAGES.RATE_LIMIT];
  }

  if (status >= 500) {
    return [ERROR_MESSAGES.SERVER_ERROR];
  }

  return [data.detail || ERROR_MESSAGES.UNKNOWN_ERROR];
}

// ======================================================
// API FUNCTIONS
// ======================================================
export async function submitRegistration(
  formData: RegistrationFormData,
): Promise<RegistrationResponse> {
  try {
    const response = await apiClient.post<RegistrationResponse>(
      "/api/v1/registrations",
      formData,
    );
    return response.data;
  } catch (err) {
    throw err;
  }
}

export async function healthCheck(): Promise<boolean> {
  try {
    const response = await apiClient.get("/health");
    return response.status === 200;
  } catch {
    return false;
  }
}
