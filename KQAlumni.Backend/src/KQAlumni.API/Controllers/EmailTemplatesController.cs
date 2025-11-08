using KQAlumni.Core.Entities;
using KQAlumni.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KQAlumni.API.Controllers;

/// <summary>
/// Email template management endpoints
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "SuperAdmin")] // Only super admins can manage templates
public class EmailTemplatesController : ControllerBase
{
    private readonly IEmailTemplateService _templateService;
    private readonly ILogger<EmailTemplatesController> _logger;

    public EmailTemplatesController(
        IEmailTemplateService templateService,
        ILogger<EmailTemplatesController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    /// <summary>
    /// Get all email templates
    /// </summary>
    /// <param name="activeOnly">Filter for active templates only</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of email templates</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<EmailTemplate>), 200)]
    public async Task<ActionResult<List<EmailTemplate>>> GetAllTemplates(
        [FromQuery] bool activeOnly = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var templates = await _templateService.GetAllTemplatesAsync(activeOnly, cancellationToken);
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email templates");
            return StatusCode(500, new { message = "Failed to retrieve email templates" });
        }
    }

    /// <summary>
    /// Get email template by ID
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Email template</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EmailTemplate), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<EmailTemplate>> GetTemplateById(
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _templateService.GetTemplateByIdAsync(id, cancellationToken);
            if (template == null)
            {
                return NotFound(new { message = $"Template with ID {id} not found" });
            }

            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template {TemplateId}", id);
            return StatusCode(500, new { message = "Failed to retrieve template" });
        }
    }

    /// <summary>
    /// Get email template by key
    /// </summary>
    /// <param name="key">Template key (CONFIRMATION, APPROVAL, REJECTION)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Email template</returns>
    [HttpGet("by-key/{key}")]
    [ProducesResponseType(typeof(EmailTemplate), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<EmailTemplate>> GetTemplateByKey(
        [FromRoute] string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _templateService.GetTemplateByKeyAsync(key, cancellationToken);
            if (template == null)
            {
                return NotFound(new { message = $"Template with key '{key}' not found" });
            }

            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template by key {TemplateKey}", key);
            return StatusCode(500, new { message = "Failed to retrieve template" });
        }
    }

    /// <summary>
    /// Create a new email template
    /// </summary>
    /// <param name="request">Template creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created template</returns>
    [HttpPost]
    [ProducesResponseType(typeof(EmailTemplate), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<EmailTemplate>> CreateTemplate(
        [FromBody] CreateTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get admin user info from claims
            var username = User.Identity?.Name ?? "Unknown";

            var template = new EmailTemplate
            {
                TemplateKey = request.TemplateKey.ToUpper(),
                Name = request.Name,
                Description = request.Description,
                Subject = request.Subject,
                HtmlBody = request.HtmlBody,
                AvailableVariables = request.AvailableVariables,
                IsActive = request.IsActive,
                CreatedBy = username
            };

            var createdTemplate = await _templateService.CreateTemplateAsync(template, cancellationToken);
            return CreatedAtAction(
                nameof(GetTemplateById),
                new { id = createdTemplate.Id },
                createdTemplate);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid template creation request");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating email template");
            return StatusCode(500, new { message = "Failed to create template" });
        }
    }

    /// <summary>
    /// Update existing email template
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <param name="request">Template update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated template</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(EmailTemplate), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<EmailTemplate>> UpdateTemplate(
        [FromRoute] int id,
        [FromBody] UpdateTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get admin user info from claims
            var username = User.Identity?.Name ?? "Unknown";

            var template = new EmailTemplate
            {
                TemplateKey = request.TemplateKey.ToUpper(),
                Name = request.Name,
                Description = request.Description,
                Subject = request.Subject,
                HtmlBody = request.HtmlBody,
                AvailableVariables = request.AvailableVariables,
                IsActive = request.IsActive,
                UpdatedBy = username
            };

            var updatedTemplate = await _templateService.UpdateTemplateAsync(id, template, cancellationToken);
            return Ok(updatedTemplate);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid template update request for ID {TemplateId}", id);
            return ex.Message.Contains("not found") ? NotFound(new { message = ex.Message }) : BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template {TemplateId}", id);
            return StatusCode(500, new { message = "Failed to update template" });
        }
    }

    /// <summary>
    /// Delete email template (only non-system templates)
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteTemplate(
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _templateService.DeleteTemplateAsync(id, cancellationToken);
            return Ok(new { message = "Template deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid template deletion request for ID {TemplateId}", id);
            return ex.Message.Contains("not found") ? NotFound(new { message = ex.Message }) : BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template {TemplateId}", id);
            return StatusCode(500, new { message = "Failed to delete template" });
        }
    }

    /// <summary>
    /// Preview template with sample data
    /// </summary>
    /// <param name="request">Preview request with template and variables</param>
    /// <returns>Rendered template</returns>
    [HttpPost("preview")]
    [ProducesResponseType(typeof(PreviewResponse), 200)]
    public ActionResult<PreviewResponse> PreviewTemplate(
        [FromBody] PreviewRequest request)
    {
        try
        {
            var (subject, htmlBody) = _templateService.PreviewTemplate(
                request.Subject,
                request.HtmlBody,
                request.Variables);

            return Ok(new PreviewResponse
            {
                Subject = subject,
                HtmlBody = htmlBody
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing template");
            return StatusCode(500, new { message = "Failed to preview template" });
        }
    }
}

// ============================================
// Request/Response DTOs
// ============================================

public class CreateTemplateRequest
{
    public string TemplateKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? AvailableVariables { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateTemplateRequest
{
    public string TemplateKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? AvailableVariables { get; set; }
    public bool IsActive { get; set; } = true;
}

public class PreviewRequest
{
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public Dictionary<string, string> Variables { get; set; } = new();
}

public class PreviewResponse
{
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
}
