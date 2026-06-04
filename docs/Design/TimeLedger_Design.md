# TimeLedger Design — Iteration 3
 
| Project Name: | TimeLedger      |
|---|-----------------|
| Date | 2026-05-22      |
| Author | Yevhenii Shvets |
| Version | 3.0             |
| Iteration | 3               |
 
 
## 1. Design goals
 
This document describes the software and database architecture of the `TimeLedger` solution for Iteration 3. It builds directly on the Iteration 2 design and documents the additions required to support inbox-based group invitations, group event management, multi-type events, and extended overlap validation. The design continues to prioritize maintainability, security, and SOLID principles.
 
## 2. Architecture overview
 
The application follows the same layered **n-tier architecture** established in Iteration 2:
 
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
 
No architectural principles changed in Iteration 3. The same SOLID commitments apply:
 
- **Separation of Concerns**: Each layer has distinct responsibilities
- **Dependency Injection**: Loose coupling through interfaces and DI container
- **Repository Pattern**: Data access abstraction for testability and maintainability
- **SOLID Principles**:
  - **S**ingle Responsibility: New services and repositories each handle one concern (`InvitationService` only handles invitation operations)
  - **O**pen/Closed: Existing services are extended via new methods without modifying core CRUD behaviour
  - **L**iskov Substitution: Repository implementations remain interchangeable
  - **I**nterface Segregation: `IInvitationRepository` added as a focused, separate interface rather than extending `IGroupRepository`
  - **D**ependency Inversion: Services continue to depend on repository interfaces
### 2.2 Layers
 
| Layer | Responsibility | Changes in Iteration 3 |
|---|---|---|
| **Presentation** (`TimeLedger.Web`) | Page rendering, request handling, form binding, redirects, session interaction | New inbox, group event, and invitation pages added |
| **Service** (`TimeLedger.Core.Services`) | Validation, business rules, entity-to-DTO mapping, password hashing, authorization | `InvitationService` added; `EventService` overlap logic extended; `GroupService` updated for invitation dispatch |
| **Data Access** (`TimeLedger.Infrastructure.Repositories`) | Persistence contracts and SQL Server implementation | `IInvitationRepository` and `InvitationRepository` added; `EventRepository` overlap query extended |
| **Core Models** (`TimeLedger.Core`) | Shared data structures, DTOs, domain models, interfaces | `GroupInvitation` entity added; `EventType` enum added to `Event`; new invitation DTOs |
 
### 2.3 Security Considerations
 
All security measures from Iteration 2 remain in place. Iteration 3 adds:
 
- **Invitation ownership**: Only a group owner can dispatch invitations; the `InvitationService` validates ownership before creating a record
- **Inbox scoping**: Users can only view and act on invitations addressed to them; the repository filters by `InviteeUserId`
- **Invitation state machine**: Only `Pending` invitations can be accepted or declined; acting on an already-resolved invitation is rejected at the service layer
## 3. Project composition
 
### 3.1 Web project (`TimeLedger.Web`)
 
No structural changes to the composition root. New Razor Pages registered automatically via convention.
 
**New registrations in `Program.cs`**:
- `InvitationService` bound to `IInvitationRepository`
### 3.2 Core project (`TimeLedger.Core`)
 
**New domain model**: `GroupInvitation` (see Section 4.5)
 
**Updated domain model**: `Event` gains an `EventType` field (see Section 4.2)
 
**New enum**: `EventType` — `OneTime`, `Recurrence`, `Deadline`
 
**New Repository Interface**: `IInvitationRepository`
 
**New Service**: `InvitationService`
 
**New DTOs**:
- `SendInvitationDto` — invitation dispatch input
- `InvitationInfoDto` — inbox projection for a pending invitation
- `ResolveInvitationDto` — accept/decline action input
**Updated DTOs**:
- `CreateEventDto` — now includes `EventType` field
- `UpdateEventDto` — now includes `EventType` field
- `EventResponseDto` — now exposes `EventType`
### 3.3 Infrastructure project (`TimeLedger.Infrastructure`)
 
**New SQL repository**: `InvitationRepository` — full CRUD and status-update operations for `GroupInvitations`
 
**Updated repository**: `EventRepository` — overlap query extended to support cross-owner comparisons for group event validation (see Section 7.2)
 
## 4. Domain model
 
### 4.1 User
 
No changes from Iteration 2.
 
**Relationships** (updated):
- Owns many `Event` records (via `Event.OwnerId` where `Event.OwnerType = User`)
- Owns many `Group` records (via `Group.OwnerId`)
- Member of many `Group` records (via `GroupMembers` junction table)
- Recipient of many `GroupInvitation` records (via `GroupInvitation.InviteeUserId`)
### 4.2 Event
 
`EventType` field added to support three scheduling modes.
 
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
| `EventType` | `EventType` (byte enum) | NOT NULL, Default = OneTime | **New.** Enum: `OneTime = 1`, `Recurrence = 2`, `Deadline = 3` |
 
**New enum — `EventType`**:
 
| Value | Meaning |
|---|---|
| `OneTime` | A single, non-repeating event |
| `Recurrence` | A repeating scheduled event |
| `Deadline` | A due-date marker with no fixed duration |
 
