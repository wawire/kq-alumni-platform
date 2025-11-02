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
            "📧 Email Service Initialized:\n" +
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
                "📧 [EMAIL 1/3] Sending CONFIRMATION email to {Email}",
                email);

            var subject = "Registration Received - KQ Alumni Network";
            var body = GetConfirmationEmailTemplate(alumniName, registrationId);

            await SendEmailAsync(email, subject, body, cancellationToken);

            _logger.LogInformation(
                "✅ [EMAIL 1/3] Confirmation email sent successfully to {Email}",
                email);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ [EMAIL 1/3] Failed to send confirmation email to {Email}. " +
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
                "📧 [EMAIL 2/3] Sending APPROVAL email to {Email}",
                email);

            var subject = "Welcome to KQ Alumni Network - Verify Your Email";
            var body = GetApprovalEmailTemplate(alumniName, verificationToken);

            await SendEmailAsync(email, subject, body, cancellationToken);

            _logger.LogInformation(
                "✅ [EMAIL 2/3] Approval email sent successfully to {Email}",
                email);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ [EMAIL 2/3] Failed to send approval email to {Email}. " +
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
                "📧 [EMAIL 3/3] Sending REJECTION email to {Email}",
                email);

            var subject = "KQ Alumni Registration - Unable to Verify";
            var body = GetRejectionEmailTemplate(alumniName, staffNumber);

            await SendEmailAsync(email, subject, body, cancellationToken);

            _logger.LogInformation(
                "✅ [EMAIL 3/3] Rejection email sent successfully to {Email}",
                email);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ [EMAIL 3/3] Failed to send rejection email to {Email}. " +
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
                "📧 [MOCK MODE] Email would be sent:\n" +
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
                "⚠️ Email sending is disabled (EnableEmailSending = false). " +
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
            "📤 Sending email:\n" +
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

        _logger.LogDebug("📬 Email delivered successfully");
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
            <h1>✈️ Kenya Airways Alumni Network</h1>
            <p style=""margin: 10px 0 0 0; font-size: 16px;"">Registration Received</p>
        </div>

        <div class=""content"">
            <h2>Dear {recipientName},</h2>

            <p>Thank you for registering with the <strong>Kenya Airways Alumni Association</strong>!</p>

            <p>We have received your registration and it is currently being processed.</p>

            <div class=""info-box"">
                <strong>📋 Registration ID:</strong> {registrationId}<br>
                <strong>⏰ Status:</strong> Pending Verification<br>
                <strong>📅 Submitted:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC
            </div>

            <h3>What Happens Next?</h3>
            <ul>
                <li>✅ Your registration details are being verified against our records</li>
                <li>✅ You will receive an approval email within 24-48 hours</li>
                <li>✅ The approval email will contain a verification link to activate your account</li>
                <li>✅ Once verified, you'll have full access to alumni benefits</li>
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
            background-color: #f5f5f5;
        }}
        .email-container {{
            background-color: white;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            background: linear-gradient(135deg, #10b981 0%, #059669 100%);
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
        .button {{
            display: inline-block;
            background: #DC143C;
            color: white !important;
            padding: 15px 40px;
            text-decoration: none;
            border-radius: 6px;
            margin: 20px 0;
            font-weight: bold;
            font-size: 16px;
        }}
        .success-box {{
            background: #f0fdf4;
            border-left: 4px solid #10b981;
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
            <h1>🎉 Welcome to KQ Alumni Network!</h1>
            <p style=""margin: 10px 0 0 0; font-size: 16px;"">Registration Approved</p>
        </div>

        <div class=""content"">
            <h2>Dear {recipientName},</h2>

            <p>Congratulations! Your registration has been <strong>approved</strong>.</p>

            <div class=""success-box"">
                <strong>✅ Status:</strong> Approved<br>
                <strong>📧 Next Step:</strong> Verify your email address
            </div>

            <p>To activate your account and access all alumni benefits, please verify your email address by clicking the button below:</p>

            <div style=""text-align: center;"">
                <a href=""{verificationLink}"" class=""button"">Verify My Email Address</a>
            </div>

            <p style=""font-size: 14px; color: #6b7280; padding: 15px; background: #f9fafb; border-radius: 4px;"">
                Or copy and paste this link into your browser:<br>
                <a href=""{verificationLink}"" style=""color: #DC143C; word-break: break-all;"">{verificationLink}</a>
            </p>

            <p><strong>⏰ This verification link expires in 30 days.</strong></p>

            <h3>What You'll Get:</h3>
            <ul>
                <li>✈️ Access to exclusive networking events and reunions</li>
                <li>💼 Career growth and mentorship opportunities</li>
                <li>🤝 Connect with fellow alumni worldwide</li>
                <li>❤️ Opportunities to give back through volunteering</li>
                <li>📰 Alumni newsletters and updates</li>
            </ul>

            <p style=""margin-top: 30px;"">
                Welcome to the family!<br>
                <strong>The KQ Alumni Team</strong>
            </p>
        </div>

        <div class=""footer"">
            <p style=""margin: 0 0 10px 0;""><strong>Kenya Airways Alumni Association</strong></p>
            <p style=""margin: 0 0 10px 0;"">
                <a href=""mailto:KQ.Alumni@kenya-airways.com"" style=""color: #DC143C; text-decoration: none;"">KQ.Alumni@kenya-airways.com</a>
            </p>
            <p style=""font-size: 12px; color: #9ca3af; margin: 10px 0 0 0;"">
                If you didn't register for this account, please ignore this email.
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
                <strong>⚠️ Registration Status:</strong> Unable to Verify<br>
                <strong>📋 Staff Number:</strong> {staffNumber}
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
                <strong>📧 Email:</strong> <a href=""mailto:KQ.Alumni@kenya-airways.com"" style=""color: #DC143C;"">KQ.Alumni@kenya-airways.com</a><br>
                <strong>📞 Phone:</strong> +254 20 661 6000
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
