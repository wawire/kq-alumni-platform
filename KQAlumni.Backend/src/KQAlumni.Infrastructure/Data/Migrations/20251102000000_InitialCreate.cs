using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KQAlumni.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ========================================
            // CREATE AlumniRegistrations TABLE
            // ========================================
            migrationBuilder.CreateTable(
                name: "AlumniRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    StaffNumber = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MobileCountryCode = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true),
                    MobileNumber = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true),
                    CurrentCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CurrentCountryCode = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    CurrentCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CityCustom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CurrentEmployer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CurrentJobTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Industry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LinkedInProfile = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    QualificationsAttained = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfessionalCertifications = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EngagementPreferences = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConsentGiven = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ConsentGivenAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailVerificationToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmailVerificationTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EmailVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErpValidated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ErpValidatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErpValidationAttempts = table.Column<int>(type: "int", nullable: true),
                    LastErpValidationAttempt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErpStaffName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ErpDepartment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ErpExitDate = table.Column<DateTime>(type: "date", nullable: true),
                    RegistrationStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequiresManualReview = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ManualReviewReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ManuallyReviewed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ReviewedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ConfirmationEmailSent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ConfirmationEmailSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalEmailSent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ApprovalEmailSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionEmailSent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RejectionEmailSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlumniRegistrations", x => x.Id);
                    table.CheckConstraint("CK_AlumniRegistrations_ConsentRequired", "[ConsentGiven] = 1");
                });

            // AlumniRegistrations Indexes
            migrationBuilder.CreateIndex(
                name: "IX_AlumniRegistrations_CreatedAt",
                table: "AlumniRegistrations",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AlumniRegistrations_ErpValidated",
                table: "AlumniRegistrations",
                column: "ErpValidated");

            migrationBuilder.CreateIndex(
                name: "IX_AlumniRegistrations_RegistrationStatus",
                table: "AlumniRegistrations",
                column: "RegistrationStatus");

            migrationBuilder.CreateIndex(
                name: "UQ_AlumniRegistrations_Email",
                table: "AlumniRegistrations",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_AlumniRegistrations_StaffNumber",
                table: "AlumniRegistrations",
                column: "StaffNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_AlumniRegistrations_LinkedIn",
                table: "AlumniRegistrations",
                column: "LinkedInProfile",
                unique: true,
                filter: "[LinkedInProfile] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_AlumniRegistrations_Mobile",
                table: "AlumniRegistrations",
                columns: new[] { "MobileCountryCode", "MobileNumber" },
                unique: true,
                filter: "[MobileCountryCode] IS NOT NULL AND [MobileNumber] IS NOT NULL");

            // Performance indexes
            migrationBuilder.CreateIndex(
                name: "IX_AlumniRegistrations_Status_CreatedAt",
                table: "AlumniRegistrations",
                columns: new[] { "RegistrationStatus", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AlumniRegistrations_ManualReview_Filter",
                table: "AlumniRegistrations",
                columns: new[] { "RequiresManualReview", "ManuallyReviewed", "RegistrationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_AlumniRegistrations_Validated_Status",
                table: "AlumniRegistrations",
                columns: new[] { "ErpValidated", "RegistrationStatus" });

            // ========================================
            // CREATE AdminUsers TABLE
            // ========================================
            migrationBuilder.CreateTable(
                name: "AdminUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "HROfficer"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LockoutEnd = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminUsers", x => x.Id);
                });

            // AdminUsers Indexes
            migrationBuilder.CreateIndex(
                name: "UQ_AdminUsers_Username",
                table: "AdminUsers",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_AdminUsers_Email",
                table: "AdminUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_IsActive",
                table: "AdminUsers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_Role",
                table: "AdminUsers",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_LastLoginAt",
                table: "AdminUsers",
                column: "LastLoginAt",
                descending: new[] { true });

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_Username_IsActive",
                table: "AdminUsers",
                columns: new[] { "Username", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_Email_IsActive",
                table: "AdminUsers",
                columns: new[] { "Email", "IsActive" });

            // ========================================
            // CREATE AuditLogs TABLE
            // ========================================
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdminUserId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PerformedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    PreviousStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NewStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsAutomated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_AlumniRegistrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "AlumniRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuditLogs_AdminUsers_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "AdminUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            // AuditLogs Indexes
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_RegistrationId",
                table: "AuditLogs",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_AdminUserId",
                table: "AuditLogs",
                column: "AdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_IsAutomated",
                table: "AuditLogs",
                column: "IsAutomated");

            // Performance indexes
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Registration_Timestamp",
                table: "AuditLogs",
                columns: new[] { "RegistrationId", "Timestamp" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_AdminUser_Timestamp",
                table: "AuditLogs",
                columns: new[] { "AdminUserId", "Timestamp" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action_Timestamp",
                table: "AuditLogs",
                columns: new[] { "Action", "Timestamp" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop tables in reverse order due to foreign keys
            migrationBuilder.DropTable(name: "AuditLogs");
            migrationBuilder.DropTable(name: "AdminUsers");
            migrationBuilder.DropTable(name: "AlumniRegistrations");
        }
    }
}
