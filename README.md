# Alza Products API

Author: Petr Jelínek

REST API that serves the products of an e-shop and allows a partial update of a single product

**Stack:** .NET 10 (LTS), ASP.NET Core, Entity Framework Core, SQL Server, xUnit.

## Architecture

The application follows an Onion (Clean) architecture: dependencies point inward, so the core business logic does not depend on the database or the web framework. The layers are folders (and namespaces) of a single project, `src/AlzaProducts`:

| Folder | Responsibility |
|---|---|
| `Domain/` | Product entity and its rules |
| `Application/` | Use cases and the interfaces they depend on |
| `Infrastructure/` | Data access: SQL Server via EF Core, and in-memory mock data |
| `Api/` | HTTP endpoints, API versioning, Swagger documentation |

Tests live in `tests/AlzaProducts.Tests`.

> Work in progress — the commit history documents each step. Prerequisites and instructions for running the application and the tests will follow.
