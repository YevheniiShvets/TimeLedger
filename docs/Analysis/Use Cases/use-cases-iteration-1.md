# TimeLedger — Use Case Document
## Iteration 1

| Project Name: | TimeLedger      |
|---------------|-----------------|
| Date:         | 2026-05-22      |
| Author:       | Yevhenii Shvets |
| Version:      | 2.0             |
| Iteration:    | 1               |

---

## Overview

Iteration 1 establishes the core event management capability of TimeLedger.
The focus is on allowing a single user to create, view, edit, and delete personal events, with basic time validation and overlap detection.

---

## Actors

| Actor | Description |
|-------|-------------|
| User  | Any visitor of the application who interacts with the event scheduling interface |

---

## Use Cases

### UC-1 — Create a New Event


**Description:** A user creates a new scheduled event by providing a title, optional description, start time, and end time.

**Preconditions:**
- The user has navigated to the event creation form.

**Main Flow:**
1. The user opens the event creation form.
2. The user enters a title (required), an optional description, a start time, and an end time.
3. The user submits the form.
4. The system validates required fields, time range and checks for overlaps with other events.
5. The system saves the event and redirects the user to the event list or day view.

**Alternative Flows:**
- **AF-4a — Invalid time range:** The system displays an inline error and does not save the event.
- **AF-4b — Overlap detected:** The system warns the user that the updated event overlaps with an existing event. The user may proceed or adjust the times.
- **AF-4c — Missing required fields:** The system displays inline errors for any missing required fields and does not save the event.

**Postconditions:**
- A new event is persisted and visible in the calendar view.

---

### UC-2 — View Events in Day-Based Calendar View

**Related requirement:** FR-4

**Description:** A user views their scheduled events.

**Preconditions:**
- At least one event exists in the system.

**Main Flow:**
1. The user navigates to the calendar view.
2. The user selects or is presented with a specific day.
3. The system retrieves and displays all events for that day, each showing its title, time range, and calculated duration.

**Alternative Flows:**
- **AF-2a — No events exist:** The system displays an empty state message indicating no events are scheduled.

**Postconditions:**
- Events for the selected day are rendered as event cards.

---

### UC-3 — View Event Duration


**Description:** The system automatically calculates and displays the duration of each event.

**Preconditions:**
- An event with a valid start and end time exists.

**Main Flow:**
1. The user views the event list or day view.
2. For each event card, the system calculates the duration from start time to end time.
3. The calculated duration is displayed alongside the event title and time range.

**Postconditions:**
- Each event card shows its duration.

---

### UC-4 — Edit an Existing Event


**Description:** A user modifies the details of an existing event.

**Preconditions:**
- The event to be edited exists and is visible to the user.

**Main Flow:**
1. The user selects an event and opens the edit form.
2. The user modifies one or more fields (title, description, start time, end time).
3. The user submits the form.
4. The system validates required fields, time range and checks for overlaps with other events.
5. The system saves the changes and returns the user to the calendar view.

**Alternative Flows:**
- **AF-4a — Invalid time range:** The system displays an inline error and does not save the event.
- **AF-4b — Overlap detected:** The system warns the user that the updated event overlaps with an existing event. The user may proceed or adjust the times.
- **AF-4c — Missing required fields:** The system displays inline errors for any missing required fields and does not save the event.

**Postconditions:**
- The event reflects the updated details.

---

### UC-5 — Delete an Event

**Related requirement:** FR-5

**Description:** A user permanently removes an event from the schedule.

**Preconditions:**
- The event to be deleted exists and is visible to the user.

**Main Flow:**
1. The user selects an event.
2. Systems shows deletion page with event details and a confirmation prompt.
3. The user confirms the deletion.
4. The system removes the event from the data store.
5. The system returns the user to the calendar view, where the deleted event is no longer listed.

**Alternative Flows:**
- **AF-3a — User cancels deletion:** The user opts not to confirm the deletion, the system retains the event and returns to the previous view.

**Postconditions:**
- The event no longer appears in any view.

---

### UC-6 — Detect and Warn on Time Overlap

**Related requirement:** FR-3

**Description:** When creating or editing an event, the system checks for time conflicts with existing events and warns the user.

**Preconditions:**
- At least one other event exists in the system.

**Main Flow:**
1. The user submits an event creation or edit form.
2. The system compares the submitted event's time range against all existing events.
3. If an overlap is detected, the system displays a warning message identifying the conflict.
4. The user may acknowledge the warning and choose to save anyway, or go back and adjust the times.

**Alternative Flows:**
- **AF-3a — No overlap:** The system proceeds to save without displaying a warning.

**Postconditions:**
- The user is informed of any schedule conflicts before the event is saved.

---



## Use Case Summary

| Use Case | Description                             | Related FRs       |
|----------|-----------------------------------------|-------------------|
| UC-1     | Create a new event                      | FR-1, FR-2, FR-7  |
| UC-2     | View events in day-based calendar view  | FR-4              |
| UC-3     | View event duration                     | FR-8              |
| UC-4     | Edit an existing event                  | FR-5, FR-2, FR-3, FR-7 |
| UC-5     | Delete an event                         | FR-5              |
| UC-6     | Detect and warn on time overlap         | FR-3              |
