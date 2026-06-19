# TimeLedger Analysis — Iteration 3

| Project Name: | TimeLedger      |
|---------------|-----------------|
| Date:         | 2026-05-22      |
| Author:       | Yevhenii Shvets |
| Version:      | 3.0             |
| Iteration:    | 3               |

## 1. Purpose

This document captures the functional requirements and user specifications for the `TimeLedger` solution, aligned with iteration 3 implementation. It provides requirements and separates delivered features from planned scope for future iterations.

## 2. Product overview

`TimeLedger` is a Razor Pages web application for scheduling personal events and managing account access.
The solution currently focuses on:

- event creation, editing, deletion, and day-based viewing
- user registration, login, profile viewing, and profile editing
- session-based authentication using ASP.NET Core session state
- SQL Server persistence through repository classes

## 3. Current implementation snapshot

### 3.1 Solution structure

- `TimeLedger.Web` — Razor Pages web UI
- `TimeLedger.Desktop` — Avalonia UI for offline use
- `TimeLedger.Business.Core` — DTOs, domain models, interfaces, and services for offline/online logic
- `TimeLedger.Business.Collaboration` — Online services
- `TimeLedger.Data` — Repository implementations for SQL Server
- `TimeLedger.Data.Local` — Repository implementations for local SQLite
- `TimeLedger.Tests` — NUnit test project scaffold

### 3.2 Technology stack

| Area | Current implementation           |
|---|----------------------------------|
| Web UI | ASP.NET Core Razor Pages         |
| Desktop UI | Avalonia UI                      |
| Target framework | `.NET 10.0`                      |
| Persistence | SQL Server via `Microsoft.Data.SqlClient` |
| Local persistence | SQLite via `Microsoft.Data.Sqlite` |
| Password hashing | `BCrypt`                         |
| State management | ASP.NET Core session             |
| Validation | Data annotations + service validation |

### 3.3 Implemented features

#### Iteration 1

- Create, view, edit, and delete events
- Detect time overlaps in the event
- Render forms and event cards with shared CSS for consistent styling

#### Iteration 2

- Register and log in with email and password
- View and edit account information or delete account
- Session-based authentication
- View the signed-in account in the sidebar when available
- Users only see and can modify events they own
- Events are owned by either users or groups
- Create groups and become group owner
- Add and remove group members by email
- View groups where user is owner or member
- Only group owners can modify group details and membership

#### Iteration 3

- Invite users to groups and manage invitations from the inbox
- Create and view group events from group pages
- Group event overlap checks include existing group events and member personal events (not includes group events from other groups)
- Event types support three modes: `OneTime`, `Recurrence`, and `Deadline`

#### Iteration 4
- Event notifications
- Week events view
- Desktop app with offline support
- Polishment of UI and UX

## 4. Problem statement

The original project goal was to build an advanced scheduling application.
The live implementation now includes account management, authenticated event scheduling, group collaboration, and proper authorization checks.

## 5. Stakeholders

| Stakeholder | Role | Interests                                                                  |
|---|---|----------------------------------------------------------------------------|
| University Student | Primary End User | Reliable schedule management, conflict prevention, simple UI               |
| Developer | Builder and Maintainer | Clean architecture, testable code, clear requirements                      |
| Academic Assessor | Evaluator | Demonstrated iterative process, clean layered design, proper documentation |

## 6. Functional requirements


