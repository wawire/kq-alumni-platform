using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KQAlumni.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnsureRegistrationNumberNoDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure RegistrationNumber column has NO default value
            // This SQL statement drops any existing default constraint and ensures proper column type
            migrationBuilder.Sql(@"
                -- Check if there's a default constraint on RegistrationNumber and drop it
                DECLARE @ConstraintName nvarchar(200)
                SELECT @ConstraintName = dc.name
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
                WHERE dc.parent_object_id = OBJECT_ID('AlumniRegistrations')
                AND c.name = 'RegistrationNumber'

                IF @ConstraintName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE AlumniRegistrations DROP CONSTRAINT ' + @ConstraintName)
                END

                -- Ensure the column is varchar(20) and NOT NULL with no default
                -- This will only alter if different from current state
                IF EXISTS (SELECT 1 FROM sys.columns
                           WHERE object_id = OBJECT_ID('AlumniRegistrations')
                           AND name = 'RegistrationNumber'
                           AND (system_type_id != TYPE_ID('varchar') OR max_length != 20))
                BEGIN
                    ALTER TABLE AlumniRegistrations ALTER COLUMN RegistrationNumber varchar(20) NOT NULL
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No action needed on rollback - we're just removing constraints
        }
    }
}
