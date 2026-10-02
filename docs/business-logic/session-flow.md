# STM – Sessions

## Overview

A **Session** represents a scheduled meeting between a consultant and a client as part of a proposal.
Sessions can be online or in‑person, may include uploaded materials, and follow a clear lifecycle defined by their status.

---

## Status field

### Purpose

The Status field defines the lifecycle of a session. It ensures the system knows whether a session is upcoming, finished, or no longer happening.

Allowed values

- **Scheduled:** the default state when a session is created
- **Completed:** the session has taken place
- **Cancelled:** the session was cancelled by consultant or client

### Implementation

**Status** is implemented as a C# enum (`SessionStatus`) and persisted as an `nvarchar(50)` column using EF Core value conversion.
This follows the same pattern used for the `Status` field in the `Clients` table.
The database stores human‑readable values (`Scheduled`, `Completed`, `Cancelled`) and enforces validity using a SQL CHECK constraint.

---

## Workflow

### 1. Create a Session

Consultant schedules a new session.

Backend receives: `POST /api/sessions`

Backend:

- validates time range (`StartTime < EndTime`) and required fields
- sets `Status = 'Scheduled'`
- sets `CreatedAt`
- stores meeting link (if `IsOnline = true`) or location (if `IsOnline = false`)
- record an event in the `ClientTimeline` table:
  ```csharp
  clientTimeline.EventType = (int)TimelineEventType.SessionScheduled;
  ```
  `MetadataJson`:
  ```json
  {
    "ProposalId": "guid",
    "SessionId": "guid",
    "StartTime": "2025-01-10T14:00:00Z",
    "ScheduledBy": "Consultant",
    "IsOnline": true, // or false
    "LocationOrUrl": "string"
  }
  ```

**Calendar Integration (if enabled)**

- Check consultant has connected calendar
- Validate or refresh token
- Create external calendar event
- Store event ID in `ExternalEventId` in the `Sessions` table

### 2. Load Sessions

Frontend requests sessions for:

- consultant calendar
- client session history
- proposal details

Backend provides:
`GET /api/sessions`
`GET /api/sessions?clientId={id}`
`GET /api/proposals/{proposalId}/sessions`

Sessions are returned ordered by `StartTime`.

### 3. Update Session Details

A session is editable only while its status is `Scheduled`, once it becomes `Completed` or `Cancelled`, it should no longer be editable.

Consultant edits:

- title
- description
- meeting link
- location
- start/end time

Backend receives: `PUT /api/sessions/{sessionId}`

Backend:

- checks `Status`
- validates new time range
- updates fields
- sets `UpdatedAt`
- if `StartTime` is changed, record an event in the `ClientTimeline` table:
  ```csharp
  clientTimeline.EventType = (int)TimelineEventType.SessionRescheduled;
  ```
  `MetadataJson`:
  ```json
  {
    "ProposalId": "guid",
    "SessionId": "guid",
    "StartTime": "2025-01-10T14:00:00Z",
    "ScheduledBy": "Consultant",
    "IsOnline": true, // or false
    "LocationOrUrl": "string"
  }
  ```

**Calendar Integration (if enabled and ExternalEventId exists)**

- Validate or refresh token
- Update external calendar event using `ExternalEventId`

### 4. Upload Session Materials

Consultant uploads files for the session.

Backend receives: `POST /api/sessions/{sessionId}/materials`

Backend:

- validates uploaded file (type and length)
- stores `Title` and `Description` in the `SessionMaterials` table
- stores file metadata in the table
- and sets `UploadedAt` and `UploadedBy` in the table

### 5. Change the Status

- After the meeting ends, consultant marks it as completed.
- Or consultant or client cancels the session.
- Cancelled sessions remain visible for audit and history.

Backend receives: `PUT /api/sessions/{sessionId}/status`

Payload:

```json
{ "status": "Completed" }
```

or

```json
{ "status": "Cancelled" }
```

Backend:

- checkes current `Status`
- updates `Status`
- sets `UpdatedAt`
- record an event in the `ClientTimeline` table:
  ```csharp
  clientTimeline.EventType = (int)TimelineEventType.SessionCompleted;
  or
  clientTimeline.EventType = (int)TimelineEventType.SessionCancelled;
  ```
  `MetadataJson`:
  ```json
  {
    "ProposalId": "guid",
    "SessionId": "guid",
    "ChangedAt": "2025-01-10T14:00:00Z",
    "ChangedBy": "Consultant"
  }
  ```

**Calendar Integration (if enabled and ExternalEventId exists)**

When session has been **cancelled**, backend:

- Validates or refreshes token
- Deletes external calendar event using `ExternalEventId`
- Sets `ExternalEventId = NULL`
