# TimeLedger Design — Iteration 2

| Project Name: | TimeLedger      |
|---|-----------------|
| Date | 2026-04-17      |
| Author | Yevhenii Shvets |
| Version | 2.0             |
| Iteration | 2               |


## 1. Design goals

This document describes the software and database architecture of the `TimeLedger` solution. It reflects the actual implementation with Razor Pages UI, session-based authentication, layered services, and SQL-backed repositories. The design prioritizes maintainability, security, and adherence to SOLID principles.

## 2. Architecture overview

The application follows a layered **n-tier architecture** design:

```
Presentation Layer (Razor Pages)
           ↓
Service Layer (Business Logic)
           ↓
Repository Pattern (Data Access)
           ↓
SQL Server (Persistence)
```

### 2.1 Architectural Principles

- **Separation of Concerns**: Each layer has distinct responsibilities
- **Dependency Injection**: Loose coupling through interfaces and DI container
- **Repository Pattern**: Data access abstraction for testability and maintainability
- **SOLID Principles**:
  - **S**ingle Responsibility: Each service/repository handles one concern (e.g., `UserService` only handles user operations)
  - **O**pen/Closed: Services and repositories extend functionality without modifying existing code
  - **L**iskov Substitution: Repository implementations are interchangeable (`InMemoryEventRepository` mirrors `EventRepository`)
  - **I**nterface Segregation: Focused interfaces (e.g., `IUserRepository`, `IEventRepository`, `IGroupRepository`)
  - **D**ependency Inversion: Services depend on repository interfaces, not concrete implementations

### 2.2 Layers

| Layer | Responsibility | SOLID Principles Applied |
|---|---|---|
| **Presentation** (`TimeLedger.Web`) | Page rendering, request handling, form binding, redirects, session interaction | SRP: Pages handle only UI logic |
| **Service** (`TimeLedger.Core.Services`) | Validation, business rules, entity-to-DTO mapping, password hashing, authorization | SRP, DI: Each service focuses on one domain |
| **Data Access** (`TimeLedger.Infrastructure.Repositories`) | Persistence contracts and SQL Server implementation | Strategy pattern via interfaces; ISP: Segregated repository interfaces |
| **Core Models** (`TimeLedger.Core`) | Shared data structures, DTOs, domain models, interfaces | SRP: Clear single purpose for each entity |

### 2.3 Security Considerations

- **Password Hashing**: BCrypt.Net-Next used for secure password storage; plaintext passwords never stored
- **Session State**: ASP.NET Core session management with 8-hour timeout
- **Authorization**: Per-user and per-group authorization checks in service layer before data access
- **SQL Injection Prevention**: Parameterized queries via `SqlCommand.Parameters`
- **Ownership Validation**: Events and groups only accessible to owners or authorized members

## 3. Project composition

### 3.1 Web project (`TimeLedger.Web`)

**Composition Root**: Registers services and repositories in `Program.cs`, configures middleware, and enables Razor Pages.

**Configuration**:
- Dependency injection container configured with:
  - `UserService` bound to `IUserRepository` implementations
  - `EventService` bound to `IEventRepository` implementations
  - `GroupService` bound to `IGroupRepository` implementations
  - `AuthSession` for session state management

**Responsibility**: Request routing, form binding, session interaction, and user interface rendering.

### 3.2 Core project (`TimeLedger.Core`)

**Domain Models**: Represent core business entities
- `User` — Account holder with authentication credentials
- `Event` — Schedulable activity owned by user or group
- `EventOwnerType` — Enum determining event owner (User or Group)
- `Group` — Collection of users for collaboration

**Data Transfer Objects (DTOs)**: Decouple API contracts from domain models
- **Account**: `RegisterDto`, `LoginDto`, `UpdateAccountDto`, `AccountInfoDto`
- **Event**: `CreateEventDto`, `UpdateEventDto`, `EventResponseDto`
- **Group**: `CreateGroupDto`, `UpdateGroupDto`, `GroupInfoDto`, `AddMemberDto`

**Repository Interfaces**: Define data access contracts
- `IUserRepository` — User persistence operations
- `IEventRepository` — Event persistence with ownership support
- `IGroupRepository` — Group and membership persistence

**Services**: Implement business logic and coordinate operations
- `UserService` — Account registration, authentication, profile management
- `EventService` — Event CRUD, overlap detection, authorization
- `GroupService` — Group creation, membership management
- `AuthSession` — Session state management and user context

### 3.3 Infrastructure project (`TimeLedger.Infrastructure`)

**SQL Repository Implementations**: Concrete data access layer
- `UserRepository` — User CRUD and authentication lookups via SQL Server
- `EventRepository` — Event CRUD with ownership filtering and overlap queries
- `GroupRepository` — Group CRUD with membership management via `GroupMembers` junction table
- `InMemoryEventRepository` — In-memory implementation for testing and development without database

**Data Persistence**: Uses parameterized `SqlCommand` queries to prevent SQL injection and ensure security.

## 4. Domain model

