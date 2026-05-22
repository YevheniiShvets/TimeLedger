# TimeLedger — Use Case Document
## Iteration 2

| Project Name: | TimeLedger      |
|---------------|-----------------|
| Date:         | 2026-05-22      |
| Author:       | Yevhenii Shvets |
| Version:      | 2.0             |
| Iteration:    | 2               |

---

## Overview

Iteration 2 extends TimeLedger with user account management, session-based authentication, event ownership, and group collaboration.
Users can now register, log in, and manage their accounts, while events are scoped to their owner.
Group creation, membership management, and access control are also introduced.

---

## Actors

| Actor        | Description |
|--------------|-------------|
| Guest        | An unauthenticated visitor who can register or log in |
| User         | An authenticated user who owns events and can belong to groups |
| Group Owner  | An authenticated user who created a group and manages its membership |
| Group Member | An authenticated user who has been added to a group by its owner |

---

## Use Cases

### UC-8 — Register an Account

**Related requirement:** FR-9

**Description:** A guest creates a new user account by providing an email address and a password.

**Preconditions:**
- The guest has navigated to the registration page.
- No account with the provided email already exists.

**Main Flow:**
1. The guest opens the registration form.
2. The guest enters a valid email address and a password.
3. The guest submits the form.
4. The system validates the input and checks that the email is not already in use.
5. The system stores the account with a hashed password.
6. The system redirects the user to the login page or directly starts a session.

**Alternative Flows:**
- **AF-8a — Email already registered:** The system displays an inline error indicating the email is taken.
- **AF-8b — Validation failure:** Missing or malformed fields produce inline error messages.

**Postconditions:**
- A new user account is persisted. The user can now log in.

---

### UC-9 — Log In

**Related requirement:** FR-10

**Description:** A registered user authenticates using their email and password and receives a session.

**Preconditions:**
- A user account with the provided email exists.

**Main Flow:**
1. The user opens the login form.
2. The user enters their email and password.
3. The user submits the form.
4. The system verifies the credentials against the stored password hash.
5. The system creates a session for the user and redirects them to the dashboard or calendar view.

**Alternative Flows:**
- **AF-9a — Invalid credentials:** The system displays a generic authentication error without revealing whether the email or password was wrong.

**Postconditions:**
- The user has an active session and is considered authenticated.

---

### UC-10 — View Signed-In Account in Sidebar

**Related requirement:** FR-10 (session auth display)

**Description:** While authenticated, the user sees their account information rendered in the application sidebar.

**Preconditions:**
- The user is logged in with an active session.

**Main Flow:**
1. The user navigates to any page within the application.
2. The system reads the active session and retrieves the corresponding user profile.
3. The sidebar displays the user's name or email to confirm the active session.

**Alternative Flows:**
- **AF-10a — No active session:** The sidebar does not display account information; the user sees a login prompt.

**Postconditions:**
- The authenticated identity is visually confirmed on every page.

---

### UC-11 — Log Out

**Related requirement:** FR-16

**Description:** An authenticated user ends their session.

**Preconditions:**
- The user has an active session.

**Main Flow:**
1. The user selects the log-out option.
2. The system clears the session data.
3. The system redirects the user to the login page or home page.

**Postconditions:**
- The session is destroyed. The user is treated as a guest until they log in again.

---

### UC-12 — View and Edit Account Information

**Related requirement:** FR-17

**Description:** An authenticated user views their profile details and can update them.

**Preconditions:**
- The user is logged in.

**Main Flow:**
1. The user navigates to the profile/account settings page.
2. The system displays the current account details (e.g., email, display name).
3. The user modifies one or more fields and submits the form.
4. The system validates the changes and saves them.

**Alternative Flows:**
- **AF-12a — Validation failure:** Inline errors are shown for invalid fields; no changes are saved.
- **AF-12b — Delete account:** The user triggers account deletion. The system removes the account and its associated data and redirects to the home page.

**Postconditions:**
- The updated account details are persisted and reflected immediately.

---

### UC-13 — View Own Events Only

**Related requirement:** FR-11, FR-12

**Description:** An authenticated user sees only the events they own; events belonging to other users are not accessible.

**Preconditions:**
- The user is logged in.
- Events from multiple users exist in the system.

**Main Flow:**
1. The user navigates to the calendar view.
2. The system filters events by the session's user ID.
3. Only events owned by the current user are displayed.

**Alternative Flows:**
- **AF-13a — Direct URL access to another user's event:** The system returns a 403 Forbidden or redirects to an error page.

**Postconditions:**
- No cross-user event data is exposed.

---

### UC-14 — Create a Group

**Related requirement:** FR-13

**Description:** An authenticated user creates a new group and automatically becomes its owner.

**Preconditions:**
- The user is logged in.

**Main Flow:**
1. The user navigates to the group creation form.
2. The user enters a group name and optional description.
3. The user submits the form.
4. The system creates the group and assigns the current user as its owner.
5. The system redirects the user to the group detail page.

**Postconditions:**
- The group is persisted. The creator is recorded as the owner.

---

### UC-15 — Add and Remove Group Members

**Related requirement:** FR-14

**Description:** A group owner manages membership by adding or removing users identified by email.

**Preconditions:**
- The current user is the owner of the group.
- The target user account exists in the system.

**Main Flow — Add member:**
1. The group owner opens the group management page.
2. The owner enters the email address of the user to add.
3. The system looks up the user account.
4. The system adds the user as a member of the group.
5. The updated member list is displayed.

**Main Flow — Remove member:**
1. The group owner selects a member from the member list.
2. The owner triggers the remove action.
3. The system removes the user from the group.
4. The updated member list is displayed.

**Alternative Flows:**
- **AF-15a — Email not found:** The system displays an error stating the user does not exist.
- **AF-15b — User already a member:** The system displays an appropriate notice.
- **AF-15c — Non-owner attempts action:** The system denies the request.

**Postconditions:**
- Group membership reflects the owner's changes.

---

### UC-16 — View Groups

**Related requirement:** FR-15

**Description:** An authenticated user views the groups they own or belong to.

**Preconditions:**
- The user is logged in.

**Main Flow:**
1. The user navigates to the groups section.
2. The system retrieves all groups where the user is either owner or member.
3. The groups are listed with their names and the user's role (owner or member).

**Alternative Flows:**
- **AF-16a — No groups:** An empty-state message is shown.

**Postconditions:**
- The user can see all groups they have access to.

---



## Use Case Summary

| Use Case | Description                                          | Related FRs          |
|----------|------------------------------------------------------|----------------------|
| UC-8     | Register an account                                  | FR-9                 |
| UC-9     | Log in                                               | FR-10                |
| UC-10    | View signed-in account in sidebar                    | FR-10                |
| UC-11    | Log out                                              | FR-16                |
| UC-12    | View and edit account information                    | FR-17                |
| UC-13    | View own events only                                 | FR-11, FR-12         |
| UC-14    | Create a group                                       | FR-13                |
| UC-15    | Add and remove group members                         | FR-14                |
| UC-16    | View groups                                          | FR-15                |
