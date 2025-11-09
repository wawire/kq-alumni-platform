using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KQAlumni.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIdPassportFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing unique constraint on StaffNumber (we'll recreate with filter)
            migrationBuilder.DropIndex(
                name: "UQ_AlumniRegistrations_StaffNumber",
                table: "AlumniRegistrations");

            // Make StaffNumber nullable (since it's now auto-populated after ID verification)
            migrationBuilder.AlterColumn<string>(
                name: "StaffNumber",
                table: "AlumniRegistrations",
                type: "varchar(7)",
                maxLength: 7,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(7)",
                oldMaxLength: 7,
                oldNullable: false);

            // Recreate unique constraint on StaffNumber with filter to allow nulls
            migrationBuilder.CreateIndex(
                name: "UQ_AlumniRegistrations_StaffNumber",
                table: "AlumniRegistrations",
                column: "StaffNumber",
                unique: true,
                filter: "[StaffNumber] IS NOT NULL");

            // Add IdNumber column (required - primary identification)
            migrationBuilder.AddColumn<string>(
                name: "IdNumber",
                table: "AlumniRegistrations",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            // Add PassportNumber column (optional - legacy field)
            migrationBuilder.AddColumn<string>(
                name: "PassportNumber",
                table: "AlumniRegistrations",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true);

            // Create UNIQUE index on IdNumber (ID/Passport numbers must be unique per person)
            migrationBuilder.CreateIndex(
                name: "UQ_AlumniRegistrations_IdNumber",
                table: "AlumniRegistrations",
                column: "IdNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop unique index on IdNumber
            migrationBuilder.DropIndex(
                name: "UQ_AlumniRegistrations_IdNumber",
                table: "AlumniRegistrations");

            // Drop filtered unique constraint on StaffNumber
            migrationBuilder.DropIndex(
                name: "UQ_AlumniRegistrations_StaffNumber",
                table: "AlumniRegistrations");

            // Drop new columns
            migrationBuilder.DropColumn(
                name: "IdNumber",
                table: "AlumniRegistrations");

            migrationBuilder.DropColumn(
                name: "PassportNumber",
                table: "AlumniRegistrations");

            // Revert StaffNumber to non-nullable
            migrationBuilder.AlterColumn<string>(
                name: "StaffNumber",
                table: "AlumniRegistrations",
                type: "varchar(7)",
                maxLength: 7,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(7)",
                oldMaxLength: 7,
                oldNullable: true);

            // Recreate original unique constraint on StaffNumber without filter
            migrationBuilder.CreateIndex(
                name: "UQ_AlumniRegistrations_StaffNumber",
                table: "AlumniRegistrations",
                column: "StaffNumber",
                unique: true);
        }
    }
}