### 4.1 User

Represents an authenticated account holder.

| Field | Type | Constraints | Notes |
|---|---|---|---|
| `Id` | `int` | Primary Key | Database identifier; auto-incremented |
| `Name` | `string` | NOT NULL | Display name; trimmed at service layer |
| `Email` | `string` | UNIQUE, NOT NULL | Login identifier; case-insensitive matching |
| `PasswordHash` | `string` | NOT NULL | BCrypt hash; plaintext never stored or logged |
| `CreatedAt` | `DateTime` | NOT NULL | UTC timestamp of account creation |

**Relationships**:
- Owns many `Event` records (via `Event.OwnerId` where `Event.OwnerType = User`)
- Owns many `Group` records (via `Group.OwnerId`)
- Member of many `Group` records (via `GroupMembers` junction table)

### 4.2 Event

Represents a schedulable activity owned by either a user or group.

| Field | Type | Constraints | Notes |
|---|---|---|---|
| `Id` | `int` | Primary Key | Database identifier; auto-incremented |
| `OwnerType` | `EventOwnerType` (byte enum) | NOT NULL, Default = User | Enum: `User = 1` or `Group = 2` |
| `OwnerId` | `int` | NOT NULL | Foreign key to User or Group depending on `OwnerType` |
| `Title` | `string` | NOT NULL, max 200 | Event title |
| `Description` | `string?` | NULL, max 1000 | Optional event details |
| `Location` | `string?` | NULL, max 300 | Optional event location |
| `StartTime` | `DateTime` | NOT NULL | Event start time (UTC) |
| `EndTime` | `DateTime` | NOT NULL | Event end time (UTC) |
| `AllowOverlap` | `bool` | NOT NULL, Default = false | Override for overlap detection |

**Relationships**:
- Owned by one `User` or `Group` (polymorphic via `OwnerType` enum)
- Filtered by `(OwnerType, OwnerId)` composite key

**Business Rules**:
- `EndTime` must be greater than `StartTime`
- Overlap detection: Events of the same owner within the same time range are flagged unless `AllowOverlap = true`
- Service-layer authorization: Callers must provide `OwnerType` and `OwnerId` to retrieve/modify events

### 4.3 Group

Represents a collaborative collection of users for group scheduling.

| Field | Type | Constraints | Notes |
|---|---|---|---|
| `Id` | `int` | Primary Key | Database identifier; auto-incremented |
| `OwnerId` | `int` | NOT NULL, Foreign Key | Reference to `User` (group creator); group creator has administrative rights |
| `Name` | `string` | NOT NULL | Group name; max 256 characters |

**Relationships**:
- Owned by one `User` (via `OwnerId`)
- Contains many `User` members (via `GroupMembers` junction table)
- Owns many `Event` records (via `Event.OwnerId` where `Event.OwnerType = Group`)

**Business Rules**:
- Only the owner can add/remove members
- Only the owner can modify group details
- Members can only view/contribute to group events, not modify membership
- Group owner is automatically a member

### 4.4 GroupMembers (Junction Table)

Represents the many-to-many relationship between `User` and `Group`.

| Field | Type | Constraints | Notes |
|---|---|---|---|
| `GroupId` | `int` | Foreign Key, Primary Key (part 1) | Reference to `Group`; CASCADE delete |
| `UserId` | `int` | Foreign Key, Primary Key (part 2) | Reference to `User`; CASCADE delete |

**Business Rules**:
- Composite primary key ensures no duplicate memberships
- Group owner is also a member (stored in `GroupMembers`)
- Deleting a user or group cascades to remove membership records

## 5. DTO design

### Account DTOs

- `RegisterDto` — register form input
- `LoginDto` — login form input
- `UpdateAccountDto` — profile edit input
- `AccountInfoDto` — read-only profile view model

### Event DTOs

- `CreateEventDto` — create form input; does not include ownership (assigned by service)
- `UpdateEventDto` — edit form input; does not include ownership
- `EventResponseDto` — event list/details projection with computed `Duration` string

### Group DTOs

- `CreateGroupDto` — group creation form input
- `UpdateGroupDto` — group edit form input
- `GroupInfoDto` — group details including owner ID and full member list (List<AccountInfoDto>)
- `AddMemberDto` — member addition form input (email-based)

## 6. Service design

### 6.1 `UserService`

Responsibilities:

- validate required fields and lengths
- register new users
- verify login credentials using BCrypt
- load account data by id
- update profile data and password
- delete users

Important behavior:

- registration returns an `AccountInfoDto`
- login returns an `AccountInfoDto` for session storage
- duplicate emails are rejected
- password confirmation must match

### 6.2 `EventService`

Responsibilities:

- list events filtered by owner (user or group) via `OwnerType` and `OwnerId` parameters
- fetch event details by id with ownership validation
- create, update, and delete events with ownership assignment
- validate time ranges and field lengths
- check overlap rules before insertion
- map between `Event` and `EventResponseDto`
- enforce authorization (caller must provide correct `OwnerType` and `OwnerId`)

