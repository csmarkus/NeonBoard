# NeonBoard - Claude Code Instructions

## Project Overview
NeonBoard is a Kanban project tracking application built with .NET 10 and Angular. Users can create projects, add boards to projects, and manage cards across columns within those boards.

**MVP Scope:**
- Single user per project (no collaboration features)
- Basic card management (title and description only)
- Drag-and-drop card positioning between columns
- Project and board organization

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
- **Frontend:** Angular 18+
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
2. **Project Aggregate** - Project metadata
3. **Board Aggregate** - Owns Columns and Cards (main aggregate)

**Rule:** Only modify entities through their aggregate root. For example, you cannot directly modify a Card - you must go through the Board aggregate.

### Entities
- Have identity (Guid Id)
- Inherit from `Entity` base class
- Use private setters
- State changes through public methods
- Raise domain events for significant changes
- Use static factory methods (`Create`) instead of public constructors

**Example Pattern:**
```csharp
public sealed class Board : Entity, IAggregateRoot
{
    private readonly List<Column> _columns = new();
    
    public string Name { get; private set; }
    public IReadOnlyCollection<Column> Columns => _columns.AsReadOnly();
    
    private Board() { } // EF Core
    
    private Board(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }
    
    public static Board Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Board name is required");
            
        var board = new Board(name);
        board.RaiseDomainEvent(new BoardCreatedEvent(board.Id));
        return board;
    }
    
    public void AddColumn(string columnName)
    {
        // Business logic here
        RaiseDomainEvent(new ColumnAddedEvent(...));
    }
}
```

### Value Objects
- No identity (defined by their values)
- Immutable
- Inherit from `ValueObject` base class
- Override `GetEqualityComponents()`
- Use static factory methods for creation

**Example Pattern:**
```csharp
public sealed class Position : ValueObject
{
    public int Value { get; }
    
    private Position(int value)
    {
        Value = value;
    }
    
    public static Position Create(int value)
    {
        if (value < 0)
            throw new DomainException("Position cannot be negative");
        return new Position(value);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

### Domain Events
- Immutable records
- Past tense naming (BoardCreated, not CreateBoard)
- Include OccurredOn timestamp
- Raised by aggregates, handled in Application layer

**Pattern:**
```csharp
public sealed record BoardCreatedEvent(Guid BoardId, Guid ProjectId, string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
```

## Application Layer (CQRS)

### Vertical Slice Structure
Organize by feature, not by technical layer:
```
Application/
└── Boards/
    ├── Commands/
    │   ├── CreateBoard/
    │   │   ├── CreateBoardCommand.cs
    │   │   ├── CreateBoardHandler.cs
    │   │   └── CreateBoardValidator.cs
    │   └── AddCard/
    │       ├── AddCardCommand.cs
    │       ├── AddCardHandler.cs
    │       └── AddCardValidator.cs
    ├── Queries/
    │   └── GetBoard/
    │       ├── GetBoardQuery.cs
    │       └── GetBoardHandler.cs
    └── DTOs/
        ├── BoardDto.cs
        └── CardDto.cs
```

### Commands (Write Operations)
- Use `IRequest<TResponse>` from MediatR
- Named with imperative verbs (CreateBoard, UpdateCard, MoveCard)
- Validated with FluentValidation
- Return DTOs, not domain entities
- Handlers orchestrate domain operations and persistence

**Pattern:**
```csharp
// Command
public record CreateBoardCommand(Guid ProjectId, string Name) : IRequest<BoardDto>;

// Validator
public class CreateBoardValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

// Handler
public class CreateBoardHandler : IRequestHandler<CreateBoardCommand, BoardDto>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IProjectRepository _projectRepository;
    
    public async Task<BoardDto> Handle(CreateBoardCommand request, CancellationToken ct)
    {
        // 1. Validate project exists
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, ct);
        if (project == null)
            throw new NotFoundException(nameof(Project), request.ProjectId);
        
        // 2. Use domain to create board
        var board = Board.Create(request.Name, request.ProjectId);
        
        // 3. Persist (transaction handled by pipeline behavior)
        await _boardRepository.AddAsync(board, ct);
        
        // 4. Return DTO
        return new BoardDto(board.Id, board.Name, board.ProjectId, board.CreatedAt);
    }
}
```

### Queries (Read Operations)
- Use `IRequest<TResponse>` from MediatR
- Named with "Get" prefix (GetBoard, GetBoardsByProject)
- Return DTOs optimized for the UI
- Can bypass domain for performance (query database directly)

**Pattern:**
```csharp
public record GetBoardQuery(Guid BoardId) : IRequest<BoardDetailsDto>;

public class GetBoardHandler : IRequestHandler<GetBoardQuery, BoardDetailsDto>
{
    private readonly IBoardRepository _boardRepository;
    
