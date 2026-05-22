# TimeLedger — Use Case Document
## Iteration 3

| Project Name: | TimeLedger      |
|---------------|-----------------|
| Date:         | 2026-05-22      |
| Author:       | Yevhenii Shvets |
| Version:      | 3.0             |
| Iteration:    | 3               |

---

## Overview

Iteration 3 evolves TimeLedger into a richer group collaboration platform.
The key additions are inbox-based group invitations, group event creation and viewing, extended event type support (`OneTime`, `Recurrence`, `Deadline`), and a broadened overlap validation model that accounts for both group schedules and individual member calendars.

---

## Actors

| Actor        | Description |
|--------------|-------------|
| User         | Any authenticated user who may receive invitations and manage their events |
| Group Owner  | An authenticated user who owns a group and can invite other users |
| Group Member | An authenticated user who belongs to a group and participates in group events |

---

## Use Cases

### UC-18 — Invite a User to a Group


**Description:** A group owner sends an invitation to a registered user by email address. The invitation enters the recipient's inbox rather than immediately adding them to the group.

**Preconditions:**
- The current user is the owner of the target group.
- The invitee has a registered account with the provided email address.
- The invitee is not already a member of or invited to the group.

**Main Flow:**
1. The group owner opens the group management page.
2. The owner enters the email address of the user they want to invite.
3. The owner submits the invitation form.
4. The system validates the email address, checks for existing membership or pending invitations, and creates a new invitation record if valid.
5. The system creates an invitation record and places it in the recipient's inbox.
6. The system confirms the invitation was sent.

**Alternative Flows:**
- **AF-4a — Email not found:** The system displays an error indicating no account matches the email.
- **AF-4b — User is already a member:** The system displays an appropriate notice and does not create a duplicate invitation.
- **AF-4c — Pending invitation already exists:** The system informs the owner that an invitation is already pending for this user.


**Postconditions:**
- A pending invitation record is stored and visible in the invitee's inbox.

---

### UC-19 — Accept a Group Invitation

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

### UC-20 — Decline a Group Invitation


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

### UC-21 — Create a Group Event

**Description:** A group owner or member creates a new event associated with a specific group, visible to all group members.

**Preconditions:**
- The user is logged in and is owner of the group.
- The user has navigated to the group detail page.

**Main Flow:**
1. The user opens the group event creation form from the group page.
2. The user enters the event title, optional description, start time, end time, and event type.
3. The user submits the form.
4. The system validates the time range and checks for overlaps (see UC-23).
5. The system saves the event and associates it with the group.
6. The user is redirected to the group event list.

**Alternative Flows:**
- **AF-4a — Invalid time range:** The system displays an inline error and does not save the event.
- **AF-4b — Overlap detected:** The system warns the user of conflicts with the group schedule or member personal events before saving.

**Postconditions:**
- The group event is persisted and visible to all group members on the group page.

---

### UC-22 — View Group Events

**Description:** A group member or owner views all events associated with a group.

**Preconditions:**
- The user is logged in and is a member or owner of the group.

**Main Flow:**
1. The user navigates to the group detail page.
2. The system retrieves all events owned by the group.
3. The events are displayed in a list or calendar view showing title, time range, duration, and event type.

**Alternative Flows:**
- **AF-3a — No group events:** The system displays an empty-state message.

**Postconditions:**
- The user has a clear view of the group's scheduled events.

---

### UC-23 — Validate Group Event Overlap


**Description:** When creating or editing a group event, the system checks for time conflicts against both the group's existing events and the personal events of all group members, then warns the user.

**Preconditions:**
- The user is submitting a group event creation or edit form.
- The group has at least one existing event or at least one member with personal events.

**Main Flow:**
1. The user submits the group event form.
2. The system retrieves all existing events for the group.
3. The system retrieves all personal events for every member of the group.
4. The system compares the new event's time range against both sets of events.
5. If any overlap is detected, the system displays a warning listing the conflicts.
6. The user may acknowledge the warning and proceed, or go back and adjust the times.

**Alternative Flows:**
- **AF-5a — No overlaps found:** The system saves the event immediately without a warning.

**Postconditions:**
- The user is informed of any scheduling conflicts before the group event is saved.

---

### UC-24 — Set Event Type on Creation


**Description:** When creating a personal or group event, a user selects one of the three supported event types: `OneTime`, `Recurrence`, or `Deadline`.

**Preconditions:**
- The user is on the event creation form.

**Main Flow:**
1. The user fills in the event details.
2. The user selects an event type from the available options: `OneTime`, `Recurrence`, or `Deadline`.
3. The user submits the form.
4. The system validates the selected type and required fields based on the type.
5. The system stores the event with the selected type.

**Alternative Flows:**
- **AF-4a — No type selected:** The system defaults to `OneTime` or displays a validation error if a selection is required.
- **AF-4b — Required fields missing for selected type:** The system displays inline errors

**Postconditions:**
- The event is stored with the correct type and the type is visible on the event card.

---

### UC-25 — Change Event Type for an Existing Event


**Description:** A user updates the event type of an existing personal or group event.

**Preconditions:**
- The event exists and the user has permission to edit it.

**Main Flow:**
1. The user opens the event edit form.
2. The user changes the event type to a different value (`OneTime`, `Recurrence`, or `Deadline`).
3. The user submits the form.
4. The system validates the new type and any required fields based on the type.
5. The system saves the event with the updated type.

**Alternative Flows:**
- **AF-4a — No change made:** The system saves the form normally without altering the type field.
- **AF-4b — Invalid type change:** Required fields for the new type are missing or invalid, resulting in inline validation errors.

**Postconditions:**
- The event reflects the newly selected type.

---

## Use Case Summary

| Use Case | Description                                              | Related FRs / Feature            |
|----------|----------------------------------------------------------|----------------------------------|
| UC-18    | Invite a user to a group                                 | FR-18                            |
| UC-19    | Accept a group invitation                                | FR-19                            |
| UC-20    | Decline a group invitation                               | FR-19                            |
| UC-21    | Create a group event                                     | Implemented feature (group events) |
| UC-22    | View group events                                        | Implemented feature (group events) |
| UC-23    | Validate group event overlap                             | FR-22                            |
| UC-24    | Set event type on creation                               | FR-20                            |
| UC-25    | Change event type for an existing event                  | FR-21                            |
