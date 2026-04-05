# UniNexus Backend: Architecture (Akdeniz University)

## Core Principles
The backend is built following **Clean Architecture** patterns to ensure separation of concerns and maintainability.

## Layers
1. **Domain**: Contains basic entities (`Club`, `Event`, `ApplicationUser`), Enums, and Core Constants. No external dependencies.
2. **Application**: Contains the business logic using **CQRS** (MediatR). Defines DTOs, Mapping (AutoMapper), and Validation (FluentValidation).
3. **Infrastructure**: Implements data access via **EF Core**, **Identity** management, and external service connectors (Email, Storage).
4. **WebApi**: Entry points for the application (Controllers). Includes middleware for error handling, JWT auth, and Swagger.

## Database & ORM (PostgreSQL)
- **Npgsql**: Used for PostgreSQL connectivity.
- **PostGIS**: Enabled via `NetTopologySuite` to store and query spatial engagement data for the **Campus Heatmap**.
- **Audit Logging**: Immutable, time-stamped transaction ledgers for financial tracking and bureaucracy audits.
- **AuditableBaseEntity**: Centralized tracking for all entities.

## Real-Time, Security & Features
- **SignalR**: Hubs for `Global Announcements`, `Budget Status`, and `Real-Time Engagement` (likes/comments).
- **JWT**: Secure session rotation and **HttpOnly cookie** support.
- **Digital Signatures**: Verification system for Bureaucracy and financial approvals.
- **Dynamic RBAC**: Support for global roles (SKS Admin) and club-specific roles (Leader, Inventory Manager) with granular permission matrices.
- **Blob Storage**: Support for justified document attachments in financial requests.
- **Identity**: Custom `ApplicationUser` with `StudentNumber` uniqueness constraints.
