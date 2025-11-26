using System.Net;
using System.Net.Mail;
using KQAlumni.Core.Entities;
using KQAlumni.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KQAlumni.Infrastructure.Services;

/// <summary>
/// Email service for sending confirmation and approval emails
/// Now uses customizable templates from database
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _emailSettings;
    private readonly IEmailTemplateService _templateService;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger,
        IOptions<EmailSettings> emailSettings,
        IEmailTemplateService templateService)
    {
        _configuration = configuration;
        _logger = logger;
        _emailSettings = emailSettings.Value;
        _templateService = templateService;

        _logger.LogInformation(
            "[EMAIL] Email Service Initialized:\n" +
            "   Host: {Host}\n" +
            "   Port: {Port}\n" +
            "   From: {From}\n" +
            "   SSL: {Ssl}",
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
        try
        {
            _logger.LogInformation(
                "[EMAIL] [EMAIL 1/3] Sending CONFIRMATION email to {Email} (Registration: {RegistrationNumber})",
                email, registrationNumber);

            // Try to use database template first
            string subject;
            string body;

            try
            {
                var variables = new Dictionary<string, string>
                {
                    { "alumniName", alumniName },
                    { "registrationNumber", registrationNumber },
                    { "currentDate", DateTime.UtcNow.ToString("MMMM dd, yyyy 'at' HH:mm UTC") }
                };

                (subject, body) = await _templateService.RenderTemplateAsync(
                    EmailTemplateKeys.Confirmation,
                    variables,
                    cancellationToken);

                _logger.LogDebug("[EMAIL] Using customizable template for confirmation email");
            }
            catch (Exception templateEx)
            {
                _logger.LogWarning(templateEx,
                    "[EMAIL] Could not load custom template, using default hardcoded template");

                subject = "Registration Received - KQ Alumni Network";
                body = GetConfirmationEmailTemplate(alumniName, registrationNumber);
            }

            await SendEmailAsync(email, subject, body, cancellationToken);

            _logger.LogInformation(
                "[SUCCESS] [EMAIL 1/3] Confirmation email sent successfully to {Email}",
                email);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[ERROR] [EMAIL 1/3] Failed to send confirmation email to {Email}. " +
                "Error: {ErrorMessage}",
                email, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Send approval email with verification link (Email 2)
    /// </summary>
    public async Task<bool> SendApprovalEmailAsync(
        string alumniName,
        string email,
        string verificationToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "[EMAIL] [EMAIL 2/3] Sending APPROVAL email to {Email}",
                email);

            // Try to use database template first
            string subject;
            string body;

            try
            {
                var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:3000";
                var verificationLink = $"{baseUrl}/verify/{verificationToken}";

                var variables = new Dictionary<string, string>
                {
                    { "alumniName", alumniName },
                    { "verificationLink", verificationLink },
                    { "verificationToken", verificationToken },
                    { "registrationNumber", "N/A" } // Can be enhanced to include actual registration number
                };

                (subject, body) = await _templateService.RenderTemplateAsync(
                    EmailTemplateKeys.Approval,
                    variables,
                    cancellationToken);

                _logger.LogDebug("[EMAIL] Using customizable template for approval email");
            }
            catch (Exception templateEx)
            {
                _logger.LogWarning(templateEx,
                    "[EMAIL] Could not load custom template, using default hardcoded template");

                subject = "Welcome to KQ Alumni Network - Verify Your Email";
                body = GetApprovalEmailTemplate(alumniName, verificationToken);
            }

            await SendEmailAsync(email, subject, body, cancellationToken);

            _logger.LogInformation(
                "[SUCCESS] [EMAIL 2/3] Approval email sent successfully to {Email}",
                email);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[ERROR] [EMAIL 2/3] Failed to send approval email to {Email}. " +
                "Error: {ErrorMessage}",
                email, ex.Message);
            return false;
        }
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
        try
        {
            _logger.LogInformation(
                "[EMAIL] [EMAIL 3/3] Sending REJECTION email to {Email}",
                email);

            // Try to use database template first
            string subject;
            string body;

            try
            {
                var variables = new Dictionary<string, string>
                {
                    { "alumniName", alumniName },
                    { "staffNumber", staffNumber },
                    { "rejectionReason", "Unable to verify staff number against our records" }
                };

                (subject, body) = await _templateService.RenderTemplateAsync(
                    EmailTemplateKeys.Rejection,
                    variables,
                    cancellationToken);

                _logger.LogDebug("[EMAIL] Using customizable template for rejection email");
            }
            catch (Exception templateEx)
            {
                _logger.LogWarning(templateEx,
                    "[EMAIL] Could not load custom template, using default hardcoded template");

                subject = "KQ Alumni Registration - Unable to Verify";
                body = GetRejectionEmailTemplate(alumniName, staffNumber);
            }

            await SendEmailAsync(email, subject, body, cancellationToken);

            _logger.LogInformation(
                "[SUCCESS] [EMAIL 3/3] Rejection email sent successfully to {Email}",
                email);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[ERROR] [EMAIL 3/3] Failed to send rejection email to {Email}. " +
                "Error: {ErrorMessage}",
                email, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Core email sending method
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

            await Task.CompletedTask;
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
            Timeout = 30000 // 30 seconds
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

        _logger.LogDebug("[SMTP] Email delivered successfully");
    }

    // ... (keep all the email template methods from before)
    private string GetConfirmationEmailTemplate(string recipientName, string registrationNumber)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""margin: 0; padding: 0; background-color: #f5f5f5; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">
    <!-- Outer wrapper table for full background -->
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f5f5f5; padding: 20px 0;"">
        <tr>
            <td align=""center"">
                <!-- Main content table with max width -->
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1); max-width: 600px;"">

                    <!-- Header with Kenya Airways Red -->
                    <tr>
                        <td style=""background-color: #DC143C; padding: 35px 30px; text-align: center;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 24px; font-weight: 600; letter-spacing: 0.5px;"">Kenya Airways Alumni Network</h1>
                            <p style=""margin: 8px 0 0 0; color: #ffffff; font-size: 14px; opacity: 0.95;"">Registration Received</p>
                        </td>
                    </tr>

                    <!-- Main content -->
                    <tr>
                        <td style=""padding: 40px 35px;"">
                            <h2 style=""margin: 0 0 20px 0; color: #1a1a1a; font-size: 20px; font-weight: 600;"">Dear {recipientName},</h2>

                            <p style=""margin: 0 0 15px 0; color: #4a5568; font-size: 15px; line-height: 1.6;"">
                                Thank you for registering with the <strong>Kenya Airways Alumni Association!</strong>
                            </p>

                            <p style=""margin: 0 0 25px 0; color: #4a5568; font-size: 15px; line-height: 1.6;"">
                                We have received your registration and it is currently being processed.
                            </p>

                            <!-- Info box -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f8f9fa; border: 1px solid #dee2e6; border-radius: 6px; margin: 25px 0;"">
                                <tr>
                                    <td style=""padding: 20px;"">
                                        <table width=""100%"" cellpadding=""8"" cellspacing=""0"" border=""0"">
                                            <tr>
                                                <td style=""padding: 8px 0; border-bottom: 1px solid #e9ecef;"">
                                                    <span style=""font-weight: 600; color: #495057; font-size: 14px;"">Registration Number:</span><br>
                                                    <span style=""color: #DC143C; font-family: 'Courier New', monospace; font-size: 16px; font-weight: 600;"">{registrationNumber}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding: 8px 0; border-bottom: 1px solid #e9ecef;"">
                                                    <span style=""font-weight: 600; color: #495057; font-size: 14px;"">Status:</span><br>
                                                    <span style=""color: #6c757d; font-size: 14px;"">Pending Verification</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding: 8px 0;"">
                                                    <span style=""font-weight: 600; color: #495057; font-size: 14px;"">Submitted:</span><br>
                                                    <span style=""color: #6c757d; font-size: 14px;"">{DateTime.UtcNow:MMMM dd, yyyy} at {DateTime.UtcNow:HH:mm} UTC</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <h3 style=""margin: 30px 0 15px 0; color: #DC143C; font-size: 16px; font-weight: 600;"">What Happens Next?</h3>

                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                                <tr>
                                    <td style=""padding: 8px 0;"">
                                        <span style=""color: #DC143C; font-size: 18px; margin-right: 10px;"">•</span>
                                        <span style=""color: #4a5568; font-size: 14px; line-height: 1.6;"">Your registration details are being verified against our records</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 8px 0;"">
                                        <span style=""color: #DC143C; font-size: 18px; margin-right: 10px;"">•</span>
                                        <span style=""color: #4a5568; font-size: 14px; line-height: 1.6;"">You will receive an approval email within 24-48 hours</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 8px 0;"">
                                        <span style=""color: #DC143C; font-size: 18px; margin-right: 10px;"">•</span>
                                        <span style=""color: #4a5568; font-size: 14px; line-height: 1.6;"">The approval email will contain a verification link to activate your account</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 8px 0;"">
                                        <span style=""color: #DC143C; font-size: 18px; margin-right: 10px;"">•</span>
                                        <span style=""color: #4a5568; font-size: 14px; line-height: 1.6;"">Once verified, you'll have full access to alumni benefits</span>
                                    </td>
                                </tr>
                            </table>

                            <!-- Important note -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin: 25px 0;"">
                                <tr>
                                    <td style=""background-color: #fffbf0; border-left: 3px solid #f59e0b; padding: 15px; border-radius: 4px;"">
                                        <p style=""margin: 0; color: #92400e; font-size: 14px; line-height: 1.6;"">
                                            <strong>Important:</strong> Please check your spam/junk folder if you don't see our next email within 48 hours.
                                        </p>
                                    </td>
                                </tr>
                            </table>

                            <p style=""margin: 30px 0 0 0; color: #4a5568; font-size: 15px; line-height: 1.6;"">
                                We're excited to have you as part of the KQ Alumni family!
                            </p>

                            <p style=""margin: 25px 0 0 0; color: #4a5568; font-size: 15px; line-height: 1.6;"">
                                Best regards,<br>
                                <strong style=""color: #1a1a1a;"">The KQ Alumni Team</strong>
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #f9fafb; padding: 25px 30px; text-align: center; border-top: 1px solid #e5e7eb;"">
                            <p style=""margin: 0 0 8px 0; font-weight: 600; color: #4a5568; font-size: 14px;"">Kenya Airways Alumni Association</p>
                            <p style=""margin: 0 0 15px 0;"">
                                <a href=""mailto:KQ.Alumni@kenya-airways.com"" style=""color: #DC143C; text-decoration: none; font-size: 14px;"">KQ.Alumni@kenya-airways.com</a>
                            </p>
                            <p style=""font-size: 12px; color: #9ca3af; margin: 0; line-height: 1.4;"">
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

    private string GetApprovalEmailTemplate(string recipientName, string verificationToken)
    {
        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:3000";
        var verificationLink = $"{baseUrl}/verify/{verificationToken}";

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
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1); max-width: 600px;"">

                    <!-- Header -->
                    <tr>
                        <td style=""background-color: #DC143C; padding: 35px 30px; text-align: center;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 24px; font-weight: 600; letter-spacing: 0.5px;"">Kenya Airways Alumni Network</h1>
                            <p style=""margin: 8px 0 0 0; color: #ffffff; font-size: 14px; opacity: 0.95;"">Welcome! Your Registration is Approved 🎉</p>
                        </td>
                    </tr>

                    <!-- Main content -->
                    <tr>
                        <td style=""padding: 40px 35px;"">
                            <h2 style=""margin: 0 0 20px 0; color: #1a1a1a; font-size: 20px; font-weight: 600;"">Dear {recipientName},</h2>

                            <p style=""margin: 0 0 15px 0; color: #4a5568; font-size: 15px; line-height: 1.6;"">
                                Congratulations! We are pleased to inform you that your registration has been <strong style=""color: #22c55e;"">approved</strong>.
                            </p>

                            <!-- Success box -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f0fdf4; border: 1px solid #86efac; border-left: 4px solid #22c55e; border-radius: 6px; margin: 25px 0;"">
                                <tr>
                                    <td style=""padding: 20px;"">
                                        <table width=""100%"" cellpadding=""6"" cellspacing=""0"" border=""0"">
                                            <tr>
                                                <td style=""font-weight: 600; color: #166534; font-size: 14px;"">Status:</td>
                                                <td style=""color: #166534; font-size: 14px; text-align: right;"">✓ Approved</td>
                                            </tr>
                                            <tr>
                                                <td style=""font-weight: 600; color: #166534; font-size: 14px;"">Next Step:</td>
                                                <td style=""color: #166534; font-size: 14px; text-align: right;"">Verify your email</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <p style=""margin: 0 0 25px 0; color: #4a5568; font-size: 15px; line-height: 1.6;"">
                                To activate your account and access all alumni benefits, please verify your email address by clicking the button below:
                            </p>

                            <!-- Verify button -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                                <tr>
                                    <td align=""center"" style=""padding: 20px 0;"">
                                        <table cellpadding=""0"" cellspacing=""0"" border=""0"">
                                            <tr>
                                                <td align=""center"" style=""background-color: #DC143C; border-radius: 6px;"">
                                                    <a href=""{verificationLink}"" style=""display: inline-block; padding: 16px 45px; color: #ffffff; text-decoration: none; font-weight: 600; font-size: 15px; letter-spacing: 0.5px;"">VERIFY EMAIL ADDRESS</a>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <!-- Alternative link -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f8f9fa; border: 1px solid #dee2e6; border-radius: 6px; margin: 20px 0;"">
                                <tr>
                                    <td style=""padding: 15px;"">
                                        <p style=""margin: 0 0 8px 0; color: #495057; font-size: 13px; font-weight: 600;"">Alternative Verification Link:</p>
                                        <a href=""{verificationLink}"" style=""color: #DC143C; word-break: break-all; font-size: 12px; text-decoration: none;"">{verificationLink}</a>
                                    </td>
                                </tr>
                            </table>

                            <!-- Expiry note -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin: 20px 0;"">
                                <tr>
                                    <td style=""background-color: #fff7ed; border-left: 3px solid #fb923c; padding: 12px 15px; border-radius: 4px;"">
                                        <p style=""margin: 0; color: #9a3412; font-size: 14px; line-height: 1.6;"">
                                            <strong>Important:</strong> This verification link will expire in 30 days.
                                        </p>
                                    </td>
                                </tr>
                            </table>

                            <h3 style=""margin: 30px 0 15px 0; color: #DC143C; font-size: 16px; font-weight: 600;"">Your Alumni Benefits</h3>

                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                                <tr>
                                    <td style=""padding: 6px 0;"">
                                        <span style=""color: #DC143C; font-size: 18px; margin-right: 10px;"">✓</span>
                                        <span style=""color: #4a5568; font-size: 14px; line-height: 1.6;"">Access to exclusive networking events and reunions</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 6px 0;"">
                                        <span style=""color: #DC143C; font-size: 18px; margin-right: 10px;"">✓</span>
                                        <span style=""color: #4a5568; font-size: 14px; line-height: 1.6;"">Career development and mentorship opportunities</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 6px 0;"">
                                        <span style=""color: #DC143C; font-size: 18px; margin-right: 10px;"">✓</span>
                                        <span style=""color: #4a5568; font-size: 14px; line-height: 1.6;"">Connect with fellow alumni worldwide</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 6px 0;"">
                                        <span style=""color: #DC143C; font-size: 18px; margin-right: 10px;"">✓</span>
                                        <span style=""color: #4a5568; font-size: 14px; line-height: 1.6;"">Volunteering and community service opportunities</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 6px 0;"">
                                        <span style=""color: #DC143C; font-size: 18px; margin-right: 10px;"">✓</span>
                                        <span style=""color: #4a5568; font-size: 14px; line-height: 1.6;"">Regular alumni newsletters and industry updates</span>
                                    </td>
                                </tr>
                            </table>

                            <p style=""margin: 30px 0 0 0; color: #4a5568; font-size: 15px; line-height: 1.6;"">
                                We look forward to your active participation in the KQ Alumni community!
                            </p>

                            <p style=""margin: 25px 0 0 0; color: #4a5568; font-size: 15px; line-height: 1.6;"">
                                Best regards,<br>
                                <strong style=""color: #1a1a1a;"">The KQ Alumni Team</strong>
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #f9fafb; padding: 25px 30px; text-align: center; border-top: 1px solid #e5e7eb;"">
                            <p style=""margin: 0 0 8px 0; font-weight: 600; color: #4a5568; font-size: 14px;"">Kenya Airways Alumni Association</p>
                            <p style=""margin: 0 0 15px 0;"">
                                <a href=""mailto:KQ.Alumni@kenya-airways.com"" style=""color: #DC143C; text-decoration: none; font-size: 14px;"">KQ.Alumni@kenya-airways.com</a>
                            </p>
                            <p style=""font-size: 12px; color: #9ca3af; margin: 0; line-height: 1.4;"">
                                If you did not register for this account, please disregard this email.
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
            background-color: #f4f4f4;
        }}
        .email-container {{
            background-color: white;
            border-radius: 4px;
            overflow: hidden;
            box-shadow: 0 2px 8px rgba(0,0,0,0.08);
        }}
        .header {{
            background: #DC143C;
            color: white;
            padding: 40px 30px;
            text-align: center;
        }}
        .content {{
            padding: 40px 30px;
        }}
        .footer {{
            background: #f9fafb;
            padding: 25px 30px;
            text-align: center;
            border-top: 1px solid #e5e7eb;
            font-size: 13px;
            color: #6b7280;
        }}
        .warning-box {{
            background: #fef3c7;
            border: 1px solid #fcd34d;
            border-left: 4px solid #f59e0b;
            padding: 18px 20px;
            margin: 25px 0;
            border-radius: 4px;
        }}
        .info-row {{
            display: block;
            padding: 6px 0;
        }}
        .info-label {{
            font-weight: 600;
            color: #92400e;
            display: inline-block;
            min-width: 140px;
        }}
        .contact-box {{
            background: #f8f9fa;
            border: 1px solid #dee2e6;
            padding: 20px;
            margin: 20px 0;
            border-radius: 4px;
        }}
        .contact-item {{
            padding: 8px 0;
            display: block;
        }}
        h1 {{
            margin: 0;
            font-size: 26px;
            font-weight: 600;
            letter-spacing: 0.5px;
        }}
        h2 {{
            color: #1a1a1a;
            font-size: 20px;
            font-weight: 600;
            margin: 0 0 20px 0;
        }}
        h3 {{
            color: #DC143C;
            font-size: 16px;
            font-weight: 600;
            margin: 30px 0 15px 0;
        }}
        p {{
            margin: 0 0 15px 0;
            color: #4a5568;
        }}
        ul {{
            padding-left: 20px;
            margin: 15px 0;
        }}
        li {{
            margin-bottom: 10px;
            color: #4a5568;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1>KENYA AIRWAYS ALUMNI NETWORK</h1>
            <p style=""margin: 10px 0 0 0; font-size: 15px; opacity: 0.95;"">Registration Status Update</p>
        </div>

        <div class=""content"">
            <h2>Dear {recipientName},</h2>

            <p>Thank you for your interest in joining the Kenya Airways Alumni Association.</p>

            <div class=""warning-box"">
                <div class=""info-row"">
                    <span class=""info-label"">Registration Status:</span>
                    <span style=""color: #92400e;"">Unable to Verify</span>
                </div>
                <div class=""info-row"">
                    <span class=""info-label"">Staff Number:</span>
                    <span style=""color: #92400e; font-family: monospace;"">{staffNumber}</span>
                </div>
            </div>

            <p>We were unable to verify your staff number against our employee records. This situation may occur due to the following reasons:</p>

            <ul>
                <li>The staff number may have been entered incorrectly</li>
                <li>Employment records may not have been updated in our system</li>
                <li>Employment may predate our digital record-keeping system</li>
                <li>There may be a discrepancy requiring manual verification</li>
            </ul>

            <h3>Next Steps</h3>
            <p>To resolve this matter, please contact our HR department with your employment details:</p>

            <div class=""contact-box"">
                <div class=""contact-item"">
                    <strong style=""color: #495057;"">Email:</strong>
                    <a href=""mailto:KQ.Alumni@kenya-airways.com"" style=""color: #DC143C; text-decoration: none;"">KQ.Alumni@kenya-airways.com</a>
                </div>
                <div class=""contact-item"">
                    <strong style=""color: #495057;"">Phone:</strong>
                    <span style=""color: #6c757d;"">+254 20 661 6000</span>
                </div>
            </div>

            <p>Our HR team will assist in verifying your employment history and guide you through the registration process.</p>

            <p style=""margin-top: 30px; color: #4a5568;"">
                We apologize for any inconvenience and look forward to welcoming you to the KQ Alumni community.
            </p>

            <p style=""margin-top: 25px; color: #4a5568;"">
                Best regards,<br>
                <strong style=""color: #1a1a1a;"">Kenya Airways Alumni Team</strong>
            </p>
        </div>

        <div class=""footer"">
            <p style=""margin: 0 0 8px 0; font-weight: 600; color: #4a5568;"">Kenya Airways Alumni Association</p>
            <p style=""margin: 0 0 15px 0;"">
                <a href=""mailto:KQ.Alumni@kenya-airways.com"" style=""color: #DC143C; text-decoration: none;"">KQ.Alumni@kenya-airways.com</a>
            </p>
            <p style=""font-size: 12px; color: #9ca3af; margin: 0;"">
                This is an automated message. Please do not reply to this email.
            </p>
        </div>
    </div>
</body>
</html>";
    }
}
