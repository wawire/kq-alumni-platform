# End-to-End Testing Guide - KQ Alumni Platform

**Date:** 2025-11-14
**Branch:** `main` (merged from `claude/fix-registration-validation-01K751PN4U1dzBGe4XdGjchN`)
**Tester:** _____________

---

## 📋 Pre-Testing Checklist

### Environment Setup
- [ ] Backend is running (`dotnet run` in KQAlumni.API)
- [ ] Frontend is running (`npm run dev` in kq-alumni-frontend)
- [ ] Database is accessible (SQL Server running)
- [ ] Database migrations applied (`dotnet ef database update`)
- [ ] Email mock mode disabled in production config (or real SMTP configured for testing)
- [ ] ERP mock mode configured appropriately

### Configuration Verification
```bash
# Check backend is running
curl http://localhost:5295/health

# Check frontend is accessible
curl http://localhost:3000

# Verify database connection
# Run in SQL Server Management Studio or sqlcmd
SELECT TOP 1 * FROM AlumniRegistrations ORDER BY CreatedAt DESC;
```

---

## 🧪 Test Suite 1: Special Character Validation

**What We Fixed:** Names with periods (.), commas (,), titles, and initials are now accepted.

### Test Case 1.1: Name with Title
**Input:**
- Full Name: `Mr. John Kamau Doe`
- Email: `mr.john@test.com`
- Other fields: Valid data

**Expected Result:**
- ✅ Form accepts the name
- ✅ No validation error shown
- ✅ Registration succeeds

**Actual Result:** _____________

---

### Test Case 1.2: Name with Initials
**Input:**
- Full Name: `J.K. Rowling`
- Email: `jk.rowling@test.com`

**Expected Result:**
- ✅ Form accepts the name
- ✅ Registration succeeds

**Actual Result:** _____________

---

### Test Case 1.3: Name with Suffix
**Input:**
- Full Name: `Robert Smith, Jr.`
- Email: `robert.jr@test.com`

**Expected Result:**
- ✅ Form accepts the name
- ✅ Registration succeeds

**Actual Result:** _____________

---

### Test Case 1.4: Name with Multiple Special Characters
**Input:**
- Full Name: `Dr. Mary O'Connor-Smith, Sr.`
- Email: `dr.mary@test.com`

**Expected Result:**
- ✅ Form accepts: periods, hyphens, apostrophes, commas
- ✅ Registration succeeds

**Actual Result:** _____________

---

### Test Case 1.5: Invalid Characters (Should Fail)
**Input:**
- Full Name: `John@Doe123`
- Email: `invalid@test.com`

**Expected Result:**
- ❌ Validation error shown
- ❌ Error message: "Full name can only contain letters, spaces, hyphens, apostrophes, periods, and commas"

**Actual Result:** _____________

---

## 🧪 Test Suite 2: Phone Number Country Code Selection

**What We Fixed:** Phone country code selection now properly updates form fields.

### Test Case 2.1: Select Country Code
**Steps:**
1. Navigate to Personal Info step
2. Click on phone number field
3. Open country dropdown
4. Select "Kenya (+254)"
5. Check if `+254` appears in the field

**Expected Result:**
- ✅ Country code `+254` is selected
- ✅ Form field shows country code
- ✅ `mobileCountryCode` is set to `+254`

**Actual Result:** _____________

---

### Test Case 2.2: Enter Phone Number After Country Selection
**Steps:**
1. Select Kenya (+254)
2. Type phone number: `712345678`
3. Review the data

**Expected Result:**
- ✅ Phone displays as: `+254 712345678`
- ✅ Form stores:
  - `mobileCountryCode`: `+254`
  - `mobileNumber`: `712345678`

**Actual Result:** _____________

---

### Test Case 2.3: Change Country Code
**Steps:**
1. Select Kenya (+254), enter `712345678`
2. Change to USA (+1)
3. Enter new number: `2025551234`

**Expected Result:**
- ✅ Country code updates to `+1`
- ✅ Phone number updates to new value
- ✅ No duplication of country codes

**Actual Result:** _____________

---

### Test Case 2.4: Preferred Countries Show First
**Steps:**
1. Open phone country dropdown
2. Check top 5 countries

