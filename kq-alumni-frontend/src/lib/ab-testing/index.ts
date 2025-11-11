/**
 * A/B Testing System
 *
 * Simple client-side A/B testing for UX experiments.
 * Assigns users to variants and persists the assignment in localStorage.
 *
 * Usage:
 * ```tsx
 * import { useABTest } from '@/lib/ab-testing';
 *
 * function MyComponent() {
 *   const { variant } = useABTest('button_color', {
 *     variants: ['blue', 'green'],
 *     weights: [0.5, 0.5], // 50/50 split
 *   });
 *
 *   return (
 *     <button className={variant === 'blue' ? 'bg-blue-600' : 'bg-green-600'}>
 *       Submit
 *     </button>
 *   );
 * }
 * ```
 */

import { useEffect, useState } from "react";
import { trackEvent } from "../analytics";

export interface ABTestConfig {
  variants: string[];
  weights?: number[]; // Optional weights (must sum to 1.0)
}

interface ABTestAssignment {
  experimentName: string;
  variant: string;
  assignedAt: string;
}

const STORAGE_KEY = "kq-alumni-ab-tests";

class ABTestManager {
  /**
   * Assign a user to an A/B test variant
   */
  assignVariant(experimentName: string, config: ABTestConfig): string {
    // Check if user already has an assignment
    const existing = this.getAssignment(experimentName);
    if (existing) {
      return existing.variant;
    }

    // Assign new variant
    const variant = this.selectVariant(config);

    // Save assignment
    const assignment: ABTestAssignment = {
      experimentName,
      variant,
      assignedAt: new Date().toISOString(),
    };

    this.saveAssignment(assignment);

    // Track assignment
    trackEvent("ab_test_assigned", {
      experiment: experimentName,
      variant,
    });

    return variant;
  }

  /**
   * Select a variant based on weights
   */
  private selectVariant(config: ABTestConfig): string {
    const { variants, weights } = config;

    // Default to equal weights if not provided
    const normalizedWeights = weights || variants.map(() => 1 / variants.length);

    // Validate weights sum to 1.0
    const sum = normalizedWeights.reduce((a, b) => a + b, 0);
    if (Math.abs(sum - 1.0) > 0.001) {
      console.warn("ABTest: Weights do not sum to 1.0, using equal weights");
      return variants[Math.floor(Math.random() * variants.length)];
    }

    // Select variant based on weighted random
    const random = Math.random();
    let cumulative = 0;

    for (let i = 0; i < variants.length; i++) {
      cumulative += normalizedWeights[i];
      if (random <= cumulative) {
        return variants[i];
      }
    }

    // Fallback (should never reach here)
    return variants[0];
  }

  /**
   * Get existing assignment for an experiment
   */
  private getAssignment(experimentName: string): ABTestAssignment | null {
    if (typeof window === "undefined") {
      return null;
    }

    try {
      const stored = window.localStorage.getItem(STORAGE_KEY);
      if (!stored) {
        return null;
      }

      const assignments: ABTestAssignment[] = JSON.parse(stored);
      return assignments.find((a) => a.experimentName === experimentName) || null;
    } catch {
      return null;
    }
  }

  /**
   * Save assignment to localStorage
   */
  private saveAssignment(assignment: ABTestAssignment): void {
    if (typeof window === "undefined") {
      return;
    }

    try {
      const stored = window.localStorage.getItem(STORAGE_KEY);
      const assignments: ABTestAssignment[] = stored ? JSON.parse(stored) : [];

      // Add new assignment
      assignments.push(assignment);

      window.localStorage.setItem(STORAGE_KEY, JSON.stringify(assignments));
    } catch (error) {
      console.error("ABTest: Failed to save assignment", error);
    }
  }

  /**
   * Get all assignments (for debugging)
   */
  getAllAssignments(): ABTestAssignment[] {
    if (typeof window === "undefined") {
      return [];
    }

    try {
      const stored = window.localStorage.getItem(STORAGE_KEY);
      return stored ? JSON.parse(stored) : [];
    } catch {
      return [];
    }
  }

  /**
   * Clear all assignments (for testing)
   */
  clearAssignments(): void {
    if (typeof window === "undefined") {
      return;
    }
    window.localStorage.removeItem(STORAGE_KEY);
  }
}

export const abTestManager = new ABTestManager();

/**
 * React hook for A/B testing
 */
export function useABTest(experimentName: string, config: ABTestConfig) {
  const [variant, setVariant] = useState<string>(config.variants[0]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const assignedVariant = abTestManager.assignVariant(experimentName, config);
    setVariant(assignedVariant);
    setIsLoading(false);
  }, [experimentName, config]);

  return {
    variant,
    isLoading,
    isVariant: (variantName: string) => variant === variantName,
  };
}
