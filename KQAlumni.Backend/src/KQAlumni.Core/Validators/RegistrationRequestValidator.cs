using FluentValidation;
using KQAlumni.Core.DTOs;
using System.Text.RegularExpressions;

namespace KQAlumni.Core.Validators;

/// <summary>
/// Validator for alumni registration requests
/// Implements all validation rules from the specification
/// </summary>
public class RegistrationRequestValidator : AbstractValidator<RegistrationRequest>
{
  // ========================================
  // REGEX PATTERNS
  // ========================================

  // Staff Number: 7 chars, starts with "00", followed by 5 alphanumeric (uppercase)
  // Accepts: 0012345, 00C5050, 00RG002, 00PW057, 00EM004, 00LON01
  private const string StaffNumberPattern = @"^00[0-9A-Z]{5}$";

  // Email: Standard RFC 5322 format
  private const string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

  // Full Name: Letters, spaces, hyphens, apostrophes only
  private const string FullNamePattern = @"^[a-zA-Z\s\-']+$";

  // LinkedIn: Must contain linkedin.com
  private const string LinkedInPattern = @"^https?:\/\/(www\.)?linkedin\.com\/.*$";

  // City Custom: Letters, spaces, hyphens only
  private const string CityPattern = @"^[a-zA-Z\s\-]+$";

  // Disposable email domains to block
  private static readonly string[] DisposableEmailDomains = new[]
  {
        "tempmail.com", "guerrillamail.com", "10minutemail.com", "throwaway.email",
        "mailinator.com", "trashmail.com", "maildrop.cc", "getnada.com"
    };