**Expected Result:**
- ✅ Kenya (KE) appears in top 5
- ✅ USA (US) appears in top 5
- ✅ UK (GB) appears in top 5

**Actual Result:** _____________

---

## 🧪 Test Suite 3: Field-Specific Error Messages

**What We Fixed:** Validation errors now show specific field errors instead of generic "contact support" message.

### Test Case 3.1: Multiple Validation Errors
**Steps:**
1. Fill form with invalid data:
   - Full Name: `Test123` (invalid)
   - Email: `not-an-email` (invalid)
   - Mobile Country Code: `254` (missing +)
2. Submit form

**Expected Result:**
- ✅ Toast shows: "Please correct the following errors:"
- ✅ Shows bullet list with specific errors:
  - `• Full Name: Full name can only contain letters...`
  - `• Email: Invalid email format`
  - `• Mobile Country Code: Invalid phone country code format`
- ❌ Does NOT show "contact KQ.Alumni@kenya-airways.com"

**Actual Result:** _____________

---

### Test Case 3.2: Server Error (500)
**Steps:**
1. Stop the backend server
2. Fill form with valid data
3. Submit form

**Expected Result:**
- ✅ Toast shows generic error message
- ✅ Shows "contact KQ.Alumni@kenya-airways.com" for support
- ✅ Duration: 10 seconds

**Actual Result:** _____________

---

### Test Case 3.3: Network Error
**Steps:**
1. Disconnect internet or block API calls
2. Submit form

**Expected Result:**
- ✅ Shows: "Network error. Please check your connection and try again."
- ✅ Shows support contact

**Actual Result:** _____________

---

## 🧪 Test Suite 4: Registration Number Generation

**What We Fixed:** Registration numbers now generate as `KQA-YYYY-XXXXX` instead of GUIDs.

### Test Case 4.1: New Registration Number
**Steps:**
1. Complete a full registration
2. Check confirmation email
3. Query database

**Expected Result:**
- ✅ Email shows: `Registration Number: KQA-2025-00001` (or next sequential)
- ❌ Email does NOT show GUID (e.g., 889bada6-...)
- ✅ Database `RegistrationNumber` column contains `KQA-2025-XXXXX`
- ✅ Database `Id` column contains GUID (this is separate and OK)

**SQL Query:**
```sql
SELECT TOP 1
    Id,
    RegistrationNumber,
    FullName,
    Email,
    CreatedAt
FROM AlumniRegistrations
ORDER BY CreatedAt DESC;
```

**Actual Result:**
- Registration Number: _____________
- Is GUID? Yes / No
- Format correct? Yes / No

---

### Test Case 4.2: Sequential Numbering
**Steps:**
1. Create 3 registrations in sequence
2. Check registration numbers

**Expected Result:**
- ✅ First: `KQA-2025-00001`
- ✅ Second: `KQA-2025-00002`
- ✅ Third: `KQA-2025-00003`
- ✅ Numbers are sequential

**SQL Query:**
```sql
SELECT TOP 3
    RegistrationNumber,
    Email,
    CreatedAt
FROM AlumniRegistrations
ORDER BY CreatedAt DESC;
```

**Actual Result:** _____________

---

### Test Case 4.3: No GUID in RegistrationNumber Field
**Steps:**
1. Check all existing registrations in database

**SQL Query:**
```sql
SELECT
    COUNT(*) AS TotalRecords,
    SUM(CASE WHEN RegistrationNumber LIKE 'KQA-____-_____' THEN 1 ELSE 0 END) AS ValidFormat,
    SUM(CASE WHEN LEN(RegistrationNumber) = 36 THEN 1 ELSE 0 END) AS GuidFormat
FROM AlumniRegistrations;
```

**Expected Result:**
- ✅ ValidFormat count = TotalRecords
- ✅ GuidFormat count = 0

**Actual Result:**
- Total: _____________
- Valid Format: _____________
- GUID Format: _____________

---

## 🧪 Test Suite 5: Email Verification Link

**What We Fixed:** Verification links now point to frontend (not API), preventing 404 errors.

### Test Case 5.1: Confirmation Email Link Format
**Steps:**
1. Register new user
2. Check confirmation email received
3. Look at the email content

