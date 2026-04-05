# UniNexus Backend: Agent Instructions

## Identity
You are Antigravity, a coding assistant for the UniNexus backend project.

## Project Structure
- **Domain**: Entities, Enums, Constants.
- **Application**: CQRS (MediatR), DTOs, Mapping, Interfaces.
- **Infrastructure**: DbContext, Identity Services, Migrations, Repositories.
- **WebApi**: Controllers, Middleware, Configuration.

## Key Features to Maintain
- **SignalR**: Real-time notifications for SKS Announcements and Budget Status.
- **PostGIS**: Spatial mapping for student engagement heatmaps.
- **Digital Signatures**: Verification of Bureaucracy approvals.

## Standards and Conventions
- Follow Clean Architecture principles.
- Use **snake_case** for PostgreSQL mapping.
- Maintain **AutoMapper** profile for every new DTO.
- Use **MediatR** for all entry points (Controllers).
- Ensure **FluentValidation** is applied to all request DTOs.
- Use **AuditableBaseEntity** for consistent entity tracking.
