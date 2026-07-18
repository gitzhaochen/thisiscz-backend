# Thisiscz Backend

ASP.NET Core 8 Web API for users, posts, comments, links, and health checks.

## Stack

- .NET 8 / ASP.NET Core
- EF Core + PostgreSQL
- ASP.NET Identity + JWT
- Swagger
- Docker

## Run Locally

```bash
dotnet restore
dotnet run
```

Swagger: `http://localhost:5239/swagger` (port may vary by your local config)

## Required Configuration

Use environment variables in production (Render), and local secrets/local file for development.

- `DatabaseProvider` (`postgres`)
- `ConnectionStrings__POSTGRES_CONNECTIONSTRING`
- `Jwt__Key`

## Development with Postgres (recommended)

Set `DatabaseProvider` to `postgres` in development config, then run:

```bash
dotnet run
```

Apply latest migrations to your development Postgres:

```bash
DatabaseProvider=postgres dotnet ef database update
```

### Important rule (avoid PendingModelChangesWarning)

- Local/production should both use PostgreSQL for schema consistency
- Keep EF migrations aligned with PostgreSQL only

## Import NZ school CSV data to local DB tables

```bash
dotnet run -- --import-nzschool-data
```

This command imports:

- `docs/schooldirectory-07-06-2026-074525.csv` -> `schools`
- `docs/10-Machine Readable-Roll by Funding year level ethnicity 2025.csv` -> `roll_ethnicity_fact`

You can override paths via config:

- `NzSchoolImport:SchoolDirectoryCsvPath`
- `NzSchoolImport:RollCsvPath`

## Database Migrations

PostgreSQL migrations only:

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## Sync prod data to dev Postgres

This script exports production `public` schema and restores it into development.
It will overwrite development data.

```bash
export PROD_DB_URL='postgresql://...'
export DEV_DB_URL='postgresql://...'
./scripts/import-prod-to-dev-postgres.sh
```

## Deploy to Render

1. Create a Web Service from this repository.
2. Use the included `Dockerfile`.
3. Add all required environment variables in Render.
4. Deploy.

## Main API Endpoints

- `POST /api/users/google-login`
- `GET /api/users/me`
- `GET /api/posts`
- `GET /api/comments`
- `GET /api/links`
- `GET /api/health/live`
- `GET /api/health/database`

See Swagger for full request/response details.

## Security Notes

- Never commit real secrets.
- Rotate any secret that was ever committed.
