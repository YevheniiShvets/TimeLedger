# TimeLedger Analysis — Iteration 2

| Project Name: | TimeLedger      |
|---------------|-----------------|
| Date:         | 2026-04-17      |
| Author:       | Yevhenii Shvets |
| Version:      | 2.0             |
| Iteration:    | 2               |

## 1. Purpose

This document captures the functional requirements and user specifications for the `TimeLedger` solution, aligned with iteration 2 implementation. It provides requirements and separates delivered features from planned scope for future iterations.

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

| Area | Current implementation           |
|---|----------------------------------|
| Web UI | ASP.NET Core Razor Pages         |
| Target framework | `.NET 10.0`                      |
| Persistence | SQL Server via `Microsoft.Data.SqlClient` |
| Password hashing | `BCrypt`                         |
| State management | ASP.NET Core session             |
| Validation | Data annotations + service validation |

### 3.3 Implemented features

- Register and log in with email and password
- Store authenticated user data in session via `AuthSession`
- View the signed-in account in the sidebar when available
- View account information in `/Account/Info`
- Edit account details in `/Account/Edit`
- Create, view, edit, and delete events
- Detect time overlaps in the event service and repository
- Events are owned by either users or groups (via `OwnerType` enum)
- Users only see and can modify events they own
- Create groups and become group owner
- Add and remove group members by email
- View groups where user is owner or member
- Only group owners can modify group details and membership
- Render forms and event cards with shared CSS files
- per-user event filtering and authorization in service layer

### 3.4 Planned items (Iteration 3)

- Event creation by groups (events with `OwnerType = Group`)
- Group invitation workflow (invite, accept, decline statuses)
- Persistence of invitation records and membership history
- Calendar view with day-based event grouping
- Advanced permission models for group events
- Recurring events and scheduling features

## 4. Problem statement

The original project goal was to build an advanced scheduling application.
The live implementation now includes account management, authenticated event scheduling, group collaboration, and proper authorization checks.

## 5. Stakeholders

| Stakeholder | Role | Interests                                                                  |
|---|---|----------------------------------------------------------------------------|
| University Student | Primary End User | Reliable schedule management, conflict prevention, simple UI               |
| Developer | Builder and Maintainer | Clean architecture, testable code, clear requirements                      |
| Academic Assessor | Evaluator | Demonstrated iterative process, clean layered design, proper documentation |

## 6. Functional requirements for iteration 2

### Implemented in the current solution

| ID | Requirement | Status |
|---|---|---|
| FR-10 | Register users with email and password | Implemented |
| FR-11 | Authenticate users via session auth | Implemented |
| FR-12 | Associate events with the user who created them | Implemented |
| FR-13 | Prevent access to other users' events | Implemented |
| FR-14 | Allow users to create groups and become group owners | Implemented |
| FR-15 | Allow group owners to add/remove members by email | Implemented |
| FR-16 | Restrict group access to owners and members | Implemented |
| FR-17 | Allow users to log out and clear session | Implemented |
| FR-18 | Allow users to view and edit account information | Implemented |

### Planned for iteration 3 or later

| ID | Requirement | Status |
|---|---|---|
| FR-20 | Allow events to be created by groups | Planned |
| FR-21 | Implement group invitation workflow (invite, accept, decline) | Planned |
| FR-22 | Store invitation records and membership history | Planned |
| FR-23 | Display invitations in user interface | Planned |

## 7. Non-functional requirements

- **Security**: passwords are stored as BCrypt hashes
- **Usability**: forms provide inline validation and shared layout styling
- **Maintainability**: business logic is isolated in services, persistence in repositories, and UI in web layer

## 8. Use Case Summary

Full use cases are documented in `UseCasesIT2.md`, but the following table summarizes the main user interactions implemented in iteration 2.

| Use Case Number | Use Case Name | Short Description |
|-----------------|---------------|-------------------|
| UC-10 | Register User | A new user creates an account using name, email, and password. |
| UC-11 | Log In | A registered user signs in and starts an authenticated session. |
| UC-12 | Manage Own Events | An authenticated user can create, view, edit, and delete only their own events. |
| UC-13 | Log Out | An authenticated user ends their session. |
| UC-14 | View Account Info | An authenticated user views their account profile details. |
| UC-15 | Edit Account Info | An authenticated user updates profile details and/or password. |
| UC-16 | Create Group | An authenticated user creates a new group and becomes its owner. |
| UC-17 | View Accessible Groups | A user views groups they own or belong to. |
| UC-18 | Manage Group Members | A group owner adds/removes members and maintains group details. |

## 9. Data Transfer Objects (DTOs)

DTOs define the data contracts between the Presentation layer and the core services.

### Example of what DTOs do in this project (Event management)

**Event flow**
- `CreateEventDto`: carries input for creating an event.
- `UpdateEventDto`: carries input for editing an event.
- `EventResponseDto`: returns event data to the UI, including computed duration text.

## 10. Summary

Iteration 2 has evolved the application from a simple event planner into a multi-user scheduling platform with account management and event ownership.
The most significant improvements are the introduction of authorization checks, event and group ownership models, and SOLID layered architecture.

