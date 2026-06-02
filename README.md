# ECommerce .NET Solution

End-to-end ecommerce platform built with ASP.NET Core 8, Blazor Server, Entity Framework Core, JWT authentication, and Azure DevOps CI/CD.

## Architecture

```
ECommerce/
├── src/
│   ├── ECommerce.Domain/          # Entities, interfaces, enums
│   ├── ECommerce.Application/     # Business logic, DTOs, validators
│   ├── ECommerce.Infrastructure/  # EF Core, Identity, repositories
│   ├── ECommerce.Api/             # REST API (products, cart, orders, auth)
│   └── ECommerce.Web/             # Blazor Server storefront
├── tests/ECommerce.Tests/         # Unit tests
├── infrastructure/                # Azure Bicep templates
└── azure-pipelines.yml            # Azure DevOps CI/CD pipeline
```

## Features

- Product catalog with categories
- User registration and JWT login
- Shopping cart (add, update, remove)
- Order checkout with inventory deduction
- Order history
- Admin role for product management (API)
- Swagger API documentation
- Azure App Service deployment via Azure DevOps

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server LocalDB (included with Visual Studio) or SQL Server Express
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) (for deployment)
- Azure DevOps organization (for CI/CD)

## Local Development

### 1. Restore and build

```powershell
dotnet restore
dotnet build
```

### 2. Run the API

```powershell
cd src/ECommerce.Api
dotnet run
```

API runs at `https://localhost:7000` — Swagger at `/swagger`.

### 3. Run the Web storefront

In a second terminal:

```powershell
cd src/ECommerce.Web
dotnet run
```

Storefront runs at `https://localhost:7001`.

### 4. Demo credentials

| Role     | Email                 | Password    |
|----------|-----------------------|-------------|
| Admin    | admin@ecommerce.com   | Admin@123!  |

Register a new account from the storefront for customer shopping.

## API Endpoints

| Method | Endpoint                    | Auth     | Description        |
|--------|-----------------------------|----------|--------------------|
| POST   | /api/auth/register          | Public   | Register user      |
| POST   | /api/auth/login             | Public   | Login, get JWT       |
| GET    | /api/products               | Public   | List products      |
| GET    | /api/products/{id}          | Public   | Get product        |
| POST   | /api/products               | Admin    | Create product     |
| GET    | /api/cart                   | Customer | View cart          |
| POST   | /api/cart                   | Customer | Add to cart        |
| POST   | /api/orders/checkout        | Customer | Place order        |
| GET    | /api/orders                 | Customer | Order history      |
| GET    | /api/health                 | Public   | Health check       |

## Run Tests

```powershell
dotnet test
```

## Azure Deployment

### Step 1: Provision Azure resources

```powershell
cd infrastructure
.\deploy.ps1 `
  -ResourceGroupName "rg-ecommerce-dev" `
  -Location "eastus" `
  -Environment "dev" `
  -SqlAdminPassword "YourStrong!Pass123" `
  -JwtKey "YourProductionJwtSigningKey_Min32Chars!"
```

This creates:
- Linux App Service Plan
- API and Web App Services (.NET 8)
- Azure SQL Database
- Application Insights

### Step 2: Azure DevOps setup

1. Push this repo to Azure DevOps (or GitHub connected to Azure DevOps).
2. Create a new **Pipeline** → select `azure-pipelines.yml`.
3. Create an **Azure Resource Manager service connection** in Project Settings → Service connections.
4. Add pipeline variables:

| Variable               | Example                          |
|------------------------|----------------------------------|
| azureServiceConnection | MyAzureSubscription              |
| apiAppNameDev          | ecommerce-api-dev-xxxxx          |
| webAppNameDev          | ecommerce-web-dev-xxxxx          |
| apiAppNameProd         | ecommerce-api-prod-xxxxx         |
| webAppNameProd         | ecommerce-web-prod-xxxxx         |

5. Create environments `ecommerce-dev` and `ecommerce-prod` in Azure DevOps (Pipelines → Environments).

### Step 3: Deploy

- Push to `develop` → deploys to **dev** environment
- Push to `main` → deploys to **production** environment

The pipeline will:
1. Restore, build, and test
2. Publish API and Web as zip artifacts
3. Deploy to Azure App Service

## Production Configuration

Set these App Service application settings (Bicep sets them automatically):

**API App:**
- `ConnectionStrings__DefaultConnection` — Azure SQL connection string
- `Jwt__Key` — Strong secret key (32+ characters)
- `AllowedOrigins__0` — Web app URL

**Web App:**
- `ApiBaseUrl` — API app URL (e.g. `https://ecommerce-api-dev.azurewebsites.net/`)

## Project Structure Notes

- Database migrations run automatically on API startup via `DbSeeder`.
- Sample products and admin user are seeded on first run.
- JWT tokens expire after 24 hours (configurable in `appsettings.json`).

## License

MIT
