-- Script to find and fix NULL values in AlumniRegistrations table
-- This will identify which column(s) are causing the SqlNullValueException

USE KQAlumni;
GO

-- Check for NULL values in required string columns
SELECT
    'RegistrationNumber' AS ColumnName,
    COUNT(*) AS NullCount
FROM AlumniRegistrations
WHERE RegistrationNumber IS NULL

UNION ALL

SELECT
    'IdNumber' AS ColumnName,
    COUNT(*) AS NullCount
FROM AlumniRegistrations
WHERE IdNumber IS NULL

UNION ALL

SELECT
    'FullName' AS ColumnName,
    COUNT(*) AS NullCount
FROM AlumniRegistrations
WHERE FullName IS NULL

UNION ALL

SELECT
    'Email' AS ColumnName,
    COUNT(*) AS NullCount
FROM AlumniRegistrations
WHERE Email IS NULL

UNION ALL

SELECT
    'CurrentCountry' AS ColumnName,
    COUNT(*) AS NullCount
FROM AlumniRegistrations
WHERE CurrentCountry IS NULL

UNION ALL

SELECT
    'CurrentCountryCode' AS ColumnName,
    COUNT(*) AS NullCount
FROM AlumniRegistrations
WHERE CurrentCountryCode IS NULL

UNION ALL

SELECT
    'CurrentCity' AS ColumnName,
    COUNT(*) AS NullCount
FROM AlumniRegistrations
WHERE CurrentCity IS NULL

UNION ALL

SELECT
    'QualificationsAttained' AS ColumnName,
    COUNT(*) AS NullCount
FROM AlumniRegistrations
WHERE QualificationsAttained IS NULL

UNION ALL

SELECT
    'EngagementPreferences' AS ColumnName,
    COUNT(*) AS NullCount
FROM AlumniRegistrations
WHERE EngagementPreferences IS NULL

UNION ALL

SELECT
    'RegistrationStatus' AS ColumnName,
    COUNT(*) AS NullCount
FROM AlumniRegistrations
WHERE RegistrationStatus IS NULL;

GO

-- FIX: Update all NULL values to appropriate defaults
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

GO

-- Verify no NULLs remain
SELECT COUNT(*) AS RemainingNulls
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
   OR RegistrationStatus IS NULL;

GO
