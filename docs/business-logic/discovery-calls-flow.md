# STM – Discovery Calls

## Overview

The **DiscoveryCalls** workflow manages the transition from a submitted lead to a scheduled discovery session. Here, "Discovery" means discovering whether the lead is a good fit, it’s the qualification conversation where the consultant uncovers the lead’s needs, goals, challenges, and readiness.

The Discovery Call workflow has three phases:

1.  **Before the call:** scheduling
2.  **During the call:** the actual conversation
3.  **After the call:** recording the outcome

---

## Status field

### Purpose

The **Status** field represents the operational state of a discovery call.
It tells App what has happened to the call itself, independent of the business outcome.

### Implementation

**Status** is implemented as a C# enum (`DiscoveryCallStatus`) and persisted as an `nvarchar(20)` column using EF Core value conversion.
This follows the same pattern used for the `Status` field in the `Clients` table.
The database stores human‑readable values (`Scheduled`, `Completed`, `No-Show`) and enforces validity using a SQL CHECK constraint.

---

## Outcome field

### Purpose

The **Outcome** field represents the business decision made after the Discovery Call. It tells App what the consultant decided after the call.

### Implementation

**Outcome** is implemented as a C# enum (`DiscoveryCallOutcome`) and persisted as an `nvarchar(20)` column using EF Core value conversion.
This follows the same pattern used for the `Status` field in the `Clients` table. The database stores human‑readable values (`Qualified`, `NotQualified`, `NoShow`) and enforces validity using a SQL CHECK constraint.

---

| Consultant Action     | Status    | Outcome      | Lead Lifecycle Status | Description                               |
| --------------------- | --------- | ------------ | --------------------- | ----------------------------------------- |
| Mark as Qualified     | Completed | Qualified    | Qualified             | Call happened and the lead is a good fit. |
| Mark as Not Qualified | Completed | NotQualified | NotQualified          | Call happened but the lead is not a fit.  |
| Mark as No-Show       | No-Show   | NoShow       | NoShow                | Lead did not attend the scheduled call.   |

---

### Backend Workflow

**Scheduled call time arrives**
Backend does not automatically change anything when the time arrives.
The call itself is an **offline real‑world event**.

The backend simply waits for the consultant to record the outcome.

**Consultant opens the lead after the call**
Frontend calls: `GET /api/lead-submissions/{id}`

Backend returns:

- status: DiscoveryScheduled
- availableActions: ["MarkQualified", "MarkNotQualified", "MarkNoShow"]
- discovery call details (ScheduledAt, DurationMinutes, Notes)

This tells the frontend: "The call happened; now record the outcome."

**Consultant records the outcome**
Consultant chooses one:

- Qualified
- Not Qualified
- No‑Show (the lead did not attend the scheduled Discovery Call)

Frontend sends: `POST /api/discovery-calls/{id}/set-outcome`
Payload is:

```json
{
  "outcome": "Qualified",
  "notes": "Lead is a great fit."
}
```

**Backend validates**
Backend checks:

- DiscoveryCall exists
- Outcome is currently `NULL`
- Status is Scheduled
- Consultant has permission

**Backend updates DiscoveryCall**
Backend sets:

- `Status = Completed` (for Qualified / NotQualified)
- `Status = No-Show` (for NoShow)
- Notes = consultant notes
- `Outcome` = `Qualified` | `NotQualified` | `NoShow`
- If `Outcome = Qualified`
  - then `client.Status = Active`
  - and record a row in the `ClientTimeline` table with:
    ```csharp
    clientTimeline.EventType = (int)TimelineEventType.ClientStatusChanged;
    ```
    `MetadataJson`:
    ```json
    {
      "OldStatus": "Lead",
      "NewStatus": "Active",
      "ChangedAt": "2026-07-31T11:50:00Z",
      "ChangedBy": "Consultant",
      "TriggerEvent": "DiscoveryCall",
      "Outcome": "Qualified"
    }
    ```
- If `Outcome = NotQualified`
  - then `client.Status = Completed`
  - and record a row in the `ClientTimeline` table with:
    ```csharp
    clientTimeline.EventType = (int)TimelineEventType.ClientStatusChanged;
    ```
    `MetadataJson`:
    ```json
    {
      "OldStatus": "Lead",
      "NewStatus": "Completed",
      "ChangedAt": "2026-07-31T11:50:00Z",
      "ChangedBy": "Consultant",
      "TriggerEvent": "DiscoveryCall",
      "Outcome": "NotQualified"
    }
    ```

This is the **qualification moment**.

**Backend updates calendar event**
Using `ExternalEventId`, backend may:

- add notes
- mark event as completed
- delete event (if No‑Show)
- update reminders

**Record an event in the `ClientTimeline` table**

```csharp
clientTimeline.EventType = (int)TimelineEventType.DiscoveryCallOutcome;
```

`MetadataJson`:

```json
{
  "DiscoveryCallId": "guid",
  "Outcome": "Qualified | NotQualified | NotShow",
  "EvaluatedBy": "Consultant",
  "EvaluatedAt": "2025-01-01T11:00:00Z",
  "Notes": "string"
}
```

**Backend returns updated state**
Example:

```csharp
{
    "status": "Qualified",
    "availableActions": [],
    "discoveryCall": {
        "status": "Completed",
        "outcome": "Qualified",
        "notes": "Lead is a great fit."
    }
}
```

### Frontend Workflow

**Consultant opens the lead after the call**
Frontend loads submission detail:

- sees status: DiscoveryScheduled
- sees available actions:
  - Mark Qualified
  - Mark Not Qualified
  - Mark No-Show

UI shows outcome buttons.

**Consultant clicks an outcome button**
Example:

- Mark as Qualified

Frontend sends: `POST /api/discovery-calls/{id}/set-outcome`

**Frontend receives updated lifecycle state**
Frontend:

- updates status badge: **Qualified**
- hides all action buttons
- shows success toast
- displays consultant notes
- refreshes discovery call details
- Lead is now fully qualified.

Lead is now **fully qualified**.

**Optional frontend behaviors**

- Show “Next Step: Onboard Client”
- Show "Send Welcome Email"
