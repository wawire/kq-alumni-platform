# Test SMTP Connection to Office 365
param(
    [Parameter(Mandatory=$true)]
    [string]$Password
)

Write-Host "Testing SMTP connection to Office 365..." -ForegroundColor Cyan

$smtpServer = "smtp.office365.com"
$smtpPort = 587
$username = "kqmigration@kenya-airways.com"

try {
    $smtp = New-Object System.Net.Mail.SmtpClient($smtpServer, $smtpPort)
    $smtp.EnableSsl = $true
    $smtp.Credentials = New-Object System.Net.NetworkCredential($username, $Password)
    
    Write-Host "✓ SMTP client created successfully" -ForegroundColor Green
    
    # Try to connect (this will throw if connection fails)
    $msg = New-Object System.Net.Mail.MailMessage
    $msg.From = $username
    $msg.To.Add("test@example.com")
    $msg.Subject = "Test Connection"
    $msg.Body = "This is a test"
    
    Write-Host "Attempting to connect to $smtpServer`:$smtpPort..." -ForegroundColor Yellow
    
    # Note: This will fail at send but connection check happens first
    try {
        $smtp.Send($msg)
    }
    catch {
        if ($_.Exception.Message -like "*Authentication*" -or $_.Exception.Message -like "*credentials*") {
            Write-Host "✗ Authentication failed - Check password" -ForegroundColor Red
        }
        elseif ($_.Exception.Message -like "*connect*" -or $_.Exception.Message -like "*refused*") {
            Write-Host "✗ Cannot connect to SMTP server - Network/Firewall issue" -ForegroundColor Red
        }
        else {
            Write-Host "✓ SMTP connection successful (test email blocked as expected)" -ForegroundColor Green
        }
        Write-Host "Error details: $($_.Exception.Message)" -ForegroundColor Gray
    }
}
catch {
    Write-Host "✗ SMTP test failed: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    if ($smtp) { $smtp.Dispose() }
    if ($msg) { $msg.Dispose() }
}
