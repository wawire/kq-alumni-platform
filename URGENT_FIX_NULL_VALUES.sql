-- URGENT FIX: Resolve SqlNullValueException by updating NULL values
-- Run this script directly against your KQAlumni database using SQL Server Management Studio or sqlcmd

USE KQAlumni;
GO

PRINT 'Starting NULL value fix...';
GO

-- Fix AlumniRegistrations table
UPDATE AlumniRegistrations SET RegistrationNumber = '' WHERE RegistrationNumber IS NULL;
UPDATE AlumniRegistrations SET IdNumber = '' WHERE IdNumber IS NULL;
UPDATE AlumniRegistrations SET FullName = '' WHERE FullName IS NULL;
UPDATE AlumniRegistrations SET Email = '' WHERE Email IS NULL;
UPDATE AlumniRegistrations SET CurrentCountry = '' WHERE CurrentCountry IS NULL;
UPDATE AlumniRegistrations SET CurrentCountryCode = '' WHERE CurrentCountryCode IS NULL;
UPDATE AlumniRegistrations SET CurrentCity = '' WHERE CurrentCity IS NULL;
UPDATE AlumniRegistrations SET QualificationsAttained = '[]' WHERE QualificationsAttained IS NULL;
UPDATE AlumniRegistrations SET EngagementPreferences = '[]' WHERE EngagementPreferences IS NULL;
UPDATE AlumniRegistrations SET RegistrationStatus = 'Pending' WHERE RegistrationStatus IS NULL;

PRINT 'Fixed AlumniRegistrations table';
GO

-- Fix EmailLogs table
UPDATE EmailLogs SET ToEmail = '' WHERE ToEmail IS NULL;
UPDATE EmailLogs SET Subject = '' WHERE Subject IS NULL;
UPDATE EmailLogs SET EmailType = '' WHERE EmailType IS NULL;
UPDATE EmailLogs SET Status = '' WHERE Status IS NULL;

PRINT 'Fixed EmailLogs table';
GO

-- Fix EmailTemplates table
UPDATE EmailTemplates SET TemplateKey = '' WHERE TemplateKey IS NULL;
UPDATE EmailTemplates SET Name = '' WHERE Name IS NULL;
UPDATE EmailTemplates SET Subject = '' WHERE Subject IS NULL;
UPDATE EmailTemplates SET HtmlBody = '' WHERE HtmlBody IS NULL;

PRINT 'Fixed EmailTemplates table';
GO

-- Fix AdminUsers table
UPDATE AdminUsers SET Username = '' WHERE Username IS NULL;
UPDATE AdminUsers SET Email = '' WHERE Email IS NULL;
UPDATE AdminUsers SET PasswordHash = '' WHERE PasswordHash IS NULL;
UPDATE AdminUsers SET Role = 'HROfficer' WHERE Role IS NULL;
UPDATE AdminUsers SET FullName = '' WHERE FullName IS NULL;

PRINT 'Fixed AdminUsers table';
GO

-- Fix AuditLogs table
UPDATE AuditLogs SET Action = '' WHERE Action IS NULL;
UPDATE AuditLogs SET PerformedBy = '' WHERE PerformedBy IS NULL;

PRINT 'Fixed AuditLogs table';
GO

-- Verify fix
SELECT
    'AlumniRegistrations' AS TableName,
    COUNT(*) AS RemainingNulls
FROM AlumniRegistrations
WHERE RegistrationNumber IS NULL
   OR IdNumber IS NULL
   OR FullName IS NULL
   OR Email IS NULL
   OR CurrentCountry IS NULL
   OR CurrentCountryCode IS NULL
   OR CurrentCity IS NULL
   OR QualificationsAttained IS NULL
   OR EngagementPreferences IS NULL
   OR RegistrationStatus IS NULL

UNION ALL

SELECT
    'EmailLogs' AS TableName,
    COUNT(*) AS RemainingNulls
FROM EmailLogs
WHERE ToEmail IS NULL
   OR Subject IS NULL
   OR EmailType IS NULL
   OR Status IS NULL

UNION ALL

SELECT
    'EmailTemplates' AS TableName,
    COUNT(*) AS RemainingNulls
FROM EmailTemplates
WHERE TemplateKey IS NULL
   OR Name IS NULL
   OR Subject IS NULL
   OR HtmlBody IS NULL

UNION ALL

SELECT
    'AdminUsers' AS TableName,
    COUNT(*) AS RemainingNulls
FROM AdminUsers
WHERE Username IS NULL
   OR Email IS NULL
   OR PasswordHash IS NULL
   OR Role IS NULL
   OR FullName IS NULL

UNION ALL

SELECT
    'AuditLogs' AS TableName,
    COUNT(*) AS RemainingNulls
FROM AuditLogs
WHERE Action IS NULL
   OR PerformedBy IS NULL;

PRINT 'NULL value fix completed successfully!';
PRINT 'All RemainingNulls counts should be 0';
GO
