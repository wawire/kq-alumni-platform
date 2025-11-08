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
            migrationBuilder.Sql(@"
                DECLARE @counter INT = 1;
                DECLARE @year INT;
                DECLARE @lastYear INT = 0;

                UPDATE AlumniRegistrations
                SET RegistrationNumber =
                    CASE
                        WHEN YEAR(CreatedAt) != @lastYear
                        THEN (
                            SELECT @lastYear = YEAR(CreatedAt),
                                   @counter = 1,
                                   'KQA-' + CAST(YEAR(CreatedAt) AS VARCHAR(4)) + '-' + RIGHT('00000' + CAST(@counter AS VARCHAR(5)), 5)
                        )
                        ELSE (
                            SELECT @counter = @counter + 1,
                                   'KQA-' + CAST(YEAR(CreatedAt) AS VARCHAR(4)) + '-' + RIGHT('00000' + CAST(@counter AS VARCHAR(5)), 5)
                        )
                    END
                FROM (
                    SELECT Id, CreatedAt
                    FROM AlumniRegistrations
                    ORDER BY CreatedAt
                ) AS OrderedRegistrations
                WHERE AlumniRegistrations.Id = OrderedRegistrations.Id;
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