    public async Task<BoardDetailsDto> Handle(GetBoardQuery request, CancellationToken ct)
    {
        var board = await _boardRepository.GetBoardWithDetailsAsync(request.BoardId, ct);
        
        if (board == null)
            throw new NotFoundException(nameof(Board), request.BoardId);
        
        // Map to DTO
        return MapToDto(board);
    }
}
```

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

**Example Configuration:**
```csharp
public class BoardConfiguration : IEntityTypeConfiguration<Board>
{
    public void Configure(EntityTypeBuilder<Board> builder)
    {
        builder.ToTable("Boards");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedNever(); // Guids generated in domain
        
        // Owned entities - Columns
        builder.OwnsMany(b => b.Columns, columnsBuilder =>
        {
            columnsBuilder.ToTable("Columns");
            columnsBuilder.WithOwner().HasForeignKey("BoardId");
            columnsBuilder.HasKey("Id");
            
            // Value object mapping
            columnsBuilder.OwnsOne(c => c.Position, positionBuilder =>
            {
                positionBuilder.Property(p => p.Value)
                    .HasColumnName("Position")
                    .IsRequired();
            });
        });
        
        builder.Ignore(b => b.DomainEvents);
    }
}
```

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
  dotnet ef migrations add MigrationName --project src/NeonBoard.Infrastructure --startup-project src/NeonBoard.AppHost
```

## API Layer (Minimal APIs)

### Endpoint Organization
- Group related endpoints using `MapGroup`
- One endpoint file per aggregate (BoardEndpoints.cs, ProjectEndpoints.cs)
- Static methods for each endpoint
- Use extension methods to register: `app.MapBoardEndpoints()`

**Pattern:**
```csharp
public static class BoardEndpoints
{
    public static void MapBoardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/boards")
            .WithTags("Boards")
            .RequireAuthorization()
            .WithOpenApi();

        group.MapPost("/", CreateBoard)
            .WithName("CreateBoard")
            .Produces<BoardDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/{id:guid}", GetBoard)
            .WithName("GetBoard")
            .Produces<BoardDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateBoard(
        CreateBoardRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new CreateBoardCommand(request.ProjectId, request.Name);
        var result = await mediator.Send(command, ct);
        return Results.Created($"/api/boards/{result.Id}", result);
    }

    private static async Task<IResult> GetBoard(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetBoardQuery(id);
        var result = await mediator.Send(query, ct);
        return Results.Ok(result);
    }
}

// Request models (API layer only, not Application commands)
public record CreateBoardRequest(Guid ProjectId, string Name);
```

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

### Entity/Aggregate Patterns
```csharp
public sealed class MyAggregate : Entity, IAggregateRoot
{
    private readonly List<MyChild> _children = new();
    
    public string Name { get; private set; }
    public IReadOnlyCollection<MyChild> Children => _children.AsReadOnly();
    
    private MyAggregate() { } // For EF Core
    
    public static MyAggregate Create(...)
    {
        // Validation and creation logic
    }
    
    public void DoSomething(...)
    {
        // Business logic
        RaiseDomainEvent(new SomethingHappenedEvent(...));
    }
}
```

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

**Example:**
```csharp
public class BoardTests
{
    [Fact]
    public void Create_ShouldCreateBoardWithValidData()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var boardName = "Sprint Board";

        // Act
        var board = Board.Create(boardName, projectId);

        // Assert
        board.Should().NotBeNull();
        board.Name.Should().Be(boardName);
        board.ProjectId.Should().Be(projectId);
    }
    
    [Fact]
    public void AddCard_ShouldThrowException_WhenColumnDoesNotExist()
    {
        // Arrange
        var board = Board.Create("Test Board", Guid.NewGuid());
        var nonExistentColumnId = Guid.NewGuid();

        // Act
        var act = () => board.AddCard(nonExistentColumnId, "Test Card", "Description");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage($"Column {nonExistentColumnId} not found in board");
    }
}
```

### Integration Tests (tests/NeonBoard.IntegrationTests/)
- Test API endpoints end-to-end
- Use `WebApplicationFactory` with in-memory database
- Test full request/response cycle
- Verify database state after operations

**Example:**
```csharp
public class BoardEndpointsTests : IClassFixture<NeonBoardWebApplicationFactory>
{
    private readonly HttpClient _client;
    
    [Fact]
    public async Task CreateBoard_ReturnsCreatedBoard()
    {
        // Arrange
        var request = new { ProjectId = Guid.NewGuid(), Name = "Test Board" };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/boards", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var board = await response.Content.ReadFromJsonAsync<BoardDto>();
        board.Should().NotBeNull();
        board.Name.Should().Be("Test Board");
    }
}
```

