-- ================================================================================
-- Fix RegistrationNumber Column Issues
-- This script checks and fixes any issues with the RegistrationNumber column
-- ================================================================================

-- STEP 1: Check current state of RegistrationNumber column
PRINT '=== Checking RegistrationNumber Column State ==='

SELECT
    COL_NAME(dc.parent_object_id, dc.parent_column_id) AS ColumnName,
    dc.name AS DefaultConstraintName,
    dc.definition AS DefaultValue
FROM sys.default_constraints dc
WHERE dc.parent_object_id = OBJECT_ID('AlumniRegistrations')
AND COL_NAME(dc.parent_object_id, dc.parent_column_id) = 'RegistrationNumber';

-- STEP 2: Show sample of current RegistrationNumber values
PRINT '=== Sample of Current RegistrationNumber Values ==='

SELECT TOP 10
    Id,
    RegistrationNumber,
    Email,
    CreatedAt,
    CASE
        WHEN RegistrationNumber LIKE 'KQA-____-_____' THEN 'Valid Format'
        WHEN LEN(RegistrationNumber) = 36 AND RegistrationNumber LIKE '%-%-%-%-%' THEN 'GUID Format (INCORRECT!)'
        ELSE 'Unknown Format'
    END AS FormatType
FROM AlumniRegistrations
ORDER BY CreatedAt DESC;

-- STEP 3: Count records with GUID format in RegistrationNumber
PRINT '=== Count of Invalid RegistrationNumbers ==='

SELECT
    COUNT(*) AS TotalRecords,
    SUM(CASE WHEN RegistrationNumber LIKE 'KQA-____-_____' THEN 1 ELSE 0 END) AS ValidFormat,
    SUM(CASE WHEN LEN(RegistrationNumber) = 36 AND RegistrationNumber LIKE '%-%-%-%-%' THEN 1 ELSE 0 END) AS GuidFormat,
    SUM(CASE WHEN RegistrationNumber NOT LIKE 'KQA-____-_____' AND NOT (LEN(RegistrationNumber) = 36 AND RegistrationNumber LIKE '%-%-%-%-%') THEN 1 ELSE 0 END) AS OtherFormat
FROM AlumniRegistrations;

-- STEP 4: Fix records with GUID in RegistrationNumber (UNCOMMENT TO RUN)
-- WARNING: This will regenerate registration numbers for ALL records with GUID format
-- The new numbers will be sequential based on creation date

/*
PRINT '=== Fixing Records with GUID in RegistrationNumber ==='

BEGIN TRANSACTION

    -- Update records with GUID format to proper KQA-YEAR-XXXXX format
    WITH NumberedRecords AS (
        SELECT
            Id,
            CreatedAt,
            'KQA-' + CAST(YEAR(CreatedAt) AS VARCHAR(4)) + '-' +
            RIGHT('00000' + CAST(ROW_NUMBER() OVER (PARTITION BY YEAR(CreatedAt) ORDER BY CreatedAt) AS VARCHAR(5)), 5) AS NewRegNumber
        FROM AlumniRegistrations
        WHERE LEN(RegistrationNumber) = 36 AND RegistrationNumber LIKE '%-%-%-%-%'
    )
    UPDATE ar
    SET ar.RegistrationNumber = nr.NewRegNumber,
        ar.UpdatedAt = GETUTCDATE()
    FROM AlumniRegistrations ar
    INNER JOIN NumberedRecords nr ON ar.Id = nr.Id;

    PRINT 'Fixed ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' records'

COMMIT TRANSACTION

-- Show updated records
SELECT TOP 10
    Id,
    RegistrationNumber,
    Email,
    CreatedAt
FROM AlumniRegistrations
ORDER BY CreatedAt DESC;
*/

-- STEP 5: Drop any default constraint on RegistrationNumber (UNCOMMENT TO RUN)
/*
DECLARE @ConstraintName nvarchar(200)

SELECT @ConstraintName = dc.name
FROM sys.default_constraints dc
INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
WHERE dc.parent_object_id = OBJECT_ID('AlumniRegistrations')
AND c.name = 'RegistrationNumber'

IF @ConstraintName IS NOT NULL
BEGIN
    PRINT '=== Dropping Default Constraint: ' + @ConstraintName + ' ==='
    EXEC('ALTER TABLE AlumniRegistrations DROP CONSTRAINT ' + @ConstraintName)
    PRINT 'Default constraint dropped successfully'
END
ELSE
BEGIN
    PRINT '=== No Default Constraint Found on RegistrationNumber ==='
END
*/

PRINT '=== Script Complete ==='
PRINT 'Review the output above to determine if fixes are needed.'
PRINT 'If you see records with GUID format, uncomment STEP 4 and STEP 5 and re-run this script.'
