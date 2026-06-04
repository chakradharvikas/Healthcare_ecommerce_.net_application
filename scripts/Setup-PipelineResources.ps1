param(
    [string]$Organization = "kcskypoint",
    [string]$Project = "HealthcareECommerce",
    [string]$PipelineId = "2",
    [string]$SonarToken = $env:SONAR_TOKEN
)

$ErrorActionPreference = "Stop"
$orgUrl = "https://dev.azure.com/$Organization"
$projectId = "cbbdcddc-82c5-498a-9caa-f2547f63105a"

function Invoke-Ado {
    param([string]$Area, [string]$Resource, [string]$Method = "GET", [hashtable]$Route = @{}, [string]$BodyFile = $null, [string]$ApiVersion = "7.1")
    $routeParams = ($Route.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join " "
    $args = @(
        "--organization", $orgUrl,
        "--area", $Area,
        "--resource", $Resource,
        "--route-parameters", "project=$Project", $routeParams,
        "--http-method", $Method,
        "--api-version", $ApiVersion,
        "-o", "json"
    )
    if ($BodyFile) { $args += @("--in-file", $BodyFile) }
    return az devops invoke @args | ConvertFrom-Json
}

function Ensure-Environment {
    param([string]$Name, [string]$Description)
    $existing = az devops invoke --organization $orgUrl --area distributedtask --resource environments `
        --route-parameters "project=$Project" --api-version 7.1 -o json | ConvertFrom-Json
    if ($existing.value.name -contains $Name) {
        Write-Host "Environment '$Name' already exists." -ForegroundColor Yellow
        return
    }
    $body = @{ name = $Name; description = $Description } | ConvertTo-Json -Compress
    $file = Join-Path $env:TEMP "ado-env-$Name.json"
    [IO.File]::WriteAllText($file, $body, [Text.UTF8Encoding]::new($false))
    Invoke-Ado -Area distributedtask -Resource environments -Method POST -BodyFile $file | Out-Null
    Write-Host "Created environment '$Name'." -ForegroundColor Green
}

function Ensure-SonarEndpoint {
    $endpoints = az devops service-endpoint list --organization $orgUrl --project $Project -o json | ConvertFrom-Json
    if ($endpoints.name -contains "SonarCloud-HealthcareECommerce") {
        Write-Host "SonarCloud service connection already exists." -ForegroundColor Yellow
        return
    }

    if (-not $SonarToken) {
        Write-Host "SONAR_TOKEN not set. Creating SonarCloud connection placeholder (update token in Azure DevOps)." -ForegroundColor Yellow
        $SonarToken = "0000000000000000000000000000000000000000"
    }

    $payload = @{
        name         = "SonarCloud-HealthcareECommerce"
        type         = "sonarcloud"
        url          = "https://sonarcloud.io"
        authorization = @{
            scheme     = "Token"
            parameters = @{ apitoken = $SonarToken }
        }
        data = @{
            environment  = "sonarcloud"
            orgName      = "kcskypoint"
            projectKey   = "HealthcareECommerce"
            projectName  = "Healthcare ECommerce"
        }
        isShared     = $false
        owner        = "library"
        serviceEndpointProjectReferences = @(
            @{
                name             = "SonarCloud-HealthcareECommerce"
                projectReference = @{
                    id   = $projectId
                    name = $Project
                }
            }
        )
    }
    $file = Join-Path $env:TEMP "ado-sonar-endpoint.json"
    [IO.File]::WriteAllText($file, ($payload | ConvertTo-Json -Depth 6 -Compress), [Text.UTF8Encoding]::new($false))

    try {
        az devops invoke --organization $orgUrl --area serviceendpoint --resource endpoints `
            --route-parameters "project=$Project" --http-method POST --api-version 7.1 --in-file $file -o json | Out-Null
        Write-Host "Created SonarCloud service connection." -ForegroundColor Green
    }
    catch {
        Write-Host "SonarCloud endpoint creation skipped or failed: $_" -ForegroundColor Yellow
    }
}

Ensure-Environment -Name "ecommerce-dev" -Description "Healthcare ECommerce development"
Ensure-Environment -Name "ecommerce-prod" -Description "Healthcare ECommerce production"
Ensure-SonarEndpoint

$vars = @(
    @{ name = "sonarCloudServiceConnection"; value = "SonarCloud-HealthcareECommerce" },
    @{ name = "sonarOrganization"; value = "kcskypoint" },
    @{ name = "sonarProjectKey"; value = "HealthcareECommerce" },
    @{ name = "trivyVersion"; value = "0.58.1" }
)
foreach ($v in $vars) {
    $existing = az pipelines variable list --pipeline-id $PipelineId --organization $orgUrl --project $Project -o json 2>$null | ConvertFrom-Json
    if ($existing.$($v.name)) { continue }
    az pipelines variable create --pipeline-id $PipelineId --name $v.name --value $v.value `
        --organization $orgUrl --project $Project -o none 2>$null
    Write-Host "Pipeline variable $($v.name) set."
}

Write-Host "Pipeline resource setup complete." -ForegroundColor Green
