-- ============================================
-- MANUAL EMAIL TEMPLATE SEEDING
-- Run this in SQL Server Management Studio
-- ============================================

USE KQAlumniDB;
GO

-- Clear existing templates
DELETE FROM EmailTemplates;
GO

-- Insert 3 default templates
INSERT INTO EmailTemplates (
    TemplateKey,
    Name,
    Description,
    Subject,
    HtmlBody,
    AvailableVariables,
    IsActive,
    IsSystemDefault,
    CreatedBy,
    CreatedAt,
    UpdatedAt
)
VALUES
-- CONFIRMATION Template
('CONFIRMATION',
 'Registration Confirmation Email',
 'Sent immediately after user submits registration form',
 'Registration Received - KQ Alumni Network',
 '<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f4f4f4; }
        .email-container { background-color: white; border-radius: 4px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.08); }
        .header { background: #DC143C; color: white; padding: 40px 30px; text-align: center; }
        .content { padding: 40px 30px; }
        .footer { background: #f9fafb; padding: 25px 30px; text-align: center; border-top: 1px solid #e5e7eb; font-size: 13px; color: #6b7280; }
        .info-box { background: #f8f9fa; border: 1px solid #dee2e6; padding: 20px; margin: 25px 0; border-radius: 4px; }
        h1 { margin: 0; font-size: 26px; font-weight: 600; }
        h2 { color: #1a1a1a; font-size: 20px; margin: 0 0 20px 0; }
    </style>
</head>
<body>
    <div class="email-container">
        <div class="header">
            <h1>KENYA AIRWAYS ALUMNI NETWORK</h1>
            <p style="margin: 10px 0 0 0; font-size: 15px;">Registration Confirmation</p>
        </div>
        <div class="content">
            <h2>Dear {{alumniName}},</h2>
            <p>Thank you for registering with the Kenya Airways Alumni Association.</p>
            <p>We have successfully received your registration and it is currently being processed by our verification team.</p>
            <div class="info-box">
                <strong>Registration Number:</strong> {{registrationNumber}}<br>
                <strong>Status:</strong> Pending Verification<br>
                <strong>Submitted:</strong> {{currentDate}}
            </div>
            <p>You will receive an approval notification within 24-48 hours.</p>
        </div>
        <div class="footer">
            <p>Kenya Airways Alumni Association</p>
            <p><a href="mailto:KQ.Alumni@kenya-airways.com">KQ.Alumni@kenya-airways.com</a></p>
        </div>
    </div>
</body>
</html>',
 '{{alumniName}}, {{registrationId}}, {{registrationNumber}}, {{currentDate}}',
 1,
 1,
 'System',
 GETUTCDATE(),
 GETUTCDATE()
),

-- APPROVAL Template
('APPROVAL',
 'Registration Approval Email',
 'Sent when registration is approved',
 'Welcome to KQ Alumni Network - Verify Your Email',
 '<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f4f4f4; }
        .email-container { background-color: white; border-radius: 4px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.08); }
        .header { background: #DC143C; color: white; padding: 40px 30px; text-align: center; }
        .content { padding: 40px 30px; }
        .button { display: inline-block; padding: 14px 32px; background: #DC143C; color: white; text-decoration: none; border-radius: 4px; font-weight: 600; margin: 20px 0; }
        .footer { background: #f9fafb; padding: 25px 30px; text-align: center; border-top: 1px solid #e5e7eb; font-size: 13px; color: #6b7280; }
        h1 { margin: 0; font-size: 26px; font-weight: 600; }
        h2 { color: #1a1a1a; font-size: 20px; margin: 0 0 20px 0; }
    </style>
</head>
<body>
    <div class="email-container">
        <div class="header">
            <h1>WELCOME TO KQ ALUMNI!</h1>
        </div>
        <div class="content">
            <h2>Dear {{alumniName}},</h2>
            <p>Congratulations! Your registration has been approved.</p>
            <p>Please verify your email address by clicking the button below:</p>
            <p style="text-align: center;">
                <a href="{{verificationLink}}" class="button">Verify Email Address</a>
            </p>
            <p><strong>Registration Number:</strong> {{registrationNumber}}</p>
            <p>Welcome to the Kenya Airways Alumni Association family!</p>
        </div>
        <div class="footer">
            <p>Kenya Airways Alumni Association</p>
            <p><a href="mailto:KQ.Alumni@kenya-airways.com">KQ.Alumni@kenya-airways.com</a></p>
        </div>
    </div>
</body>
</html>',
 '{{alumniName}}, {{registrationNumber}}, {{verificationLink}}',
 1,
 1,
 'System',
 GETUTCDATE(),
 GETUTCDATE()
),

-- REJECTION Template
('REJECTION',
 'Registration Rejection Email',
 'Sent when registration cannot be verified',
 'KQ Alumni Registration - Unable to Verify',
 '<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f4f4f4; }
        .email-container { background-color: white; border-radius: 4px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.08); }
        .header { background: #DC143C; color: white; padding: 40px 30px; text-align: center; }
        .content { padding: 40px 30px; }
        .reason-box { background: #fff3cd; border: 1px solid #ffc107; padding: 20px; margin: 25px 0; border-radius: 4px; }
        .footer { background: #f9fafb; padding: 25px 30px; text-align: center; border-top: 1px solid #e5e7eb; font-size: 13px; color: #6b7280; }
        h1 { margin: 0; font-size: 26px; font-weight: 600; }
        h2 { color: #1a1a1a; font-size: 20px; margin: 0 0 20px 0; }
    </style>
</head>
<body>
    <div class="email-container">
        <div class="header">
            <h1>KENYA AIRWAYS ALUMNI</h1>
        </div>
        <div class="content">
            <h2>Dear {{alumniName}},</h2>
            <p>Thank you for your interest in joining the Kenya Airways Alumni Association.</p>
            <p>Unfortunately, we were unable to verify your registration at this time.</p>
            <div class="reason-box">
                <strong>Reason:</strong><br>
                {{rejectionReason}}
            </div>
            <p>If you believe this is an error or would like to provide additional information, please contact us at <a href="mailto:KQ.Alumni@kenya-airways.com">KQ.Alumni@kenya-airways.com</a></p>
        </div>
        <div class="footer">
            <p>Kenya Airways Alumni Association</p>
        </div>
    </div>
</body>
</html>',
 '{{alumniName}}, {{staffNumber}}, {{rejectionReason}}',
 1,
 1,
 'System',
 GETUTCDATE(),
 GETUTCDATE()
);

GO

-- Verify templates were inserted
SELECT
    Id,
    TemplateKey,
    Name,
    Subject,
    IsActive,
    IsSystemDefault,
    CreatedAt
FROM EmailTemplates
ORDER BY Id;

-- Should show 3 templates
SELECT COUNT(*) AS TotalTemplates FROM EmailTemplates;

PRINT 'Email templates seeded successfully!';
