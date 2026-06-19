# TimeLedger - Iteration 3 Plan

| Field | Value |
|---|---|
| Iteration | 3 |
| Period | 2 weeks |
| Date | 2026-04-22 |
| Team | TimeLedger |
| Document owner | Team |

## Iteration goal
Deliver collaboration and planning improvements so users can manage group participation and events faster, with clear ownership separation and reliable automated tests.

## Scope for this iteration

### In scope
1. Group invites via in-program inbox.
2. Registration code sent by email.
3. Week calendar view.
4. Event views separated for user-owned and group-owned events.
5. Event types (one-time and recurring) with basic recurrence support.
6. Unit tests for new and changed service logic.

### Out of scope
- Push/mobile notifications.
- Advanced recurrence management (custom intervals, exceptions, series-wide edits/deletes).
- Advanced calendar drag-and-drop interactions.

## Prioritized backlog

### Must
- Invitation flow: create invite, list pending invites, accept/decline, prevent duplicate pending invites.
- Inbox page for current user to process invites.
- Registration flow with email code generation, send, verify, and expiry check.
- Week view page showing events by day and time blocks.
- Separate event lists/filters for user-owned vs group-owned events.
- Event-type handling for one-time and recurring events, including basic recurrence validation.
- Unit tests for invitation flow, registration-code verification, and event filtering logic.

### Should
- Better validation and user-facing error messages for invite/registration edge cases.
- Basic anti-abuse checks for registration code retries.
- Additional unit tests for negative/permission paths.

### Could
- Quick links from inbox invite to related group details.
- Week view quality improvements (labels, compact density toggle).

## Technical approach
- Core: add or extend DTOs/services for invites, email-code verification, and event ownership filtering.
- Infrastructure: add repository operations and SQL queries for invites and registration codes.
- Web: add inbox and week-view Razor Pages plus updates to registration and events pages.
- Tests: extend service tests with fake repositories for deterministic behavior.

## Timeline (2 weeks)

### Week 1
- Day 1: refine acceptance criteria and data contracts.
- Day 2-3: implement invite + inbox service/repository paths.
- Day 4: implement registration email-code backend flow.
- Day 5: unit tests for invite + registration core rules.

### Week 2
- Day 6-7: implement week calendar view and event ownership separation in UI.
- Day 8: add unit tests for event separation and edge cases.
- Day 9: bug fixing, refactor, and documentation updates.
- Day 10: final regression testing and demo preparation.

## Risks and mitigations
| Risk | Impact | Mitigation |
|---|---|---|
| Email provider setup delays | Registration flow blocked | Use local/mock sender first; switch provider after logic is stable |
| Invite authorization bugs | Unauthorized group access | Add explicit service-level permission checks and tests |
| Calendar complexity growth | Timeline slip | Keep week view read-only this iteration |
| Test gaps | Regressions during refactor | Require tests for every changed service path |

## Definition of done
- All Must items implemented.
- Critical paths covered by unit tests and passing.
- Existing test suite remains green.
- UI flows validated manually for success and failure paths.
- Documentation updated to match implemented behavior.

## Demo checklist
1. Owner sends group invite.
2. Recipient sees invite in inbox and accepts.
3. Accepted membership affects available group data.
4. Registration code is generated, sent, and verified.
5. Week view shows events grouped by day.
6. Event views clearly separate user-owned and group-owned events.
7. Unit tests run successfully for changed logic.

