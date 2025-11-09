-- Fix existing approved/rejected registrations that have RequiresManualReview = true
-- This script clears the RequiresManualReview flag for all records that have been reviewed

-- Update approved registrations
UPDATE AlumniRegistrations
SET RequiresManualReview = 0
WHERE RegistrationStatus = 'Approved'
  AND RequiresManualReview = 1;

-- Update rejected registrations
UPDATE AlumniRegistrations
SET RequiresManualReview = 0
WHERE RegistrationStatus = 'Rejected'
  AND RequiresManualReview = 1;

-- Verify the changes
SELECT
    RegistrationNumber,
    FullName,
    RegistrationStatus,
    RequiresManualReview,
    ManuallyReviewed,
    ReviewedBy,
    ReviewedAt
FROM AlumniRegistrations
WHERE RegistrationStatus IN ('Approved', 'Rejected')
ORDER BY UpdatedAt DESC;
