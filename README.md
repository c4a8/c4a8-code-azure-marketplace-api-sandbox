# Azure Marketplace Sandbox

A local sandbox that mimics the Azure Marketplace **Fulfillment API v2**, **Operations API**, and **Metering API**. Test your ISV SaaS integration locally before hitting the real Microsoft APIs.

## Quick Start

```bash
dotnet run --project src/AzureMarketplaceSandbox
```

The app starts on `https://localhost:5050` (default). Open the browser to access the Admin UI.

On first run, a default offer ("contoso-saas-offer") with three plans (free, silver, gold) and metering dimensions is seeded automatically.

## Pointing Your Client at the Sandbox

Replace the Microsoft API base URL in your client:

```
https://marketplaceapi.microsoft.com  →  https://localhost:5050
```

All API routes are identical. The sandbox accepts any `Bearer <token>` in the Authorization header.

## API Endpoints

### Fulfillment Subscription API (`/api/saas/subscriptions`)

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/resolve?api-version=2018-08-31` | Resolve marketplace token |
| POST | `/{id}/activate?api-version=2018-08-31` | Activate subscription |
| GET | `/?api-version=2018-08-31` | List all subscriptions |
| GET | `/{id}?api-version=2018-08-31` | Get subscription |
| GET | `/{id}/listAvailablePlans?api-version=2018-08-31` | List available plans |
| PATCH | `/{id}?api-version=2018-08-31` | Change plan or quantity |
| DELETE | `/{id}?api-version=2018-08-31` | Cancel subscription |

### Operations API (`/api/saas/subscriptions/{id}/operations`)

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/?api-version=2018-08-31` | List pending operations |
| GET | `/{opId}?api-version=2018-08-31` | Get operation status |
| PATCH | `/{opId}?api-version=2018-08-31` | Update operation (Success/Failure) |

### Metering API

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/usageEvent?api-version=2018-08-31` | Post single usage event |
| POST | `/api/batchUsageEvent?api-version=2018-08-31` | Post batch usage events (max 25) |
| GET | `/api/usageEvents?api-version=2018-08-31` | Retrieve usage events |

## Admin UI

The Blazor-based Admin UI provides:

- **Dashboard** — subscription counts by status, recent operations
- **Offers** — create/edit offers, plans, and metering dimensions
- **Subscriptions** — create subscriptions (simulate purchases), manage lifecycle (activate, suspend, reinstate, unsubscribe), change plan/quantity
- **Metering** — view usage event log with filters
- **Webhooks** — manually trigger webhook events, view delivery log with payload details
- **Landing Page** — generate marketplace tokens, simulate the landing page redirect flow

## Configuration

Edit `src/AzureMarketplaceSandbox/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=marketplace-sandbox.db"
  },
  "DatabaseProvider": "Sqlite",
  "Sandbox": {
    "PublisherId": "contoso",
    "WebhookUrl": "https://localhost:7100/api/webhook",
    "LandingPageUrl": "https://localhost:7100/landing",
    "BaseUrl": "https://localhost:5050"
  },
  "Auth": {
    "RequiredToken": null
  },
  "SeedData": {
    "Enabled": true
  }
}
```

| Setting | Description |
|---------|-------------|
| `DatabaseProvider` | `Sqlite` (default) or `SqlServer` for Azure SQL |
| `Sandbox:WebhookUrl` | URL where the sandbox sends webhook POST requests |
| `Sandbox:LandingPageUrl` | Your app's landing page URL for token redirect |
| `Sandbox:BaseUrl` | The sandbox's own base URL (used in Operation-Location headers) |
| `Auth:RequiredToken` | If set, only this specific Bearer token is accepted |
| `SeedData:Enabled` | Seed a default offer with plans on first run |

## Azure Deployment

To deploy as an Azure Web App:

1. Set `DatabaseProvider` to `SqlServer`
2. Set `ConnectionStrings:DefaultConnection` to your Azure SQL connection string
3. Configure GitHub Secrets: `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `AZURE_WEBAPP_NAME`
4. Push to `main` — the `deploy.yml` workflow handles the rest

Database migrations are applied automatically on startup.
