# TimeLedger Entity Relationship Diagram (ERD)

## 1. ER Diagram

### 1.1 Visual Representation

```text
TIMELEDGER DATABASE SCHEMA

+----------------------+      1 ------ M      +----------------------+
| USERS                |---------------------->| GROUPS               |
|----------------------|                       |----------------------|
| PK id                |                       | PK id                |
| name                 |                       | FK ownerId -> USERS  |
| UK email             |                       | name                 |
| passwordHash         |                       +----------------------+
| createdAt            |
+----------------------+

+----------------------+      M ------ M      +----------------------+
| USERS                |<-------------------> | GROUPS               |
+----------------------+      via             +----------------------+
                                +----------------------+
                                | GROUPMEMBERS         |
                                |----------------------|
                                | PK,FK groupId        |
                                | PK,FK userId         |
                                +----------------------+

+----------------------+
| EVENTS               |
|----------------------|
| PK id                |
| ownerType            |
| ownerId              |
| title                |
| description          |
| location             |
| startTime            |
| endTime              |
| allowOverlap         |
+----------------------+

ownerType = 'User'  -> ownerId references USERS.id
ownerType = 'Group' -> ownerId references GROUPS.id
```

Notes:
- `EVENTS.ownerId` is polymorphic: it references `USERS.id` when `ownerType = 'User'`, or `GROUPS.id` when `ownerType = 'Group'`.
- This polymorphic relation is validated in the service layer.

### 1.2 Entity-Relationship Notation

```
TimeLedger Database Schema

USERS (1) --------< (M) GROUPS
  id PK                id PK
  name                 ownerId FK -> USERS.id
  email UK             name
  passwordHash
  createdAt

USERS (M) ----< GROUPMEMBERS >---- (M) GROUPS
                 groupId PK,FK -> GROUPS.id
                 userId  PK,FK -> USERS.id

EVENTS
  id PK
  ownerType  ('User' | 'Group')
  ownerId    (polymorphic FK)
  title
  description
  location
  startTime
  endTime
  allowOverlap
```

## 2. Entity Descriptions

### 2.1 Users Table

**Purpose**: Stores user account information with authentication credentials.

**Primary Key**: `id` (auto-increment)

| Column | Type | Constraints | Purpose |
|--------|------|-------------|---------|
| `id` | INT | PRIMARY KEY, AUTO_INCREMENT | Unique identifier for each user |
| `name` | VARCHAR(256) | NOT NULL | Display name of the user |
| `email` | VARCHAR(256) | NOT NULL, UNIQUE | Unique email for login and identification |
| `passwordHash` | VARCHAR(MAX) | NOT NULL | BCrypt-hashed password (never plaintext) |
| `createdAt` | DATETIME | NOT NULL, DEFAULT GETUTCDATE() | UTC timestamp of account creation |

**Indexes**:
- PRIMARY KEY on `id`
- UNIQUE INDEX on `email` for fast login lookups

**Security Considerations**:
- Passwords are hashed with BCrypt; plaintext passwords never stored
- Email is case-insensitive in queries for user-friendly login

---

### 2.2 Groups Table

**Purpose**: Represents collaborative groups created by users for shared scheduling.

**Primary Key**: `id` (auto-increment)

**Foreign Keys**:
- `ownerId` → `users.id` (NOT NULL; group creator/administrator)

| Column | Type | Constraints | Purpose |
|--------|------|-------------|---------|
| `id` | INT | PRIMARY KEY, AUTO_INCREMENT | Unique identifier for each group |
| `ownerId` | INT | NOT NULL, FOREIGN KEY → users.id | User who created and owns the group |
| `name` | VARCHAR(256) | NOT NULL | Name of the group |

**Indexes**:
- PRIMARY KEY on `id`
- FOREIGN KEY INDEX on `ownerId` for efficient owner lookups

**Business Rules**:
- Group owner is automatically a member (stored in `GroupMembers`)
- Deleting the owner cascades to delete the group (if configured with CASCADE)

---

### 2.3 Events Table

**Purpose**: Stores scheduled events that can be owned by either a user or a group (polymorphic ownership).

**Primary Key**: `id` (auto-increment)

