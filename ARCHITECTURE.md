# NeonBoard Architecture Overview

## System Architecture

```mermaid
graph TB
    subgraph Client["Client Layer"]
        Browser["Browser"]
    end

    subgraph Angular["NeonBoard.UI (Angular 21)"]
        Auth0FE["Auth0 Angular SDK"]
        Router["Angular Router"]
        Guards["Guards\n(authGuard, unsavedChangesGuard)"]

        subgraph Features["Feature Modules"]
            Projects["Projects\n(list, create)"]
            ProjectDetail["Project Detail\n(layout, overview)"]
            BoardView["Board View\n(kanban canvas)"]
            BoardSettings["Board Settings"]
        end

        subgraph State["State Management (Signals)"]
            BoardFacade["BoardStateFacade"]
            SettingsFacade["BoardSettingsFacade"]
            SidebarFacade["SidebarStateFacade"]
            DrawerSvc["DrawerService"]
        end

        subgraph HttpSvcs["HTTP Services"]
            ProjectSvc["ProjectService"]
            BoardSvc["BoardService"]
            ColumnSvc["ColumnService"]
            CardSvc["CardService"]
            LabelSvc["LabelService"]
        end

        SharedUI["Shared Components\n(Button, Modal, Drawer, Badge, ...)"]
    end

    subgraph API["NeonBoard.Api (.NET 10 Minimal APIs)"]
        Auth0BE["Auth0 JWT Bearer"]
        GlobalExHandler["GlobalExceptionHandler"]
        CurrUserSvc["CurrentUserService"]
        OwnershipFilter["ProjectOwnershipFilter"]

        subgraph Endpoints["Endpoints"]
            ProjectEP["ProjectEndpoints\n/api/projects"]
            BoardEP["BoardEndpoints\n/api/projects/{id}/boards"]
            ColumnEP["ColumnEndpoints\n.../columns"]
            CardEP["CardEndpoints\n.../cards"]
            LabelEP["LabelEndpoints\n.../labels"]
        end
    end

    subgraph Application["NeonBoard.Application (CQRS)"]
        MediatR["MediatR"]

        subgraph Behaviors["Pipeline Behaviors"]
            LogBehavior["LoggingBehavior"]
            ValBehavior["ValidationBehavior"]
            TxBehavior["TransactionBehavior"]
        end

        subgraph Commands["Commands"]
            BoardCmds["Board Commands\n(Create, Update, Delete)"]
            ProjectCmds["Project Commands\n(Create, Update, Delete)"]
            ColumnCmds["Column Commands\n(Add, Rename, Delete, Reorder)"]
            CardCmds["Card Commands\n(Add, Update, Move, Delete)"]
            LabelCmds["Label Commands\n(Add, Update, Remove)"]
        end

        subgraph Queries["Queries"]
            BoardQueries["Board Queries\n(GetDetails, GetByProject)"]
            ProjectQueries["Project Queries\n(GetProject, GetByUser)"]
            CardQueries["Card Queries\n(GetCard)"]
        end

        FluentVal["FluentValidation\nValidators"]
        AppInterfaces["Interfaces\n(IRepository, IBoardRepository,\nIUnitOfWork, ICurrentUserService)"]
    end

    subgraph Domain["NeonBoard.Domain (DDD)"]
        subgraph Aggregates["Aggregates"]
            UserAgg["User\n(Auth0UserId, Email, DisplayName)"]
            ProjectAgg["Project\n(Name, Description, OwnerId)"]

            subgraph BoardAgg["Board Aggregate"]
                Board["Board\n(Name, ProjectId)"]
                Column["Column\n(Name, Position)"]
                Card["Card\n(Content, Position, LabelIds)"]
                Label["Label\n(Name, Color)"]
                Board -->|owns| Column
                Board -->|owns| Card
                Board -->|owns| Label
            end
        end

        subgraph DomainEvents["Domain Events"]
            BoardEvents["Board Events\n(Created, Deleted)"]
            ColumnEvents["Column Events\n(Added, Renamed, Deleted, Reordered)"]
            CardEvents["Card Events\n(Created, Updated, Moved, Deleted)"]
            LabelEvents["Label Events\n(Added, Updated, Removed)"]
        end

        subgraph ValueObjects["Value Objects"]
            Position["Position"]
            CardContent["CardContent\n(Title, Description)"]
        end

        DomainBase["Common Base\n(Entity, AggregateRoot, ValueObject,\nDomainException)"]
    end

    subgraph Infrastructure["NeonBoard.Infrastructure (EF Core + PostgreSQL)"]
        DbContext["ApplicationDbContext\n(IUnitOfWork)"]
        DomainEventInterceptor["DomainEventDispatcherInterceptor"]

        subgraph Repos["Repositories"]
            UserRepo["UserRepository"]
            ProjectRepo["ProjectRepository"]
            BoardRepo["BoardRepository"]
            GenericRepo["Repository&lt;T&gt;"]
        end

        subgraph EFConfig["EF Core Configuration"]
            UserConfig["UserConfiguration"]
            ProjectConfig["ProjectConfiguration"]
            BoardConfig["BoardConfiguration\n(owned entities, JSONB)"]
        end

        Migrations["EF Core Migrations"]
    end

    subgraph ExternalServices["External Services"]
        Auth0["Auth0\n(Identity Provider)"]
        PostgreSQL[("PostgreSQL 16\nneonboarddb")]
    end

    subgraph Orchestration["NeonBoard.AppHost (.NET Aspire)"]
        Aspire["Aspire Orchestration\n(local dev dashboard)"]
    end

    %% Client to Angular
    Browser <-->|HTTPS| Angular

    %% Angular internals
    Auth0FE <-->|OIDC/JWT| Auth0
    Router --> Guards
    Guards --> Features
    Features --> State
    State --> HttpSvcs

    %% Angular to API
    HttpSvcs <-->|REST + JWT Bearer| API

    %% API internals
    Auth0BE <-->|Validate JWT| Auth0
    Endpoints --> OwnershipFilter
    Endpoints --> CurrUserSvc
    Endpoints --> MediatR

    %% Application layer
    MediatR --> Behaviors
    Behaviors --> Commands
    Behaviors --> Queries
    Commands --> FluentVal
    Commands --> AppInterfaces
    Queries --> AppInterfaces

    %% Domain events
    Board -->|raises| DomainEvents

    %% Infrastructure implementations
    AppInterfaces -.->|implemented by| Repos
    Repos --> DbContext
    DbContext --> DomainEventInterceptor
    DomainEventInterceptor -->|MediatR.Publish| MediatR
    DbContext --> EFConfig
    DbContext <-->|SQL| PostgreSQL
    DbContext --> Migrations

    %% Aspire orchestration
    Aspire -->|hosts| API
    Aspire -->|hosts| Angular
    Aspire -->|provisions| PostgreSQL
```

