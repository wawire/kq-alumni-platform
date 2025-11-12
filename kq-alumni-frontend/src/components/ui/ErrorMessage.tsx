"use client";

import React from "react";
import { ExclamationCircleIcon, InformationCircleIcon } from "@heroicons/react/24/outline";
import { cn } from "@/lib/utils/cn";

interface ErrorMessageProps {
  message: string;
  type?: "error" | "warning" | "info";
  helpText?: string;
  className?: string;
}

export default function ErrorMessage({
  message,
  type = "error",
  helpText,
  className,
}: ErrorMessageProps) {
  const styles = {
    error: {
      container: "bg-red-50 border-red-200 text-red-900",
      icon: "text-red-600",
      Icon: ExclamationCircleIcon,
    },
    warning: {
      container: "bg-yellow-50 border-yellow-200 text-yellow-900",
      icon: "text-yellow-600",
      Icon: ExclamationCircleIcon,
    },
    info: {
      container: "bg-blue-50 border-blue-200 text-blue-900",
      icon: "text-blue-600",
      Icon: InformationCircleIcon,
    },
  };

  const style = styles[type];
  const Icon = style.Icon;

  return (
    <div className={cn("border rounded-lg p-3 flex gap-3", style.container, className)}>
      <Icon className={cn("w-5 h-5 flex-shrink-0 mt-0.5", style.icon)} />
      <div className="flex-1 min-w-0">
        <p className="text-sm font-roboto font-medium">{message}</p>
        {helpText && (
          <p className="text-xs font-roboto mt-1 opacity-90">{helpText}</p>
        )}
      </div>
    </div>
  );
}

/**
 * Field-level error message
 */
interface FieldErrorProps {
  error?: string;
  touched?: boolean;
}

export function FieldError({ error, touched }: FieldErrorProps) {
  if (!error || !touched) {
    return null;
  }

  return (
    <p className="text-xs text-red-600 font-roboto mt-1 flex items-start gap-1">
      <ExclamationCircleIcon className="w-3 h-3 flex-shrink-0 mt-0.5" />
      <span>{error}</span>
    </p>
  );
}
