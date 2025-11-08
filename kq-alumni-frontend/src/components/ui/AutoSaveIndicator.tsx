"use client";

import React, { useEffect, useState } from "react";
import { CheckCircleIcon, CloudArrowUpIcon } from "@heroicons/react/24/outline";

interface AutoSaveIndicatorProps {
  isSaving?: boolean;
  lastSaved?: Date | null;
}

export default function AutoSaveIndicator({
  isSaving = false,
  lastSaved = null,
}: AutoSaveIndicatorProps) {
  const [displayText, setDisplayText] = useState<string>("");

  useEffect(() => {
    if (isSaving) {
      setDisplayText("Saving...");
      return;
    }

    if (lastSaved) {
      const now = new Date();
      const diff = Math.floor((now.getTime() - lastSaved.getTime()) / 1000);

      if (diff < 10) {
        setDisplayText("Saved just now");
      } else if (diff < 60) {
        setDisplayText(`Saved ${diff}s ago`);
      } else if (diff < 3600) {
        const minutes = Math.floor(diff / 60);
        setDisplayText(`Saved ${minutes}m ago`);
      } else {
        setDisplayText("Saved");
      }
    }
  }, [isSaving, lastSaved]);

  if (!displayText) return null;

  return (
    <div className="fixed bottom-4 right-4 sm:bottom-6 sm:right-6 z-40">
      <div className="flex items-center gap-2 px-4 py-2 bg-white border border-gray-200 rounded-full shadow-lg">
        {isSaving ? (
          <CloudArrowUpIcon className="w-4 h-4 text-blue-600 animate-pulse" />
        ) : (
          <CheckCircleIcon className="w-4 h-4 text-green-600" />
        )}
        <span className="text-xs font-roboto font-medium text-gray-700">
          {displayText}
        </span>
      </div>
    </div>
  );
}
