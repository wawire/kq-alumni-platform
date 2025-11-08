using KQAlumni.Core.Entities;

namespace KQAlumni.Core.Interfaces;

/// <summary>
/// Service for managing email templates with variable substitution
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Get all email templates
    /// </summary>
    /// <param name="activeOnly">Filter for active templates only</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of email templates</returns>
    Task<List<EmailTemplate>> GetAllTemplatesAsync(
        bool activeOnly = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get template by key (e.g., "CONFIRMATION", "APPROVAL", "REJECTION")
    /// </summary>
    /// <param name="templateKey">Template key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Email template or null if not found</returns>
    Task<EmailTemplate?> GetTemplateByKeyAsync(
        string templateKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get template by ID
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Email template or null if not found</returns>
    Task<EmailTemplate?> GetTemplateByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new email template
    /// </summary>
    /// <param name="template">Template to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created template</returns>
    Task<EmailTemplate> CreateTemplateAsync(
        EmailTemplate template,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing email template
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <param name="template">Updated template data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated template</returns>
    Task<EmailTemplate> UpdateTemplateAsync(
        int id,
        EmailTemplate template,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete email template (only non-system templates)
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteTemplateAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Render email template with variable substitution
    /// </summary>
    /// <param name="templateKey">Template key</param>
    /// <param name="variables">Dictionary of variable name-value pairs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple of (subject, htmlBody)</returns>
    Task<(string Subject, string HtmlBody)> RenderTemplateAsync(
        string templateKey,
        Dictionary<string, string> variables,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Preview template with sample data
    /// </summary>
    /// <param name="subject">Subject line with variables</param>
    /// <param name="htmlBody">HTML body with variables</param>
    /// <param name="variables">Dictionary of variable name-value pairs</param>
    /// <returns>Tuple of (renderedSubject, renderedHtmlBody)</returns>
    (string Subject, string HtmlBody) PreviewTemplate(
        string subject,
        string htmlBody,
        Dictionary<string, string> variables);

    /// <summary>
    /// Seed default email templates if they don't exist
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SeedDefaultTemplatesAsync(CancellationToken cancellationToken = default);
}
