using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KQAlumni.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApprovalEmailTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update the existing APPROVAL email template to the new welcome message format
            migrationBuilder.Sql(@"
                UPDATE EmailTemplates
                SET
                    Subject = 'Welcome to Kenya Airways Alumni Network!',
                    Description = 'Sent when registration is approved - Welcome message to alumni',
                    AvailableVariables = '{{alumniName}}, {{registrationNumber}}',
                    HtmlBody = '
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""margin: 0; padding: 0; background-color: #f5f5f5; font-family: ''Segoe UI'', Tahoma, Geneva, Verdana, sans-serif;"">
    <!-- Outer wrapper table -->
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f5f5f5; padding: 20px 0;"">
        <tr>
            <td align=""center"">
                <!-- Main content table -->
                <table width=""520"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1); max-width: 520px;"">

                    <!-- Header -->
                    <tr>
                        <td style=""background-color: #DC143C; padding: 30px 30px; text-align: center;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 22px; font-weight: 600; letter-spacing: 0.5px;"">Welcome to KQ Alumni Network!</h1>
                        </td>
                    </tr>

                    <!-- Main content -->
                    <tr>
                        <td style=""padding: 35px 30px;"">
                            <h2 style=""margin: 0 0 20px 0; color: #1a1a1a; font-size: 18px; font-weight: 600;"">Dear {{alumniName}},</h2>

                            <p style=""margin: 0 0 15px 0; color: #4a5568; font-size: 15px; line-height: 1.7;"">
                                We are delighted to welcome you to the <strong>Kenya Airways Alumni Association!</strong>
                            </p>

                            <p style=""margin: 0 0 20px 0; color: #4a5568; font-size: 15px; line-height: 1.7;"">
                                Your registration has been successfully approved, and your profile is now active in our Alumni Network.
                            </p>

                            <h3 style=""margin: 25px 0 15px 0; color: #DC143C; font-size: 16px; font-weight: 600;"">As a valued member, you will have access to:</h3>

                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin-bottom: 20px;"">
                                <tr>
                                    <td style=""padding: 5px 0;"">
                                        <span style=""color: #DC143C; font-size: 16px; margin-right: 8px;"">·</span>
                                        <span style=""color: #4a5568; font-size: 14px; line-height: 1.6;"">Exclusive networking events and reunions</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 5px 0;"">
                                        <span style=""color: #DC143C; font-size: 16px; margin-right: 8px;"">·</span>
                                        <span style=""color: #4a5568; font-size: 14px; line-height: 1.6;"">Alumni newsletters and updates</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 5px 0;"">
                                        <span style=""color: #DC143C; font-size: 16px; margin-right: 8px;"">·</span>
                                        <span style=""color: #4a5568; font-size: 14px; line-height: 1.6;"">Mentorship and career growth opportunities</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 5px 0;"">
                                        <span style=""color: #DC143C; font-size: 16px; margin-right: 8px;"">·</span>
                                        <span style=""color: #4a5568; font-size: 14px; line-height: 1.6;"">Opportunities to participate in CSR and community projects</span>
                                    </td>
                                </tr>
                            </table>

                            <p style=""margin: 20px 0 15px 0; color: #4a5568; font-size: 15px; line-height: 1.7;"">
                                We''re proud to continue this journey with you beyond your time at Kenya Airways.
                            </p>

                            <p style=""margin: 0 0 20px 0; color: #4a5568; font-size: 15px; line-height: 1.7;"">
                                Stay tuned for upcoming activities, and don''t forget to keep your profile updated with your current professional journey.
                            </p>

                            <!-- Corporate webpage link box -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f8f9fa; border: 1px solid #dee2e6; border-radius: 6px; margin: 20px 0;"">
                                <tr>
                                    <td style=""padding: 15px; text-align: center;"">
                                        <p style=""margin: 0 0 8px 0; color: #495057; font-size: 14px; font-weight: 600;"">Learn More About Our Alumni Network</p>
                                        <a href=""https://corporate.kenya-airways.com/en/alumni-network/"" style=""color: #DC143C; font-size: 14px; text-decoration: none; font-weight: 500;"">Visit Our Corporate Alumni Webpage →</a>
                                    </td>
                                </tr>
                            </table>

                            <p style=""margin: 25px 0 0 0; color: #4a5568; font-size: 15px; line-height: 1.7;"">
                                Warm regards,<br>
                                <strong style=""color: #1a1a1a;"">Kenya Airways Alumni Relations Team</strong>
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #f9fafb; padding: 20px 25px; text-align: center; border-top: 1px solid #e5e7eb;"">
                            <p style=""margin: 0 0 8px 0; font-weight: 600; color: #4a5568; font-size: 13px;"">Kenya Airways Alumni Association</p>
                            <p style=""margin: 0 0 12px 0;"">
                                <a href=""mailto:KQ.Alumni@kenya-airways.com"" style=""color: #DC143C; text-decoration: none; font-size: 13px;"">KQ.Alumni@kenya-airways.com</a>
                            </p>
                            <p style=""font-size: 11px; color: #9ca3af; margin: 0; line-height: 1.4;"">
                                This is an automated message. Please do not reply to this email.
                            </p>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>',
                    UpdatedAt = GETUTCDATE(),
                    UpdatedBy = 'System Migration'
                WHERE TemplateKey = 'APPROVAL';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback to the old approval template
            migrationBuilder.Sql(@"
                UPDATE EmailTemplates
                SET
                    Subject = 'Welcome to KQ Alumni Network - Verify Your Email',
                    Description = 'Sent when registration is approved with verification link',
                    AvailableVariables = '{{alumniName}}, {{registrationNumber}}, {{verificationLink}}',
                    UpdatedAt = GETUTCDATE(),
                    UpdatedBy = 'System Rollback'
                WHERE TemplateKey = 'APPROVAL';
            ");
        }
    }
}
