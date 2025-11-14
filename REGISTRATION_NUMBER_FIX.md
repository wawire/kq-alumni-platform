# Registration Number Issue - Fix Guide

## Problem Description

The `RegistrationNumber` field in the database may be storing GUIDs instead of the expected format: `KQA-YYYY-XXXXX` (e.g., `KQA-2025-00001`).

## Root Cause

This issue can occur if:
1. The database has a default constraint on the `RegistrationNumber` column that generates GUIDs
2. An old version of the code was running when records were created
3. The migration that added the `RegistrationNumber` column wasn't properly applied

## Solution

We've implemented several fixes:

### 1. Code Changes

- **DbContext Configuration** (`AppDbContext.cs`): Added explicit configuration for `RegistrationNumber` to ensure it's required, varchar(20), and has no default value
- **Logging** (`RegistrationService.cs`): Added detailed logging to track what registration numbers are being generated and saved
- **Migration**: Created `EnsureRegistrationNumberNoDefault` migration to remove any default constraints

### 2. Database Migration

Apply the new migration to fix the database schema:

```bash
cd KQAlumni.Backend/src/KQAlumni.API
dotnet ef database update
```

This will:
- Remove any default constraint on the `RegistrationNumber` column
- Ensure the column is `varchar(20)` type
- Ensure the column is NOT NULL

### 3. Fix Existing Data

If you have existing records with GUIDs in the `RegistrationNumber` field, run the diagnostic and fix script:

```bash
cd KQAlumni.Backend/database-scripts
sqlcmd -S YOUR_SERVER_NAME -d KQAlumniDB -i FixRegistrationNumbers.sql
```

**Steps:**
1. Run the script as-is first to diagnose the issue
2. Review the output to see how many records have GUID format
3. If records need fixing, uncomment STEP 4 and STEP 5 in the script
4. Re-run the script to fix the data and remove the default constraint

### 4. Verify the Fix

After applying the migration and running the fix script:

1. **Check the database:**
   ```sql
   SELECT TOP 10
       Id,
       RegistrationNumber,
       Email,
       CreatedAt
   FROM AlumniRegistrations
   ORDER BY CreatedAt DESC;
   ```

   All `RegistrationNumber` values should be in format: `KQA-2025-00001`

2. **Check for default constraints:**
   ```sql
   SELECT
       COL_NAME(dc.parent_object_id, dc.parent_column_id) AS ColumnName,
       dc.name AS DefaultConstraintName,
       dc.definition AS DefaultValue
   FROM sys.default_constraints dc
   WHERE dc.parent_object_id = OBJECT_ID('AlumniRegistrations')
   AND COL_NAME(dc.parent_object_id, dc.parent_column_id) = 'RegistrationNumber';
   ```

   This should return NO rows (no default constraint)

3. **Test new registration:**
   - Submit a new registration through the UI
   - Check the logs for messages like:
     ```
     Generated registration number: KQA-2025-00001 for email: test@example.com
     Registration created - ID: {guid}, RegistrationNumber: KQA-2025-00001, Email: test@example.com, Status: Pending
     ```
   - Verify in the database that the new record has the correct format

## How Registration Numbers Work

### Format
- **Pattern:** `KQA-{YEAR}-{SEQUENCE}`
- **Example:** `KQA-2025-00001`
- **Breakdown:**
  - `KQA` - Prefix for Kenya Airways Alumni
  - `2025` - Year of registration (current year)
  - `00001` - Sequential 5-digit number (resets each year)

### Generation Logic
1. System gets current year
2. Queries database for the latest registration number for that year
3. Extracts the sequence number and increments by 1
4. Formats as `KQA-YYYY-XXXXX` with zero-padding

### Example Sequence
- First registration of 2025: `KQA-2025-00001`
- Second registration of 2025: `KQA-2025-00002`
- ...
- First registration of 2026: `KQA-2026-00001` (sequence resets)

## Important Notes

- The `Id` field (GUID) is separate from `RegistrationNumber` - both exist in the database
- `Id` is used internally for foreign keys and system operations
- `RegistrationNumber` is the user-friendly identifier shown to alumni and HR staff
- Never confuse the two fields!

## Troubleshooting

### If registration numbers are still showing as GUIDs:

1. **Check if the migration was applied:**
   ```bash
   dotnet ef migrations list
   ```
   Ensure `EnsureRegistrationNumberNoDefault` appears in the list

2. **Check application logs:**
   Look for the log messages we added:
   - "Generated registration number: ..."
   - "Registration created - ID: ..., RegistrationNumber: ..."

3. **Verify database column definition:**
   ```sql
   SELECT
       COLUMN_NAME,
       DATA_TYPE,
       CHARACTER_MAXIMUM_LENGTH,
       IS_NULLABLE
   FROM INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_NAME = 'AlumniRegistrations'
   AND COLUMN_NAME = 'RegistrationNumber';
   ```

   Should show: `varchar(20)`, `NOT NULL`

4. **Check if there's a trigger or stored procedure:**
   ```sql
   SELECT
       name,
       type_desc,
       OBJECT_DEFINITION(object_id) AS definition
   FROM sys.objects
   WHERE parent_object_id = OBJECT_ID('AlumniRegistrations')
   AND type IN ('TR'); -- Triggers
   ```

### Contact Support

If the issue persists after following all steps, provide:
- Output from `FixRegistrationNumbers.sql` script
- Application logs showing the registration number generation
- Screenshots of the database schema for `AlumniRegistrations` table
