# TimeLedger Project Plan — Iteration 2

## 1. Project overview

`TimeLedger` is a Razor Pages scheduling application that supports event management and user accounts.
The original plan emphasized a broader advanced-planner vision, but the current implementation focuses on the iteration-2 account/authentication workflow plus the existing event engine.

## 2. Current status

### Implemented

- event create, edit, delete, and list pages
- overlap detection in the event service and repository
- user registration and login
- session-based account state
- account info and account edit pages
- sidebar that shows the signed-in user name and email

### Planned or still missing

- event ownership per account
- per-user event filtering and access checks
- groups, invitations, and membership flows

## 3. Iteration 2 objectives

### Main objective

Deliver a usable authenticated experience for the scheduling app by ensuring that users can create accounts, log in, view and update profile data, and continue using the event pages in a session-aware UI.

### Supporting objective

Keep the documentation synchronized with the live Razor Pages implementation so the analysis, design, and plan documents describe what actually exists instead of the older PDF-era stack.

## 4. Scope

### In scope for the current solution

- account registration and login
- account information and edit screens
- session state using `AuthSession`
- shared sidebar account display
- event CRUD and overlap handling
- SQL Server persistence through repositories

### Deferred to a later iteration

- event ownership assignment
- authorization checks around event ownership
- group creation and invitations
- invitation acceptance/decline flows

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

### Phase 1 — Document alignment

- rewrite the outdated PDF content into Markdown
- align terminology with `UseCasesIT2.md`
- add source traceability for major claims

### Phase 2 — Authentication and account flow

- confirm login/register behavior
- keep the sidebar account state in sync with session values
- verify account info and edit navigation

### Phase 3 — Event workflow review

- keep CRUD and overlap behavior consistent
- identify missing event ownership rules for future work

### Phase 4 — Future scope definition

- capture groups and invitations as planned items, not implemented features
- define the next iteration boundary clearly

## 7. Risks and mitigations

| Risk | Impact | Mitigation |
|---|---|---|
| Documentation drift | Readers rely on outdated architecture assumptions | Keep the Markdown docs tied to source files and current runtime behavior |
| Scope creep | Iteration 2 becomes too broad | Mark group features and ownership rules as deferred until the data model supports them |
| Auth/session mismatch | UI and page models may disagree on signed-in state | Centralize session keys with `AuthSession` and update the layout from session values |
| Data access inconsistency | Different repositories may behave differently | Keep SQL repositories as the default implementation and document the in-memory repository as optional |

## 8. Definition of done

Iteration 2 is considered complete when:

- registration and login work end to end
- account info and edit pages are accessible to signed-in users
- the sidebar reflects the current account state
- event pages continue to work with overlap validation
- the analysis, design, and plan documents are available in Markdown and match the current solution

## 9. Notes for future iterations

Future work should only introduce features that can be supported by the current domain model or by explicitly extending it.
In particular, event ownership and groups will require new fields, repository methods, and page-level authorization rules before the iteration-2 use cases can be fully fulfilled.
