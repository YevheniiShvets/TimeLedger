# TimeLedger Use Cases — Iteration 2
**Events, Authentication, Account Management, and Groups (Implemented Scope)**

---
## Functional Requirements
| ID | Requirement | Priority |
|----|-------------|----------|
| FR-10 | The system shall allow users to register with name, email, and password. | Must Have |
| FR-11 | The system shall authenticate users via server-side session login. | Must Have |
| FR-12 | The system shall allow authenticated users to manage only their own events. | Must Have |
| FR-13 | The system shall allow authenticated users to log out and clear the active session. | Must Have |
| FR-14 | The system shall allow authenticated users to view and edit their account information. | Must Have |
| FR-15 | The system shall allow authenticated users to create groups and become group owners. | Must Have |
| FR-16 | The system shall allow group owners to add and remove members by email. | Must Have |
| FR-17 | The system shall show groups to users who are owners or members of those groups. | Must Have |
| FR-18 | The system shall prevent non-owners from modifying group details and membership. | Must Have |

---
## Use Case Summary
| Use Case Number | Use Case Name | Short Description |
|-----------------|---------------|-------------------|
| UC-10 | Register User | A new user creates an account using name, email, and password. |
| UC-11 | Log In | A registered user signs in and starts an authenticated session. |
| UC-12 | Manage Own Events | An authenticated user can create, view, edit, and delete only their own events. |
| UC-13 | Log Out | An authenticated user ends their session. |
| UC-14 | View Account Info | An authenticated user views their account profile details. |
| UC-15 | Edit Account Info | An authenticated user updates profile details and/or password. |
| UC-16 | Create Group | An authenticated user creates a new group and becomes its owner. |
| UC-17 | View Accessible Groups | A user views groups they own or belong to. |
| UC-18 | Manage Group Members | A group owner adds/removes members and maintains group details. |

---
## Use Case Diagram (Textual)

**Actors and Use Cases**
- **New User**
  - UC-10: Register User

- **Registered User**
  - UC-11: Log In

- **Authenticated User**
  - UC-12: Manage Own Events
  - UC-13: Log Out
  - UC-14: View Account Info
  - UC-15: Edit Account Info
  - UC-16: Create Group
  - UC-17: View Accessible Groups

- **Group Owner**
  - UC-18: Manage Group Members

---
## Use Cases

## UC-10: Register User
**Actor:** New User

**Preconditions:**
- The user is not logged in.

**Main Success Scenario:**
1. The user opens the registration page.
2. The system displays a form for name, email, password, and confirm password.
3. The user submits valid data.
4. The system validates input and business rules.
5. The system stores the account with hashed password.
6. The system confirms registration and redirects to login.

**Extensions:**
- 3a. Email already exists
  - 1. The system shows an error message.
- 3b. Password and confirmation do not match
  - 1. The system shows an error message.
- 4a. Validation fails
  - 1. The system stays on the form and shows inline errors.

**Postconditions:**
- A user account exists.
- Credentials are stored securely.

---
## UC-11: Log In
**Actor:** Registered User

**Preconditions:**
- The user account exists.

**Main Success Scenario:**
1. The user opens the login page.
2. The system displays email and password fields.
3. The user submits credentials.
4. The system validates credentials.
5. The system creates an authenticated session.
6. The system redirects to the events page.

**Extensions:**
- 4a. Invalid credentials
  - 1. The system denies login and shows an error message.

**Postconditions:**
- The user is authenticated.
- Session data is stored server-side.

---
## UC-12: Manage Own Events
**Actor:** Authenticated User

**Preconditions:**
- The user is logged in.

**Main Success Scenario:**
1. The user opens the events pages.
2. The system retrieves only events belonging to the current user.
3. The user can create an event.
4. The user can edit an existing own event.
5. The user can delete an existing own event.
6. The system persists changes and refreshes the list.

**Extensions:**
- 2a. No events found
  - 1. The system shows an empty state.
- 4a/5a. Target event does not belong to the user
  - 1. The system denies access.

**Postconditions:**
- Only the actor's events are visible and modifiable by that actor.

---
## UC-13: Log Out
**Actor:** Authenticated User

**Preconditions:**
- The user is logged in.

**Main Success Scenario:**
1. The user chooses logout.
2. The system clears the authenticated session.
3. The system redirects to login page.

**Postconditions:**
- The user session is terminated.

---
## UC-14: View Account Info
**Actor:** Authenticated User

**Preconditions:**
- The user is logged in.

**Main Success Scenario:**
1. The user opens the account info page.
2. The system loads and displays the current account details.

**Postconditions:**
- The user can view their account profile data.

---
## UC-15: Edit Account Info
**Actor:** Authenticated User

**Preconditions:**
- The user is logged in.

**Main Success Scenario:**
1. The user opens account edit page.
2. The system displays editable account fields.
3. The user updates profile data and optionally password.
4. The system validates input and applies updates.
5. The system confirms update.

**Extensions:**
- 4a. Email already in use
  - 1. The system rejects update and shows an error.
- 4b. Password confirmation mismatch
  - 1. The system rejects update and shows an error.

**Postconditions:**
- The account data is updated.

---
## UC-16: Create Group
**Actor:** Authenticated User

**Preconditions:**
- The user is logged in.

**Main Success Scenario:**
1. The user opens create-group page.
2. The system displays group form.
3. The user submits valid group name.
4. The system validates and creates the group.
5. The system stores the creator as owner.
6. The group is shown in the actor's accessible groups.

**Postconditions:**
- New group exists with owner relationship.

---
## UC-17: View Accessible Groups
**Actor:** Authenticated User

**Preconditions:**
- The user is logged in.

**Main Success Scenario:**
1. The user opens groups list page.
2. The system retrieves groups where the actor is owner or member.
3. The system displays those groups.

**Postconditions:**
- The user sees all groups they can access.

---
## UC-18: Manage Group Members
**Actor:** Group Owner

**Preconditions:**
- The actor is logged in.
- The actor owns the group.

**Main Success Scenario:**
1. The owner opens group manage page.
2. The system displays members, including owner marker.
3. The owner enters a member email and adds user.
4. The system validates the user and membership rules.
5. The system saves membership.
6. The owner can remove non-owner members.

**Extensions:**
- 3a. Email does not match existing account
  - 1. The system shows "User not found".
- 4a. User already member (or owner)
  - 1. The system shows duplicate member error.
- 6a. Attempt to remove owner
  - 1. The system blocks operation.
- 1a/2a. Non-owner accesses manage actions
  - 1. The system denies modification.

**Postconditions:**
- Group membership reflects owner actions.
