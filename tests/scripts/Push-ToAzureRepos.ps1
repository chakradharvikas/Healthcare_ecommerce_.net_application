param(
    [string]$Organization = "kcskypoint",
    [string]$Project = "HealthcareECommerce",
    [string]$RepositoryId = "83b7c864-e598-4b9d-a70f-a225f1f549e4",
    [string]$Branch = "main",
    [string]$Comment = "Initial commit: ECommerce .NET solution with Azure DevOps pipeline"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path $PSScriptRoot -Parent
Set-Location $repoRoot

$textExtensions = @(
    ".cs", ".csproj", ".sln", ".json", ".yml", ".yaml", ".md", ".razor", ".css",
    ".html", ".http", ".bicep", ".ps1", ".gitignore", ".txt", ".props", ".targets"
)

$changes = New-Object System.Collections.Generic.List[object]
foreach ($relativePath in git ls-files) {
    $fullPath = Join-Path $repoRoot $relativePath
    $repoPath = "/" + ($relativePath -replace '\\', '/')
    $ext = [System.IO.Path]::GetExtension($relativePath).ToLowerInvariant()

    if ($textExtensions -contains $ext) {
        $content = [System.IO.File]::ReadAllText($fullPath, [System.Text.UTF8Encoding]::new($false))
        $changes.Add([ordered]@{
            changeType = "add"
            item       = @{ path = $repoPath }
            newContent = @{ content = $content; contentType = "rawtext" }
        })
    }
    else {
        $bytes = [System.IO.File]::ReadAllBytes($fullPath)
        $content = [Convert]::ToBase64String($bytes)
        $changes.Add([ordered]@{
            changeType = "add"
            item       = @{ path = $repoPath }
            newContent = @{ content = $content; contentType = "base64encoded" }
        })
    }
}

$body = @{
    refUpdates = @(
        @{
            name         = "refs/heads/$Branch"
            oldObjectId  = "0000000000000000000000000000000000000000"
        }
    )
    commits = @(
        @{
            comment = $Comment
            changes = $changes
        }
    )
}

$json = $body | ConvertTo-Json -Depth 12 -Compress
Write-Host "Pushing $($changes.Count) files ($([math]::Round($json.Length / 1KB, 1)) KB payload)..."

$pushFile = Join-Path $env:TEMP "ado-git-push.json"
[System.IO.File]::WriteAllText($pushFile, $json, [System.Text.UTF8Encoding]::new($false))

$response = az devops invoke `
    --organization "https://dev.azure.com/$Organization" `
    --area git `
    --resource pushes `
    --route-parameters project=$Project repositoryId=$RepositoryId `
    --http-method POST `
    --api-version 7.1 `
    --in-file $pushFile `
    -o json | ConvertFrom-Json

if (-not $response -or -not $response.pushId) {
    throw "Git push failed. Response: $($response | ConvertTo-Json -Compress)"
}

$commitId = $response.commits[0].commitId
Write-Host "Push succeeded (pushId: $($response.pushId), commit: $commitId)" -ForegroundColor Green
Write-Host "Repository: https://dev.azure.com/$Organization/$Project/_git/$Project"
