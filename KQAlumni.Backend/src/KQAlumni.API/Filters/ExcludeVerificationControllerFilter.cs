using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace KQAlumni.API.Filters;

/// <summary>
/// Filter to exclude VerificationController from Swagger documentation.
/// This controller was deleted but may still exist in cached assemblies.
/// </summary>
public class ExcludeVerificationControllerFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        try
        {
            // Remove VerificationController tag if it exists
            if (swaggerDoc.Tags != null)
            {
                var verificationTag = swaggerDoc.Tags
                    .FirstOrDefault(t => t.Name?.Equals("Verification", StringComparison.OrdinalIgnoreCase) == true);

                if (verificationTag != null)
                {
                    swaggerDoc.Tags.Remove(verificationTag);
                }
            }

            // Remove paths that have Verification tag but not from RegistrationsController
            var pathsToRemove = new List<string>();

            foreach (var path in swaggerDoc.Paths)
            {
                var hasVerificationTag = path.Value.Operations.Values
                    .Any(op => op.Tags?.Any(tag =>
                        tag.Name?.Equals("Verification", StringComparison.OrdinalIgnoreCase) == true) == true);

                // Only remove if it has Verification tag (from old VerificationController)
                // Keep the verify endpoint from RegistrationsController
                if (hasVerificationTag)
                {
                    // Check if this is from RegistrationsController
                    var isFromRegistrations = path.Value.Operations.Values
                        .Any(op => op.Tags?.Any(tag =>
                            tag.Name?.Equals("Registrations", StringComparison.OrdinalIgnoreCase) == true) == true);

                    if (!isFromRegistrations)
                    {
                        pathsToRemove.Add(path.Key);
                    }
                }
            }

            foreach (var path in pathsToRemove)
            {
                swaggerDoc.Paths.Remove(path);
            }
        }
        catch (Exception)
        {
            // Silently fail to avoid breaking Swagger generation
            // The filter is a safety net for cached assemblies only
        }
    }
}
