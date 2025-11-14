-- ================================================================================
-- Testing Verification Queries - KQ Alumni Platform
-- Use these queries during end-to-end testing to verify all fixes are working
-- ================================================================================

PRINT '========================================';
PRINT 'KQ ALUMNI PLATFORM - TESTING QUERIES';
PRINT '========================================';
PRINT '';

-- ================================================================================
-- 1. CHECK RECENT REGISTRATIONS
-- ================================================================================
PRINT '1. RECENT REGISTRATIONS (Last 10)';
PRINT '-----------------------------------';

SELECT TOP 10
    Id,
    RegistrationNumber,
    FullName,
    Email,
    MobileCountryCode,
    MobileNumber,
    RegistrationStatus,
    EmailVerified,
    CreatedAt,
    -- Check if RegistrationNumber is in correct format
    CASE
        WHEN RegistrationNumber LIKE 'KQA-____-_____' THEN '✓ Valid Format'
        WHEN LEN(RegistrationNumber) = 36 THEN '✗ GUID Format (WRONG!)'
        ELSE '? Unknown Format'
    END AS FormatCheck
FROM AlumniRegistrations
ORDER BY CreatedAt DESC;

PRINT '';
PRINT '';

-- ================================================================================
-- 2. REGISTRATION NUMBER FORMAT VALIDATION
-- ================================================================================
PRINT '2. REGISTRATION NUMBER FORMAT ANALYSIS';
PRINT '---------------------------------------';

SELECT
    'Total Registrations' AS Metric,
    COUNT(*) AS Count,
    '' AS Percentage
FROM AlumniRegistrations

UNION ALL

SELECT
    'Valid KQA Format' AS Metric,
    SUM(CASE WHEN RegistrationNumber LIKE 'KQA-____-_____' THEN 1 ELSE 0 END) AS Count,
    CAST(
        ROUND(
            (SUM(CASE WHEN RegistrationNumber LIKE 'KQA-____-_____' THEN 1.0 ELSE 0 END) / COUNT(*)) * 100,
            2
        ) AS VARCHAR(10)
    ) + '%' AS Percentage
FROM AlumniRegistrations

UNION ALL

SELECT
    'GUID Format (ERROR)' AS Metric,
    SUM(CASE WHEN LEN(RegistrationNumber) = 36 THEN 1 ELSE 0 END) AS Count,
    CAST(
        ROUND(
            (SUM(CASE WHEN LEN(RegistrationNumber) = 36 THEN 1.0 ELSE 0 END) / COUNT(*)) * 100,
            2
        ) AS VARCHAR(10)
    ) + '%' AS Percentage
FROM AlumniRegistrations;

PRINT '';
PRINT '';

-- ================================================================================
-- 3. CHECK FOR SPECIAL CHARACTERS IN NAMES
-- ================================================================================
PRINT '3. SPECIAL CHARACTERS IN NAMES TEST';
PRINT '------------------------------------';
PRINT 'Names with periods, commas, or titles should be stored correctly:';
PRINT '';

SELECT
    RegistrationNumber,
    FullName,
    Email,
    CASE
        WHEN FullName LIKE '%.%' THEN '✓ Contains period'
        WHEN FullName LIKE '%,%' THEN '✓ Contains comma'
        WHEN FullName LIKE '%''%' THEN '✓ Contains apostrophe'
        ELSE 'No special chars'
    END AS SpecialCharsCheck,
    CreatedAt
