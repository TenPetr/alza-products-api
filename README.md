# Alza Products API

[![CI](https://github.com/TenPetr/alza-products-api/actions/workflows/ci.yml/badge.svg)](https://github.com/TenPetr/alza-products-api/actions/workflows/ci.yml)

Author: Petr Jelínek

REST API that serves the products of an e-shop and allows a partial update of a single product

**Stack:** .NET 10 (LTS), ASP.NET Core, Entity Framework Core, SQL Server, xUnit.

## API

Swagger UI documents both API versions at http://localhost:5169/swagger (Development only).

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/v1/products` | All products, ordered by name |
| `GET` | `/api/v2/products?page=1&pageSize=10` | One page of products, ordered by name |
| `GET` | `/api/v1/products/{id}`, `/api/v2/products/{id}` | One product |
| `PATCH` | `/api/v1/products/{id}`, `/api/v2/products/{id}` | Updates the description: `{ "description": "New text" }` |

- A product has an `id` (GUID), `name`, `imgUri`, `price` and an optional `description`.
- Paging (v2): `page` starts at 1 and `pageSize` is 1–100, 10 by default. The response holds `page`, `pageSize`, `totalCount`, `totalPages` and `items`; a page after the last one has no items.
- Updating: the `description` property is required, `null` or `""` removes the description, and it can be at most 4000 characters long.
- Versioning: the version is part of the URL, and every response lists the supported versions in the `api-supported-versions` header.
- Errors are returned as ProblemDetails (`application/problem+json`): 400 for invalid input, 404 for an unknown product or API version.
- Health checks: `/health/live` answers while the process runs, `/health/ready` also checks that the database answers.

## Architecture

The application follows an Onion (Clean) architecture: dependencies point inward, so the core business logic does not depend on the database or the web framework. The layers are folders (and namespaces) of a single project, `src/AlzaProducts`:

| Folder | Responsibility |
|---|---|
| `Domain/` | Product entity and its rules |
| `Application/` | Use cases and the interfaces they depend on |
| `Infrastructure/` | Data access: SQL Server via EF Core, and in-memory mock data |
| `Api/` | HTTP endpoints, API versioning, Swagger documentation |

Tests live in `tests/AlzaProducts.Tests`.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- For the database: LocalDB (installed with Visual Studio on Windows) or [Docker](https://www.docker.com/products/docker-desktop/)
- Optional: Docker for the database tests, and the EF Core tools (`dotnet tool install --global dotnet-ef`) for working with migrations

## Running the application

The `DataSource` setting chooses where the products come from: `Database` (SQL Server, the default) or `Mock` (25 sample products kept in memory, so changes are lost when the application stops). In an IDE, run the `http` launch profile for the database or `http (mock data)` for the mock data; in VS Code, the configurations are `AlzaProducts (database)` and `AlzaProducts (mock data)`.

From the command line, run the commands from the repository root. Swagger UI is then at http://localhost:5169/swagger; stop the application with Ctrl+C.

### With mock data

No database needed:

```bash
dotnet run --project src/AlzaProducts --launch-profile "http (mock data)"
```

### With the database

On Windows with Visual Studio, the application uses LocalDB and needs no setup. Elsewhere, SQL Server runs in Docker. Set it up once, with a password of at least 8 characters from three of: upper case, lower case, digits, symbols:

```bash
cat > .env <<'EOF'
MSSQL_SA_PASSWORD='YourPasswordHere'
EOF
```

```bash
dotnet user-secrets set "ConnectionStrings:Products" 'Server=localhost,1433;Database=AlzaProducts;User Id=sa;Password=YourPasswordHere;TrustServerCertificate=True' --project src/AlzaProducts
```

Then start SQL Server, which keeps running until it is stopped, and the application, which creates the database and fills it with the sample products on its first start:

```bash
docker compose up -d --wait
```

```bash
dotnet run --project src/AlzaProducts
```

To start again with a fresh database, delete it with `docker compose down -v`; the next start creates and fills it again.

## Running the tests

```bash
dotnet test
```

| Tests | What they cover |
|---|---|
| Unit tests | The domain rules, use cases, repositories and controllers, with dependencies mocked by NSubstitute |
| HTTP tests | The whole application running in memory on the mock data: routes, versions, paging, validation, errors and Swagger |
| Architecture tests | The dependency rules between the layers |
| Database tests | The SQL Server repository, migrations, seeding and health checks, against a real SQL Server in Docker (Testcontainers) |

The database tests need Docker running; without it, they are reported as skipped, except on CI, where they fail, so a missing Docker can't go unnoticed. Their first run downloads the SQL Server image (about 1.7 GB), unless `docker compose` has already done so.

## Database

The schema comes from the EF Core migrations in `src/AlzaProducts/Infrastructure/Persistence/Migrations`. In Development, the application applies them on startup and fills an empty database with the sample products; a database that already holds products is never changed by the seeding.

```bash
dotnet ef migrations script --idempotent --project src/AlzaProducts --output migrate.sql
```

After a change to the data model, add a migration:

```bash
dotnet ef migrations add <Name> --project src/AlzaProducts --output-dir Infrastructure/Persistence/Migrations
```
