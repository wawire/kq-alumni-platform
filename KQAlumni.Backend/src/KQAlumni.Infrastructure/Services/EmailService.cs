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
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _emailSettings;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger,
        IOptions<EmailSettings> emailSettings)
    {
        _configuration = configuration;
        _logger = logger;
        _emailSettings = emailSettings.Value;

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
        Guid registrationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "[EMAIL] [EMAIL 1/3] Sending CONFIRMATION email to {Email}",
                email);

            var subject = "Registration Received - KQ Alumni Network";
            var body = GetConfirmationEmailTemplate(alumniName, registrationId);

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

            var subject = "Welcome to KQ Alumni Network - Verify Your Email";
            var body = GetApprovalEmailTemplate(alumniName, verificationToken);

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

            var subject = "KQ Alumni Registration - Unable to Verify";
            var body = GetRejectionEmailTemplate(alumniName, staffNumber);

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
    private string GetConfirmationEmailTemplate(string recipientName, Guid registrationId)
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
        .info-box {{
            background: #f8f9fa;
            border: 1px solid #dee2e6;
            padding: 20px;
            margin: 25px 0;
            border-radius: 4px;
        }}
        .info-row {{
            display: block;
            padding: 8px 0;
            border-bottom: 1px solid #e9ecef;
        }}
        .info-row:last-child {{
            border-bottom: none;
        }}
        .info-label {{
            font-weight: 600;
            color: #495057;
            display: inline-block;
            min-width: 140px;
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
        .note {{
            background: #fffbf0;
            border-left: 3px solid #f59e0b;
            padding: 15px;
            margin: 20px 0;
            font-size: 14px;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1>KENYA AIRWAYS ALUMNI NETWORK</h1>
            <p style=""margin: 10px 0 0 0; font-size: 15px; opacity: 0.95;"">Registration Confirmation</p>
        </div>

        <div class=""content"">
            <h2>Dear {recipientName},</h2>

            <p>Thank you for registering with the Kenya Airways Alumni Association.</p>

            <p>We have successfully received your registration and it is currently being processed by our verification team.</p>

            <div class=""info-box"">
                <div class=""info-row"">
                    <span class=""info-label"">Registration ID:</span>
                    <span style=""color: #6c757d; font-family: monospace; font-size: 13px;"">{registrationId}</span>
                </div>
                <div class=""info-row"">
                    <span class=""info-label"">Status:</span>
                    <span style=""color: #6c757d;"">Pending Verification</span>
                </div>
                <div class=""info-row"">
                    <span class=""info-label"">Submitted:</span>
                    <span style=""color: #6c757d;"">{DateTime.UtcNow:MMMM dd, yyyy 'at' HH:mm} UTC</span>
                </div>
            </div>

            <h3>Next Steps</h3>
            <ul>
                <li>Your registration details will be verified against our employee records</li>
                <li>You will receive an approval notification within 24-48 hours</li>
                <li>The approval email will contain a verification link to activate your account</li>
                <li>Once verified, you will have full access to all alumni benefits and services</li>
            </ul>

            <div class=""note"">
                <strong>Important:</strong> Please check your spam or junk folder if you do not receive our approval email within 48 hours.
            </div>

            <p style=""margin-top: 30px; color: #4a5568;"">
                We look forward to welcoming you to the KQ Alumni community.
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

    private string GetApprovalEmailTemplate(string recipientName, string verificationToken)
    {
        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:3000";
        var verificationLink = $"{baseUrl}/verify/{verificationToken}";

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
        .button {{
            display: inline-block;
            background: #DC143C;
            color: white !important;
            padding: 16px 45px;
            text-decoration: none;
            border-radius: 4px;
            margin: 25px 0;
            font-weight: 600;
            font-size: 15px;
            letter-spacing: 0.3px;
        }}
        .button:hover {{
            background: #B01030;
        }}
        .success-box {{
            background: #f0fdf4;
            border: 1px solid #86efac;
            border-left: 4px solid #22c55e;
            padding: 18px 20px;
            margin: 25px 0;
            border-radius: 4px;
        }}
        .status-row {{
            display: block;
            padding: 6px 0;
        }}
        .status-label {{
            font-weight: 600;
            color: #166534;
            display: inline-block;
            min-width: 100px;
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
        .link-box {{
            background: #f8f9fa;
            border: 1px solid #dee2e6;
            padding: 15px;
            margin: 20px 0;
            border-radius: 4px;
            font-size: 13px;
        }}
        .expiry-note {{
            background: #fff7ed;
            border-left: 3px solid #fb923c;
            padding: 12px 15px;
            margin: 20px 0;
            font-size: 14px;
            color: #9a3412;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1>KENYA AIRWAYS ALUMNI NETWORK</h1>
            <p style=""margin: 10px 0 0 0; font-size: 15px; opacity: 0.95;"">Registration Approved</p>
        </div>

        <div class=""content"">
            <h2>Dear {recipientName},</h2>

            <p>We are pleased to inform you that your registration has been <strong>approved</strong>.</p>

            <div class=""success-box"">
                <div class=""status-row"">
                    <span class=""status-label"">Status:</span>
                    <span style=""color: #166534;"">Approved</span>
                </div>
                <div class=""status-row"">
                    <span class=""status-label"">Next Step:</span>
                    <span style=""color: #166534;"">Verify your email address</span>
                </div>
            </div>

            <p>To activate your account and access all alumni benefits, please verify your email address by clicking the button below:</p>

            <div style=""text-align: center;"">
                <a href=""{verificationLink}"" class=""button"">VERIFY EMAIL ADDRESS</a>
            </div>

            <div class=""link-box"">
                <strong style=""color: #495057; display: block; margin-bottom: 8px;"">Alternative Verification Link:</strong>
                <a href=""{verificationLink}"" style=""color: #DC143C; word-break: break-all; font-size: 12px;"">{verificationLink}</a>
            </div>

            <div class=""expiry-note"">
                <strong>Important:</strong> This verification link will expire in 30 days.
            </div>

            <h3>Alumni Benefits</h3>
            <ul>
                <li>Access to exclusive networking events and reunions</li>
                <li>Career development and mentorship opportunities</li>
                <li>Connect with fellow alumni worldwide</li>
                <li>Opportunities to contribute through volunteering and community service</li>
                <li>Regular alumni newsletters and industry updates</li>
            </ul>

            <p style=""margin-top: 30px; color: #4a5568;"">
                We look forward to your active participation in the KQ Alumni community.
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
                If you did not register for this account, please disregard this email.
            </p>
        </div>
    </div>
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
