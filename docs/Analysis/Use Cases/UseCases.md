# TimeLedger — Use Case Document
## Iteration 1

| Project Name: | TimeLedger      |
|---------------|-----------------|
| Date:         | 2026-05-22      |
| Author:       | Yevhenii Shvets |
| Version:      | 1.0             |
| Iteration:    | 1               |

## Use Case Summary

| Use Case | Description                             | Related FRs       |
|----------|-----------------------------------------|-------------------|
| UC-1     | Create a new event                      | FR-1, FR-2, FR-7  |
| UC-2     | View events in day-based calendar view  | FR-4              |
| UC-3     | Edit an existing event                  | FR-5, FR-2, FR-3, FR-7 |
| UC-4     | Delete an event                         | FR-5              |
| UC-5     | Register an account                     | FR-9                 |
| UC-6     | Log in                                  | FR-10                |
| UC-7     | View signed-in account in sidebar       | FR-10                |
| UC-8     | Log out                                 | FR-16                |
| UC-9     | View and edit account information       | FR-17                |
| UC-10    | Create a group                          | FR-13                |
| UC-11    | Manage group members                    | FR-14                |
| UC-12    | View groups                             | FR-15                |
| UC-13    | Accept a group invitation               | FR-19                            |
| UC-14    | Decline a group invitation              | FR-19                            |
| UC-15    | Create a group event                    | Implemented feature (group events) |
| UC-16    | View group events                       | Implemented feature (group events) |

---

## Actors

| Actor | Description |
|-------|-------------|
| Guest        | An unauthenticated visitor who can register or log in |
| User         | An authenticated user |
| Group Owner  | An authenticated user who who owns a group |
| Group Member | An authenticated user who has been added to a group |

---

## Use Cases

### UC-1 — Create a New Event

**Related requirements:** FR-1, FR-2, FR-3, FR-7, FR-20

**Description:** A user creates a new scheduled event by providing a title, optional description, start time, end time, and event type.

**Preconditions:**
- The user is logged in and has navigated to the event creation form.
  **Main Flow:**
1. The user opens the event creation form.
2. The user enters a title (required), an optional description, a start time, and an end time.
3. The user selects an event type and fills in any additional required fields based on the type.
4. The user submits the form.
5. The system validates required fields and the time range.
6. The system checks for overlaps with the user's existing events.
7. The system saves the event and redirects the user to the calendar view.
   **Alternative Flows:**
- **AF-1a — Invalid time range:** The system displays an inline error and does not save the event.
- **AF-1b — Missing required fields:** The system displays inline errors for missing or invalid fields and does not save the event.
- **AF-1c — Overlap detected:** The system warns the user that the new event overlaps with an existing event. The user may proceed or adjust the times.
- **AF-1d — No event type selected:** The system defaults to `OneTime`.
  **Postconditions:**
- A new event is persisted and visible in the calendar view with its type shown on the event card.
---

### UC-2 — View Events in Day-Based Calendar View

**Related requirements:** FR-4, FR-8

**Description:** A user views their scheduled events for a selected day. Each event card shows its title, time range, and calculated duration. Only events owned by the authenticated user are shown.

**Preconditions:**
- The user is logged in.
  **Main Flow:**
1. The user navigates to the calendar view.
2. The user selects or is presented with a specific day.
3. The system retrieves all events owned by the current user for that day.
4. Each event card is displayed with its title, time range, event type, and duration calculated from start to end time.
   **Alternative Flows:**
- **AF-2a — No events on selected day:** The system displays an empty-state message indicating no events are scheduled.
  **Postconditions:**
- Events for the selected day are rendered as event cards with duration visible.
---

### UC-3 — Edit an Existing Event

**Related requirements:** FR-2, FR-3, FR-5, FR-7, FR-21

**Description:** A user modifies the details of an existing event, including changing its event type.

**Preconditions:**
- The event exists and is owned by the current user.
  **Main Flow:**
1. The user selects an event and opens the edit form.
2. The user modifies one or more fields (title, description, start time, end time).
3. The user submits the form.
4. The system validates required fields and the time range for the selected type.
5. The system checks for overlaps with other existing events (skipped for `Deadline` type).
6. The system saves the changes and returns the user to the calendar view.
   **Alternative Flows:**
