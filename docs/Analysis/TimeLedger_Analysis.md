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



### 3.4 Planned items (Iteration 4+)

- Calendar view improvements with richer day/week aggregation
- Advanced notification types and inbox filtering
- Tag support for events

## 4. Problem statement

The original project goal was to build an advanced scheduling application.
The live implementation now includes account management, authenticated event scheduling, group collaboration, and proper authorization checks.

## 5. Stakeholders

| Stakeholder | Role | Interests                                                                  |
|---|---|----------------------------------------------------------------------------|
| University Student | Primary End User | Reliable schedule management, conflict prevention, simple UI               |
| Developer | Builder and Maintainer | Clean architecture, testable code, clear requirements                      |
| Academic Assessor | Evaluator | Demonstrated iterative process, clean layered design, proper documentation |

## 6. Functional requirements for iteration 3

### Implemented in the current solution

| ID    | Requirement | Status      |
|-------|-|-------------|
| FR-1  | The system shall allow a user to create a new event with a title, optional description, start time, and end time | Implemented |
| FR-2  | The system shall validate that the start time is strictly before the end time | Implemented |
| FR-3  | The system shall warn user when creating or updating events that overlap with existing events | Implemented |
| FR-4  | The system shall allow users to view their events in a day-based calendar view | Implemented |
| FR-5  | The system shall allow users to edit and delete their events | Implemented |
| FR-6  | The system shall persist event data across application restarts | Implemented |
| FR-7  | The system shall display descriptive inline error messages when validation fails | Implemented |
| FR-8  | The system shall display the calculated duration of each event in the event list | Implemented |
| FR-9  | Register users with email and password | Implemented |
| FR-10 | Authenticate users via session auth | Implemented |
| FR-11 | Associate events with the user who created them | Implemented |
| FR-12 | Prevent access to other users' events | Implemented |
| FR-13 | Allow users to create groups and become group owners | Implemented |
| FR-14 | Allow group owners to add/remove members by email | Implemented |
| FR-15 | Restrict group access to owners and members | Implemented |
| FR-16 | Allow users to log out and clear session | Implemented |
| FR-17 | Allow users to view and edit account information | Implemented |
| FR-18 | Allow group owners to invite users to groups by email | Implemented |
| FR-19 | Show group invitations in inbox and allow accept/decline actions | Implemented |
| FR-20 | Support event types: `OneTime`, `Recurrence`, and `Deadline` | Implemented |
| FR-21 | Allow user to change event type for existing events | Implemented |
| FR-22 | Validate group event overlaps against group schedule and member events | Implemented |


## 7. Non-functional requirements

- **Security**: passwords are stored as hashes, sql queries use parameterization to prevent injection
- **Usability**: forms provide inline validation and shared layout styling
- **Maintainability**: business logic is isolated in services, persistence in repositories, and UI in web layer

## Use case summary

Full use case details are documented in the separate documents for use cases per iteration.

| Use Case | Description                             | Related FRs       | Iteration |
|----------|-----------------------------------------|-------------------|----------|
| UC-1     | Create a new event                      | FR-1, FR-2, FR-7  |1|
| UC-2     | View events in day-based calendar view  | FR-4              |1|
| UC-3     | View event duration                     | FR-8              |1|
| UC-4     | Edit an existing event                  | FR-5, FR-2, FR-3, FR-7 |1|
| UC-5     | Delete an event                         | FR-5              |1|
| UC-6     | Detect and warn on time overlap         | FR-3              |1|
| UC-8     | Register an account                                  | FR-9                 |2|
| UC-9     | Log in                                               | FR-10                |2|
| UC-10    | View signed-in account in sidebar                    | FR-10                |2|
| UC-11    | Log out                                              | FR-16                |2|
| UC-12    | View and edit account information                    | FR-17                |2|
| UC-13    | View own events only                                 | FR-11, FR-12         |2|
| UC-14    | Create a group                                       | FR-13                |2|
| UC-15    | Add and remove group members                         | FR-14                |2|
| UC-16    | View groups                                          | FR-15                |2|
| UC-18    | Invite a user to a group                                 | FR-18                            |3|
| UC-19    | Accept a group invitation                                | FR-19                            |3|
| UC-20    | Decline a group invitation                               | FR-19                            |3|
| UC-21    | Create a group event                                     | Implemented feature (group events) |3|
| UC-22    | View group events                                        | Implemented feature (group events) |3|
| UC-23    | Validate group event overlap                             | FR-22                            |3|
| UC-24    | Set event type on creation                               | FR-20                            |3|
| UC-25    | Change event type for an existing event                  | FR-21                            |3|


## 9. Summary

Iteration 3 has evolved the application into a richer collaboration platform with invitation inbox flows, multi-type events, and group-event conflict awareness.
The most significant improvements in this iteration are inbox-based group invitations, support for `OneTime`/`Recurrence`/`Deadline` event types, and overlap validation against both group and member schedules.