**Business Rules** (updated):
- `EndTime` must be greater than `StartTime`
- `EventType` is required; defaults to `OneTime` if not supplied
- `EventType` can be changed on any existing event via the edit form
- Overlap detection applies to all event types
- Service-layer authorization unchanged: callers must provide `OwnerType` and `OwnerId`
### 4.3 Group
 
No changes from Iteration 2.
 
**Relationships** (updated):
- Owned by one `User` (via `OwnerId`)
- Contains many `User` members (via `GroupMembers` junction table)
- Owns many `Event` records (via `Event.OwnerId` where `Event.OwnerType = Group`)
- Has many `GroupInvitation` records (via `GroupInvitation.GroupId`)
### 4.4 GroupMembers (Junction Table)
 
No changes from Iteration 2.
 
### 4.5 GroupInvitation (New)
 
Represents a pending, accepted, or declined invitation for a user to join a group.
 
| Field | Type | Constraints | Notes |
|---|---|---|---|
| `Id` | `int` | Primary Key | Auto-incremented |
| `GroupId` | `int` | NOT NULL, Foreign Key | Reference to `Group`; CASCADE delete on group deletion |
| `InviteeUserId` | `int` | NOT NULL, Foreign Key | Reference to the invited `User`; CASCADE delete on user deletion |
| `InvitedByUserId` | `int` | NOT NULL, Foreign Key | Reference to the `User` (group owner) who sent the invitation |
| `Status` | `InvitationStatus` (byte enum) | NOT NULL, Default = Pending | Enum: `Pending = 1`, `Accepted = 2`, `Declined = 3` |
| `CreatedAt` | `DateTime` | NOT NULL | UTC timestamp of invitation creation |
 
**New enum — `InvitationStatus`**:
 
| Value | Meaning |
|---|---|
| `Pending` | Invitation dispatched; awaiting recipient action |
| `Accepted` | Recipient accepted; user added to group |
| `Declined` | Recipient declined; no membership change |
 
**Business Rules**:
- Only the group owner (`InvitedByUserId` must match group `OwnerId`) can create an invitation
- Only one `Pending` invitation per `(GroupId, InviteeUserId)` pair is permitted at a time
- An invitation cannot be sent to an existing group member
- Only `Pending` invitations can be transitioned to `Accepted` or `Declined`
- Accepting an invitation triggers member addition via `GroupService`
- Deleting a group cascades to delete its invitation records
## 5. DTO design
 
### Account DTOs
 
No changes from Iteration 2.
 
### Event DTOs
 
- `CreateEventDto` — **updated**: now includes `EventType` field
- `UpdateEventDto` — **updated**: now includes `EventType` field
- `EventResponseDto` — **updated**: now exposes `EventType`
### Group DTOs
 
No changes from Iteration 2.
 
### Invitation DTOs (New)
 
- `SendInvitationDto` — invitation dispatch input; contains `GroupId` and `InviteeEmail`
- `InvitationInfoDto` — inbox read model; contains `InvitationId`, `GroupId`, `GroupName`, `InvitedByName`, `CreatedAt`, `Status`
- `ResolveInvitationDto` — accept/decline action input; contains `InvitationId` and `Accept` (bool)
## 6. Service design
 
### 6.1 `UserService`
 
No changes from Iteration 2.
 
### 6.2 `EventService`
 
All existing responsibilities from Iteration 2 are retained.
 
**Extended behaviour in Iteration 3**:
 
- `GetOverlappingGroupEvents(groupId, startTime, endTime)` — new method that checks for conflicts against both the group's own events and the personal events of all group members
- Group event overlap excludes events owned by other groups that a member belongs to; only the current group's events and members' personal (`OwnerType = User`) events are checked
- `EventType` is now mapped to/from `CreateEventDto`, `UpdateEventDto`, and `EventResponseDto`; it is persisted and returned in all event operations
### 6.3 `GroupService`
 
All existing responsibilities from Iteration 2 are retained.
 
**Extended behaviour in Iteration 3**:
 
- `InviteUserByEmail(groupId, inviteeEmail, ownerUserId)` — validates ownership, resolves email to user ID, checks for existing membership and pending invitation, then delegates to `IInvitationRepository` to create the invitation record
- Group event creation delegates to `EventService` with `OwnerType = Group` and the group's ID; no new method is required on `GroupService` itself
### 6.4 `InvitationService` (New)
 
Responsibilities:
 
- retrieve all pending invitations for a given user (inbox)
- accept an invitation: validate status is `Pending`, add user as group member, update status to `Accepted`
- decline an invitation: validate status is `Pending`, update status to `Declined`
- enforce that only the addressee (`InviteeUserId`) can act on an invitation
Important behaviour:
 
- `GetInbox(userId)` returns a list of `InvitationInfoDto` containing group name and inviter name resolved via repository joins
- `ResolveInvitation(invitationId, userId, accept)` validates ownership of the invitation before mutating state
- Membership addition on acceptance is coordinated with `GroupService.AddMember()` to reuse existing membership validation
- Attempting to resolve an already-resolved invitation returns an error without side effects
### 6.5 `AuthSession`
 
