using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace KQAlumni.API.Filters;

/// <summary>
/// Filter to exclude VerificationController from Swagger documentation.
/// This controller was deleted but may still exist in cached assemblies.
/// The verification endpoint is now handled by Minimal API in Program.cs
/// </summary>
public class ExcludeVerificationControllerFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Remove any paths that belong to VerificationController
        var pathsToRemove = swaggerDoc.Paths
            .Where(p => p.Key.Contains("/verify", StringComparison.OrdinalIgnoreCase))
            .Where(p =>
            {
                // Check if any operation comes from VerificationController
                return p.Value.Operations.Any(op =>
                    op.Value.Tags?.Any(tag =>
                        tag.Name.Contains("Verification", StringComparison.OrdinalIgnoreCase) &&
                        context.ApiDescriptions.Any(d =>
                            d.ActionDescriptor.DisplayName?.Contains("VerificationController") == true)
                    ) == true
                );
            })
            .Select(p => p.Key)
            .ToList();

        foreach (var path in pathsToRemove)
        {
            swaggerDoc.Paths.Remove(path);
        }

        // Remove VerificationController tag if it exists
        var verificationTag = swaggerDoc.Tags?
            .FirstOrDefault(t => t.Name.Equals("Verification", StringComparison.OrdinalIgnoreCase));

        if (verificationTag != null && swaggerDoc.Tags != null)
        {
            swaggerDoc.Tags.Remove(verificationTag);
        }
    }
}
