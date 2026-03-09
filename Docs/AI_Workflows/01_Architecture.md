UniNexus Backend: Architecture & Project Mapping
1. Project Overview

    Target Framework: .NET 8.0.

    Architecture Style: Monolithic Onion Architecture (Clean Architecture).

    Primary Goal: Digitalize campus club management, event discovery, and financial oversight for Akdeniz University.

2. Layer Responsibilities & Folder Mapping

The following mapping ensures the AI agent places logic in the correct directories within your CleanArchitecture solution:
A. CleanArchitecture.Domain

    Location: CleanArchitecture.Application/Entities (and Enums).

    Contents: * Core Entities based on the ER diagram (User, Club, ClubRole, Permission, Membership).

        Domain logic and interfaces that have zero external dependencies.

B. CleanArchitecture.Application

    Location: CleanArchitecture.Application/Features.

    Contents:

        MediatR Commands/Queries: Use cases such as "Create Event," "Approve Budget," and "Borrow Item".

        DTOs & Mappings: Request/Response models for React and Flutter clients.

        Validation: FluentValidation rules for business logic (e.g., the Monopoly rule: one leader per club).

C. CleanArchitecture.Infrastructure

    Location: CleanArchitecture.Infrastructure/Contexts and /Services.

    Contents:

        Persistence: ApplicationDbContext using PostgreSQL 15+ and the PostGIS 3.x extension for spatial data.

        Identity: BCrypt implementation for custom password hashing and JWT generation for mobile/web sessions.

        Real-time: SignalR Hubs for pushing notifications to clients in under 500ms.

        File Services: PDF generation for the "Co-Curricular Transcript".

D. CleanArchitecture.WebAPI

    Location: CleanArchitecture.WebApi/Controllers.

    Contents:

        Controllers: REST endpoints for SKS Admins, Club Leaders, and Student members.

        Middlewares: Global exception handling and Auth filters to verify roles on every request.

3. Technical Constraints

    Database: PostgreSQL with PostGIS for the campus heatmap.

    Security: Use TLS 1.2+ for all communications.

    Sessions: Implement 30-minute inactivity timeouts.

    Performance: API response times must be under 2 seconds.