Important behavior:

- All methods require caller to specify `EventOwnerType` and `OwnerId`; events are filtered strictly by this pair
- If overlap is detected and `AllowOverlap = false`, the create/update methods return `(dto, hasOverlap: true)` but do NOT save the event
- `Duration` is calculated in the response DTO computed property rather than stored in the entity
- Attempting to access an event without providing the correct `OwnerType`/`OwnerId` returns `null` or throws `KeyNotFoundException`

### 6.3 `GroupService`

Responsibilities:

- list groups accessible to the user (groups they own or are members of)
- fetch group details by id with authorization check
- create, update, and delete groups
- manage group membership (add/remove members)
- validate form input (group name, email addresses)
- enforce ownership and authorization rules

Important behavior:

- Groups created have `OwnerId = userId` (creator becomes owner)
- Only owners can modify group details or membership
- Members are added by email address, which must match an existing user
- Owner is implicitly included in the group (not stored as member)
- Attempting to access a group as non-owner/member raises `InvalidOperationException`
- Removing members is only allowed for non-owner members
- Group membership is validated to prevent duplicates

### 6.4 `AuthSession`

`AuthSession` centralizes the session keys used by the web project:

- `Auth.UserId`
- `Auth.UserEmail`
- `Auth.UserName`

This avoids hard-coded strings in page models and layout code.

## 7. Repository design

### 7.1 Repository interfaces

- `IUserRepository` — User lookup and persistence
- `IEventRepository` — Event persistence with ownership support (OwnerType and OwnerId parameters on all queries)
- `IGroupRepository` — Group and membership persistence

These interfaces define the persistence contract used by the services, keeping the business layer independent from SQL details. Ownership parameters are baked into method signatures to enforce authorization at the repository level.

### 7.2 SQL repository implementation

All repositories use `Microsoft.Data.SqlClient` directly with parameterized SQL queries.
They read the default connection string from configuration.

#### UserRepository

- `GetById()`, `GetByEmail()` — Lookups by identifier or email
- `Add()`, `Update()`, `Delete()` — CRUD operations
- `Exists()` — Email uniqueness check

#### EventRepository

- `GetAll(ownerType, ownerId)` — Retrieve all events for a specific owner (user or group)
- `GetById(id, ownerType, ownerId)` — Fetch event with ownership validation
- `Create()`, `Update()`, `Delete()` — CRUD with ownership assignment
- Uses `OUTPUT INSERTED.Id` to capture inserted ids
- Overlap detection via SQL range comparison (`StartTime < @EndTime AND EndTime > @StartTime`)
- All queries validate ownership before returning data (prevents unauthorized access)

#### GroupRepository

- `GetAllGroups()` — Filter groups by member or owner status (accessible to user)
- `GetGroupById()` — Fetch group with ownership validation
- `CreateGroup()`, `UpdateGroup()`, `DeleteGroup()` — CRUD with owner assignment
- `GetGroupMembers()` — List members (excluding owner)
- `AddGroupMember()`, `RemoveGroupMember()` — Manage membership
- `IsMember()` — Check membership status
- Maintains `GroupMembers` junction table for many-to-many relationships


### 7.3 Database constraints

- User deletion cascades to owned events (via `ON DELETE CASCADE`)
- Group deletion cascades to group memberships (via `ON DELETE NO ACTION` to prevent cycles)
- Composite indexes on `(OwnerId, StartTime)` for efficient event queries

## 8. Razor Pages design

### 8.1 Shared layout

`Pages/Shared/_Layout.cshtml` is responsible for global navigation.
It reads session values to decide whether the user is signed in.

When signed in, the sidebar shows:

- account name
- account email
- links to account info, edit, and log out

When signed out, it shows:

- Log In
- Register

### 8.2 Event pages

| Page | Purpose |
|---|---|
| `/Events` | View all scheduled events grouped by date |
| `/Events/Create` | Create a new event |
| `/Events/Edit/{id}` | Edit an existing event |
| `/Events/Delete/{id}` | Confirm and delete an event |

### 8.3 Account pages

| Page | Purpose |
|---|---|
| `/Account/Login` | Authenticate a user and create session state |
| `/Account/Register` | Create a new account and sign the user in |
| `/Account/Info` | View account information |
| `/Account/Edit` | Update account details |
| `/Account/Logout` | Clear session state on post |



## 9. Summary

The design reflects the actual Razor Pages implementation with session-based authentication, service-layer validation, and repository-based SQL access. Key achievements in iteration 2:

- **Event ownership model** via `EventOwnerType` enum and `OwnerId` allows events to be owned by either users or groups, with authorization enforced at service and repository layers
- **Group collaboration** through `Group` entities and `GroupMembers` junction table, supporting owner-managed membership
- **Layered architecture** keeping concerns separated: Razor Pages for UI, services for business logic, repositories for data access
- **Parameterized queries** throughout repositories ensure SQL injection prevention
- **Authorization-by-design** through ownership parameters baked into repository method signatures


