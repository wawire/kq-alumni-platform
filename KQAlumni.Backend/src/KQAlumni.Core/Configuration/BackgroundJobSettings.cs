namespace KQAlumni.Core.Configuration;

/// <summary>
/// Configuration for background job scheduling
/// </summary>
public class BackgroundJobSettings
{
  /// <summary>
  /// Cron expression for business hours (Mon-Fri, 8 AM - 6 PM EAT)
  /// Default: Every 2 minutes
  /// </summary>
  public string BusinessHoursSchedule { get; set; } = "*/2 8-17 * * 1-5";

  /// <summary>
  /// Cron expression for off-hours (Mon-Fri, 6 PM - 8 AM EAT)
  /// Default: Every 15 minutes
  /// </summary>
  public string OffHoursSchedule { get; set; } = "*/15 18-23,0-7 * * 1-5";

  /// <summary>
  /// Cron expression for weekends (Sat-Sun)
  /// Default: Every 30 minutes
  /// </summary>
  public string WeekendSchedule { get; set; } = "*/30 * * * 0,6";

  /// <summary>
  /// Timezone for scheduling (East Africa Time)
  /// </summary>
  public string TimeZone { get; set; } = "E. Africa Standard Time";

  /// <summary>
  /// Number of registrations to process per batch
  /// </summary>
  public int BatchSize { get; set; } = 100;

  /// <summary>
  /// Maximum number of ERP validation retry attempts
  /// </summary>
  public int MaxRetryAttempts { get; set; } = 5;

  /// <summary>
  /// Minutes to wait between retry attempts
  /// </summary>
  public int RetryDelayMinutes { get; set; } = 10;

  /// <summary>
  /// Enable smart scheduling (multiple schedules based on time)
  /// If false, uses BusinessHoursSchedule for all times
  /// </summary>
  public bool EnableSmartScheduling { get; set; } = true;
}
