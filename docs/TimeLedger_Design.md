# TimeLedger Design — Iteration 2

## 1. Design goals

This document describes the current architecture of the `TimeLedger` solution in Markdown form.
It updates the older PDF design so it reflects the actual implementation:
Razor Pages UI, session-based authentication, layered services, and SQL-backed repositories.

## 2. Architecture overview

The application follows a layered design:

`Razor Pages -> Services -> Repositories -> SQL Server`

### Layers

| Layer | Responsibility |
|---|---|
| Razor Pages (`TimeLedger.Web`) | Page rendering, request handling, form binding, redirects, and session interaction |
| Services (`TimeLedger.Core.Services`) | Validation, business rules, entity-to-DTO mapping, password hashing, and flow control |
| Repositories (`TimeLedger.Core.Interfaces` + `TimeLedger.Infrastructure.Repositories`) | Persistence contracts and SQL Server implementation |
| Core models/DTOs (`TimeLedger.Core`) | Shared data structures for requests and responses |

## 3. Project composition

### 3.1 Web project

`TimeLedger.Web` is the composition root.
It registers the services and repositories in `Program.cs`, enables Razor Pages, and configures session state.

Important configuration details:

- `AddRazorPages()` for the UI
- `AddSession()` with an 8-hour idle timeout
- `UseSession()` before Razor Pages are mapped
- dependency injection for `UserService`, `EventService`, `IUserRepository`, and `IEventRepository`

### 3.2 Core project

`TimeLedger.Core` contains:

- domain models: `User`, `Event`
- DTOs for account and event operations
- repository interfaces
- services: `UserService`, `EventService`, and `AuthSession`

### 3.3 Infrastructure project

`TimeLedger.Infrastructure` contains the SQL repository implementations:

- `UserRepository`
- `EventRepository`
- `InMemoryEventRepository` for alternate/local testing scenarios

## 4. Domain model

### 4.1 User

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | Database identifier |
| `Name` | `string` | Display name |
| `Email` | `string` | Unique login identifier |
| `PasswordHash` | `string` | BCrypt hash, never plain text |
| `CreatedAt` | `DateTime` | Account creation timestamp |

### 4.2 Event

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | Database identifier |
| `Title` | `string` | Event title |
| `Description` | `string?` | Optional details |
| `Location` | `string?` | Optional location |
| `StartTime` | `DateTime` | Event start |
| `EndTime` | `DateTime` | Event end |
| `AllowOverlap` | `bool` | User override for overlap warnings |

## 5. DTO design

### Account DTOs

- `RegisterDto` — register form input
- `LoginDto` — login form input
- `UpdateAccountDto` — profile edit input
- `AccountInfoDto` — read-only profile view model

### Event DTOs

- `CreateEventDto` — create form input
- `UpdateEventDto` — edit form input
- `EventResponseDto` — event list/details projection, including a computed `Duration` string

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

- list events
- fetch event details by id
- create, update, and delete events
- validate time ranges and field lengths
- check overlap rules
- map between `Event` and `EventResponseDto`

Important behavior:

- if overlap is detected and overlap is not allowed, the service returns the mapped DTO with `hasOverlap = true`
- `Duration` is calculated in the response DTO rather than stored in the entity

### 6.3 `AuthSession`

`AuthSession` centralizes the session keys used by the web project:

- `Auth.UserId`
- `Auth.UserEmail`
- `Auth.UserName`

This avoids hard-coded strings in page models and layout code.

## 7. Repository design

### 7.1 Repository interfaces

`IUserRepository` and `IEventRepository` define the persistence contract used by the services.
This keeps the business layer independent from SQL details.

### 7.2 SQL repository implementation

`UserRepository` and `EventRepository` use `Microsoft.Data.SqlClient` directly.
They read the default connection string from configuration and execute parameterized SQL commands.

Design notes:

- parameterized queries reduce injection risk
- `SCOPE_IDENTITY()` is used to capture inserted user ids
- overlap detection is handled in SQL by range comparison
- `GetAll()` orders events by `StartTime`

### 7.3 In-memory repository

`InMemoryEventRepository` mirrors the event repository contract for non-database scenarios.
It is not currently the default runtime implementation.

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

## 9. Known design gaps for iteration 2

The current implementation intentionally does not yet include:

- group entities
- group invitation workflows
- event ownership fields
- per-user event filtering
- authorization checks that prevent users from viewing other users’ events

These items remain consistent with the iteration-2 use-case document but should be implemented in a later pass.

## 10. Traceability matrix

| Design element | Source |
|---|---|
| Composition root and DI | `TimeLedger.Web/Program.cs` |
| Session keys | `TimeLedger.Core/Services/AuthSession.cs` |
| Account service behavior | `TimeLedger.Core/Services/UserService.cs` |
| Event service behavior | `TimeLedger.Core/Services/EventService.cs` |
| User SQL persistence | `TimeLedger.Infrastructure/Repositories/UserRepository.cs` |
| Event SQL persistence | `TimeLedger.Infrastructure/Repositories/EventRepository.cs` |
| Sidebar account links | `TimeLedger.Web/Pages/Shared/_Layout.cshtml` |
| Event page models | `TimeLedger.Web/Pages/Events/*.cshtml.cs` |
| Account page models | `TimeLedger.Web/Pages/Account/*.cshtml.cs` |

## 11. Summary

The updated design is simpler than the older PDF version because the actual solution is not using controllers, EF Core, React, or JWT.
Instead, it is a Razor Pages app with session auth, service-layer validation, and repository-based SQL access.

