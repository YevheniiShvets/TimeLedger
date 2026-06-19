# TimeLedger Design — Iteration 3
 
| Project Name: | TimeLedger      |
|---|-----------------|
| Date | 2026-05-22      |
| Author | Yevhenii Shvets |
| Version | 3.0             |
| Iteration | 3               |
 
 
## 1. Time Ledger
TimeLedger is a personal and group scheduling application. Users manage one-time events, recurring events, and deadlines; they can also form groups and share a calendar with other members. The system ships two clients against a shared backend: a server-hosted web application and a desktop application built with Avalonia

The desktop client is offline-first: it keeps a local SQLite cache of events and can create, edit, and delete events without a network connection. When connectivity is available, a synchronization service reconciles the local cache with the hosted SQL Server database. This offline/online duality is the most significant architectural driver in the system and shapes the repository and service layers described below
 
## 2. Architecture overview

The solution follows a layered (n-tier) architecture. Each layer depends only on the layer beneath it
 
```
                              Presentation Layer
                 Web (Razor Pages)        Desktop (Avalonia / MVVM)
                             \                    /
                              \                  /
                              Business Logic Layer
                         Business.Core      Business.Collaboration
                                        |
                                        |
                                 Data Access Layer
                              Data           Data.Local
                                |                 |
                                |                 |         
                            MSSQL Server        SQLite

```

 
Two independent presentation projects sit on top of the same business logic:
* **Web (Razor Pages)** — server-rendered pages for account management, events,
groups, and the invitation inbox. Pages bind directly to DTOs and call into the service
layer from their page models.
* **Desktop (Avalonia, MVVM)** — a native desktop client. Views (.axaml) are paired with
ViewModels that expose observable properties and relay commands. Navigation
between screens is handled by swapping a bound CurrentPage object rather than by a
router.

### Business Logic Layer
Services contain all validation and business rules. They are the only layer that both presentation
projects depend on, which is what allows the two clients to share behavior without duplicating it.
### Data Access Layer
Both the hosted SQL Server store and the local SQLite cache implement the same core
repository contract, which lets the business logic layer remain unaware of which store it is
actually talking to.
* **IEventRepository** — the shared contract implemented by both
EventRepository (SQL Server) and LocalEventRepository (SQLite).
* **IRemoteEventRepository** — extends IEventRepository with
a multi-owner overlap query needed for group event validation. Only the remote
repository implements it, since the offline cache has no use for checking overlaps across
other users' schedules.
* **SyncedEventRepository** — the implementation actually injected into the business logic
layer on desktop. It is not a third data store; it composes the remote and local
repositories and decides, per call, which one to use based on connectivity and the active
sync mode (automatic or manual).
 
## Object-Oriented Principles

### Encapsulation
Repository implementations hide their storage mechanism completely behind the
IEventRepository contract. EventRepository opens ADO.NET SqlConnections and builds
parameterized T-SQL; LocalEventRepository opens SqliteConnections and builds
parameterized SQLite statements. Neither detail is visible to EventService, which only ever calls methods on the interface.

### Abstraction
The business logic layer is written entirely against interfaces: IEventRepository,
IConnectivityService, ISettingsStore, ISyncStateStore. This is what allows the same
EventService to run unmodified against either the web project's SQL-Server-only setup or the desktop project's dual local and remote setup — the service has no knowledge of which concrete repository it was given.
 
## SOLID Principles

### Single Responsibility
Recurrence is split across two services rather than folded into one. RecurrenceService's only job is calculating the next occurrence date given a rule; EventOccurrenceService's only job is fetching recurring events and expanding each one into dated occurrences within a range. Neither class knows how to persist anything, and EventService — which does persist events — never performs recurrence math itself. 

SyncService and SyncedEventRepository look similar at first glance but serve different responsibilities: SyncedEventRepository decides, for a single call, whether to use the remote or local store right now. SyncService is the only component responsible for reconciling the two stores against each other over time.

### Open/Closed
Adding a new event type required no changes to EventRepository's persistence methods;
EventType is simply a stored field that EventService branches on for validation rules — a
Deadline requires a due date instead of a start and end time. Adding a new
RecurrenceFrequency value requires only a new case in RecurrenceService's frequency switch;
every caller of GenerateOccurrences is unaffected.

### Liskov Substitution
Both EventRepository and LocalEventRepository can be substituted for each other anywhere an
IEventRepository is expected, and SyncedEventRepository — itself an IEventRepository — can
be substituted for either one without the business logic layer behaving any differently. None of
the three implementations narrows the contract or throws for a method the interface promises to
support; the interface was deliberately split (see Interface Segregation, below) precisely so that
no implementation would be forced into that position.

### Interface Segregation
IEventRepository originally exposed GetOverlappingOwnerIds — a query that only makes
sense against the hosted database, since it checks overlaps across other users' schedules and the offline cache has no visibility into other users at all. Keeping it on the shared interface would have forced LocalEventRepository to implement a method it could never honestly support.
The method was extracted into IRemoteEventRepository, which IEventRepository does not
include. EventRepository implements both interfaces; LocalEventRepository implements only
the core one. Code that genuinely needs cross-owner overlap checking, such as group event
creation, depends on IRemoteEventRepository directly, making that dependency explicit rather
than hidden inside a fat shared interface.

### Dependency Inversion
Every service in the business logic layer receives its repositories and supporting collaborators through constructor injection, resolved against interfaces by the dependency injection container at startup. EventService depends on IEventRepository, never on EventRepository or LocalEventRepository directly; SyncService depends on IEventRepository for both its remote and local collaborators, leaving the composition root — App.axaml.cs on desktop, Program.cs on web — as the only place that ever references a concrete repository type.


### Class Diagram

![event](event-class-diagram-TimeLedger_Event_Layered_Class_Diagram.png)