**Expected Result:**
- ✅ Email subject: "Registration Received - KQ Alumni Network"
- ✅ Email shows: `Registration Number: KQA-2025-XXXXX`
- ❌ Email does NOT show GUID
- ✅ Email has professional formatting

**Actual Result:** _____________

---

### Test Case 5.2: Approval Email Verification Link
**Steps:**
1. Admin approves a registration
2. Check approval email sent to user
3. Examine the verification link

**Expected Result:**
- ✅ Link format: `http://localhost:3000/verify/{token}`
- ❌ Link is NOT: `http://localhost:5295/verify/{token}` (API)
- ✅ Link is clickable

**Actual Result:**
- Link URL: _____________
- Points to frontend? Yes / No

---

### Test Case 5.3: Click Verification Link
**Steps:**
1. Receive approval email with verification link
2. Click the verification link
3. Observe what happens

**Expected Result:**
- ✅ Browser opens verification page (not 404)
- ✅ Shows: "Verifying Your Email..." (loading state)
- ✅ Then shows: "Email Verified Successfully!"
- ✅ Shows welcome message with user's name
- ✅ Auto-redirects to homepage in 5 seconds
- ✅ Progress bar animates correctly

**Actual Result:**
- Page loads? Yes / No
- Shows success message? Yes / No
- Auto-redirect works? Yes / No

---

### Test Case 5.4: Invalid Verification Token
**Steps:**
1. Manually visit: `http://localhost:3000/verify/invalid-token-12345`
2. Observe error handling

**Expected Result:**
- ✅ Shows error message
- ✅ Shows support contact email
- ✅ Shows "Back to Registration" button

**Actual Result:** _____________

---

### Test Case 5.5: Expired Verification Token
**Steps:**
1. Use a token older than 30 days (or manually expire in DB)
2. Click verification link

**Expected Result:**
- ✅ Shows: "Link Expired ⏰"
- ✅ Shows instructions for user
- ✅ Provides contact support option

**Actual Result:** _____________

---

## 🧪 Test Suite 6: Complete End-to-End Flow

**Full Registration Journey**

### Test Case 6.1: Complete Happy Path
**Steps:**

**Step 1: Register New User**
1. Navigate to `http://localhost:3000/register`
2. Fill Personal Info:
   - Full Name: `Dr. Jane O'Connor, Sr.`
   - Email: `jane.oconnor@test.com`
   - Country: Kenya
   - City: Nairobi
   - Phone: +254 712345678
3. Fill Employment Info
4. Fill Engagement Info
5. Review and Submit

**Expected Result - Registration:**
- ✅ Form accepts all special characters in name
- ✅ Phone country code works correctly
- ✅ No validation errors
- ✅ Registration succeeds

**Step 2: Check Confirmation Email**
**Expected Result - Email:**
- ✅ Email received
- ✅ Shows: `Registration Number: KQA-2025-XXXXX`
- ✅ Does NOT show GUID
- ✅ Professional formatting

**Step 3: Check Database**
**SQL Query:**
```sql
SELECT TOP 1
    RegistrationNumber,
    FullName,
    Email,
    MobileCountryCode,
    MobileNumber,
    RegistrationStatus
FROM AlumniRegistrations
WHERE Email = 'jane.oconnor@test.com'
ORDER BY CreatedAt DESC;
```

**Expected Result - Database:**
- ✅ RegistrationNumber: `KQA-2025-XXXXX` (not GUID)
- ✅ FullName: `Dr. Jane O'Connor, Sr.` (special chars preserved)
- ✅ MobileCountryCode: `+254`
- ✅ MobileNumber: `712345678`
- ✅ RegistrationStatus: `Pending`

**Step 4: Admin Approves**
1. Admin logs in to HR dashboard
2. Approves the registration

**Expected Result - Approval:**
- ✅ Status changes to `Approved`
- ✅ Approval email sent

**Step 5: User Receives Approval Email**
**Expected Result - Approval Email:**
- ✅ Subject: "Welcome to KQ Alumni Network - Verify Your Email"
- ✅ Contains verification link
- ✅ Link points to: `http://localhost:3000/verify/{token}`

