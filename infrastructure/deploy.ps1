param(
    [Parameter(Mandatory = $true)]
    [string]$ResourceGroupName,

    [Parameter(Mandatory = $true)]
    [string]$Location = "eastus",

    [Parameter(Mandatory = $true)]
    [string]$Environment = "dev",

    [Parameter(Mandatory = $true)]
    [string]$SqlAdminPassword,

    [Parameter(Mandatory = $true)]
    [string]$JwtKey
)

$ErrorActionPreference = "Stop"

Write-Host "Creating resource group: $ResourceGroupName"
az group create --name $ResourceGroupName --location $Location | Out-Null

Write-Host "Deploying Azure infrastructure..."
$deployment = az deployment group create `
    --resource-group $ResourceGroupName `
    --template-file "$PSScriptRoot\main.bicep" `
    --parameters baseName=ecommerce environment=$Environment sqlAdminPassword=$SqlAdminPassword jwtKey=$JwtKey `
    --query properties.outputs `
    -o json | ConvertFrom-Json

Write-Host ""
Write-Host "=== Deployment Complete ===" -ForegroundColor Green
Write-Host "API URL:  $($deployment.apiUrl.value)"
Write-Host "Web URL:  $($deployment.webUrl.value)"
Write-Host "API App:  $($deployment.apiAppName.value)"
Write-Host "Web App:  $($deployment.webAppName.value)"
Write-Host ""
Write-Host "Next: Configure Azure DevOps pipeline variables:"
Write-Host "  azureServiceConnection = Your-Azure-Service-Connection"
Write-Host "  apiAppNameDev/Prod     = $($deployment.apiAppName.value)"
Write-Host "  webAppNameDev/Prod     = $($deployment.webAppName.value)"
