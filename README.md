# Thisiscz Backend

ASP.NET Core 8 Web API for users, posts, comments, links, and health checks.

## Stack

- .NET 8 / ASP.NET Core
- EF Core + PostgreSQL / SQLite
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

- `DatabaseProvider` (`postgres` or `sqlite`)
- `ConnectionStrings__POSTGRES_CONNECTIONSTRING`
- `ConnectionStrings__SQLITE_CONNECTIONSTRING` (e.g. `Data Source=data/thisiscz-dev.db`)
- `Jwt__Key`

## Development with SQLite

Set `DatabaseProvider` to `sqlite` in development config, then run:

```bash
dotnet run
```

The app will auto-create the SQLite schema file if it does not exist.

## Sync production Postgres to local SQLite

```bash
dotnet run -- --sync-prod-to-sqlite
```

This command reads from `POSTGRES_CONNECTIONSTRING` and rebuilds the SQLite file
defined by `SQLITE_CONNECTIONSTRING`.

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

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
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
