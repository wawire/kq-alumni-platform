using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KQAlumni.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add RegistrationNumber column
            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "AlumniRegistrations",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            // For existing records, generate registration numbers based on their creation date
            // Uses ROW_NUMBER() to generate sequential numbers per year
            migrationBuilder.Sql(@"
                WITH NumberedRegistrations AS (
                    SELECT
                        Id,
                        'KQA-' + CAST(YEAR(CreatedAt) AS VARCHAR(4)) + '-' +
                        RIGHT('00000' + CAST(ROW_NUMBER() OVER (PARTITION BY YEAR(CreatedAt) ORDER BY CreatedAt) AS VARCHAR(5)), 5) AS RegNumber
                    FROM AlumniRegistrations
                )
                UPDATE ar
                SET ar.RegistrationNumber = nr.RegNumber
                FROM AlumniRegistrations ar
                INNER JOIN NumberedRegistrations nr ON ar.Id = nr.Id;
            ");

            // Create unique index on RegistrationNumber
            migrationBuilder.CreateIndex(
                name: "UQ_AlumniRegistrations_RegistrationNumber",
                table: "AlumniRegistrations",
                column: "RegistrationNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop unique index on RegistrationNumber
            migrationBuilder.DropIndex(
                name: "UQ_AlumniRegistrations_RegistrationNumber",
                table: "AlumniRegistrations");

            // Drop RegistrationNumber column
            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "AlumniRegistrations");
        }
    }
}