## Common Tasks

### Adding a New Feature
1. **Start with Domain:** Create/update entities, value objects, domain events
2. **Application Layer:** Create command/query + handler + validator
3. **Infrastructure:** Update EF Core configurations if needed, create migration
4. **API:** Add endpoint in appropriate endpoints file
5. **Tests:** Add unit tests for domain logic, integration tests for API

### Creating a Migration
```bash
dotnet ef migrations add MigrationName --project src/NeonBoard.Infrastructure --startup-project src/NeonBoard.AppHost
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

### Folder Structure (Strict!)
```
Domain/
  Common/           # Base classes, interfaces
  {Aggregate}/      # One folder per aggregate
    {Aggregate}.cs  # Aggregate root
    Entities/       # Child entities
    ValueObjects/   # Value objects
    Events/         # Domain events

Application/
  Common/           # Shared interfaces, behaviors, exceptions
  {Feature}/        # One folder per feature area
    Commands/
      {Command}/    # One folder per command
    Queries/
      {Query}/      # One folder per query
    DTOs/           # Data transfer objects

Infrastructure/
  Data/
    Configurations/ # EF Core configurations
    Interceptors/   # EF Core interceptors
  Repositories/     # Repository implementations
  Services/         # Service implementations

Api/
  Endpoints/        # Endpoint definitions
  Middleware/       # Custom middleware
  Extensions/       # Extension methods
```

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
❌ **Don't use localStorage/sessionStorage in Angular** - Won't work when served from .NET  

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

### Why These Choices?
- **DDD:** Keeps business logic in domain, protects against anemic models
- **CQRS:** Separates reads/writes, makes code easier to reason about
- **Clean Architecture:** Enables testing, keeps domain pure
- **Vertical Slices:** Features are self-contained, easier to navigate
- **Minimal APIs:** Modern, lightweight, less ceremony than controllers
- **Aspire:** Great local dev experience, doesn't affect production

### Performance Considerations
- Use `.AsNoTracking()` for read-only queries
- Eager load related entities when needed (`.Include()`)
- Index foreign keys and commonly queried fields
- Keep aggregates small (don't load entire object graphs)

### Security Notes (To Be Implemented)
- JWT authentication required for all endpoints
- User can only access their own projects/boards
- Validate user ownership in command handlers
- Use `ICurrentUserService` to get authenticated user ID

## When in Doubt
- Follow existing patterns in the codebase
- Prioritize domain-driven design principles
- Keep it simple - don't over-engineer
- Write tests first if it helps clarify requirements
- Ask for clarification before making architectural decisions

## Angular UI

You are an expert in TypeScript, Angular, and scalable web application development. You write functional, maintainable, performant, and accessible code following Angular and TypeScript best practices.
## TypeScript Best Practices
- Use strict type checking
- Prefer type inference when the type is obvious
- Avoid the `any` type; use `unknown` when type is uncertain
## Angular Best Practices
- Always use standalone components over NgModules
- Must NOT set `standalone: true` inside Angular decorators. It's the default in Angular v20+.
- Use signals for state management
- Implement lazy loading for feature routes
- Do NOT use the `@HostBinding` and `@HostListener` decorators. Put host bindings inside the `host` object of the `@Component` or `@Directive` decorator instead
- Use `NgOptimizedImage` for all static images.
  - `NgOptimizedImage` does not work for inline base64 images.
## Accessibility Requirements
- It MUST pass all AXE checks.
- It MUST follow all WCAG AA minimums, including focus management, color contrast, and ARIA attributes.
### Components
- Keep components small and focused on a single responsibility
- Use `input()` and `output()` functions instead of decorators
- Use `computed()` for derived state
- Set `changeDetection: ChangeDetectionStrategy.OnPush` in `@Component` decorator
- Prefer inline templates for small components
- Prefer Reactive forms instead of Template-driven ones
- Do NOT use `ngClass`, use `class` bindings instead
- Do NOT use `ngStyle`, use `style` bindings instead
- When using external templates/styles, use paths relative to the component TS file.
## State Management
- Use signals for local component state
- Use `computed()` for derived state
- Keep state transformations pure and predictable
- Do NOT use `mutate` on signals, use `update` or `set` instead
## Templates
- Keep templates simple and avoid complex logic
- Use native control flow (`@if`, `@for`, `@switch`) instead of `*ngIf`, `*ngFor`, `*ngSwitch`
- Use the async pipe to handle observables
- Do not assume globals like (`new Date()`) are available.
- Do not write arrow functions in templates (they are not supported).
## Services
- Design services around a single responsibility
- Use the `providedIn: 'root'` option for singleton services
- Use the `inject()` function instead of constructor injection
