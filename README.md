# Thisiscz Backend

ASP.NET Core 8 Web API for users, posts, comments, links, and health checks.

## Stack

- .NET 8 / ASP.NET Core
- EF Core + PostgreSQL (Npgsql)
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

- `ConnectionStrings__POSTGRES_CONNECTIONSTRING`
- `Jwt__Key`

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