- **AF-3a — Invalid time range:** The system displays an inline error and does not save the event.
- **AF-3b — Missing required fields for selected type:** The system displays inline validation errors and does not save the event.
- **AF-3c — Overlap detected:** The system warns the user that the updated event overlaps with an existing event. The user may proceed or adjust the times.
- **AF-3d — Changing event type:** Uses is required to fill in any additional fields required by the new type. Validation is performed based on the new type's rules.
  **Postconditions:**
- The event reflects the updated details and event type.
---

### UC-4 — Delete an Event

**Related requirements:** FR-5

**Description:** A user permanently removes an event from the schedule.

**Preconditions:**
- The event exists and is owned by the current user.
  **Main Flow:**
1. The user selects an event.
2. The system shows a deletion confirmation page with the event details.
3. The user confirms the deletion.
4. The system removes the event from the data store.
5. The system returns the user to the calendar view, where the deleted event is no longer listed.
   **Alternative Flows:**
- **AF-4a — User cancels deletion:** The user opts not to confirm; the system retains the event and returns to the previous view.
  **Postconditions:**
- The event no longer appears in any view.
---

### UC-5 — Register an Account

**Related requirements:** FR-9

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
- **AF-5a — Email already registered:** The system displays an inline error indicating the email is taken.
- **AF-5b — Validation failure:** Missing or malformed fields produce inline error messages.
  **Postconditions:**
- A new user account is persisted. The user can now log in.
---

### UC-6 — Log In

**Related requirements:** FR-10

**Description:** A registered user authenticates using their email and password and receives a session.

**Preconditions:**
- A user account with the provided email exists.
  **Main Flow:**
1. The user opens the login form.
2. The user enters their email and password.
3. The user submits the form.
4. The system verifies the credentials against the stored password hash.
5. The system creates a session and redirects the user to the calendar view.
   **Alternative Flows:**
- **AF-6a — Invalid credentials:** The system displays a generic authentication error without revealing whether the email or password was wrong.
  **Postconditions:**
- The user has an active session and is considered authenticated.
---

### UC-7 — View Signed-In Account in Sidebar

**Related requirements:** FR-10

**Description:** While authenticated, the user sees their account information rendered in the application sidebar.

**Preconditions:**
- The user is logged in with an active session.
  **Main Flow:**
1. The user navigates to any page within the application.
2. The system reads the active session and retrieves the corresponding user profile.
3. The sidebar displays the user's name or email to confirm the active session.
   **Alternative Flows:**
- **AF-7a — No active session:** The sidebar does not display account information; the user sees a login prompt.
  **Postconditions:**
- The authenticated identity is visually confirmed on every page.
---

### UC-8 — Log Out

**Related requirements:** FR-16

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

### UC-9 — View and Edit Account Information

**Related requirements:** FR-17

**Description:** An authenticated user views their profile details and can update them.

**Preconditions:**
- The user is logged in.
  **Main Flow:**
1. The user navigates to the account settings page.
2. The system displays the current account details (e.g., email, display name).
3. The user modifies one or more fields and submits the form.
4. The system validates the changes and saves them.
   **Alternative Flows:**
- **AF-9a — Validation failure:** Inline errors are shown for invalid fields; no changes are saved.
- **AF-9b — Delete account:** The user triggers account deletion. The system removes the account and its associated data and redirects to the home page.
  **Postconditions:**
- The updated account details are persisted and reflected immediately.
---

### UC-10 — Create a Group

**Related requirements:** FR-13

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

### UC-11 — Manage Group Members

**Related requirements:** FR-14, FR-18

**Description:** A group owner manages group membership by directly adding or removing members, or by sending invitations to registered users by email.

**Preconditions:**
- The current user is the owner of the group.
  **Main Flow — Add member directly:**
1. The group owner opens the group management page.
2. The owner enters the email address of the user to add.
3. The system looks up the user account and adds them as a member.
4. The updated member list is displayed.
   **Main Flow — Remove member:**
