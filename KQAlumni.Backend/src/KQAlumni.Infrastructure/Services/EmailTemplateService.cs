using System.Text.RegularExpressions;
using KQAlumni.Core.Entities;
using KQAlumni.Core.Interfaces;
using KQAlumni.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KQAlumni.Infrastructure.Services;

/// <summary>
/// Service for managing email templates with variable substitution
/// </summary>
public class EmailTemplateService : IEmailTemplateService
{
    private readonly AppDbContext _context;
    private readonly ILogger<EmailTemplateService> _logger;

    public EmailTemplateService(
        AppDbContext context,
        ILogger<EmailTemplateService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all email templates
    /// </summary>
    public async Task<List<EmailTemplate>> GetAllTemplatesAsync(
        bool activeOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.EmailTemplates.AsQueryable();

        if (activeOnly)
        {
            query = query.Where(t => t.IsActive);
        }

        return await query
            .OrderBy(t => t.TemplateKey)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get template by key
    /// </summary>
    public async Task<EmailTemplate?> GetTemplateByKeyAsync(
        string templateKey,
        CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .FirstOrDefaultAsync(
                t => t.TemplateKey == templateKey.ToUpper() && t.IsActive,
                cancellationToken);
    }

    /// <summary>
    /// Get template by ID
    /// </summary>
    public async Task<EmailTemplate?> GetTemplateByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    /// <summary>
    /// Create a new email template
    /// </summary>
    public async Task<EmailTemplate> CreateTemplateAsync(
        EmailTemplate template,
        CancellationToken cancellationToken = default)
    {
        // Validate template key is unique
        var existingTemplate = await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.TemplateKey == template.TemplateKey.ToUpper(), cancellationToken);

        if (existingTemplate != null)
        {
            throw new InvalidOperationException($"Template with key '{template.TemplateKey}' already exists");
        }

        // Ensure template key is uppercase
        template.TemplateKey = template.TemplateKey.ToUpper();
        template.CreatedAt = DateTime.UtcNow;
        template.UpdatedAt = DateTime.UtcNow;

        _context.EmailTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created email template: {TemplateKey} - {Name}",
            template.TemplateKey, template.Name);

        return template;
    }

    /// <summary>
    /// Update existing email template
    /// </summary>
    public async Task<EmailTemplate> UpdateTemplateAsync(
        int id,
        EmailTemplate template,
        CancellationToken cancellationToken = default)
    {
        var existingTemplate = await GetTemplateByIdAsync(id, cancellationToken);
        if (existingTemplate == null)
        {
            throw new InvalidOperationException($"Template with ID {id} not found");
        }

        // Cannot change template key
        if (existingTemplate.TemplateKey != template.TemplateKey.ToUpper())
        {
            throw new InvalidOperationException("Cannot change template key");
        }

        // Update fields
        existingTemplate.Name = template.Name;
        existingTemplate.Description = template.Description;
        existingTemplate.Subject = template.Subject;
        existingTemplate.HtmlBody = template.HtmlBody;
        existingTemplate.AvailableVariables = template.AvailableVariables;
        existingTemplate.IsActive = template.IsActive;
        existingTemplate.UpdatedBy = template.UpdatedBy;
        existingTemplate.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Updated email template: {TemplateKey} - {Name}",
            existingTemplate.TemplateKey, existingTemplate.Name);