**Step 6: User Clicks Verification Link**
**Expected Result - Verification:**
- ✅ Page loads (no 404)
- ✅ Shows success message
- ✅ Displays user's name: "Welcome, Dr. Jane O'Connor, Sr.!"
- ✅ Auto-redirects after 5 seconds

**Step 7: Verify Final Database State**
**SQL Query:**
```sql
SELECT
    RegistrationNumber,
    RegistrationStatus,
    EmailVerified,
    EmailVerifiedAt
FROM AlumniRegistrations
WHERE Email = 'jane.oconnor@test.com';
```

**Expected Result - Final State:**
- ✅ RegistrationStatus: `Active`
- ✅ EmailVerified: `1` (true)
- ✅ EmailVerifiedAt: (recent timestamp)

**Overall Result:** PASS / FAIL

---

## 🧪 Test Suite 7: Edge Cases & Error Scenarios

### Test Case 7.1: Duplicate Email
**Steps:**
1. Register user with email `duplicate@test.com`
2. Try to register again with same email

**Expected Result:**
- ❌ Validation error
- ✅ Shows: "This email is already registered"
- ✅ Field-specific error (not generic message)

**Actual Result:** _____________

---

### Test Case 7.2: Invalid Email Format
**Steps:**
1. Enter email: `not-an-email`
2. Submit form

**Expected Result:**
- ❌ Validation error
- ✅ Shows: "Invalid email format"

**Actual Result:** _____________

---

### Test Case 7.3: Phone Number Too Short
**Steps:**
1. Select country code: +254
2. Enter phone: `123`
3. Submit

**Expected Result:**
- ❌ Validation error
- ✅ Shows phone number length requirement

**Actual Result:** _____________

---

### Test Case 7.4: Missing Required Fields
**Steps:**
1. Leave Full Name empty
2. Submit form

**Expected Result:**
- ❌ Multiple validation errors shown
- ✅ Each field shows its specific error
- ✅ No generic "contact support" message

**Actual Result:** _____________

---

## 📊 Test Results Summary

| Test Suite | Test Cases | Passed | Failed | Notes |
|------------|------------|--------|--------|-------|
| 1. Special Characters | 5 | ___ | ___ | |
| 2. Phone Country Code | 4 | ___ | ___ | |
| 3. Field Errors | 3 | ___ | ___ | |
| 4. Registration Number | 3 | ___ | ___ | |
| 5. Email Verification | 5 | ___ | ___ | |
| 6. End-to-End Flow | 1 | ___ | ___ | |
| 7. Edge Cases | 4 | ___ | ___ | |
| **TOTAL** | **25** | ___ | ___ | |

**Pass Rate:** ___% (Passed / Total * 100)

---

## 🐛 Issues Found

| # | Test Case | Issue Description | Severity | Status |
|---|-----------|-------------------|----------|--------|
| 1 | | | High/Medium/Low | Open/Fixed |
| 2 | | | | |
| 3 | | | | |

---

## ✅ Sign-Off

**Tested By:** _____________
**Date:** _____________
**Environment:** Development / Staging / Production
**Overall Status:** PASS / FAIL / NEEDS WORK

**Approved for Production:** YES / NO

**Signature:** _____________

---

## 📝 Notes & Observations

_Use this space for any additional observations, concerns, or recommendations:_

---

## 🔧 Quick Reference Commands

### Backend
```bash
# Start backend
cd KQAlumni.Backend/src/KQAlumni.API
dotnet run

# Apply migrations
dotnet ef database update

# Check health
curl http://localhost:5295/health
```

### Frontend
```bash
# Start frontend
cd kq-alumni-frontend
npm run dev

# Build for production
npm run build
```

### Database Queries
```sql
-- Check recent registrations
SELECT TOP 10
    RegistrationNumber,
    FullName,
    Email,
    RegistrationStatus,
    CreatedAt
FROM AlumniRegistrations
ORDER BY CreatedAt DESC;

-- Check for GUID format in RegistrationNumber
SELECT * FROM AlumniRegistrations
WHERE LEN(RegistrationNumber) = 36;

-- Verify email logs
SELECT TOP 10
    ToEmail,
    EmailType,
    Status,
    SentAt
FROM EmailLogs
ORDER BY SentAt DESC;
```

---

**End of Testing Guide**
