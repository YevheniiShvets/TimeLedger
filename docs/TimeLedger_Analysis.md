# TimeLedger Analysis — Iteration 2

## 1. Purpose

This document replaces the outdated PDF analysis with a Markdown version that reflects the current `TimeLedger` solution.
It captures the real implementation state of iteration 2 and separates implemented features from planned scope.

## 2. Product overview

`TimeLedger` is a Razor Pages web application for scheduling personal events and managing account access.
The solution currently focuses on:

- event creation, editing, deletion, and day-based viewing
- user registration, login, profile viewing, and profile editing
- session-based authentication using ASP.NET Core session state
- SQL Server persistence through repository classes

## 3. Current implementation snapshot

### 3.1 Solution structure

- `TimeLedger.Web` — Razor Pages UI and application startup
- `TimeLedger.Core` — DTOs, domain models, interfaces, and services
- `TimeLedger.Infrastructure` — SQL repositories
- `TimeLedger.Tests` — NUnit test project scaffold

### 3.2 Technology stack

| Area | Current implementation |
|---|---|
| Web UI | ASP.NET Core Razor Pages |
| Target framework | `net10.0` |
| Persistence | SQL Server via `Microsoft.Data.SqlClient` |
| Password hashing | `BCrypt.Net-Next` |
| State management | ASP.NET Core session |
| Validation | Data annotations + service validation |

### 3.3 Implemented features

- Register and log in with email and password
- Store authenticated user data in session via `AuthSession`
- View the signed-in account in the sidebar when available
- View account information in `/Account/Info`
- Edit account details in `/Account/Edit`
- View event list in `/Events/Index`
- Create, update, and delete events
- Detect time overlaps in the event service and repository
- Render forms and event cards with shared CSS files

### 3.4 Partially implemented or planned items

- Event ownership is not yet enforced in the event model or repository layer
- Per-user event filtering is not implemented; `Events/Index` currently shows all events returned by the repository
- Group management, invitations, and membership workflows are described in the iteration-2 use cases but are not present in the current codebase
- The solution includes `InMemoryEventRepository`, but the web app is wired to the SQL repository implementation

## 4. Problem statement

The original project goal was broader than the current codebase. The live solution is now centered on account management and schedule organization, but the older documentation still describes a React-based architecture, JWT authentication, and EF Core persistence.
Those details are no longer accurate and should be replaced with the current Razor Pages, session-based, repository-driven design.

## 5. Stakeholders and user roles

| Role | Needs |
|---|---|
| New user | Create an account and start using the app |
| Registered user | Log in, manage events, and update profile data |
| Authenticated user | Access account pages and schedule views |
| Future group owner | Create and manage groups once group features are implemented |
| Future invited member | Respond to invitations once membership workflows exist |

## 6. Functional requirements for iteration 2

### Implemented in the current solution

| ID | Requirement | Status |
|---|---|---|
| FR-10 | Register users with email and password | Implemented |
| FR-11 | Authenticate users via session auth | Implemented |
| FR-12 | Associate events with the user who created them | Planned |
| FR-13 | Prevent access to other users’ events | Planned |
| FR-14 | Allow users to create groups and invite members | Planned |

### Supporting application behavior

- The sidebar shows the signed-in account name and email when session data exists.
- When no user is signed in, the sidebar exposes Log In and Register links.
- Account pages redirect unauthenticated users to the login page.

## 7. Non-functional requirements

- **Security**: passwords are stored as BCrypt hashes; session data uses safe keys in `AuthSession`
- **Usability**: forms provide inline validation and shared layout styling
- **Maintainability**: business logic is isolated in services, persistence in repositories, and UI in Razor Pages
- **Traceability**: source files and use-case documents should remain aligned with the Markdown docs

## 8. Constraints and assumptions

- The app uses SQL Server connection strings configured in the web project.
- `TimeLedger.Web` is the composition root and registers the repository and service implementations.
- Session state is required for authentication and account display.
- Group-related use cases remain future work until matching entities, repositories, services, and pages are added.

## 9. Primary source mapping

| Document claim | Source files |
|---|---|
| Session-based auth | `TimeLedger.Web/Program.cs`, `TimeLedger.Core/Services/AuthSession.cs` |
| Register/login flow | `TimeLedger.Web/Pages/Account/Register.cshtml.cs`, `TimeLedger.Web/Pages/Account/Login.cshtml.cs`, `TimeLedger.Core/Services/UserService.cs` |
| Account view/edit flow | `TimeLedger.Web/Pages/Account/Info.cshtml.cs`, `TimeLedger.Web/Pages/Account/Edit.cshtml.cs` |
| Event CRUD flow | `TimeLedger.Web/Pages/Events/*.cshtml.cs`, `TimeLedger.Core/Services/EventService.cs` |
| SQL persistence | `TimeLedger.Infrastructure/Repositories/UserRepository.cs`, `TimeLedger.Infrastructure/Repositories/EventRepository.cs` |
| Current UI navigation | `TimeLedger.Web/Pages/Shared/_Layout.cshtml` |

## 10. Summary

Iteration 2 has moved the application from a simple event-only planner to a user-aware Razor Pages application.
The most important documentation correction is that authentication and account management are implemented with session state, while group management and per-user event ownership are still planned.

