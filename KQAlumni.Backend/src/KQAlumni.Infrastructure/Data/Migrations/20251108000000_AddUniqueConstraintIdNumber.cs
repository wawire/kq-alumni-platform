using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KQAlumni.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintIdNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing index on IdNumber (non-unique)
            migrationBuilder.DropIndex(
                name: "IX_AlumniRegistrations_IdNumber",
                table: "AlumniRegistrations");

            // Create unique constraint on IdNumber
            // ID/Passport numbers should be unique per person
            migrationBuilder.CreateIndex(
                name: "UQ_AlumniRegistrations_IdNumber",
                table: "AlumniRegistrations",
                column: "IdNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop unique constraint on IdNumber
            migrationBuilder.DropIndex(
                name: "UQ_AlumniRegistrations_IdNumber",
                table: "AlumniRegistrations");

            // Recreate non-unique index on IdNumber
            migrationBuilder.CreateIndex(
                name: "IX_AlumniRegistrations_IdNumber",
                table: "AlumniRegistrations",
                column: "IdNumber");
        }
    }
}
