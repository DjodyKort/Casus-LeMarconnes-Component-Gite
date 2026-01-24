// ============================================================
// Azure Bicep Template - LeMarconnes Gîte Component
// ============================================================
// Deployment: App Service voor Gîte Web API (gebruikt bestaande resources)
// Gebruikt: Bestaande App Service Plan + Bestaande SQL Server + Database
// ============================================================

@description('Naam van de resource group locatie')
param location string = resourceGroup().location

@description('Omgeving (dev/test/prod)')
@allowed([
  'dev'
  'test'
  'prod'
])
param environment string = 'dev'

@description('Unieke suffix voor resource namen')
param uniqueSuffix string = uniqueString(resourceGroup().id)

// ============================================================
// VARIABLES
// ============================================================

// Bestaande resources (worden niet aangemaakt)
var existingAppServicePlanName = 'asp-lemarconnes-${environment}'
var existingSqlServerName = 'sql-lemarconnes-${environment}-${uniqueSuffix}'
var existingDatabaseName = 'LeMarconnesGite' // Database bestaat al!

// Nieuwe resource (wordt aangemaakt)
var giteAppServiceName = 'app-lemarconnes-gite-${environment}-${uniqueSuffix}'

// SQL Admin credentials (voor connection string)
var sqlAdminLogin = 'sqladmin'

@description('SQL Server Administrator Password')
@secure()
param sqlAdminPassword string

// ============================================================
// REFERENCE BESTAANDE RESOURCES
// ============================================================

// Reference naar bestaande App Service Plan
resource existingAppServicePlan 'Microsoft.Web/serverfarms@2023-01-01' existing = {
  name: existingAppServicePlanName
}

// Reference naar bestaande SQL Server
resource existingSqlServer 'Microsoft.Sql/servers@2023-05-01-preview' existing = {
  name: existingSqlServerName
}

// ============================================================
// NIEUWE APP SERVICE VOOR GÎTE
// ============================================================

resource giteAppService 'Microsoft.Web/sites@2023-01-01' = {
  name: giteAppServiceName
  location: location
  tags: {
    environment: environment
    project: 'LeMarconnes-Gite'
    component: 'Gite'
  }
  kind: 'app,linux'
  properties: {
    serverFarmId: existingAppServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: true
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environment == 'prod' ? 'Production' : 'Development'
        }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: 'Server=tcp:${existingSqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${existingDatabaseName};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
        }
      ]
    }
  }
}

// ============================================================
// OUTPUTS
// ============================================================

output giteAppServiceUrl string = 'https://${giteAppService.properties.defaultHostName}'
output giteAppServiceName string = giteAppService.name
output usedAppServicePlan string = existingAppServicePlan.name
output usedSqlServer string = existingSqlServer.name
output usedDatabase string = existingDatabaseName