**Foreign Keys**:
- `ownerId` → `users.id` OR `groups.id` (polymorphic; determined by `ownerType`)

| Column | Type | Constraints | Purpose |
|--------|------|-------------|---------|
| `id` | INT | PRIMARY KEY, AUTO_INCREMENT | Unique identifier for each event |
| `ownerType` | VARCHAR(50) | NOT NULL, DEFAULT 'User' | Enum: 'User' or 'Group'; determines what `ownerId` refers to |
| `ownerId` | INT | NOT NULL | Foreign key to `users.id` or `groups.id` |
| `title` | VARCHAR(256) | NOT NULL | Event title/name |
| `description` | VARCHAR(MAX) | NULL | Optional event details |
| `location` | VARCHAR(256) | NULL | Optional event location |
| `startTime` | DATETIME | NOT NULL | Event start time (UTC) |
| `endTime` | DATETIME | NOT NULL | Event end time (UTC) |
| `allowOverlap` | BIT | NOT NULL, DEFAULT 0 | User override flag for time overlap detection |

**Indexes**:
- PRIMARY KEY on `id`
- COMPOUND INDEX on (`ownerId`, `ownerType`) for efficient owner-based queries
- RANGE INDEX on (`startTime`, `endTime`) for overlap detection queries

**Business Rules**:
- `endTime` must be > `startTime` (enforced at service layer)
- Overlap detection: Two events by the same owner cannot share time unless `allowOverlap = true`
- Only owner (user or group members) can view/modify the event

---

### 2.4 GroupMembers Table (Junction Table)

**Purpose**: Represents the many-to-many relationship between users and groups. Tracks group membership.

**Primary Key**: Composite (`groupId`, `userId`)

**Foreign Keys**:
- `groupId` → `groups.id` (CASCADE DELETE)
- `userId` → `users.id` (CASCADE DELETE)

| Column | Type | Constraints | Purpose |
|--------|------|-------------|---------|
| `groupId` | INT | NOT NULL, PRIMARY KEY (part 1), FOREIGN KEY → groups.id | Group identifier |
| `userId` | INT | NOT NULL, PRIMARY KEY (part 2), FOREIGN KEY → users.id | User identifier (group member) |

**Indexes**:
- COMPOSITE PRIMARY KEY on (`groupId`, `userId`)
- INDEX on `userId` for reverse lookup (all groups a user belongs to)

**Business Rules**:
- Composite primary key prevents duplicate memberships
- Group owner is also stored as a member in this table
- Cascading deletes ensure orphaned records are automatically removed

---

## 3. Relationships

### 3.1 User-to-Group (1:M)

**Direction**: One User creates many Groups

**Relationship**:
```
Users.id ──1──┐
              ├──M── Groups.ownerId
```

**Cardinality**: 
- A user can create multiple groups
- Each group has exactly one owner (creator)

**Enforcement**: FOREIGN KEY constraint on `Groups.ownerId` → `Users.id`

---

### 3.2 User-to-Event (1:M)

**Direction**: One User owns many Events (when `Event.OwnerType = 'User'`)

**Relationship**:
```
Users.id ──1──┐
              ├──M── Events.ownerId (when ownerType = 'User')
```

**Cardinality**: 
- A user can create multiple events
- Each user-owned event references exactly one user

**Enforcement**: FOREIGN KEY constraint (logical; enforced at service layer based on `OwnerType`)

---

### 3.3 Group-to-Event (1:M)

**Direction**: One Group owns many Events (when `Event.OwnerType = 'Group'`)

**Relationship**:
```
Groups.id ──1──┐
               ├──M── Events.ownerId (when ownerType = 'Group')
```

**Cardinality**: 
- A group can own multiple events
- Each group-owned event references exactly one group

**Enforcement**: FOREIGN KEY constraint (logical; enforced at service layer based on `OwnerType`)

---

### 3.4 User-to-Group (M:M via GroupMembers)

**Direction**: Many Users are Members of Many Groups

