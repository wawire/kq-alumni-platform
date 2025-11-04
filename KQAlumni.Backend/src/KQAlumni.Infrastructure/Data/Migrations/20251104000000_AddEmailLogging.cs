using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KQAlumni.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailLogging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    RegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EmailType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SmtpServer = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DurationMs = table.Column<int>(type: "int", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Metadata = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailLogs_AlumniRegistrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "AlumniRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_RegistrationId",
                table: "EmailLogs",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_ToEmail",
                table: "EmailLogs",
                column: "ToEmail");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_Status",
                table: "EmailLogs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_EmailType",
                table: "EmailLogs",
                column: "EmailType");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_SentAt",
                table: "EmailLogs",
                column: "SentAt",
                descending: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_Status_SentAt",
                table: "EmailLogs",
                columns: new[] { "Status", "SentAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_EmailType_Status",
                table: "EmailLogs",
                columns: new[] { "EmailType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_Registration_SentAt",
                table: "EmailLogs",
                columns: new[] { "RegistrationId", "SentAt" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailLogs");
        }
    }
}
