param(
    [Parameter(Mandatory = $true)]
    [string]$RelativePath,
    [string]$Organization = "kcskypoint",
    [string]$Project = "HealthcareECommerce",
    [string]$RepositoryId = "83b7c864-e598-4b9d-a70f-a225f1f549e4",
    [string]$Branch = "main",
    [string]$Comment = "Update file"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path $PSScriptRoot -Parent
Set-Location $repoRoot
$fullPath = Join-Path $repoRoot $RelativePath
$repoPath = "/" + ($RelativePath -replace '\\', '/')
$content = [System.IO.File]::ReadAllText($fullPath, [System.Text.UTF8Encoding]::new($false))

$ref = az devops invoke `
    --organization "https://dev.azure.com/$Organization" `
    --area git --resource refs `
    --route-parameters project=$Project repositoryId=$RepositoryId filter="heads/$Branch" `
    --api-version 7.1 -o json | ConvertFrom-Json

$oldObjectId = $ref.value[0].objectId
$body = @{
    refUpdates = @(@{ name = "refs/heads/$Branch"; oldObjectId = $oldObjectId })
    commits    = @(@{
            comment = $Comment
            changes = @(@{
                    changeType = "edit"
                    item       = @{ path = $repoPath }
                    newContent = @{ content = $content; contentType = "rawtext" }
                })
        })
}
$pushFile = Join-Path $env:TEMP "ado-git-update.json"
[System.IO.File]::WriteAllText($pushFile, ($body | ConvertTo-Json -Depth 10 -Compress), [System.Text.UTF8Encoding]::new($false))

$response = az devops invoke `
    --organization "https://dev.azure.com/$Organization" `
    --area git --resource pushes `
    --route-parameters project=$Project repositoryId=$RepositoryId `
    --http-method POST --api-version 7.1 --in-file $pushFile -o json | ConvertFrom-Json

Write-Host "Updated $repoPath (commit $($response.commits[0].commitId))" -ForegroundColor Green
