# TimeLedger Use Cases — Iteration 2
**User Management, Authentication, and Groups**

---
## Functional Requirements
| ID | Requirement | Priority |
|----|-------------|----------|
| FR-10 | The system shall allow users to register with an email and password. | Must Have |
| FR-11 | The system shall authenticate users via cookie-based session auth. | Must Have |
| FR-12 | The system shall associate events with the user who created them. | Must Have |
| FR-13 | The system shall prevent users from accessing events belonging to other users. | Must Have |
| FR-14 | The system shall allow users to create groups and invite members. | Must Have |

---
## Use Case Summary
| Use Case Number | Use Case Name | Short Description |
|-----------------|---------------|-------------------|
| UC-10 | Register User | A new user creates an account using an email address and password. |
| UC-11 | Log In | A registered user signs in and starts an authenticated session. |
| UC-12 | View Own Events | An authenticated user views only the events that belong to their account. |
| UC-13 | Create Group | An authenticated user creates a new group and becomes its owner. |
| UC-14 | Invite User to Group | A group owner invites another user to join the group. |
| UC-15 | Accept or Decline Group Invitation | An invited user responds to a pending group invitation. |

---
## Use Case Diagram

**Actors and Use Cases**
- **New User**
  - UC-10: Register User

- **Registered User**
  - UC-11: Log In

- **Authenticated User**
  - UC-12: View Own Events
  - UC-13: Create Group

- **Group Owner**
  - UC-14: Invite User to Group

- **Invited User**
  - UC-15: Accept or Decline Group Invitation

---
## Use Cases:

## UC-10: Register User
**Actor:** New User

**Preconditions:**
- The user is not logged in.
- The user does not already have an account.

**Main Success Scenario:**
1. The user opens the registration page.
2. The system displays a form requesting email and password.
3. The user enters valid details.
4. The system validates the input.
5. The system stores the user account with a hashed password.
6. The system confirms successful registration.

**Extensions:**
- 3a. Email is already registered
  - 1. The system shows an error message.
- 3b. Password does not meet security rules
  - 1. The system shows an error message.
- 4a. Validation fails
  - 1. The system keeps the user on the form and displays inline validation messages.

**Postconditions:**
- A new user account exists in the system.
- The password is stored securely.

---
## UC-11: Log In
**Actor:** Registered User

**Preconditions:**
- The user already has an account.

**Main Success Scenario:**
1. The user opens the login page.
2. The system displays a login form.
3. The user enters email and password.
4. The system validates the credentials.
5. The system creates a cookie-based authenticated session.
6. The system redirects the user to their events page.

**Extensions:**
- 4a. Invalid email or password
  - 1. The system displays an authentication error.
- 4b. Account data is missing or corrupted
  - 1. The system denies login and shows an error message.

**Postconditions:**
- The user is authenticated.
- A cookie-based session is active after successful login.

---
## UC-12: View Own Events
**Actor:** Authenticated User

**Preconditions:**
- The user is logged in.

**Main Success Scenario:**
1. The user opens the events page.
2. The system identifies the currently authenticated user.
3. The system retrieves only events belonging to that user.
4. The system displays the user’s events ordered by start time and grouped by date.
   
**Extensions:**
- 3a. The user has no events
  - 1. The system displays an empty state message.
- 3b. The user attempts to access another user’s event
  - 1. The system denies access.

**Postconditions:**
- The user sees only their own events.

---
## UC-13: Create Group
**Actor:** Authenticated User

**Preconditions:**
- The user is logged in.
  
**Main Success Scenario:**
1. The user opens the create-group page.
2. The system displays a form for group details.
3. The user enters the group information.
4. The system validates the input.
5. The system creates the group.
6. The system assigns the current user as the Owner of the group.
7. The system shows the group details page or group list.
   
**Extensions:**
- 4a. Validation fails
  - 1. The system displays inline validation errors.

**Postconditions:**
- A new group exists in the system.
- The current user is stored as the Owner of the group.


---
## UC-14: Invite User to Group
**Actor:** Group Owner

**Preconditions:**
- The actor is logged in.
- The actor owns the group.
- The target group exists.
  
**Main Success Scenario:**
1. The owner opens the group members/invitations page.
2. The system displays an invitation form.
3. The owner enters the email address of the user to invite.
4. The system validates the request.
5. The system creates a group invitation record.
6. The system marks the invitation as pending.
7. The system confirms that the invitation was sent.
   
**Extensions:**
- 3a. Email address is invalid
  - 1. The system shows a validation error.
- 5a. The invited user is already a member
  - 1. The system shows an error and does not create a duplicate invitation.

**Postconditions:**
- A pending invitation exists for the specified user.

---
## UC-15: Accept or Decline Group Invitation
**Actor:** Invited User

**Preconditions:**
- The user is logged in.
- A pending group invitation exists for that user.
  
**Main Success Scenario:**
1. The user opens their pending invitations.
2. The system displays the invitation details.
3. The user chooses to accept or decline.
4. The system updates the invitation status.
5. If accepted, the system creates a record for the user.
6. The system confirms the result.

**Extensions:**
- 3a. The user chooses to decline the invitation
  - 1. The system updates the invitation status to declined and does not add the user to the group.
       
**Postconditions:**
- The invitation is no longer pending after the user responds.
- If accepted, the user becomes a member of the group.