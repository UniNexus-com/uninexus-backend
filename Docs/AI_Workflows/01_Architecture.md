# UniNexus Backend: Architecture & Project Mapping

## 1. Project Overview
* **Target Framework:** .NET 8.0.
* **Architecture Style:** Monolithic Onion Architecture (Clean Architecture).
* **Primary Goal:** Digitalize campus club management, event discovery, and financial oversight for Akdeniz University.

## 2. Layer Responsibilities & Folder Mapping
- **CleanArchitecture.Domain**: 
    - **Location**: `CleanArchitecture.Application/Entities`.
    - **Contents**: Core Entities based on the ERD (User, Club, ClubRole), Enums, and Domain Exceptions.
- **CleanArchitecture.Application**: 
    - **Location**: `CleanArchitecture.Application/Features`.
    - **Contents**: MediatR Commands/Queries, DTOs, Mappings (AutoMapper), and FluentValidation.
- **CleanArchitecture.Infrastructure**: 
    - **Location**: `CleanArchitecture.Infrastructure/Contexts` and `/Services`.
    - **Contents**: `ApplicationDbContext` (PostgreSQL + PostGIS), BCrypt hashing for **custom passwords**, JWT Generation, and SignalR Hubs.
- **CleanArchitecture.WebAPI**: 
    - **Location**: `CleanArchitecture.WebApi/Controllers`.
    - **Contents**: REST Controllers, Middleware for session handling (30-min timeout), and Swagger.

## 3. Technical Constraints
* **Database**: PostgreSQL with PostGIS for campus heatmap functionality.
* **Security**: Use TLS 1.2+ for all communications; no plaintext passwords.
* **Performance**: API response times must be under 2 seconds.
