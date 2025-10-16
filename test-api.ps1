# Test script for AI Healthcare Copilot API
Write-Host "Testing AI Healthcare Copilot API..." -ForegroundColor Green

# Test 1: Get all patients
Write-Host "`n1. Testing GET /api/patients" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "https://localhost:7174/api/patients" -Method GET -SkipCertificateCheck
    Write-Host "Success! Response: $($response | ConvertTo-Json)" -ForegroundColor Green
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Get all doctors
Write-Host "`n2. Testing GET /api/doctors" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "https://localhost:7174/api/doctors" -Method GET -SkipCertificateCheck
    Write-Host "Success! Response: $($response | ConvertTo-Json)" -ForegroundColor Green
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Create a test patient
Write-Host "`n3. Testing POST /api/patients" -ForegroundColor Yellow
try {
    $patient = @{
        FirstName = "John"
        LastName = "Doe"
        DateOfBirth = "1990-01-01T00:00:00Z"
        Gender = "Male"
        MedicalRecordNumber = "MRN001"
        ContactInfo = "john.doe@email.com"
    } | ConvertTo-Json -Depth 3
    
    $response = Invoke-RestMethod -Uri "https://localhost:7174/api/patients" -Method POST -Body $patient -ContentType "application/json" -SkipCertificateCheck
    Write-Host "Success! Created patient with ID: $($response.Id)" -ForegroundColor Green
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nAPI testing completed!" -ForegroundColor Green
