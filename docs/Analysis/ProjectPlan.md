# TimeLedger Project Plan — Iteration 2

## 1. Project overview

`TimeLedger` is a Razor Pages scheduling application that supports user accounts, event management, and group collaboration.
The current implementation focuses on authenticated user experiences with user-owned event management, account management, and group creation with member management. The backend event model also supports group ownership, but the current UI exposes user-owned event flows.

## 2. Current status

### Fully Implemented

- User registration and login with session-based authentication
- Account information viewing and editing
- Event CRUD (create, edit, delete, list) with overlap detection for user-owned events
- Event ownership support in the backend via `OwnerType` and `OwnerId`
- Group creation and ownership
- Group member management (add/remove members by email)
- Per-user event filtering in the events UI
- Group list visibility for owners and members
- Authorization checks for event and group ownership
- Sidebar account display showing signed-in user, account links, and logout
- SQL Server persistence with repository pattern

### Planned or deferred to iteration 3

- Group invitation workflow (pending, accept, decline)
- Group-owned event creation in the UI
- Advanced scheduling features

## 3. Iteration 2 objectives

### Main objectives

1. Deliver authenticated event management for user-owned events
2. Implement group creation and membership management
3. Establish per-user data visibility and authorization checks
4. Maintain documentation alignment with the actual Razor Pages implementation

### Supporting objectives

1. Keep documentation synchronized with the live codebase
2. Establish SOLID layered design patterns for repositories, services, and pages
3. Keep the backend event ownership model ready for future group-based event creation in the UI

## 4. Scope

### In scope for iteration 2 (Implemented)

- Account registration and login with session state
- Account information and edit screens
- Session state management using `AuthSession`
- Sidebar account display with links and logout
- Event CRUD with overlap detection
- Event ownership assignment to users in the current UI via `OwnerId` and `OwnerType`
- Per-user event filtering and authorization
- Group creation, ownership, and basic info
- Group member management (add/remove by email)
- Group-specific visibility (owners and members only)
- SQL Server persistence through repository pattern
- DTOs for all major flows
- Service layer validation and business logic

### Deferred to a later iteration (IT3)

- Group-owned event creation in the UI
- Group invitations workflow (invite, accept, decline)
- Persistence of group invitations and membership status history
- Recurring events
- User notifications for group invitations

## 5. Technology stack

| Area | Current implementation |
|---|---|
| Web | ASP.NET Core Razor Pages |
| Runtime | `net10.0` |
| Data access | `Microsoft.Data.SqlClient` |
| Password hashing | `BCrypt.Net-Next` |
| Session state | ASP.NET Core session |
| Architecture | Page model -> service -> repository |

## 6. Iteration 2 work plan

### Phase 1 — Document alignment ✓

- Rewrote outdated PDF content into Markdown
- Aligned terminology with `UseCasesIT2.md` and `UseCasesIT3.md`
- Added source traceability for major claims

### Phase 2 — Authentication and account flow ✓

- Confirmed login/register behavior with email/password
- Implemented sidebar account state tied to session
- Verified account info and edit navigation with redirects

### Phase 3 — Event workflow with ownership ✓

- Implemented event ownership support via `OwnerId` and `OwnerType`
- Added per-user event filtering in the current events UI and service layer
- Enforced authorization checks for event access

### Phase 4 — Group management ✓

- Implemented group creation and ownership model
- Built group member management (add/remove by email)
- Enforced group-specific visibility rules
- Created repository and service for group operations

### Phase 5 — Future scope definition ✓

- Captured invitations as planned items for IT3
- Defined IT3 boundary clearly in separate use-case document
- Kept the event ownership model ready for future group-based event creation in the UI

## 7. Risks and mitigations

| Risk | Impact | Mitigation |
|---|---|---|
| Documentation drift | Readers rely on outdated architecture assumptions | Keep the Markdown docs tied to source files and current runtime behavior |
| Scope creep | Iteration 2 becomes too broad | Keep group invitations and group-owned event pages deferred until the UI and tests are ready |
| Auth/session mismatch | UI and page models may disagree on signed-in state | Centralize session keys with `AuthSession` and update the layout from session values |
| Data access inconsistency | Different repositories may behave differently | Keep SQL repositories as the default implementation and document the in-memory repository as optional |

## 8. Definition of done

Iteration 2 is considered complete when:

- ✓ All account registration and login flows are functional
- ✓ Event ownership is enforced with authorization checks
- ✓ Group creation and member management are working
- ✓ Authorization prevents unauthorized access to resources
- ✓ Documentation (Analysis, Design, Plan) reflects the current implementation
- ✓ Use cases separate IT2 (implemented) from IT3 (planned)
- ✓ All Razor Pages and services follow SOLID layered design
- ✓ DTOs properly separate presentation from domain models
- ✓ Repositories provide clean persistence abstraction

Additional checks:

- registration and login work end to end
- account info and edit pages are accessible to signed-in users
- the sidebar reflects the current account state
- event pages continue to work with overlap validation
- the analysis, design, and plan documents are available in Markdown and match the current solution

## 9. Notes for future iterations

Future work should only introduce features that can be supported by the current domain model or by explicitly extending it.
In particular, group invitations and group-owned event creation will require new pages, repository methods, and page-level workflows before they can be exposed in the UI.
