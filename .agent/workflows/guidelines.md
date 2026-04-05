# UniNexus Backend: Development Guidelines

## General Coding Standards
- Use **C# Coding Conventions** (PascalCase for classes/methods, camelCase for parameters).
- Follow **SOLID** principles.
- Use **Asynchronous Programming** (Task/await) for all I/O bound operations.
- Avoid **Magic Strings**; use constants or enums.

## Clean Architecture Layers
- **Domain**: NO dependencies.
- **Application**: Context-agnostic business logic. MediatR handlers only.
- **Infrastructure**: Database access, external APIs.
- **WebApi**: Minimal controllers, routing, Swagger.

## Database (PostgreSQL)
- Use **snake_case** for table and column names via EF Core fluent mapping.
- All entities must inherit from `AuditableBaseEntity`.
- **PostGIS**: Store spatial data using `NetTopologySuite`.
- Use **EF Core Migrations** for version-controlled schema changes.

## Security
- **JWT**: Manage access and refresh tokens.
- **RBAC**: Implement role-based access checks at the handler or controller level.
- **Validation**: Use `FluentValidation` to secure all request inputs.
- **Audit Logs**: Track all changes to sensitive records.
