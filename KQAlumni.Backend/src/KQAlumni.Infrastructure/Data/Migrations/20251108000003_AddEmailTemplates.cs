using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KQAlumni.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateKey = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HtmlBody = table.Column<string>(type: "ntext", nullable: false),
                    AvailableVariables = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsSystemDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_EmailTemplates_TemplateKey",
                table: "EmailTemplates",
                column: "TemplateKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_IsActive",
                table: "EmailTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_TemplateKey_IsActive",
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "IsActive" });

            // Seed default email templates
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Description", "Subject", "HtmlBody", "AvailableVariables", "IsActive", "IsSystemDefault", "CreatedBy" },
                values: new object[,]
                {
                    {
                        "CONFIRMATION",
                        "Registration Confirmation Email",
                        "Sent immediately after user submits registration form",
                        "Registration Received - KQ Alumni Network",
                        @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #DC143C; color: white; padding: 30px; text-align: center; }
        .content { padding: 30px; background: #f9f9f9; }
        .footer { text-align: center; padding: 20px; color: #666; font-size: 12px; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>KQ ALUMNI NETWORK</h1>
        </div>
        <div class=""content"">
            <h2>Dear {{alumniName}},</h2>
            <p>Thank you for registering with the Kenya Airways Alumni Association.</p>
            <p><strong>Registration Number:</strong> {{registrationNumber}}</p>
            <p><strong>Status:</strong> Pending Verification</p>
            <p><strong>Submitted:</strong> {{currentDate}}</p>
            <p>You will receive notification within 24-48 hours.</p>
        </div>
        <div class=""footer"">
            <p>Kenya Airways Alumni Association</p>
        </div>
    </div>
</body>
</html>",
                        "{{alumniName}}, {{registrationId}}, {{registrationNumber}}, {{currentDate}}",
                        true,
                        true,
                        "System"
                    },
                    {
                        "APPROVAL",
                        "Registration Approval Email",
                        "Sent when registration is approved",
                        "Welcome to KQ Alumni Network - Verify Your Email",
                        @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #DC143C; color: white; padding: 30px; text-align: center; }
        .content { padding: 30px; background: #f9f9f9; }
        .button { display: inline-block; padding: 12px 24px; background: #DC143C; color: white; text-decoration: none; border-radius: 4px; }
        .footer { text-align: center; padding: 20px; color: #666; font-size: 12px; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>WELCOME TO KQ ALUMNI!</h1>
        </div>
        <div class=""content"">
            <h2>Dear {{alumniName}},</h2>
            <p>Congratulations! Your registration has been approved.</p>
            <p><strong>Registration Number:</strong> {{registrationNumber}}</p>
            <p style=""text-align: center;"">
                <a href=""{{verificationLink}}"" class=""button"">Verify Email Address</a>
            </p>
            <p>Welcome to the Kenya Airways Alumni Association family!</p>
        </div>
        <div class=""footer"">
            <p>Kenya Airways Alumni Association</p>
        </div>
    </div>
</body>
</html>",
                        "{{alumniName}}, {{registrationNumber}}, {{verificationLink}}",
                        true,
                        true,
                        "System"
                    },
                    {
                        "REJECTION",
                        "Registration Rejection Email",
                        "Sent when registration cannot be verified",
                        "KQ Alumni Registration - Unable to Verify",
                        @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #DC143C; color: white; padding: 30px; text-align: center; }
        .content { padding: 30px; background: #f9f9f9; }
        .reason { background: #fff3cd; border: 1px solid #ffc107; padding: 15px; margin: 20px 0; }
        .footer { text-align: center; padding: 20px; color: #666; font-size: 12px; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>KQ ALUMNI</h1>
        </div>
        <div class=""content"">
            <h2>Dear {{alumniName}},</h2>
            <p>Thank you for your interest in joining the Kenya Airways Alumni Association.</p>
            <p>Unfortunately, we were unable to verify your registration.</p>
            <div class=""reason"">
                <strong>Reason:</strong><br>{{rejectionReason}}
            </div>
            <p>For assistance, contact: KQ.Alumni@kenya-airways.com</p>
        </div>
        <div class=""footer"">
            <p>Kenya Airways Alumni Association</p>
        </div>
    </div>
</body>
</html>",
                        "{{alumniName}}, {{staffNumber}}, {{rejectionReason}}",
                        true,
                        true,
                        "System"
                    }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailTemplates");
        }
    }
}