**Relationship**:
```
Users.id ──1──┐                    ┌──1── Groups.id
              ├──M── GroupMembers ─┤
              └──────────────────┐ │
                                 └─┴────────────

Detailed:
Users.id ──M──┐
              ├──GroupMembers──┐
              └─────────────────┤
                                ├──M── Groups.id
                            ┌───┘
                        ┌───┘
                    ┌───┘
```

**Cardinality**: 
- A user can be a member of multiple groups
- A group can have multiple member users

**Enforcement**: Composite FOREIGN KEY on `GroupMembers(groupId, userId)` with CASCADE DELETE

---

## 4. Data Integrity & Constraints

### 4.1 Referential Integrity

| Constraint | Details |
|------------|---------|
| Users PK | `id` must be unique and not null |
| Users UNIQUE | `email` must be unique and not null |
| Groups FK | `ownerId` must reference existing `users.id` |
| Events FK | `ownerId` must be valid (validated at service layer based on `ownerType`) |
| GroupMembers FK (groupId) | Must reference existing `groups.id`; DELETE CASCADE |
| GroupMembers FK (userId) | Must reference existing `users.id`; DELETE CASCADE |

### 4.2 Domain Constraints

| Constraint | Details | Enforcement |
|------------|---------|-------------|
| Email Uniqueness | Users cannot share the same email | DB UNIQUE INDEX |
| Email Format | Must be valid email format | Service layer validation |
| Password Strength | Minimum complexity enforced | Service layer validation |
| Event Duration | `EndTime > StartTime` | Service layer validation |
| Time Overlap | No overlapping events unless `allowOverlap = true` | Service layer logic |
| Group Ownership | Only owner can modify group | Service layer authorization |
| Member Access | Only owner and members can view group events | Service layer authorization |

### 4.3 Cascading Deletes

| Delete Scenario | Cascade Behavior |
|-----------------|------------------|
| Delete User | All owned Groups, Events, and GroupMembers entries are deleted |
| Delete Group | All owned Events and GroupMembers entries are deleted |
| Delete Event | No cascading (leaf entity) |
| Remove GroupMember | No cascading (just remove from junction table) |


---

## 5. Database Design Justifications

### 5.1 Why Polymorphic Ownership (OwnerType)?

**Rationale**:
- Supports future group-based event creation while maintaining single Events table
- Avoids duplicate event schemas or complex union queries
- Allows reuse of overlap detection logic regardless of owner type

**Alternative Considered**: Separate tables (`UserEvents`, `GroupEvents`)
**Rejected**: Would duplicate schema and business logic; polymorphic design is cleaner for this domain

---

### 5.2 Why Junction Table for GroupMembers?

**Rationale**:
- Standard M:M relationship design allows many users in many groups
- Enables efficient queries in both directions
- Supports future features like membership status or role tracking

**Alternative Considered**: Store group membership as serialized data in Groups
**Rejected**: Would limit queryability and make authorization checks inefficient

---

### 5.3 Why Composite Primary Key in GroupMembers?

**Rationale**:
- Guarantees no duplicate memberships (same user cannot be added to same group twice)
- Efficient lookup for "is user X a member of group Y?"
- Supports CASCADE DELETE automatically removing memberships when groups/users are deleted

**Alternative Considered**: Surrogate key (auto-increment ID)
**Rejected**: Natural composite key is semantically correct and provides built-in duplicate prevention

---

## 6. Scalability & Performance Considerations

### 6.1 Indexes for Performance

- **Users.email**: UNIQUE INDEX for fast login lookups
- **Groups.ownerId**: INDEX for fast "my groups" queries
- **Events (ownerId, ownerType)**: Composite INDEX for efficient ownership filtering
- **Events (startTime, endTime)**: Range INDEX for overlap detection queries
- **GroupMembers.userId**: INDEX for reverse lookup (all groups a user is in)

### 6.2 Query Optimization

- Overlap detection uses indexed range queries (`startTime`, `endTime`)
- User-to-group queries use composite keys in junction table
- Ownership filtering leverages `OwnerType` enum to efficiently filter Events

### 6.3 Future Scalability

- **Partitioning**: Events table could be partitioned by `startTime` for large datasets
- **Caching**: Frequently accessed group/user relationships could be cached
- **Denormalization**: For reporting, consider materialized views (e.g., group event counts)

