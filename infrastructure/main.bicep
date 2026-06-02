@description('Base name for all resources')
param baseName string = 'ecommerce'

@description('Azure region')
param location string = resourceGroup().location

@description('Environment tag (dev or prod)')
@allowed(['dev', 'prod'])
param environment string = 'dev'

@description('SQL admin login')
param sqlAdminLogin string = 'sqladmin'

@secure()
param sqlAdminPassword string

@description('JWT signing key for API')
@secure()
param jwtKey string

var uniqueSuffix = uniqueString(resourceGroup().id)
var appServicePlanName = '${baseName}-plan-${environment}'
var apiAppName = '${baseName}-api-${environment}-${uniqueSuffix}'
var webAppName = '${baseName}-web-${environment}-${uniqueSuffix}'
var sqlServerName = '${baseName}-sql-${environment}-${uniqueSuffix}'
var sqlDbName = '${baseName}db'
var appInsightsName = '${baseName}-ai-${environment}'

resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: environment == 'prod' ? 'B2' : 'B1'
    tier: 'Basic'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: sqlDbName
  location: location
  sku: {
    name: environment == 'prod' ? 'S0' : 'Basic'
    tier: environment == 'prod' ? 'Standard' : 'Basic'
  }
}

resource sqlFirewallAzure 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
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
      alwaysOn: true
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environment == 'prod' ? 'Production' : 'Development'
        }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDbName};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
        }
        {
          name: 'Jwt__Key'
          value: jwtKey
        }
        {
          name: 'Jwt__Issuer'
          value: 'ECommerce.Api'
        }
        {
          name: 'Jwt__Audience'
          value: 'ECommerce.Clients'
        }
        {
          name: 'Jwt__ExpiryHours'
          value: '24'
        }
        {
          name: 'AllowedOrigins__0'
          value: 'https://${webAppName}.azurewebsites.net'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
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
      alwaysOn: true
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environment == 'prod' ? 'Production' : 'Development'
        }
        {
          name: 'ApiBaseUrl'
          value: 'https://${apiAppName}.azurewebsites.net/'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
      ]
    }
    httpsOnly: true
  }
}

output apiUrl string = 'https://${apiApp.properties.defaultHostName}'
output webUrl string = 'https://${webApp.properties.defaultHostName}'
output apiAppName string = apiApp.name
output webAppName string = webApp.name
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