        return existingTemplate;
    }

    /// <summary>
    /// Delete email template (only non-system templates)
    /// </summary>
    public async Task DeleteTemplateAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var template = await GetTemplateByIdAsync(id, cancellationToken);
        if (template == null)
        {
            throw new InvalidOperationException($"Template with ID {id} not found");
        }

        if (template.IsSystemDefault)
        {
            throw new InvalidOperationException("Cannot delete system default templates");
        }

        _context.EmailTemplates.Remove(template);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Deleted email template: {TemplateKey} - {Name}",
            template.TemplateKey, template.Name);
    }

    /// <summary>
    /// Render email template with variable substitution
    /// </summary>
    public async Task<(string Subject, string HtmlBody)> RenderTemplateAsync(
        string templateKey,
        Dictionary<string, string> variables,
        CancellationToken cancellationToken = default)
    {
        var template = await GetTemplateByKeyAsync(templateKey, cancellationToken);
        if (template == null)
        {
            throw new InvalidOperationException($"Template with key '{templateKey}' not found or not active");
        }

        return PreviewTemplate(template.Subject, template.HtmlBody, variables);
    }

    /// <summary>
    /// Preview template with sample data
    /// </summary>
    public (string Subject, string HtmlBody) PreviewTemplate(
        string subject,
        string htmlBody,
        Dictionary<string, string> variables)
    {
        var renderedSubject = ReplaceVariables(subject, variables);
        var renderedHtmlBody = ReplaceVariables(htmlBody, variables);

        return (renderedSubject, renderedHtmlBody);
    }

    /// <summary>
    /// Seed default email templates if they don't exist
    /// </summary>
    public async Task SeedDefaultTemplatesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking for default email templates...");

        await SeedConfirmationTemplateAsync(cancellationToken);
        await SeedApprovalTemplateAsync(cancellationToken);
        await SeedRejectionTemplateAsync(cancellationToken);

        _logger.LogInformation("Default email templates seeding completed");
    }

    /// <summary>
    /// Replace template variables with actual values
    /// Supports {{variableName}} syntax
    /// </summary>
    private string ReplaceVariables(string template, Dictionary<string, string> variables)
    {
        if (string.IsNullOrEmpty(template))
        {
            return template;
        }

        var result = template;

        // Replace each variable
        foreach (var variable in variables)
        {
            // Support both {{variable}} and {variable} syntax
            var patterns = new[]
            {
                $"{{{{{variable.Key}}}}}",  // {{variable}}
                $"{{{variable.Key}}}"       // {variable}
            };

            foreach (var pattern in patterns)
            {
                result = result.Replace(pattern, variable.Value ?? string.Empty);
            }
        }

        return result;
    }

    /// <summary>
    /// Seed confirmation email template
    /// </summary>
    private async Task SeedConfirmationTemplateAsync(CancellationToken cancellationToken)
    {
        var existingTemplate = await GetTemplateByKeyAsync(EmailTemplateKeys.Confirmation, cancellationToken);
        if (existingTemplate != null)
        {
            _logger.LogInformation("Confirmation template already exists, skipping...");
            return;
        }

        var template = new EmailTemplate
        {
            TemplateKey = EmailTemplateKeys.Confirmation,
            Name = "Registration Confirmation Email",
            Description = "Sent immediately after user submits registration form",
            Subject = "Registration Received - KQ Alumni Network",
            AvailableVariables = "{{alumniName}}, {{registrationId}}, {{registrationNumber}}, {{currentDate}}",
            IsActive = true,
            IsSystemDefault = true,
            CreatedBy = "System",
            HtmlBody = GetDefaultConfirmationTemplate()
        };

        await CreateTemplateAsync(template, cancellationToken);
        _logger.LogInformation("Created default confirmation template");
    }

    /// <summary>
    /// Seed approval email template
    /// </summary>
    private async Task SeedApprovalTemplateAsync(CancellationToken cancellationToken)
    {
        var existingTemplate = await GetTemplateByKeyAsync(EmailTemplateKeys.Approval, cancellationToken);
        if (existingTemplate != null)
        {
            _logger.LogInformation("Approval template already exists, skipping...");
            return;
        }

        var template = new EmailTemplate
        {
            TemplateKey = EmailTemplateKeys.Approval,
            Name = "Registration Approval Email",
            Description = "Sent when registration is approved with verification link",
            Subject = "Welcome to KQ Alumni Network - Verify Your Email",
            AvailableVariables = "{{alumniName}}, {{registrationNumber}}, {{verificationLink}}",
            IsActive = true,
            IsSystemDefault = true,
            CreatedBy = "System",
            HtmlBody = GetDefaultApprovalTemplate()
        };

        await CreateTemplateAsync(template, cancellationToken);
        _logger.LogInformation("Created default approval template");
    }

    /// <summary>
    /// Seed rejection email template
    /// </summary>
    private async Task SeedRejectionTemplateAsync(CancellationToken cancellationToken)
    {
        var existingTemplate = await GetTemplateByKeyAsync(EmailTemplateKeys.Rejection, cancellationToken);
        if (existingTemplate != null)
        {
            _logger.LogInformation("Rejection template already exists, skipping...");
            return;
        }

        var template = new EmailTemplate
        {
            TemplateKey = EmailTemplateKeys.Rejection,
            Name = "Registration Rejection Email",
            Description = "Sent when registration cannot be verified",
            Subject = "KQ Alumni Registration - Unable to Verify",
            AvailableVariables = "{{alumniName}}, {{staffNumber}}, {{rejectionReason}}",
            IsActive = true,
            IsSystemDefault = true,
            CreatedBy = "System",
            HtmlBody = GetDefaultRejectionTemplate()
        };

        await CreateTemplateAsync(template, cancellationToken);
        _logger.LogInformation("Created default rejection template");
    }

    private string GetDefaultConfirmationTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }
        .email-container {
            background-color: white;
            border-radius: 4px;
            overflow: hidden;
            box-shadow: 0 2px 8px rgba(0,0,0,0.08);
        }
        .header {
            background: #DC143C;
            color: white;
            padding: 40px 30px;
            text-align: center;
        }
        .content {
            padding: 40px 30px;
        }
        .footer {
            background: #f9fafb;
            padding: 25px 30px;
            text-align: center;
            border-top: 1px solid #e5e7eb;
            font-size: 13px;
            color: #6b7280;
        }
        .info-box {
            background: #f8f9fa;
            border: 1px solid #dee2e6;
            padding: 20px;
            margin: 25px 0;
            border-radius: 4px;
        }
        h1 {
            margin: 0;
            font-size: 26px;
            font-weight: 600;
        }
        h2 {
            color: #1a1a1a;
            font-size: 20px;
            margin: 0 0 20px 0;
        }
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1>KENYA AIRWAYS ALUMNI NETWORK</h1>
            <p style=""margin: 10px 0 0 0; font-size: 15px;"">Registration Confirmation</p>
        </div>
        <div class=""content"">
            <h2>Dear {{alumniName}},</h2>
            <p>Thank you for registering with the Kenya Airways Alumni Association.</p>
            <p>We have successfully received your registration and it is currently being processed by our verification team.</p>
            <div class=""info-box"">
                <strong>Registration Number:</strong> {{registrationNumber}}<br>
                <strong>Status:</strong> Pending Verification<br>
                <strong>Submitted:</strong> {{currentDate}}
            </div>
            <p>You will receive an approval notification within 24-48 hours.</p>
        </div>
        <div class=""footer"">
            <p>Kenya Airways Alumni Association</p>
            <p><a href=""mailto:KQ.Alumni@kenya-airways.com"">KQ.Alumni@kenya-airways.com</a></p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetDefaultApprovalTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }
        .email-container {
            background-color: white;
            border-radius: 4px;
            overflow: hidden;
            box-shadow: 0 2px 8px rgba(0,0,0,0.08);
        }
        .header {
            background: #DC143C;
            color: white;
            padding: 40px 30px;
            text-align: center;
        }
        .content {
            padding: 40px 30px;
        }
        .button {
            display: inline-block;
            background: #DC143C;
            color: white !important;
            padding: 16px 45px;
            text-decoration: none;
            border-radius: 4px;
            margin: 25px 0;
            font-weight: 600;
        }
        h1 {
            margin: 0;
            font-size: 26px;
        }
        h2 {
            color: #1a1a1a;
            font-size: 20px;
        }
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1>KENYA AIRWAYS ALUMNI NETWORK</h1>
            <p style=""margin: 10px 0 0 0;"">Registration Approved</p>
        </div>
        <div class=""content"">
            <h2>Dear {{alumniName}},</h2>
            <p>We are pleased to inform you that your registration has been <strong>approved</strong>.</p>
            <p>To activate your account, please verify your email address:</p>
            <div style=""text-align: center;"">
                <a href=""{{verificationLink}}"" class=""button"">VERIFY EMAIL ADDRESS</a>
            </div>
            <p><strong>Registration Number:</strong> {{registrationNumber}}</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetDefaultRejectionTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }
        .email-container {
            background-color: white;
            border-radius: 4px;
            overflow: hidden;
            box-shadow: 0 2px 8px rgba(0,0,0,0.08);
        }
        .header {
            background: #DC143C;
            color: white;
            padding: 40px 30px;
            text-align: center;
        }
        .content {
            padding: 40px 30px;
        }
        .warning-box {
            background: #fef3c7;
            border-left: 4px solid #f59e0b;
            padding: 18px 20px;
            margin: 25px 0;
        }
        h1 {
            margin: 0;
            font-size: 26px;
        }
        h2 {
            color: #1a1a1a;
            font-size: 20px;
        }
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1>KENYA AIRWAYS ALUMNI NETWORK</h1>
            <p style=""margin: 10px 0 0 0;"">Registration Status Update</p>
        </div>
        <div class=""content"">
            <h2>Dear {{alumniName}},</h2>
            <p>Thank you for your interest in joining the Kenya Airways Alumni Association.</p>
            <div class=""warning-box"">
                <strong>Registration Status:</strong> Unable to Verify<br>
                <strong>Staff Number:</strong> {{staffNumber}}<br>
                <strong>Reason:</strong> {{rejectionReason}}
            </div>
            <p>To resolve this matter, please contact our HR department at <a href=""mailto:KQ.Alumni@kenya-airways.com"">KQ.Alumni@kenya-airways.com</a></p>
        </div>
    </div>
</body>
</html>";
    }
}