| ID    | Requirement                                                                                                                                     | Scope          | Status      |
|-------|-------------------------------------------------------------------------------------------------------------------------------------------------|----------------|-------------|
| FR-1  | The system shall allow a user to create differently-typed events with a title, optional description and other type-related fields               | Both | Implemented |
| FR-2  | The system shall validate that the start time is strictly before the end time                                                                   | Both           | Implemented |
| FR-3  | The system shall warn user when creating or updating events that overlap with existing events                                                   | Both           | Implemented |
| FR-4  | The system shall allow users to view their events in a day-based calendar view                                                                  | Both           | Implemented |
| FR-5  | The system shall allow users to edit and delete their events                                                                                    | Both           | Implemented |
| FR-6  | The system shall persist event data across application restarts                                                                                 | Both           | Implemented |
| FR-7  | The system shall display descriptive inline error messages when validation fails                                                                | Both           | Implemented |
| FR-8  | The system shall display the calculated duration of each event in the event list                                                                | Both           | Implemented |
| FR-9  | Register users with email and password                                                                                                          | Web            | Implemented |
| FR-10 | Authenticate users via session auth                                                                                                             | Web            | Implemented |
| FR-11 | Associate events with the user who created them                                                                                                 | Both           | Implemented |
| FR-12 | System should prevent users from accessing pages or data they do not have access to                                                             | Both           | Implemented |
| FR-13 | Allow users to create groups and become group owners                                                                                            | Both (online)  | Implemented |
| FR-14 | Allow group owners to manage group members                                                                                                      | Both (online)  | Implemented |
| FR-15 | Restrict group access to owners and members                                                                                                     | Both (online)  | Implemented |
| FR-16 | Allow users to log out and clear session                                                                                                        | Web            | Implemented |
| FR-17 | Allow users to view and edit account information                                                                                                | Both (online)  | Implemented |
| FR-18 | Allow group owners to invite users to groups by email                                                                                           | Both (online)  | Implemented |
| FR-19 | Show group invitations in inbox and allow accept/decline actions                                                                                | Both (online)  | Implemented |
| FR-20 | The system shall provide a desktop application with full event management capabilities                                                          | Desktop        | Planned     |
| FR-21 | The desktop application shall maintain a local SQLite database as its primary data store                                                        | Desktop        | Planned     |
| FR-22 | The desktop application shall sync with the hosted database on startup, pulling only changes since last sync                                    | Desktop        | Planned     |
| FR-23 | The desktop application shall support offline access, allowing users to view and manage their events without an active internet connection      | Desktop        | Planned     |
| FR-24 | The desktop application shall write all changes to both local and hosted databases while the server is available                                | Desktop     | Planned     |
| FR-25 | The desktop application shall allow user to turn off autosync and manually trigger sync with the hosted database                                                                 | Desktop     | Planned     |
| FR-26 | The desktop application shall fall back to local data when the hosted server is unreachable                                                     | Desktop        | Planned     |
| FR-27 | Online features (groups, invitations) shall be available on the desktop application connected to internet until the hosted server is taken down | Desktop (online) | Planned |




## 7. Non-functional requirements

- **Security**: passwords are stored as hashes, sql queries use parameterization to prevent injection
- **Usability**: forms provide inline validation and shared layout styling
- **Maintainability**: business logic is isolated in services, persistence in repositories, and UI in web layer

## Use case summary

Full use case details are documented in the separate documents for use cases per iteration.

| Use Case | Description                              | Scope          | Related FRs            |
|----------|------------------------------------------|----------------|------------------------|
| UC-1     | Create a new event                      | Both              | FR-1, FR-2, FR-3, FR-7 |
| UC-2     | View events in day-based calendar view  | Both              | FR-4, FR-8             |
| UC-3     | Edit an existing event                  | Both              | FR-2, FR-3, FR-5, FR-7 |
| UC-4     | Delete an event                         | Both              | FR-5                   |
| UC-5     | Register an account                     | Web               | FR-9                   |
| UC-6     | Log in                                  | Web               | FR-10                  |
| UC-7     | View signed-in account in sidebar       | Web               | FR-10                  |
| UC-8     | Log out                                 | Web               | FR-16                  |
| UC-9     | View and edit account information       | Both (online)     | FR-17                  |
| UC-10    | Create a group                          | Both (online)     | FR-13                  |
| UC-11    | Invite user to group                    | Both (online)     | FR-14, FR-18           |
| UC-12    | Remove member from group                | Both (online)     | FR-14                  |
| UC-13    | View groups                             | Both (online)     | FR-15                  |
| UC-14    | Accept a group invitation               | Both (online)     | FR-19                  |
| UC-15    | Decline a group invitation              | Both (online)     | FR-19                  |
| UC-16    | Create a group event                    | Both (online)     | FR-22                  |
| UC-17    | View group events                       | Both (online)     | FR-15                  |
| UC-18    | Sync events on desktop startup          | Desktop           | FR-22                  |
| UC-19    | Sync events manually                    | Both              | FR-25                  |
| UC-20    | Use online features on desktop          | Desktop (online)  | FR-27                  |
| UC-21    | Use offline features on desktop         | Desktop (offline) | FR-27                  |

## 9. Summary

Iteration 3 has evolved the application into a richer collaboration platform with invitation inbox flows, multi-type events, and group-event conflict awareness.
The most significant improvements in this iteration are inbox-based group invitations, support for `OneTime`/`Recurrence`/`Deadline` event types, and overlap validation against both group and member schedules.