  public RegistrationRequestValidator()
  {
    // ========================================
    // PERSONAL INFORMATION
    // ========================================

    RuleFor(x => x.StaffNumber)
        .NotEmpty()
        .WithMessage("Staff number is required")
        .Length(7)
        .WithMessage("Staff number must be exactly 7 characters")
        .Matches(StaffNumberPattern)
        .WithMessage("Invalid staff number format. Must be 7 characters starting with '00' (e.g., 0012345, 00C5050, 00RG002)")
        .Must(BeUpperCase)
        .WithMessage("Staff number must be in UPPERCASE");

    // ID Number (Optional - but provide validation if given)
    When(x => !string.IsNullOrEmpty(x.IdNumber), () =>
    {
      RuleFor(x => x.IdNumber)
              .MaximumLength(50)
              .WithMessage("ID number too long (max 50 characters)")
              .Matches(@"^[A-Z0-9\-]+$")
              .WithMessage("ID number can only contain letters, numbers, and hyphens");
    });

    // Passport Number (Optional - but provide validation if given)
    When(x => !string.IsNullOrEmpty(x.PassportNumber), () =>
    {
      RuleFor(x => x.PassportNumber)
              .MaximumLength(50)
              .WithMessage("Passport number too long (max 50 characters)")
              .Matches(@"^[A-Z0-9\-]+$")
              .WithMessage("Passport number can only contain letters, numbers, and hyphens");
    });

    RuleFor(x => x.FullName)
        .NotEmpty()
        .WithMessage("Full name is required")
        .Length(2, 200)
        .WithMessage("Full name must be between 2 and 200 characters")
        .Matches(FullNamePattern)
        .WithMessage("Full name can only contain letters, spaces, hyphens, and apostrophes");

    // ========================================
    // CONTACT INFORMATION
    // ========================================

    RuleFor(x => x.Email)
        .NotEmpty()
        .WithMessage("Email address is required")
        .MaximumLength(255)
        .WithMessage("Email address too long (max 255 characters)")
        .Matches(EmailPattern)
        .WithMessage("Invalid email format (e.g., name@example.com)")
        .Must(NotBeDisposableEmail)
        .WithMessage("Please use a permanent email address");

    // Mobile Number (Optional)
    When(x => !string.IsNullOrEmpty(x.MobileNumber), () =>
    {
      RuleFor(x => x.MobileNumber)
              .Length(6, 15)
              .WithMessage("Phone number must be between 6 and 15 digits")
              .Matches(@"^\d+$")
              .WithMessage("Phone number must contain only digits");
    });

    RuleFor(x => x.CurrentCountry)
        .NotEmpty()
        .WithMessage("Please select your current country")
        .MaximumLength(100)
        .WithMessage("Country name too long");

    RuleFor(x => x.CurrentCountryCode)
        .NotEmpty()
        .WithMessage("Country code is required")
        .Length(2)
        .WithMessage("Country code must be 2 characters (ISO 3166-1)")
        .Must(BeUpperCase)
        .WithMessage("Country code must be uppercase");

    RuleFor(x => x.CurrentCity)
        .NotEmpty()
        .WithMessage("Please select your current city")
        .MaximumLength(100)
        .WithMessage("City name too long");

    // City Custom (only if "Other" selected)
    When(x => x.CurrentCity == "Other" || !string.IsNullOrEmpty(x.CityCustom), () =>
    {
      RuleFor(x => x.CityCustom)
              .NotEmpty()
              .WithMessage("Please specify your city")
              .Length(2, 100)
              .WithMessage("City name must be between 2 and 100 characters")
              .Matches(CityPattern)
              .WithMessage("City name can only contain letters, spaces, and hyphens");
    });

    // ========================================
    // EMPLOYMENT INFORMATION (Optional)
    // ========================================

    When(x => !string.IsNullOrEmpty(x.CurrentEmployer), () =>
    {
      RuleFor(x => x.CurrentEmployer)
              .MaximumLength(200)
              .WithMessage("Employer name too long (max 200 characters)");
    });

    When(x => !string.IsNullOrEmpty(x.CurrentJobTitle), () =>
    {
      RuleFor(x => x.CurrentJobTitle)
              .MaximumLength(200)
              .WithMessage("Job title too long (max 200 characters)");
    });

    When(x => !string.IsNullOrEmpty(x.Industry), () =>
    {
      RuleFor(x => x.Industry)
              .MaximumLength(100)
              .WithMessage("Industry name too long (max 100 characters)");
    });

    When(x => !string.IsNullOrEmpty(x.LinkedInProfile), () =>
    {
      RuleFor(x => x.LinkedInProfile)
              .MaximumLength(500)
              .WithMessage("LinkedIn URL too long")
              .Matches(LinkedInPattern)
              .WithMessage("Please provide a valid LinkedIn profile URL");
    });

    // ========================================
    // EDUCATION & PROFESSIONAL DEVELOPMENT
    // ========================================

    RuleFor(x => x.QualificationsAttained)
        .NotEmpty()
        .WithMessage("Please select at least one qualification")
        .Must(qualifications => qualifications.Count >= 1)
        .WithMessage("Please select at least one qualification")
        .Must(qualifications => qualifications.Count <= 8)
        .WithMessage("Maximum 8 qualifications allowed");

    When(x => !string.IsNullOrEmpty(x.ProfessionalCertifications), () =>
    {
      RuleFor(x => x.ProfessionalCertifications)
              .MaximumLength(1000)
              .WithMessage("Professional certifications text too long (max 1000 characters)");
    });

    // ========================================
    // ALUMNI ENGAGEMENT
    // ========================================

    RuleFor(x => x.EngagementPreferences)
        .NotEmpty()
        .WithMessage("Please select at least one area of interest")
        .Must(preferences => preferences.Count >= 1)
        .WithMessage("Please select at least one area of interest")
        .Must(preferences => preferences.Count <= 6)
        .WithMessage("Maximum 6 areas of interest allowed");

    // ========================================
    // CONSENT & VERIFICATION
    // ========================================

    RuleFor(x => x.ConsentGiven)
        .Equal(true)
        .WithMessage("You must give consent to register");
  }

  // ========================================
  // CUSTOM VALIDATORS
  // ========================================

  private bool BeUpperCase(string value)
  {
    return value == value?.ToUpper();
  }

  private bool NotBeDisposableEmail(string email)
  {
    if (string.IsNullOrEmpty(email))
      return true;

    var domain = email.Split('@').LastOrDefault()?.ToLower();
    return !DisposableEmailDomains.Contains(domain);
  }
}
