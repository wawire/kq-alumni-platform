/**
 * Analytics Tracking System
 *
 * Supports multiple analytics providers:
 * - Google Analytics
 * - Facebook Pixel
 * - Custom event tracking
 *
 * Usage:
 * ```tsx
 * import { trackEvent, trackPageView } from '@/lib/analytics';
 *
 * // Track page view
 * trackPageView('/register');
 *
 * // Track custom event
 * trackEvent('registration_started', { step: 'personal_info' });
 * ```
 */

type EventProperties = Record<string, string | number | boolean | undefined>;

interface AnalyticsEvent {
  name: string;
  properties?: EventProperties;
  timestamp: string;
}

class Analytics {
  private events: AnalyticsEvent[] = [];
  private isEnabled: boolean;

  constructor() {
    this.isEnabled = typeof window !== "undefined" && process.env.NODE_ENV === "production";
  }

  /**
   * Track a custom event
   */
  trackEvent(eventName: string, properties?: EventProperties): void {
    const event: AnalyticsEvent = {
      name: eventName,
      properties,
      timestamp: new Date().toISOString(),
    };

    this.events.push(event);

    if (!this.isEnabled) {
      console.log("[Analytics - Dev]", eventName, properties);
      return;
    }

    // Google Analytics
    if (typeof window !== "undefined" && (window as any).gtag) {
      (window as any).gtag("event", eventName, properties);
    }

    // Facebook Pixel
    if (typeof window !== "undefined" && (window as any).fbq) {
      (window as any).fbq("trackCustom", eventName, properties);
    }
  }

  /**
   * Track page view
   */
  trackPageView(path: string): void {
    this.trackEvent("page_view", { path });

    if (!this.isEnabled) {
      return;
    }

    // Google Analytics
    if (typeof window !== "undefined" && (window as any).gtag) {
      (window as any).gtag("config", process.env.NEXT_PUBLIC_GA_MEASUREMENT_ID, {
        page_path: path,
      });
    }

    // Facebook Pixel
    if (typeof window !== "undefined" && (window as any).fbq) {
      (window as any).fbq("track", "PageView");
    }
  }

  /**
   * Track form step completion
   */
  trackFormStep(step: string, stepNumber: number, properties?: EventProperties): void {
    this.trackEvent("form_step_completed", {
      step,
      step_number: stepNumber,
      ...properties,
    });
  }

  /**
   * Track form abandonment
   */
  trackFormAbandonment(step: string, stepNumber: number): void {
    this.trackEvent("form_abandoned", {
      step,
      step_number: stepNumber,
    });
  }

  /**
   * Track form submission
   */
  trackFormSubmission(success: boolean, properties?: EventProperties): void {
    this.trackEvent(success ? "form_submission_success" : "form_submission_error", properties);
  }

  /**
   * Track field interaction
   */
  trackFieldInteraction(fieldName: string, action: "focus" | "blur" | "change"): void {
    this.trackEvent("field_interaction", {
      field_name: fieldName,
      action,
    });
  }

  /**
   * Get all tracked events (for debugging)
   */
  getEvents(): AnalyticsEvent[] {
    return this.events;
  }

  /**
   * Clear all events
   */
  clearEvents(): void {
    this.events = [];
  }
}

export const analytics = new Analytics();

// Convenience exports
export const trackEvent = analytics.trackEvent.bind(analytics);
export const trackPageView = analytics.trackPageView.bind(analytics);
export const trackFormStep = analytics.trackFormStep.bind(analytics);
export const trackFormAbandonment = analytics.trackFormAbandonment.bind(analytics);
export const trackFormSubmission = analytics.trackFormSubmission.bind(analytics);
export const trackFieldInteraction = analytics.trackFieldInteraction.bind(analytics);
