# Use Cases: Group Invites Feature (Iteration 3)

## Overview
This document describes the use cases for the Group Invites & Inbox feature, which allows group owners to invite members and users to manage their invitations.

---

## Use Case Summary Table

| UC# | Title | Actor | Description | Iteration |
|-----|-------|-------|-------------|-----------|
| UC-19 | Send Group Invitation | Group Owner | Group owner invites another user to join their group | IT3 |
| UC-20 | View Inbox | User | User views all pending group invitations they have received | IT3 |
| UC-21 | Accept Group Invitation | Invitee | Invitee accepts a pending invitation and joins the group | IT3 |
| UC-22 | Decline Group Invitation | Invitee | Invitee declines a pending invitation | IT3 |

---

## Detailed Use Cases

### UC-19: Send Group Invitation

**Actor:** Group Owner

**Preconditions:**
- Group owner is logged in
- Group owner owns the target group

**Main Flow:**
1. Group owner navigates to the group and clicks "Invite Member"
2. Group owner enters the email address of the user they want to invite
3. Group owner clicks "Send Invite"
4. If successful, the invitation is sent and the group owner receives a confirmation

**Alternative Flows:**

**A1: Invalid Email**
- If the email format is invalid or the user does not exist
- System shows an error message
- User can try again

**A2: User Already a Member**
- If the invited user is already a member of the group
- System shows an error message

**A3: Invitation Already Pending**
- If an invitation is already pending for this user
- System shows an error message

**Postconditions:**
- Invitation is created and the invitee can see it in their inbox
- Group owner receives confirmation that the invitation was sent

---

### UC-20: View Inbox

**Actor:** User

**Preconditions:**
- User is logged in

**Main Flow:**
1. User navigates to their Inbox
2. System displays all pending invitations the user has received
3. For each invitation, the user can see:
   - The group name
   - Who sent the invitation
   - When the invitation was sent
4. User can view the list without taking any action

**Alternative Flows:**

**A1: No Pending Invitations**
- If the user has no pending invitations
- System displays an empty state message

**Postconditions:**
- User can see all their pending invitations in one place

---

### UC-21: Accept Group Invitation

**Actor:** Invitee

**Preconditions:**
- User is logged in
- User has a pending invitation
- The invitation has not expired

**Main Flow:**
1. User views their inbox
2. User finds the invitation they want to accept
3. User clicks "Accept"
4. User is now added to the group
5. The invitation is removed from the inbox

**Alternative Flows:**

**A1: Invitation Has Expired**
- If the invitation is older than 30 days
- System shows an error message and the invitation cannot be accepted

**A2: User Already a Member**
- If the user is already a member of the group
- System shows an appropriate message

**Postconditions:**
- User is now a member of the group
- User can start participating in group activities
- The invitation is no longer in the pending inbox

---

### UC-22: Decline Group Invitation

**Actor:** Invitee

**Preconditions:**
- User is logged in
- User has a pending invitation

**Main Flow:**
1. User views their inbox
2. User finds the invitation they want to decline
3. User clicks "Decline"
4. The invitation is removed from the inbox
5. User receives confirmation

**Postconditions:**
- The invitation is marked as declined
- User is not added to the group
- The user can still be invited to the group again in the future