FROM AlumniRegistrations
WHERE
    FullName LIKE '%.%'  -- Contains period (Mr., Dr., initials)
    OR FullName LIKE '%,%'  -- Contains comma (Jr., Sr.)
    OR FullName LIKE '%''%' -- Contains apostrophe (O'Connor)
ORDER BY CreatedAt DESC;

PRINT '';
PRINT '';

-- ================================================================================
-- 4. PHONE NUMBER VALIDATION
-- ================================================================================
PRINT '4. PHONE NUMBER FORMAT CHECK';
PRINT '-----------------------------';
PRINT 'Verify phone numbers are split correctly (country code + local number):';
PRINT '';

SELECT TOP 10
    RegistrationNumber,
    FullName,
    MobileCountryCode,
    MobileNumber,
    -- Reconstruct full mobile for verification
    CASE
        WHEN MobileCountryCode IS NOT NULL AND MobileNumber IS NOT NULL
        THEN MobileCountryCode + ' ' + MobileNumber
        ELSE 'No phone provided'
    END AS FullMobile,
    CASE
        WHEN MobileCountryCode LIKE '+%' THEN '✓ Country code has +'
        WHEN MobileCountryCode IS NOT NULL THEN '✗ Missing + prefix'
        ELSE 'No country code'
    END AS CountryCodeCheck,
    CASE
        WHEN LEN(MobileNumber) BETWEEN 6 AND 15 THEN '✓ Valid length'
        WHEN MobileNumber IS NOT NULL THEN '✗ Invalid length'
        ELSE 'No number'
    END AS NumberLengthCheck
FROM AlumniRegistrations
ORDER BY CreatedAt DESC;

PRINT '';
PRINT '';

-- ================================================================================
-- 5. EMAIL VERIFICATION STATUS
-- ================================================================================
PRINT '5. EMAIL VERIFICATION STATUS';
PRINT '-----------------------------';

SELECT
    RegistrationStatus,
    EmailVerified,
    COUNT(*) AS Count,
    CAST(
        ROUND(
            (COUNT(*) * 100.0 / (SELECT COUNT(*) FROM AlumniRegistrations)),
            2
        ) AS VARCHAR(10)
    ) + '%' AS Percentage
FROM AlumniRegistrations
GROUP BY RegistrationStatus, EmailVerified
ORDER BY RegistrationStatus, EmailVerified;

PRINT '';
PRINT '';

-- ================================================================================
-- 6. EMAIL LOGS VERIFICATION
-- ================================================================================
PRINT '6. RECENT EMAIL LOGS';
PRINT '--------------------';
PRINT 'Check emails sent and their delivery status:';
PRINT '';

SELECT TOP 10
    el.ToEmail,
    el.EmailType,
    el.Status,
    el.SentAt,
    el.DurationMs,
    ar.RegistrationNumber,
    CASE
        WHEN el.Status = 'Sent' THEN '✓ Delivered'
        WHEN el.Status = 'MockMode' THEN 'ℹ Mock Mode (Testing)'
        WHEN el.Status = 'Failed' THEN '✗ Failed'
        ELSE el.Status
    END AS DeliveryStatus
FROM EmailLogs el
LEFT JOIN AlumniRegistrations ar ON el.RegistrationId = ar.Id
ORDER BY el.SentAt DESC;

PRINT '';
PRINT '';

-- ================================================================================
-- 7. CHECK FOR ISSUES
-- ================================================================================
PRINT '7. POTENTIAL ISSUES CHECK';
PRINT '-------------------------';

-- Issue 1: GUIDs in RegistrationNumber field
DECLARE @GuidCount INT;
SELECT @GuidCount = COUNT(*) FROM AlumniRegistrations WHERE LEN(RegistrationNumber) = 36;

IF @GuidCount > 0
BEGIN
    PRINT '✗ ISSUE FOUND: ' + CAST(@GuidCount AS VARCHAR) + ' records have GUID in RegistrationNumber field';
    PRINT '  Action: Run FixRegistrationNumbers.sql script';
    PRINT '';

    -- Show the problematic records
    SELECT TOP 5
        Id,
        RegistrationNumber AS ProblematicValue,
        Email,
        CreatedAt
    FROM AlumniRegistrations
    WHERE LEN(RegistrationNumber) = 36
    ORDER BY CreatedAt DESC;
END
ELSE
BEGIN
    PRINT '✓ OK: All registration numbers are in correct KQA-YYYY-XXXXX format';
END

PRINT '';

-- Issue 2: Missing country code + prefix
DECLARE @MissingPlusCount INT;
SELECT @MissingPlusCount = COUNT(*)
FROM AlumniRegistrations
WHERE MobileCountryCode IS NOT NULL
AND MobileCountryCode NOT LIKE '+%';

IF @MissingPlusCount > 0
BEGIN
    PRINT '✗ ISSUE FOUND: ' + CAST(@MissingPlusCount AS VARCHAR) + ' records have country code without + prefix';
    PRINT '';
END
ELSE
BEGIN
    PRINT '✓ OK: All country codes have + prefix';
END

PRINT '';

-- Issue 3: Failed emails
DECLARE @FailedEmailCount INT;
SELECT @FailedEmailCount = COUNT(*) FROM EmailLogs WHERE Status = 'Failed';

IF @FailedEmailCount > 0
BEGIN
    PRINT '⚠ WARNING: ' + CAST(@FailedEmailCount AS VARCHAR) + ' emails failed to send';
    PRINT '  Check SMTP configuration';
    PRINT '';
END
ELSE
BEGIN
    PRINT '✓ OK: No failed emails';
END

PRINT '';

-- Issue 4: Registrations pending too long
DECLARE @OldPendingCount INT;
SELECT @OldPendingCount = COUNT(*)
FROM AlumniRegistrations
WHERE RegistrationStatus = 'Pending'
AND CreatedAt < DATEADD(DAY, -7, GETUTCDATE());

IF @OldPendingCount > 0
BEGIN
    PRINT '⚠ WARNING: ' + CAST(@OldPendingCount AS VARCHAR) + ' registrations pending for more than 7 days';
    PRINT '  These may need manual review';
    PRINT '';
END
ELSE
BEGIN
    PRINT '✓ OK: No registrations pending too long';
END

PRINT '';
PRINT '';

-- ================================================================================
-- 8. SEQUENTIAL NUMBERING CHECK
-- ================================================================================
PRINT '8. SEQUENTIAL NUMBERING VERIFICATION';
PRINT '-------------------------------------';
PRINT 'Check if registration numbers are sequential per year:';
PRINT '';

WITH NumberedRegistrations AS (
    SELECT
        RegistrationNumber,
        CreatedAt,
        -- Extract year and sequence from registration number
        SUBSTRING(RegistrationNumber, 5, 4) AS RegYear,
        TRY_CAST(RIGHT(RegistrationNumber, 5) AS INT) AS Sequence,
        ROW_NUMBER() OVER (
            PARTITION BY SUBSTRING(RegistrationNumber, 5, 4)
            ORDER BY CreatedAt
        ) AS ExpectedSequence
    FROM AlumniRegistrations
    WHERE RegistrationNumber LIKE 'KQA-____-_____'
)
SELECT TOP 10
    RegistrationNumber,
    RegYear,
    Sequence AS ActualSequence,
    ExpectedSequence,
    CASE
        WHEN Sequence = ExpectedSequence THEN '✓ Correct'
        ELSE '✗ Gap detected'
    END AS SequenceCheck,
    CreatedAt
FROM NumberedRegistrations
ORDER BY RegYear DESC, Sequence DESC;

PRINT '';
PRINT '';

-- ================================================================================
-- 9. COMPLETE REGISTRATION WORKFLOW STATUS
-- ================================================================================
PRINT '9. REGISTRATION WORKFLOW STATUS';
PRINT '--------------------------------';

SELECT
    CASE
        WHEN RegistrationStatus = 'Pending' AND EmailVerified = 0 THEN '1. Pending (Not Verified)'
        WHEN RegistrationStatus = 'Pending' AND EmailVerified = 1 THEN '2. Pending (Verified)'
        WHEN RegistrationStatus = 'Approved' AND EmailVerified = 0 THEN '3. Approved (Not Verified)'
        WHEN RegistrationStatus = 'Approved' AND EmailVerified = 1 THEN '4. Approved (Verified)'
        WHEN RegistrationStatus = 'Active' THEN '5. Active'
        WHEN RegistrationStatus = 'Rejected' THEN '6. Rejected'
        ELSE 'Other'
    END AS WorkflowStage,
    COUNT(*) AS Count,
    CAST(
        ROUND(
            (COUNT(*) * 100.0 / (SELECT COUNT(*) FROM AlumniRegistrations)),
            2
        ) AS VARCHAR(10)
    ) + '%' AS Percentage
FROM AlumniRegistrations
GROUP BY
    CASE
        WHEN RegistrationStatus = 'Pending' AND EmailVerified = 0 THEN '1. Pending (Not Verified)'
        WHEN RegistrationStatus = 'Pending' AND EmailVerified = 1 THEN '2. Pending (Verified)'
        WHEN RegistrationStatus = 'Approved' AND EmailVerified = 0 THEN '3. Approved (Not Verified)'
        WHEN RegistrationStatus = 'Approved' AND EmailVerified = 1 THEN '4. Approved (Verified)'
        WHEN RegistrationStatus = 'Active' THEN '5. Active'
        WHEN RegistrationStatus = 'Rejected' THEN '6. Rejected'
        ELSE 'Other'
    END
ORDER BY WorkflowStage;

PRINT '';
PRINT '';

-- ================================================================================
-- SUMMARY
-- ================================================================================
PRINT '========================================';
PRINT 'TESTING VERIFICATION COMPLETE';
PRINT '========================================';
PRINT '';
PRINT 'Review the results above to verify:';
PRINT '1. Registration numbers are in KQA-YYYY-XXXXX format (not GUIDs)';
PRINT '2. Special characters in names are preserved';
PRINT '3. Phone numbers are split correctly (country code + number)';
PRINT '4. Emails are being sent successfully';
PRINT '5. Sequential numbering is working';
PRINT '';
PRINT 'If any issues are found, refer to the fix scripts in:';
PRINT '  - FixRegistrationNumbers.sql';
PRINT '  - Database migration: 20251114000001_EnsureRegistrationNumberNoDefault.cs';
PRINT '';
