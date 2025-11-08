-- Migration: AddRequiresPasswordChangeToAdminUser
-- This adds the RequiresPasswordChange column and index to AdminUsers table

USE KQAlumniDB;
GO

-- Check if column already exists
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'AdminUsers'
    AND COLUMN_NAME = 'RequiresPasswordChange'
)
BEGIN
    PRINT 'Adding RequiresPasswordChange column...';

    -- Add RequiresPasswordChange column with default value of FALSE
    ALTER TABLE [AdminUsers]
    ADD [RequiresPasswordChange] bit NOT NULL DEFAULT 0;

    PRINT 'Column added successfully.';
END
ELSE
BEGIN
    PRINT 'Column RequiresPasswordChange already exists.';
END
GO

-- Check if index already exists
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_AdminUsers_RequiresPasswordChange'
    AND object_id = OBJECT_ID('AdminUsers')
)
BEGIN
    PRINT 'Creating index IX_AdminUsers_RequiresPasswordChange...';

    -- Create index for faster queries
    CREATE INDEX [IX_AdminUsers_RequiresPasswordChange]
    ON [AdminUsers] ([RequiresPasswordChange]);

    PRINT 'Index created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_AdminUsers_RequiresPasswordChange already exists.';
END
GO

-- Insert migration record so EF Core knows it's been applied
IF NOT EXISTS (
    SELECT 1
    FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = '20251108000001_AddRequiresPasswordChangeToAdminUser'
)
BEGIN
    PRINT 'Recording migration in __EFMigrationsHistory...';

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251108000001_AddRequiresPasswordChangeToAdminUser', '8.0.10');

    PRINT 'Migration recorded successfully.';
END
ELSE
BEGIN
    PRINT 'Migration already recorded.';
END
GO

PRINT 'Migration complete!';
GO
