![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)

# uninexus-backend

The core engine of the UniNexus platform. This backend is built with **.NET 8/9** using **Clean Architecture** principles, featuring a robust authentication system and Dockerized database management.

## 🛠️ Tech Stack

- **Framework:** .NET Web API
- **Architecture:** Clean Architecture (Domain, Application, Infrastructure, Persistence)
- **Database:** PostgreSQL (Running on Docker)
- **ORM:** Entity Framework Core
- **Security:** JWT (JSON Web Tokens) with Refresh Token support
- **Containerization:** Docker & Docker Compose

## 🔒 Authentication (Auth)

The system uses a JWT-based authentication flow:

- **Register:** /api/Account/register

- **Login:** /api/Account/authenticate

- **Refresh Token:** /api/Account/refresh-token

## 📁 Project Structure

- **Domain:** Enterprise-level logic, entities, and constants.

- **Application:** DTOs, interfaces, and business logic (MediatR/CQRS).

- **Infrastructure:** External services (Email, File Storage, etc.).

- **Persistence:** DbContext, migrations, and database configurations.

- **WebApi:** Controllers and Swagger documentation.

## ⭐ Developed by UniNexus Team.
