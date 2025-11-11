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
            // PERMANENT FIX: Part 1 - Fix existing NULL values
            // Part 2 - Add DEFAULT constraints to prevent future NULLs

            // ========== PART 1: FIX EXISTING NULL DATA ==========

            // AlumniRegistrations table - Fix existing NULLs
            migrationBuilder.Sql(@"
                UPDATE AlumniRegistrations SET RegistrationNumber = '' WHERE RegistrationNumber IS NULL;
                UPDATE AlumniRegistrations SET IdNumber = '' WHERE IdNumber IS NULL;
                UPDATE AlumniRegistrations SET FullName = '' WHERE FullName IS NULL;
                UPDATE AlumniRegistrations SET Email = '' WHERE Email IS NULL;
                UPDATE AlumniRegistrations SET CurrentCountry = '' WHERE CurrentCountry IS NULL;
                UPDATE AlumniRegistrations SET CurrentCountryCode = '' WHERE CurrentCountryCode IS NULL;
                UPDATE AlumniRegistrations SET CurrentCity = '' WHERE CurrentCity IS NULL;
                UPDATE AlumniRegistrations SET QualificationsAttained = '[]' WHERE QualificationsAttained IS NULL;
                UPDATE AlumniRegistrations SET EngagementPreferences = '[]' WHERE EngagementPreferences IS NULL;
                UPDATE AlumniRegistrations SET RegistrationStatus = 'Pending' WHERE RegistrationStatus IS NULL;
            ");

            // EmailLogs table - Fix existing NULLs
            migrationBuilder.Sql(@"
                UPDATE EmailLogs SET ToEmail = '' WHERE ToEmail IS NULL;
                UPDATE EmailLogs SET Subject = '' WHERE Subject IS NULL;
                UPDATE EmailLogs SET EmailType = '' WHERE EmailType IS NULL;
                UPDATE EmailLogs SET Status = '' WHERE Status IS NULL;
            ");

            // EmailTemplates table - Fix existing NULLs
            migrationBuilder.Sql(@"
                UPDATE EmailTemplates SET TemplateKey = '' WHERE TemplateKey IS NULL;
                UPDATE EmailTemplates SET Name = '' WHERE Name IS NULL;
                UPDATE EmailTemplates SET Subject = '' WHERE Subject IS NULL;
                UPDATE EmailTemplates SET HtmlBody = '' WHERE HtmlBody IS NULL;
            ");

            // AdminUsers table - Fix existing NULLs
            migrationBuilder.Sql(@"
                UPDATE AdminUsers SET Username = '' WHERE Username IS NULL;
                UPDATE AdminUsers SET Email = '' WHERE Email IS NULL;
                UPDATE AdminUsers SET PasswordHash = '' WHERE PasswordHash IS NULL;
                UPDATE AdminUsers SET Role = 'HROfficer' WHERE Role IS NULL;
                UPDATE AdminUsers SET FullName = '' WHERE FullName IS NULL;
            ");

            // AuditLogs table - Fix existing NULLs
            migrationBuilder.Sql(@"
                UPDATE AuditLogs SET Action = '' WHERE Action IS NULL;
                UPDATE AuditLogs SET PerformedBy = '' WHERE PerformedBy IS NULL;
            ");

            // ========== PART 2: ADD DEFAULT CONSTRAINTS (PERMANENT PROTECTION) ==========

            // AlumniRegistrations - Add defaults for required columns
            migrationBuilder.AlterColumn<string>(
                name: "RegistrationNumber",
                table: "AlumniRegistrations",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "IdNumber",
                table: "AlumniRegistrations",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AlumniRegistrations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AlumniRegistrations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "CurrentCountry",
                table: "AlumniRegistrations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CurrentCountryCode",
                table: "AlumniRegistrations",
                type: "varchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(2)",
                oldMaxLength: 2);

            migrationBuilder.AlterColumn<string>(
                name: "CurrentCity",
                table: "AlumniRegistrations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "QualificationsAttained",
                table: "AlumniRegistrations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EngagementPreferences",
                table: "AlumniRegistrations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // Note: RegistrationStatus already has default value in AppDbContext
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove default constraints
            migrationBuilder.AlterColumn<string>(
                name: "RegistrationNumber",
                table: "AlumniRegistrations",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "IdNumber",
                table: "AlumniRegistrations",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AlumniRegistrations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AlumniRegistrations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "CurrentCountry",
                table: "AlumniRegistrations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "CurrentCountryCode",
                table: "AlumniRegistrations",
                type: "varchar(2)",
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(2)",
                oldMaxLength: 2,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "CurrentCity",
                table: "AlumniRegistrations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "QualificationsAttained",
                table: "AlumniRegistrations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "[]");

            migrationBuilder.AlterColumn<string>(
                name: "EngagementPreferences",
                table: "AlumniRegistrations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "[]");
        }
    }
}