## Request Flow

```mermaid
sequenceDiagram
    participant UI as Angular UI
    participant Auth0 as Auth0
    participant API as ASP.NET Core API
    participant Filter as ProjectOwnershipFilter
    participant MediatR as MediatR Pipeline
    participant Handler as Command/Query Handler
    participant Domain as Domain Aggregate
    participant DB as PostgreSQL

    UI->>Auth0: Login (OIDC)
    Auth0-->>UI: JWT Token

    UI->>API: HTTP Request + JWT Bearer
    API->>API: Validate JWT (Auth0)
    API->>Filter: Check project ownership
    Filter->>DB: IsProjectOwnedByUser?
    DB-->>Filter: true/false
    Filter-->>API: 403 if not owner

    API->>MediatR: Send(Command/Query)
    MediatR->>MediatR: LoggingBehavior
    MediatR->>MediatR: ValidationBehavior (FluentValidation)
    MediatR->>MediatR: TransactionBehavior (begin tx)

    MediatR->>Handler: Handle(command)
    Handler->>DB: Load Aggregate (Repository)
    DB-->>Handler: Aggregate
    Handler->>Domain: Call domain method
    Domain->>Domain: Validate & raise DomainEvent
    Handler->>DB: SaveChanges (UnitOfWork)
    DB->>DB: DomainEventDispatcherInterceptor
    DB->>MediatR: Publish(DomainEvent)

    MediatR->>MediatR: TransactionBehavior (commit tx)
    Handler-->>API: Result DTO
    API-->>UI: HTTP Response (JSON)
```

## Domain Model

```mermaid
classDiagram
    class User {
        +Guid Id
        +string Auth0UserId
        +string Email
        +string DisplayName
    }

    class Project {
        +Guid Id
        +string Name
        +string Description
        +Guid OwnerId
    }

    class Board {
        +Guid Id
        +string Name
        +Guid ProjectId
        +List~Column~ Columns
        +List~Card~ Cards
        +List~Label~ Labels
        +AddColumn(name) Column
        +AddCard(columnId, content) Card
        +MoveCard(cardId, targetColumnId, position)
        +AddLabel(name, color) Label
    }

    class Column {
        +Guid Id
        +string Name
        +Position Position
    }

    class Card {
        +Guid Id
        +Guid ColumnId
        +CardContent Content
        +Position Position
        +List~Guid~ LabelIds
        +Move(columnId, position)
        +UpdateContent(title, description)
    }

    class Label {
        +Guid Id
        +string Name
        +string Color
    }

    class Position {
        +int Value
    }

    class CardContent {
        +string Title
        +string Description
    }

    User "1" --> "many" Project : owns
    Project "1" --> "many" Board : contains
    Board "1" *-- "many" Column : owns
    Board "1" *-- "many" Card : owns
    Board "1" *-- "many" Label : owns
    Card --> Column : belongs to
    Card --> "0..*" Label : tagged with
    Column --> Position
    Card --> Position
    Card --> CardContent
```

## Tech Stack Summary

| Layer | Technology |
|---|---|
| Frontend Framework | Angular 21 (Standalone Components) |
| Frontend State | Angular Signals (Facade Pattern) |
| Frontend Styling | TailwindCSS 3 |
| Frontend Auth | @auth0/auth0-angular |
| Frontend Testing | Vitest + @analogjs/vitest-angular |
| Backend Framework | ASP.NET Core 10 Minimal APIs |
| Backend Pattern | Clean Architecture + DDD + CQRS |
| Backend Mediator | MediatR 12 |
| Backend Validation | FluentValidation 11 |
| Backend Auth | Auth0 JWT Bearer |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL 16 |
| Logging | Serilog + OpenTelemetry |
| Dev Orchestration | .NET Aspire 13 |
| Containerization | Docker (multi-stage build) |
| Identity Provider | Auth0 |
