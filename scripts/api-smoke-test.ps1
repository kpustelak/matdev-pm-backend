# MatDev PM - smoke test API (wymaga dzialajacego backendu na :5196)
param(
    [string]$BaseUrl = "http://127.0.0.1:5196",
    [int]$ProjectId = 0,
    [int]$TimeoutSec = 15
)

$ErrorActionPreference = "Continue"
$passed = 0
$failed = 0
$skipped = 0
$results = @()

function Test-Endpoint {
    param([string]$Name, [string]$Path, [int[]]$Ok = @(200), [string]$Method = "GET")
    $url = "$BaseUrl$Path"
    try {
        $r = Invoke-WebRequest -Uri $url -Method $Method -UseBasicParsing -TimeoutSec $TimeoutSec
        if ($Ok -contains $r.StatusCode) {
            $script:passed++
            $script:results += [pscustomobject]@{ Area = $Name; Status = "PASS"; Code = $r.StatusCode; Path = $Path }
        } else {
            $script:failed++
            $script:results += [pscustomobject]@{ Area = $Name; Status = "FAIL"; Code = $r.StatusCode; Path = $Path }
        }
    } catch {
        $code = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { 0 }
        if ($Ok -contains $code) {
            $script:passed++
            $script:results += [pscustomobject]@{ Area = $Name; Status = "PASS"; Code = $code; Path = $Path }
        } else {
            $script:failed++
            $script:results += [pscustomobject]@{ Area = $Name; Status = "FAIL"; Code = $code; Path = $Path; Note = $_.Exception.Message }
        }
    }
}

Write-Host "=== MatDev API Smoke Test ===" -ForegroundColor Cyan
Write-Host "Base: $BaseUrl`n"

# Health & lookups
Test-Endpoint "Health" "/api/health"
Test-Endpoint "Projects list" "/api/project"
Test-Endpoint "Users" "/api/user"
Test-Endpoint "Issue types" "/api/issuetype"
Test-Endpoint "Topics" "/api/topic"
Test-Endpoint "Workpackages" "/api/workpackage"
Test-Endpoint "Task categories" "/api/taskcategory"

# Resolve project id
if ($ProjectId -le 0) {
    try {
        $list = Invoke-RestMethod -Uri "$BaseUrl/api/project" -TimeoutSec $TimeoutSec
        $ProjectId = $list.data[0].projectId
        Write-Host "Using first project ID: $ProjectId`n"
    } catch {
        Write-Host "Cannot resolve project ID - skipping project-scoped tests" -ForegroundColor Yellow
        $ProjectId = 0
    }
}

if ($ProjectId -gt 0) {
    $p = $ProjectId
    Test-Endpoint "Project by id" "/api/project/id/$p"
    Test-Endpoint "Project view" "/api/project/$p/view"
    Test-Endpoint "Assignable users" "/api/project/$p/view/assignable-users"
    Test-Endpoint "Task list" ("/api/project/$p/task-list?page=1" + '&pageSize=10')
    Test-Endpoint "Task create form" "/api/project/$p/task-list/create-form"
    Test-Endpoint "Budget" "/api/project/$p/budget" -Ok @(200, 404)
    Test-Endpoint "Budget categories" "/api/project/$p/budget/categories"
    Test-Endpoint "Budget lines" "/api/project/$p/budget/lines"
    Test-Endpoint "Risks" "/api/project/$p/risks"
    Test-Endpoint "Lab order statuses" "/api/project/$p/lab-orders/statuses"
    Test-Endpoint "Lab orders" "/api/project/$p/lab-orders"
} else {
    $skipped += 10
}

Write-Host "`n--- Results ---"
$results | Format-Table -AutoSize
$color = if ($failed -eq 0) { "Green" } else { "Red" }
Write-Host "PASS: $passed  FAIL: $failed  SKIP: $skipped" -ForegroundColor $color
exit $(if ($failed -gt 0) { 1 } else { 0 })
