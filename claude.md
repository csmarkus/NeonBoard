# NeonBoard - Claude Code Instructions

## Project Overview
NeonBoard is a Kanban project tracking application built with .NET 10 and Angular. Users can create projects, add boards to projects, and manage cards across columns within those boards.

**Current Feature Set:**
- Single user per project (no collaboration features)
- Card management: title, description, labels
- Board prefixes and human-readable card display IDs (e.g. `SPR-1`)
- Drag-and-drop card and column positioning
- Board settings: rename, prefix, label management, delete
- Human-readable URLs: `/p/{shortId}/b/{slug}` (projects use a 7-char base62 short ID, boards use a name-derived slug)

## Architecture

### Clean Architecture Layers
```
src/
├── NeonBoard.Domain/          # Pure business logic, no dependencies
├── NeonBoard.Application/     # Use cases, commands, queries (CQRS with MediatR)
├── NeonBoard.Infrastructure/  # Data access, external services (EF Core, PostgreSQL)
├── NeonBoard.Api/            # HTTP endpoints (Minimal APIs)
├── NeonBoard.UI/             # Angular frontend
├── NeonBoard.AppHost/        # Aspire orchestration (local dev only)
└── NeonBoard.ServiceDefaults/ # Shared Aspire configuration
```

### Dependency Flow (Dependency Rule)
- Domain → No dependencies (pure C#)
- Application → Domain only
- Infrastructure → Application + Domain
- Api → Application + Infrastructure + ServiceDefaults
- AppHost → Api + ServiceDefaults + UI

**Never violate this flow!** Outer layers depend on inner layers, never the reverse.

### Technology Stack
- **Backend:** .NET 10, C# 13, Minimal APIs
- **Frontend:** Angular 18+, Tailwind CSS, Vitest
- **Database:** PostgreSQL 16
- **ORM:** Entity Framework Core 10
- **Patterns:** DDD, CQRS, Clean Architecture, Vertical Slice Architecture
- **Libraries:** MediatR, FluentValidation, Aspire
- **Testing:** xUnit, FluentAssertions, Moq
- **Local Dev:** .NET Aspire orchestration
- **Deployment:** Docker (single container with Angular + API), Dokploy

### Project Files
- Do not add properties to the project files that are already defined in Directory.Build.Props

## Domain-Driven Design (DDD) Patterns

### Aggregates
Each aggregate has a single root entity that controls access to child entities:

1. **User Aggregate** - Simple identity
2. **Project Aggregate** - Project metadata; has a `ShortId` (7-char base62, unambiguous alphabet) for URL routing
3. **Board Aggregate** - Owns Columns, Cards, and Labels (main aggregate); has a `Slug` derived from its name, updated on rename

**Rule:** Only modify entities through their aggregate root. For example, you cannot directly modify a Card - you must go through the Board aggregate.

### Entities
- Have identity (Guid Id)
- Inherit from `Entity` base class
- Use private setters
- State changes through public methods
- Raise domain events for significant changes
- Use static factory methods (`Create`) instead of public constructors

### Value Objects
- No identity (defined by their values)
- Immutable
- Inherit from `ValueObject` base class
- Override `GetEqualityComponents()`
- Use static factory methods for creation

### Domain Events
- Immutable records
- Past tense naming (BoardCreated, not CreateBoard)
- Include OccurredOn timestamp
- Raised by aggregates, handled in Application layer

## Application Layer (CQRS)

### Vertical Slice Structure
Organize by feature, not by technical layer.

### Commands (Write Operations)
- Use `IRequest<TResponse>` from MediatR
- Named with imperative verbs (CreateBoard, UpdateCard, MoveCard)
- Validated with FluentValidation
- Return DTOs, not domain entities
- Handlers orchestrate domain operations and persistence

### Queries (Read Operations)
- Use `IRequest<TResponse>` from MediatR
- Named with "Get" prefix (GetBoard, GetBoardsByProject)
- Return DTOs optimized for the UI
- Can bypass domain for performance (query database directly)

### Pipeline Behaviors
Automatically applied to all commands/queries via MediatR:

1. **LoggingBehavior** - Logs request/response
2. **ValidationBehavior** - Runs FluentValidation
3. **TransactionBehavior** - Wraps commands in database transaction

Order matters! Defined in `Application/DependencyInjection.cs`.

## Infrastructure Layer

### Entity Framework Core Patterns

**DbContext:**
- Implements `IUnitOfWork`
- Applies configurations from assembly
- Dispatches domain events before SaveChanges

**Entity Configurations:**
- One file per aggregate root
- Use `IEntityTypeConfiguration<T>`
- Configure owned entities (Columns, Cards) within Board configuration
- Map value objects using `OwnsOne`
- Ignore domain events (transient, not persisted)

**Repositories:**
- One repository per aggregate root
- Inherit from base `Repository<T>`
- Only expose methods needed by Application layer
- Return domain entities, not DTOs

### Migrations
- Run automatically on startup in Development (via Aspire)
- Run on startup in Production with advisory lock (prevents concurrent migrations)
- Create migrations from command line:
```bash
  dotnet ef migrations add MigrationName --project src/NeonBoard.Infrastructure --startup-project src/NeonBoard.Api
```

## API Layer (Minimal APIs)

### Endpoint Organization
- Group related endpoints using `MapGroup`
- One endpoint file per aggregate (BoardEndpoints.cs, ProjectEndpoints.cs)
- Static methods for each endpoint
- Use extension methods to register: `app.MapBoardEndpoints()`

### Global Exception Handling
- Use `IExceptionHandler` for consistent error responses
- Maps domain/application exceptions to HTTP status codes:
  - `NotFoundException` → 404
  - `ValidationException` → 400 with errors
  - `DomainException` → 400
  - `UnauthorizedAccessException` → 403
  - Everything else → 500

## Coding Standards

### General C# Conventions
- Use C# 13 features (collection expressions, etc.)
- Do not use Primary Constructors
- Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- Implicit usings enabled
- File-scoped namespaces
- `var` for local variables when type is obvious
- Expression-bodied members for simple properties/methods

### Naming Conventions
- PascalCase: Classes, methods, properties, public fields
- camelCase: Local variables, parameters, private fields
- _camelCase: Private fields (with underscore prefix)
- SCREAMING_SNAKE_CASE: Constants
- Async methods: Suffix with `Async`
- Interfaces: Prefix with `I`

### Async/Await
- Always pass `CancellationToken` to async methods
- Use `ConfigureAwait(false)` only in library code, not in ASP.NET Core
- Prefer `Task<T>` over `ValueTask<T>` unless performance critical

### Null Handling
- Use nullable reference types (`string?` vs `string`)
- Validate inputs early, throw `ArgumentNullException` or domain exception
- Use null-conditional operators (`?.`, `??`)

### Error Handling
- **Domain Layer:** Throw `DomainException` for business rule violations
- **Application Layer:** Throw `NotFoundException`, `ValidationException`, `UnauthorizedAccessException`
- **API Layer:** Let `GlobalExceptionHandler` convert to HTTP responses
- Never swallow exceptions

## Testing Strategy

### Unit Tests (tests/NeonBoard.UnitTests/)
- Test domain logic in isolation
- Test command/query handlers with mocked dependencies
- Use FluentAssertions for readable assertions
- Use Moq for mocking

### Integration Tests (tests/NeonBoard.IntegrationTests/)
- Test API endpoints end-to-end
- Use `WebApplicationFactory` with in-memory database
- Test full request/response cycle
- Verify database state after operations

## Common Tasks

### Adding a New Feature
1. **Start with Domain:** Create/update entities, value objects, domain events
2. **Application Layer:** Create command/query + handler + validator
3. **Infrastructure:** Update EF Core configurations if needed, create migration
4. **API:** Add endpoint in appropriate endpoints file
5. **Tests:** Add unit tests for domain logic, integration tests for API

### Creating a Migration
```bash
dotnet ef migrations add MigrationName --project src/NeonBoard.Infrastructure --startup-project src/NeonBoard.Api
```

### Running Locally with Aspire
```bash
dotnet run --project src/NeonBoard.AppHost
```
This starts PostgreSQL, API, and Angular dev server automatically.

### Building for Production
```bash
docker build -t neonboard:latest .
```

## File Organization Rules

### Keep Files Small and Focused
- One class per file
- File name matches class name
- Group related files in folders (Commands/CreateBoard/, Events/, etc.)

### Namespace Conventions
- Match folder structure
- Example: `NeonBoard.Application.Boards.Commands.CreateBoard`

## Important Don'ts

❌ **Don't expose domain entities from API** - Always return DTOs  
❌ **Don't put business logic in Application handlers** - That belongs in Domain  
❌ **Don't reference outer layers from inner layers** - Violates Clean Architecture  
❌ **Don't create public setters on entities** - State changes through methods  
❌ **Don't modify child entities directly** - Go through aggregate root  
❌ **Don't swallow exceptions** - Let them bubble up to GlobalExceptionHandler  
❌ **Don't use AutoMapper** - Explicit mapping is clearer for this project
❌ **Don't create anemic domain models** - Rich domain with behavior, not just data
❌ **Don't forget CancellationToken** - Always pass it through async calls

## Questions to Ask Before Coding

Before implementing a feature, verify:
1. Which aggregate does this belong to?
2. Is this a command (write) or query (read)?
3. What business rules need to be enforced in the domain?
4. What domain events should be raised?
5. Do I need to update EF Core configurations?
6. What validation is needed?
7. What DTOs are needed for the API response?

## Additional Context

### Performance Considerations
- Use `.AsNoTracking()` for read-only queries
- Eager load related entities when needed (`.Include()`)
- Index foreign keys and commonly queried fields
- Keep aggregates small (don't load entire object graphs)

## When in Doubt
- Follow existing patterns in the codebase
- Prioritize domain-driven design principles
- Keep it simple - don't over-engineer
- Write tests first if it helps clarify requirements
- Ask for clarification before making architectural decisions

## Angular UI

For all frontend work inside `src/NeonBoard.UI/`, refer to the dedicated instructions file:

**`src/NeonBoard.UI/CLAUDE.md`** - Angular-specific conventions, component patterns, state management, Tailwind CSS rules, and accessibility requirements.