1. The group owner selects a member from the member list.
2. The owner triggers the remove action.
3. The system removes the user from the group.
4. The updated member list is displayed.
   **Main Flow — Invite member:**
1. The group owner enters the email address of the user to invite.
2. The owner submits the invitation form.
3. The system validates the email, checks for existing membership or a pending invitation, and creates an invitation record in the recipient's inbox.
4. The system confirms the invitation was sent.
   **Alternative Flows:**
- **AF-11a — Email not found:** The system displays an error stating the user does not exist.
- **AF-11b — User is already a member:** The system displays an appropriate notice and takes no further action.
- **AF-11c — Pending invitation already exists:** The system informs the owner that an invitation is already pending for this user.
- **AF-11d — Non-owner attempts action:** The system denies the request.
  **Postconditions:**
- Group membership or pending invitations reflect the owner's changes.
---

### UC-12 — View Groups

**Related requirements:** FR-15

**Description:** An authenticated user views the groups they own or belong to.

**Preconditions:**
- The user is logged in.
  **Main Flow:**
1. The user navigates to the groups section.
2. The system retrieves all groups where the user is either owner or member.
3. The groups are listed with their names and the user's role (owner or member).
   **Alternative Flows:**
- **AF-12a — No groups:** An empty-state message is shown.
  **Postconditions:**
- The user can see all groups they have access to.
---

### UC-13 — Accept a Group Invitation

**Related requirements:** FR-19

**Description:** A user views a pending group invitation in their inbox and accepts it, becoming a group member.

**Preconditions:**
- The user is logged in.
- At least one pending group invitation exists in the user's inbox.
  **Main Flow:**
1. The user navigates to their inbox.
2. The system displays all pending invitations, each showing the group name and inviting owner.
3. The user selects an invitation and chooses to accept.
4. The system adds the user as a member of the group.
5. The system marks the invitation as accepted and removes it from the pending list.
6. The group now appears in the user's group list.
   **Postconditions:**
- The user is a confirmed member of the group.
- The invitation is resolved and no longer shown as pending.
---

### UC-14 — Decline a Group Invitation

**Related requirements:** FR-19

**Description:** A user views a pending group invitation in their inbox and declines it.

**Preconditions:**
- The user is logged in.
- At least one pending group invitation exists in the user's inbox.
  **Main Flow:**
1. The user navigates to their inbox.
2. The system displays all pending invitations.
3. The user selects an invitation and chooses to decline.
4. The system marks the invitation as declined and removes it from the pending list.
   **Postconditions:**
- The user is not added to the group.
- The invitation is resolved and no longer shown as pending.
---

### UC-15 — Create a Group Event

**Related requirements:** FR-22

**Description:** A group owner creates a new event associated with a specific group, visible to all group members. The system validates the time range against both the group's existing schedule and the personal events of all group members.

**Preconditions:**
- The user is logged in and is the owner of the group.
- The user has navigated to the group detail page.
  **Main Flow:**
1. The user opens the group event creation form from the group page.
2. The user enters the event title, optional description, start time, end time, and event type.
3. The user submits the form.
4. The system validates the time range and required fields.
5. The system retrieves all existing events for the group and all personal events of every group member, then checks for overlaps against both sets.
6. The system saves the event and associates it with the group.
7. The user is redirected to the group event list.
   **Alternative Flows:**
- **AF-15a — Invalid time range:** The system displays an inline error and does not save the event.
- **AF-15b — Overlap with group schedule or member events:** The system warns the user of the specific conflicts. The user may acknowledge and proceed, or go back and adjust the times. Events from other groups that members belong to are excluded from this check.
  **Postconditions:**
- The group event is persisted and visible to all group members on the group page.
---

### UC-16 — View Group Events

**Description:** A group member or owner views all events associated with a group.

**Preconditions:**
- The user is logged in and is a member or owner of the group.
  **Main Flow:**
1. The user navigates to the group detail page.
2. The system retrieves all events owned by the group.
3. The events are displayed showing title, time range, duration, and event type.
   **Alternative Flows:**
- **AF-16a — No group events:** The system displays an empty-state message.
  **Postconditions:**
- The user has a clear view of the group's scheduled events.