No changes from Iteration 2.
 
## 7. Repository design
 
### 7.1 Repository interfaces
 
- `IUserRepository` — unchanged
- `IEventRepository` — extended with `GetAllForUsers(userIds, ownerType)` to support cross-member overlap lookups
- `IGroupRepository` — unchanged
- `IInvitationRepository` — **new** (see below)
#### `IInvitationRepository` (New)
 
| Method | Purpose |
|---|---|
| `Create(invitation)` | Persist a new invitation record |
| `GetById(invitationId)` | Retrieve a single invitation by ID |
| `GetPendingByInvitee(inviteeUserId)` | Retrieve all pending invitations for a user's inbox |
| `GetPendingByGroupAndInvitee(groupId, inviteeUserId)` | Check for a pre-existing pending invitation |
| `UpdateStatus(invitationId, status)` | Transition invitation to `Accepted` or `Declined` |
 
### 7.2 SQL repository implementation
 
#### UserRepository
 
No changes from Iteration 2.
 
#### EventRepository
 
Extended with one new query method:
 
- `GetAllForUsers(userIds, ownerType)` — retrieves all events owned by any user in the provided ID list where `OwnerType = User`. Used by `EventService` when computing group event overlaps against member personal schedules.
Overlap detection query remains unchanged for single-owner checks. For group event overlap, `EventService` calls `GetAll(Group, groupId)` and `GetAllForUsers(memberIds, User)` separately, then merges the results before comparing time ranges.
 
#### GroupRepository
 
No changes from Iteration 2.
 
#### InvitationRepository (New)
 
Uses `Microsoft.Data.SqlClient` with parameterized queries against the `GroupInvitations` table.
 
- `Create()` — inserts a new row with `Status = Pending`; captures inserted ID via `OUTPUT INSERTED.Id`
- `GetById()` — single lookup including joined group name and inviter name for the response DTO
- `GetPendingByInvitee()` — filters by `InviteeUserId` and `Status = Pending`; joins `Groups` and `Users` for display fields
- `GetPendingByGroupAndInvitee()` — uniqueness check before dispatch
- `UpdateStatus()` — parameterized `UPDATE` setting `Status` by `Id`
### 7.3 Database constraints
 
All constraints from Iteration 2 apply. The following are added for Iteration 3:
 
- `GroupInvitations.GroupId` — foreign key to `Groups` with `ON DELETE CASCADE`
- `GroupInvitations.InviteeUserId` — foreign key to `Users` with `ON DELETE CASCADE`
- `GroupInvitations.InvitedByUserId` — foreign key to `Users` with `ON DELETE NO ACTION` (prevents multiple cascade paths)
- Unique index on `(GroupId, InviteeUserId)` filtered to `Status = Pending` — enforces at most one pending invitation per user per group at the database level
- `Events.EventType` — `NOT NULL` with a default constraint of `1` (`OneTime`) for backward compatibility with existing rows
## 8. Razor Pages design
 
### 8.1 Shared layout
 
No structural changes. The sidebar session display from Iteration 2 is unchanged.
 
### 8.2 Event pages
 
No new event pages. Existing create and edit pages updated to include the `EventType` selector field.
 
| Page | Change |
|---|---|
| `/Events/Create` | `EventType` radio or dropdown added to form |
| `/Events/Edit/{id}` | `EventType` selector pre-populated from existing value |
 
### 8.3 Account pages
 
No changes from Iteration 2.
 
### 8.4 Group pages (updated)
 
| Page | Purpose | Change |
|---|---|---|
| `/Groups` | List all groups the user owns or belongs to | Unchanged |
| `/Groups/Create` | Create a new group | Unchanged |
| `/Groups/{id}` | View group details and member list | **Updated**: shows group events; includes invite-by-email form |
| `/Groups/{id}/Edit` | Edit group details | Unchanged |
| `/Groups/{id}/Events/Create` | **New** — Create a new group event | New page |
 
### 8.5 Inbox pages (New)
 
| Page | Purpose |
|---|---|
| `/Inbox` | List all pending group invitations for the signed-in user |
| `/Inbox/Resolve` | POST handler to accept or decline a selected invitation |
 
## 9. Summary
 
Iteration 3 extends the Iteration 2 design with three focused additions:
 
**Invitation system** — A new `GroupInvitation` entity, `IInvitationRepository`, and `InvitationService` implement an inbox-based invitation flow that is cleanly isolated from existing group membership logic. The service coordinates with `GroupService` on acceptance to reuse membership validation.
 
**Group events** — No new architectural components were needed. Group event creation and viewing reuses the existing `EventService` and `EventRepository` with `OwnerType = Group`, surfaced through new Razor Pages on the group detail screen.
 
**Extended event model** — The `EventType` enum (`OneTime`, `Recurrence`, `Deadline`) is added as a single non-breaking field on `Event`, with a database default ensuring backward compatibility. All existing event DTOs and pages are updated to carry this field.
 
**Broadened overlap validation** — `EventService` overlap detection for group events now spans both the group's own schedule and the personal events of all group members, implemented by composing two existing repository calls rather than a complex joined query.
