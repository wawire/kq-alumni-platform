using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using KQAlumni.Core.Entities;
using KQAlumni.Core.Interfaces;
using KQAlumni.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KQAlumni.Infrastructure.Services;

/// <summary>
/// Enhanced Email service with delivery tracking and logging
/// </summary>
public class EmailServiceWithTracking : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailServiceWithTracking> _logger;
    private readonly EmailSettings _emailSettings;
    private readonly AppDbContext _dbContext;

    public EmailServiceWithTracking(
        IConfiguration configuration,
        ILogger<EmailServiceWithTracking> logger,
        IOptions<EmailSettings> emailSettings,
        AppDbContext dbContext)
    {
        _configuration = configuration;
        _logger = logger;
        _emailSettings = emailSettings.Value;
        _dbContext = dbContext;

        _logger.LogInformation(
            "[EMAIL] Email Service Initialized (With Tracking):\n" +
            "   Host: {Host}\n" +
            "   Port: {Port}\n" +
            "   From: {From}\n" +
            "   SSL: {Ssl}\n" +
            "   Tracking: Enabled",
            _emailSettings.SmtpServer,
            _emailSettings.SmtpPort,
            _emailSettings.From,
            _emailSettings.EnableSsl);
    }

    /// <summary>
    /// Send confirmation email immediately after registration (Email 1)
    /// </summary>
    public async Task<bool> SendConfirmationEmailAsync(
        string alumniName,
        string email,
        string registrationNumber,
        CancellationToken cancellationToken = default)
    {
        var subject = "Registration Received - KQ Alumni Network";
        var body = GetConfirmationEmailTemplate(alumniName, registrationNumber);

        return await SendEmailWithTrackingAsync(
            email,
            subject,
            body,
            EmailType.Confirmation,
            null, // No registration ID tracking for confirmation email
            cancellationToken);
    }

    /// <summary>
    /// Send approval email (Email 2)
    /// </summary>
    public async Task<bool> SendApprovalEmailAsync(
        string alumniName,
        string email,
        string verificationToken,
        CancellationToken cancellationToken = default)
    {
        var subject = "Welcome to Kenya Airways Alumni Network!";
        var body = GetApprovalEmailTemplate(alumniName, verificationToken);

        return await SendEmailWithTrackingAsync(
            email,
            subject,
            body,
            EmailType.Approval,
            null,
            cancellationToken);
    }

    /// <summary>
    /// Send rejection email with HR contact info (Email 3)
    /// </summary>
    public async Task<bool> SendRejectionEmailAsync(
        string alumniName,
        string email,
        string staffNumber,
        CancellationToken cancellationToken = default)
    {
        var subject = "KQ Alumni Registration - Unable to Verify";
        var body = GetRejectionEmailTemplate(alumniName, staffNumber);

        return await SendEmailWithTrackingAsync(
            email,
            subject,
            body,
            EmailType.Rejection,
            null,
            cancellationToken);
    }

    /// <summary>
    /// Core email sending method with delivery tracking
    /// </summary>
    private async Task<bool> SendEmailWithTrackingAsync(
        string toEmail,
        string subject,
        string body,
        string emailType,
        Guid? registrationId,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var emailLog = new EmailLog
        {
            ToEmail = toEmail,
            Subject = subject,
            EmailType = emailType,
            RegistrationId = registrationId,
            SmtpServer = _emailSettings.SmtpServer,
            SentAt = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation(
                "[EMAIL] [{EmailType}] Sending email to {Email}",
                emailType, toEmail);

            await SendEmailAsync(toEmail, subject, body, cancellationToken);

            stopwatch.Stop();
            emailLog.DurationMs = (int)stopwatch.ElapsedMilliseconds;
            emailLog.Status = _emailSettings.UseMockEmailService ? EmailStatus.MockMode : EmailStatus.Sent;

            _logger.LogInformation(
                "[SUCCESS] [{EmailType}] Email sent successfully to {Email} in {Duration}ms",
                emailType, toEmail, emailLog.DurationMs);

            // Save to database
            await LogEmailDeliveryAsync(emailLog);

            return true;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            emailLog.DurationMs = (int)stopwatch.ElapsedMilliseconds;
            emailLog.Status = EmailStatus.Failed;
            emailLog.ErrorMessage = $"{ex.GetType().Name}: {ex.Message}";

            _logger.LogError(ex,
                "[ERROR] [{EmailType}] Failed to send email to {Email}. " +
                "Error: {ErrorMessage}. Duration: {Duration}ms",
                emailType, toEmail, ex.Message, emailLog.DurationMs);

            // Save failed attempt to database
            await LogEmailDeliveryAsync(emailLog);

            return false;
        }
    }

    /// <summary>
    /// Save email delivery log to database
    /// </summary>
    private async Task LogEmailDeliveryAsync(EmailLog emailLog)
    {
        try
        {
            _dbContext.EmailLogs.Add(emailLog);
            await _dbContext.SaveChangesAsync();

            _logger.LogDebug(
                "[DB] Email delivery logged: {EmailId} | Status: {Status} | Type: {Type} | To: {To}",
                emailLog.Id, emailLog.Status, emailLog.EmailType, emailLog.ToEmail);
        }
        catch (Exception ex)
        {
            // Don't fail email sending if logging fails
            _logger.LogWarning(ex,
                "[WARNING] Failed to log email delivery to database. Email was still sent/attempted. " +
                "Error: {ErrorMessage}",
                ex.Message);
        }
    }

    /// <summary>
    /// Core SMTP email sending method
    /// </summary>
    private async Task SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        // Mock mode - just log the email instead of actually sending
        if (_emailSettings.UseMockEmailService)
        {
            _logger.LogInformation(
                "[EMAIL] [MOCK MODE] Email would be sent:\n" +
                "   To: {To}\n" +
                "   From: {From}\n" +
                "   Subject: {Subject}\n" +
                "   Body Length: {BodyLength} characters\n" +
                "   (Email sending is disabled - UseMockEmailService = true)",
                toEmail, _emailSettings.From, subject, body.Length);

            await Task.Delay(100, cancellationToken); // Simulate network delay
            return;
        }

        // Check if email sending is disabled
        if (!_emailSettings.EnableEmailSending)
        {
            _logger.LogWarning(
                "[WARNING] Email sending is disabled (EnableEmailSending = false). " +
                "Email to {To} with subject '{Subject}' was NOT sent.",
                toEmail, subject);

            await Task.CompletedTask;
            return;
        }

        // Validate configuration
        if (string.IsNullOrEmpty(_emailSettings.SmtpServer))
            throw new InvalidOperationException("SMTP Server is not configured");

        if (string.IsNullOrEmpty(_emailSettings.Username) || string.IsNullOrEmpty(_emailSettings.Password))
            throw new InvalidOperationException("SMTP credentials are not configured");

        _logger.LogDebug(
            "[SENDING] Sending email:\n" +
            "   To: {To}\n" +
            "   Subject: {Subject}\n" +
            "   Via: {SmtpServer}:{SmtpPort}",
            toEmail, subject, _emailSettings.SmtpServer, _emailSettings.SmtpPort);

        using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
        {
            Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
            EnableSsl = _emailSettings.EnableSsl,
            Timeout = _emailSettings.TimeoutSeconds * 1000
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_emailSettings.From, _emailSettings.DisplayName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        await client.SendMailAsync(mailMessage, cancellationToken);

        _logger.LogDebug("[SMTP] Email delivered successfully via SMTP");
    }

    // Email template methods (same as original)
    private string GetConfirmationEmailTemplate(string recipientName, string registrationNumber)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f5f5f5;
        }}
        .email-container {{
            background-color: white;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            background: linear-gradient(135deg, #DC143C 0%, #B01030 100%);
            color: white;
            padding: 30px;
            text-align: center;
        }}
        .content {{
            padding: 30px;
        }}
        .footer {{
            background: #f9fafb;
            padding: 20px;
            text-align: center;
            border-top: 1px solid #e5e7eb;
            font-size: 14px;
            color: #6b7280;
        }}
        .info-box {{
            background: #f0f9ff;
            border-left: 4px solid #3b82f6;
            padding: 15px;
            margin: 20px 0;
            border-radius: 4px;
        }}
        h1 {{ margin: 0; font-size: 28px; }}
        h2 {{ color: #DC143C; font-size: 22px; }}
        ul {{ padding-left: 20px; }}
        li {{ margin-bottom: 8px; }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1>Kenya Airways Alumni Network</h1>
            <p style=""margin: 10px 0 0 0; font-size: 16px;"">Registration Received</p>
        </div>

        <div class=""content"">
            <h2>Dear {recipientName},</h2>

            <p>Thank you for registering with the <strong>Kenya Airways Alumni Association</strong>!</p>

            <p>We have received your registration and it is currently being processed.</p>

            <div class=""info-box"">
                <strong>Registration Number:</strong> {registrationNumber}<br>
                <strong>Status:</strong> Pending Verification<br>
                <strong>Submitted:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC
            </div>

            <h3>What Happens Next?</h3>
            <ul>
                <li>Your registration details are being verified against our records</li>
                <li>You will receive an approval email within 24-48 hours</li>
                <li>The approval email will contain a verification link to activate your account</li>
                <li>Once verified, you'll have full access to alumni benefits</li>
            </ul>

            <p><strong>Important:</strong> Please check your spam/junk folder if you don't see our next email.</p>

            <p>We're excited to have you as part of the KQ Alumni family!</p>

            <p style=""margin-top: 30px;"">
                Best regards,<br>
                <strong>The KQ Alumni Team</strong>
            </p>
        </div>

        <div class=""footer"">
            <p style=""margin: 0 0 10px 0;""><strong>Kenya Airways Alumni Association</strong></p>
            <p style=""margin: 0 0 10px 0;"">
                <a href=""mailto:KQ.Alumni@kenya-airways.com"" style=""color: #DC143C; text-decoration: none;"">KQ.Alumni@kenya-airways.com</a>
            </p>
            <p style=""font-size: 12px; color: #9ca3af; margin: 10px 0 0 0;"">
                This is an automated message. Please do not reply to this email.
            </p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetApprovalEmailTemplate(string recipientName, string verificationToken)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""margin: 0; padding: 0; background-color: #f5f5f5; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">
    <!-- Outer wrapper table -->
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f5f5f5; padding: 20px 0;"">
        <tr>
            <td align=""center"">
                <!-- Main content table -->
                <table width=""520"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1); max-width: 520px;"">

                    <!-- Header -->
                    <tr>
                        <td style=""background-color: #DC143C; padding: 30px 30px; text-align: center;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 22px; font-weight: 600; letter-spacing: 0.5px;"">Welcome to KQ Alumni Network!</h1>
                        </td>
                    </tr>

                    <!-- Main content -->
                    <tr>
                        <td style=""padding: 35px 30px;"">
                            <h2 style=""margin: 0 0 20px 0; color: #1a1a1a; font-size: 18px; font-weight: 600;"">Dear {recipientName},</h2>

                            <p style=""margin: 0 0 15px 0; color: #4a5568; font-size: 15px; line-height: 1.7;"">
                                We are delighted to welcome you to the <strong>Kenya Airways Alumni Association!</strong>
                            </p>

                            <p style=""margin: 0 0 20px 0; color: #4a5568; font-size: 15px; line-height: 1.7;"">
                                Your registration has been successfully approved, and your profile is now active in our Alumni Network.
                            </p>

                            <h3 style=""margin: 25px 0 15px 0; color: #DC143C; font-size: 16px; font-weight: 600;"">As a valued member, you will have access to:</h3>

                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin-bottom: 20px;"">
                                <tr>
                                    <td style=""padding: 5px 0;"">
                                        <span style=""color: #DC143C; font-size: 16px; margin-right: 8px;"">·</span>
                                        <span style=""color: #4a5568; font-size: 14px; line-height: 1.6;"">Exclusive networking events and reunions</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 5px 0;"">
                                        <span style=""color: #DC143C; font-size: 16px; margin-right: 8px;"">·</span>
                                        <span style=""color: #4a5568; font-size: 14px; line-height: 1.6;"">Alumni newsletters and updates</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 5px 0;"">
                                        <span style=""color: #DC143C; font-size: 16px; margin-right: 8px;"">·</span>
                                        <span style=""color: #4a5568; font-size: 14px; line-height: 1.6;"">Mentorship and career growth opportunities</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 5px 0;"">
                                        <span style=""color: #DC143C; font-size: 16px; margin-right: 8px;"">·</span>
                                        <span style=""color: #4a5568; font-size: 14px; line-height: 1.6;"">Opportunities to participate in CSR and community projects</span>
                                    </td>
                                </tr>
                            </table>

                            <p style=""margin: 20px 0 15px 0; color: #4a5568; font-size: 15px; line-height: 1.7;"">
                                We're proud to continue this journey with you beyond your time at Kenya Airways.
                            </p>

                            <p style=""margin: 0 0 20px 0; color: #4a5568; font-size: 15px; line-height: 1.7;"">
                                Stay tuned for upcoming activities, and don't forget to keep your profile updated with your current professional journey.
                            </p>

                            <!-- Corporate webpage link box -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f8f9fa; border: 1px solid #dee2e6; border-radius: 6px; margin: 20px 0;"">
                                <tr>
                                    <td style=""padding: 15px; text-align: center;"">
                                        <p style=""margin: 0 0 8px 0; color: #495057; font-size: 14px; font-weight: 600;"">Learn More About Our Alumni Network</p>
                                        <a href=""https://corporate.kenya-airways.com/en/alumni-network/"" style=""color: #DC143C; font-size: 14px; text-decoration: none; font-weight: 500;"">Visit Our Corporate Alumni Webpage →</a>
                                    </td>
                                </tr>
                            </table>

                            <p style=""margin: 25px 0 0 0; color: #4a5568; font-size: 15px; line-height: 1.7;"">
                                Warm regards,<br>
                                <strong style=""color: #1a1a1a;"">Kenya Airways Alumni Relations Team</strong>
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #f9fafb; padding: 20px 25px; text-align: center; border-top: 1px solid #e5e7eb;"">
                            <p style=""margin: 0 0 8px 0; font-weight: 600; color: #4a5568; font-size: 13px;"">Kenya Airways Alumni Association</p>
                            <p style=""margin: 0 0 12px 0;"">
                                <a href=""mailto:KQ.Alumni@kenya-airways.com"" style=""color: #DC143C; text-decoration: none; font-size: 13px;"">KQ.Alumni@kenya-airways.com</a>
                            </p>
                            <p style=""font-size: 11px; color: #9ca3af; margin: 0; line-height: 1.4;"">
                                This is an automated message. Please do not reply to this email.
                            </p>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private string GetRejectionEmailTemplate(string recipientName, string staffNumber)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f5f5f5;
        }}
        .email-container {{
            background-color: white;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
            color: white;
            padding: 30px;
            text-align: center;
        }}
        .content {{
            padding: 30px;
        }}
        .footer {{
            background: #f9fafb;
            padding: 20px;
            text-align: center;
            border-top: 1px solid #e5e7eb;
            font-size: 14px;
            color: #6b7280;
        }}
        .warning-box {{
            background: #fffbeb;
            border-left: 4px solid #f59e0b;
            padding: 15px;
            margin: 20px 0;
            border-radius: 4px;
        }}
        h1 {{ margin: 0; font-size: 28px; }}
        h2 {{ color: #DC143C; font-size: 22px; }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1>Kenya Airways Alumni Network</h1>
            <p style=""margin: 10px 0 0 0; font-size: 16px;"">Registration Update</p>
        </div>

        <div class=""content"">
            <h2>Dear {recipientName},</h2>

            <p>Thank you for your interest in joining the Kenya Airways Alumni Association.</p>

            <div class=""warning-box"">
                <strong>[WARNING] Registration Status:</strong> Unable to Verify<br>
                <strong>Staff Number:</strong> {staffNumber}
            </div>

            <p>We were unable to verify your staff number against our employee records. This could be due to:</p>

            <ul>
                <li>The staff number may have been entered incorrectly</li>
                <li>The records may not have been updated in our system yet</li>
                <li>You may have been employed before our digital records began</li>
            </ul>

            <h3>Next Steps:</h3>
            <p>Please contact our HR department to verify your employment record:</p>

            <p style=""background: #f9fafb; padding: 15px; border-radius: 4px;"">
                <strong>Email:</strong> <a href=""mailto:KQ.Alumni@kenya-airways.com"" style=""color: #DC143C;"">KQ.Alumni@kenya-airways.com</a><br>
                <strong>Phone:</strong> +254 20 661 6000
            </p>

            <p>We apologize for any inconvenience and look forward to welcoming you to our alumni network.</p>

            <p style=""margin-top: 30px;"">
                Best regards,<br>
                <strong>The KQ Alumni Team</strong>
            </p>
        </div>

        <div class=""footer"">
            <p style=""margin: 0 0 10px 0;""><strong>Kenya Airways Alumni Association</strong></p>
            <p style=""margin: 0 0 10px 0;"">
                <a href=""mailto:KQ.Alumni@kenya-airways.com"" style=""color: #DC143C; text-decoration: none;"">KQ.Alumni@kenya-airways.com</a>
            </p>
        </div>
    </div>
</body>
</html>";
    }
}
