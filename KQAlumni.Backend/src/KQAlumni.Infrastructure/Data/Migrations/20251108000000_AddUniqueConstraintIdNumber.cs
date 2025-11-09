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
            // Drop existing index on IdNumber (non-unique) - ONLY IF IT EXISTS
            // This makes the migration idempotent and handles inconsistent database states
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AlumniRegistrations_IdNumber' AND object_id = OBJECT_ID('AlumniRegistrations'))
                BEGIN
                    DROP INDEX IX_AlumniRegistrations_IdNumber ON AlumniRegistrations;
                END
            ");

            // Create unique constraint on IdNumber - ONLY IF IT DOESN'T EXIST
            // ID/Passport numbers should be unique per person
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_AlumniRegistrations_IdNumber' AND object_id = OBJECT_ID('AlumniRegistrations'))
                BEGIN
                    CREATE UNIQUE INDEX UQ_AlumniRegistrations_IdNumber ON AlumniRegistrations(IdNumber);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop unique constraint on IdNumber - ONLY IF IT EXISTS
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_AlumniRegistrations_IdNumber' AND object_id = OBJECT_ID('AlumniRegistrations'))
                BEGIN
                    DROP INDEX UQ_AlumniRegistrations_IdNumber ON AlumniRegistrations;
                END
            ");

            // Recreate non-unique index on IdNumber - ONLY IF IT DOESN'T EXIST
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AlumniRegistrations_IdNumber' AND object_id = OBJECT_ID('AlumniRegistrations'))
                BEGIN
                    CREATE INDEX IX_AlumniRegistrations_IdNumber ON AlumniRegistrations(IdNumber);
                END
            ");
        }
    }
}
