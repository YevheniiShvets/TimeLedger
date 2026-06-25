# TimeLedger

`TimeLedger` is a Razor Pages web and AvaloniaUI desktop applications for scheduling personal events and managing account access.
The solution currently focuses on:

- event creation, editing, deletion, and day-based viewing
- user registration, login, profile viewing, and profile editing
- session-based authentication using ASP.NET Core session state
- SQL Server persistence through repository classes
- desktop version with offline features and synchronisation

## Current implementation snapshot

### Solution structure

- `TimeLedger.Web` — Razor Pages web UI
- `TimeLedger.Desktop` — Avalonia UI for offline use
- `TimeLedger.Business.Core` — DTOs, domain models, interfaces, and services for offline/online logic
- `TimeLedger.Business.Collaboration` — Online services
- `TimeLedger.Data` — Repository implementations for SQL Server
- `TimeLedger.Data.Local` — Repository implementations for local SQLite
- `TimeLedger.Tests` — NUnit test project scaffold

### Technology stack

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
