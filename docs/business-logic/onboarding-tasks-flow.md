# STM – Onboarding Tasks

## Overview

**Onboarding Tasks** represent the checklist of actions a consultant must complete after a client accepts a proposal.
They are generated from the **active accepted proposal** and guide the consultant through the onboarding process.

Each task is:

- tied to a Client
- tied to a Proposal (the active one)
- ordered for UI display
- tracked through a simple lifecycle (Pending => Completed => Skipped)

---

## Status field

### Purpose

The **Status** field indicates the current state of an onboarding task, allowing App to track its progress through the onboarding workflow and determine what actions the consultant can take next.

It tracks task progress:

- **Pending:** default state
- **Completed:** consultant finished the task
- **Skipped:** intentionally bypassed

### Implementation

**Status** is implemented as a C# enum (`OnboardingTaskStatus`) and persisted as an `nvarchar(20)` column using EF Core value conversion.
This follows the same pattern used for the `Status` field in the `Clients` table.
The database stores human‑readable values (`Pending`, `Completed`, `Skipped`) and enforces validity using a SQL CHECK constraint.

---

## Order field

### Purpose

The **Order** field defines the exact sequence in which onboarding tasks should appear and be completed, ensuring the consultant follows a clear, structured progression during the onboarding process.
By assigning each task a numeric position, App can consistently display tasks in the intended workflow order, and maintain predictable navigation for the consultant.

---

## Backend Workflow

### 1. Proposal Accepted => Generate Tasks

When a proposal is accepted:

- Backend marks the proposal as `IsActive = true`
- Backend deactivates previous accepted proposals (`IsActive = false`)
- Backend automatically creates onboarding tasks in `OnboardingTasks` table from `OnboardingTaskTemplates`:
  - Based on consultant’s templates: `ConsultantId = proposal.ConsultantId`
  - And optionally package-specific templates: `PackageId = proposal.PackageId OR NULL`

### 2. Load Onboarding Tasks

Frontend requests: `GET /api/clients/{clientId}/onboarding-tasks`

Backend returns tasks that are already generated automatically, ordered by `Order ASC`.

### 3. Update List of Tasks (Bulk Update)

Consultant can add new tasks and remove or reorder existing ones.

Frontend sends the entire updated list of tasks:
`PUT /api/onboarding-tasks/bulk-update`

Backend:

- deletes tasks that are missing from the payload
- inserts tasks with `id = null`
- updates the `Order` field for all tasks
- returns the updated list
- performs all changes in a single transaction

### 4. Update Task Status

Frontend sends:
`POST /api/onboarding-tasks/{taskId}/complete`
or
`POST /api/onboarding-tasks/{taskId}/skip`

Backend:

- Updates `Status`, `UpdatedAt`, `CompletedAt` (if completed)
- Record an event in the `ClientTimeline` table:
  ```csharp
   clientTimeline.EventType = (int)TimelineEventType.OnboardingTaskFinished;
  ```
  `MetadataJson`:
  ```json
  {
    "ProposalId": "guid",
    "TaskId": "guid",
    "TaskTitle": "string",
    "FinishedAt": "2025-01-05T15:30:00Z",
    "FinishedBy": "Consultant",
    "As": "Completed" // or "Skipped"
  }
  ```

Backend ensures:

- Status transitions are valid
- `CompletedAt` is only set when `Status = Completed`
- Skipped tasks do not set `CompletedAt`

### 5. Modify a Specific Task

Consultant may update:

- `Title`
- `Description`
- `DueDate`

Backend updates fields and sets `UpdatedAt`.

Snapshot fields (`ProposalId`, `ClientId`) are **immutable**.

---

## Frontend Workflow

### 1. Client Profile => Onboarding Section

Frontend loads:

- Active proposal
- Onboarding tasks

Displays tasks in order:

- Title
- Description
- Status badge
- Due date
- Completion controls (UI elements that let the consultant mark a task as **Completed** or **Skipped**.)

### 2. Save Updated Task List

Consultant can:

- add new tasks
- delete existing tasks
- reorder tasks

When the consultant clicks **Save**, the frontend sends the entire updated list of tasks:
`PUT /api/onboarding-tasks/bulk-update`

### 3. Modify a Specific Task

When the consultant clicks on a task in the list:

- The frontend opens a task detail panel or modal.
- Consultant can edit:
  - Title
  - Description
  - Due date (if applicable)

After editing, the frontend sends: `PUT /api/onboarding-tasks/{taskId}`

### 4. Consultant Completes or Skips Tasks

UI provides:

- "Mark as Completed"
- "Skip Task"
- "Edit Task"

Actions call backend endpoints.

UI updates:

- Status
- CompletedAt
- Progress bar

### 5. Onboarding Completion

When all tasks are:

- Completed
- or Skipped

Frontend shows:
**"Onboarding Complete. You can now schedule sessions."**

This transitions the client into the active coaching phase.
