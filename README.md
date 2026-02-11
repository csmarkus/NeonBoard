# NeonBoard

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-18+-DD0031?logo=angular)](https://angular.io/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.0+-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)

A modern Kanban project tracking application built with Clean Architecture principles, featuring a .NET 10 backend and Angular frontend.

**Live Demo:** [https://nb.dev.neonboard.app/](https://nb.dev.neonboard.app/)

**Email/Pass:** test@neonboard.app/neondev123!

## Overview

NeonBoard provides an intuitive interface for managing projects using Kanban boards. Create projects, organize work into boards, and track progress with cards that move through customizable columns.

### Key Features

- **Project Management**: Organize multiple projects with dedicated boards
- **Kanban Boards**: Visual workflow management with drag-and-drop functionality
- **Card Management**: Create and manage task cards with titles and descriptions
- **Column Customization**: Define workflow stages with custom columns
- **Real-time Updates**: Responsive UI with immediate feedback
- **Secure Authentication**: Auth0 integration for user management

## Technology Stack

### Backend
- .NET 10 with C# 13
- ASP.NET Core Minimal APIs
- Entity Framework Core 10
- PostgreSQL 16
- MediatR (CQRS pattern)
- FluentValidation
- Serilog (structured logging)

### Frontend
- Angular 18+
- TypeScript
- Standalone components with signals
- Reactive forms

### Development & Deployment
- .NET Aspire (local orchestration)
- Docker & Docker Compose
- Dokploy (deployment platform)

## Architecture

NeonBoard follows Clean Architecture and Domain-Driven Design principles:

```
src/
├── NeonBoard.Domain/          # Core business logic and domain models
├── NeonBoard.Application/     # Use cases, CQRS handlers, DTOs
├── NeonBoard.Infrastructure/  # Data access, EF Core, repositories
├── NeonBoard.Api/            # HTTP endpoints, middleware
├── NeonBoard.UI/             # Angular frontend
├── NeonBoard.AppHost/        # Aspire orchestration (development)
└── NeonBoard.ServiceDefaults/ # Shared Aspire configuration
```

### Design Patterns

- **Clean Architecture**: Separation of concerns with dependency inversion
- **Domain-Driven Design**: Rich domain models with aggregates and value objects
- **CQRS**: Separate read and write operations via MediatR
- **Repository Pattern**: Abstraction over data access
- **Vertical Slice Architecture**: Feature-focused organization

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 22+](https://nodejs.org/)
- [PostgreSQL 16](https://www.postgresql.org/download/) (or Docker)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optional, for containerized development)

### Local Development with Aspire

The easiest way to run NeonBoard locally is using .NET Aspire, which orchestrates all services:

```bash
# Clone the repository
git clone https://github.com/yourusername/neonboard.git
cd neonboard

# Run with Aspire (starts PostgreSQL, API, and Angular dev server)
dotnet run --project src/NeonBoard.AppHost
```

Aspire Dashboard will open automatically, providing:
- PostgreSQL database (automatically provisioned)
- API running at `http://localhost:5000`
- Angular dev server at `http://localhost:4200`
- Logs and metrics for all services

### Manual Setup

If you prefer to run services individually:

#### 1. Database Setup

```bash
# Using Docker
docker run --name neonboard-postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=neonboard \
  -p 5432:5432 \
  -d postgres:16
```

#### 2. Backend API

```bash
# Navigate to API project
cd src/NeonBoard.Api

# Update connection string in appsettings.Development.json if needed

# Run the API
dotnet run
```

API will be available at `http://localhost:5000` (HTTP) and `https://localhost:5001` (HTTPS)

#### 3. Frontend UI

```bash
# Navigate to UI project
cd src/NeonBoard.UI

# Install dependencies
npm install

# Run development server
npm start
```

Angular dev server will be available at `http://localhost:4200`

### Configuration

#### Auth0 Setup

1. Create an Auth0 account and application
2. Update `src/NeonBoard.Api/appsettings.Development.json`:

```json
{
  "Auth0": {
    "Domain": "your-tenant.auth0.com",
    "Audience": "your-api-identifier"
  }
}
```

3. Update Angular environment files with Auth0 client configuration

#### Database Connection

Connection string is configured in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=neonboard;Username=postgres;Password=postgres"
  }
}
```

## Docker Deployment

### Build and Run

```bash
# Build the Docker image
docker build -t neonboard:latest .

# Run the container
docker run -d \
  -p 8080:5001 \
  -e ConnectionStrings__DefaultConnection="Host=your-db;Database=neonboard;Username=user;Password=pass" \
  -e Auth0__Domain="your-tenant.auth0.com" \
  -e Auth0__Audience="your-api-identifier" \
  --name neonboard \
  neonboard:latest
```

The application will be available at `http://localhost:8080`

### Docker Compose

A `docker-compose.yml` file is provided for easy deployment with PostgreSQL:

```bash
docker-compose up -d
```

## Development Workflow

### Creating Database Migrations

```bash
dotnet ef migrations add MigrationName \
  --project src/NeonBoard.Infrastructure \
  --startup-project src/NeonBoard.AppHost
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/NeonBoard.UnitTests
dotnet test tests/NeonBoard.IntegrationTests
```

### Code Structure

Follow the established patterns:

1. **New Feature**: Start with Domain entities, then Application handlers, Infrastructure persistence, API endpoints
2. **Commands**: Use for write operations (Create, Update, Delete)
3. **Queries**: Use for read operations (Get, List)
4. **Validation**: FluentValidation rules in Application layer
5. **DTOs**: Return from API, never expose domain entities directly

See [CLAUDE.md](./CLAUDE.md) for detailed architectural guidelines and coding standards.

## Project Structure

```
NeonBoard/
├── src/
│   ├── NeonBoard.Domain/              # Domain models and business logic
│   │   ├── Common/                    # Base classes and interfaces
│   │   ├── Users/                     # User aggregate
│   │   ├── Projects/                  # Project aggregate
│   │   └── Boards/                    # Board aggregate (main)
│   ├── NeonBoard.Application/         # Application services and CQRS
│   │   ├── Common/                    # Shared interfaces and behaviors
│   │   ├── Users/                     # User commands/queries
│   │   ├── Projects/                  # Project commands/queries
│   │   └── Boards/                    # Board commands/queries
│   ├── NeonBoard.Infrastructure/      # Data access and external services
│   │   ├── Persistence/               # EF Core context and configurations
│   │   └── Repositories/              # Repository implementations
│   ├── NeonBoard.Api/                 # HTTP API
│   │   ├── Endpoints/                 # Minimal API endpoints
│   │   ├── Middleware/                # Custom middleware
│   │   └── Services/                  # API-layer services
│   └── NeonBoard.UI/                  # Angular frontend
│       ├── src/app/                   # Application code
│       │   ├── features/              # Feature modules
│       │   ├── core/                  # Core services and guards
│       │   └── shared/                # Shared components
│       └── src/assets/                # Static assets
├── tests/
│   ├── NeonBoard.UnitTests/           # Unit tests
│   └── NeonBoard.IntegrationTests/    # Integration tests
├── Dockerfile                         # Production Docker image
├── docker-compose.yml                 # Docker Compose setup
└── README.md                          # This file
```

## API Endpoints

All endpoints require authentication via Auth0 JWT tokens.

### Projects
- `POST /api/projects` - Create a new project
- `GET /api/projects` - List all projects for current user
- `GET /api/projects/{id}` - Get project details
- `PUT /api/projects/{id}` - Update project
- `DELETE /api/projects/{id}` - Delete project

### Boards
- `POST /api/boards` - Create a new board
- `GET /api/boards/{id}` - Get board with columns and cards
- `PUT /api/boards/{id}` - Update board
- `DELETE /api/boards/{id}` - Delete board

### Columns
- `POST /api/columns` - Add column to board
- `PUT /api/columns/{id}` - Update column
- `DELETE /api/columns/{id}` - Delete column
- `PUT /api/columns/{id}/reorder` - Reorder column

### Cards
- `POST /api/cards` - Create card in column
- `GET /api/cards/{id}` - Get card details
- `PUT /api/cards/{id}` - Update card
- `DELETE /api/cards/{id}` - Delete card
- `PUT /api/cards/{id}/move` - Move card to different column/position

## Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Follow the coding standards in [CLAUDE.md](./CLAUDE.md)
4. Write tests for new functionality
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

## Links

- **Live Demo**: [https://nb.dev.neonboard.app/](https://nb.dev.neonboard.app/)
- **Documentation**: [CLAUDE.md](./CLAUDE.md)
- **Issue Tracker**: [GitHub Issues](https://github.com/yourusername/neonboard/issues)

## Support

For questions, issues, or feature requests, please open an issue on GitHub.

---

Built with Clean Architecture, Domain-Driven Design, and modern .NET practices.
