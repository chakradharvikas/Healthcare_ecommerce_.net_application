// Lightweight template for subscriptions with limited App Service quota (Free tier)
param baseName string = 'ecommerce'
param location string = resourceGroup().location
@allowed(['dev', 'prod'])
param environment string = 'dev'
@secure()
param jwtKey string

var uniqueSuffix = uniqueString(resourceGroup().id)
var appServicePlanName = '${baseName}-plan-${environment}-${uniqueSuffix}'
var apiAppName = '${baseName}-api-${environment}-${uniqueSuffix}'
var webAppName = '${baseName}-web-${environment}-${uniqueSuffix}'

resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'F1'
    tier: 'Free'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource apiApp 'Microsoft.Web/sites@2023-01-01' = {
  name: apiAppName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      appSettings: [
        { name: 'ASPNETCORE_ENVIRONMENT', value: environment == 'prod' ? 'Production' : 'Development' }
        { name: 'Jwt__Key', value: jwtKey }
        { name: 'Jwt__Issuer', value: 'ECommerce.Api' }
        { name: 'Jwt__Audience', value: 'ECommerce.Clients' }
        { name: 'AllowedOrigins__0', value: 'https://${webAppName}.azurewebsites.net' }
      ]
    }
    httpsOnly: true
  }
}

resource webApp 'Microsoft.Web/sites@2023-01-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      appSettings: [
        { name: 'ASPNETCORE_ENVIRONMENT', value: environment == 'prod' ? 'Production' : 'Development' }
        { name: 'ApiBaseUrl', value: 'https://${apiAppName}.azurewebsites.net/' }
      ]
    }
    httpsOnly: true
  }
}

output apiUrl string = 'https://${apiApp.properties.defaultHostName}'
output webUrl string = 'https://${webApp.properties.defaultHostName}'
output apiAppName string = apiApp.name
output webAppName string = webApp.name
