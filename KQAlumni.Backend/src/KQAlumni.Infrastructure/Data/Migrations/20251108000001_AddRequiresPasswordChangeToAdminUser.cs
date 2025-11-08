using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KQAlumni.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRequiresPasswordChangeToAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add RequiresPasswordChange column to AdminUsers table
            // Default is FALSE for existing users (they've already logged in)
            // New seeded users will be set to TRUE to force password change
            migrationBuilder.AddColumn<bool>(
                name: "RequiresPasswordChange",
                table: "AdminUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Create index for faster queries when checking password change requirements
            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_RequiresPasswordChange",
                table: "AdminUsers",
                column: "RequiresPasswordChange");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop index
            migrationBuilder.DropIndex(
                name: "IX_AdminUsers_RequiresPasswordChange",
                table: "AdminUsers");

            // Drop RequiresPasswordChange column
            migrationBuilder.DropColumn(
                name: "RequiresPasswordChange",
                table: "AdminUsers");
        }
    }
}
