using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KQAlumni.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixNullStringValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix NULL values in required string columns by updating them to empty strings
            // This prevents SqlNullValueException when Entity Framework tries to materialize entities

            // AlumniRegistrations table
            migrationBuilder.Sql(@"
                UPDATE AlumniRegistrations
                SET RegistrationNumber = ''
                WHERE RegistrationNumber IS NULL;

                UPDATE AlumniRegistrations
                SET IdNumber = ''
                WHERE IdNumber IS NULL;

                UPDATE AlumniRegistrations
                SET FullName = ''
                WHERE FullName IS NULL;

                UPDATE AlumniRegistrations
                SET Email = ''
                WHERE Email IS NULL;

                UPDATE AlumniRegistrations
                SET CurrentCountry = ''
                WHERE CurrentCountry IS NULL;

                UPDATE AlumniRegistrations
                SET CurrentCountryCode = ''
                WHERE CurrentCountryCode IS NULL;

                UPDATE AlumniRegistrations
                SET CurrentCity = ''
                WHERE CurrentCity IS NULL;

                UPDATE AlumniRegistrations
                SET QualificationsAttained = '[]'
                WHERE QualificationsAttained IS NULL;

                UPDATE AlumniRegistrations
                SET EngagementPreferences = '[]'
                WHERE EngagementPreferences IS NULL;

                UPDATE AlumniRegistrations
                SET RegistrationStatus = 'Pending'
                WHERE RegistrationStatus IS NULL;
            ");

            // EmailLogs table
            migrationBuilder.Sql(@"
                UPDATE EmailLogs
                SET ToEmail = ''
                WHERE ToEmail IS NULL;

                UPDATE EmailLogs
                SET Subject = ''
                WHERE Subject IS NULL;

                UPDATE EmailLogs
                SET EmailType = ''
                WHERE EmailType IS NULL;

                UPDATE EmailLogs
                SET Status = ''
                WHERE Status IS NULL;
            ");

            // EmailTemplates table
            migrationBuilder.Sql(@"
                UPDATE EmailTemplates
                SET TemplateKey = ''
                WHERE TemplateKey IS NULL;

                UPDATE EmailTemplates
                SET Name = ''
                WHERE Name IS NULL;

                UPDATE EmailTemplates
                SET Subject = ''
                WHERE Subject IS NULL;

                UPDATE EmailTemplates
                SET HtmlBody = ''
                WHERE HtmlBody IS NULL;
            ");

            // AdminUsers table
            migrationBuilder.Sql(@"
                UPDATE AdminUsers
                SET Username = ''
                WHERE Username IS NULL;

                UPDATE AdminUsers
                SET Email = ''
                WHERE Email IS NULL;

                UPDATE AdminUsers
                SET PasswordHash = ''
                WHERE PasswordHash IS NULL;

                UPDATE AdminUsers
                SET Role = 'HROfficer'
                WHERE Role IS NULL;

                UPDATE AdminUsers
                SET FullName = ''
                WHERE FullName IS NULL;
            ");

            // AuditLogs table
            migrationBuilder.Sql(@"
                UPDATE AuditLogs
                SET Action = ''
                WHERE Action IS NULL;

                UPDATE AuditLogs
                SET PerformedBy = ''
                WHERE PerformedBy IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This is a data migration - we cannot reliably reverse it
            // as we don't know which empty strings were originally NULL
        }
    }
}
