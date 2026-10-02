# STM – Client Timeline

## Overview

The **Client Timeline** is a chronological, unified view of a client’s journey inside App.

It aggregates all meaningful events related to the client:

- lead & sales
- sessions
- packages & proposals
- onboarding
- reminders
- client profile
- consultant notes

and presents them as a single ordered timeline.

This feature allows consultants to:

- understand client progress at a glance
- track improvements and regressions
- review historical interactions
- prepare for upcoming sessions
- maintain continuity across long‑term coaching or therapy

The timeline is **read‑only**, automatically generated, and updated whenever relevant client data changes.

---

## Required Event Types

### 1. Lead & Sales Events

##### LeadCaptured

- Description: A new lead is created (client enters App for the first time).
- Why: Marks the beginning of the client journey.

##### DiscoveryCallScheduled

- Description: A discovery call is scheduled with the lead, including date and time.
- Why: Represents a key milestone where the consultant initiates the first meaningful interaction to evaluate fit and readiness.

##### DiscoveryCallOutcome

- Description: Records the result of the discovery call. Outcome is stored in _metadata_ as `Qualified`, `NotQualified`, or `NotShow`.
- Why: Determining whether the lead moves forward to proposal or is closed.

### 2. Packages & Proposals Events

##### PackageCreated

- Description: Consultant creates a package for clients.
- Why: Shows what will be offered to clients.

##### PackageUpdated

- Description: Consultant changes a package.
- Why: Shows refinement or corrections.

##### PackageDeleted

- Description: Consultant deletes a package.
- Why: Important lifecycle event.

##### ProposalSent

- Description: Proposal is sent to the client.
- Why: Key sales milestone.

##### ProposalAccepted

- Description: Client accepts the proposal.
- Why: Marks conversion from lead => client.

##### ProposalRejected

- Description: Client rejects the proposal.
- Why: Important for understanding pipeline outcomes.

### 3. Onboarding Events

##### OnboardingStarted

- Description: Client onboarding begins after proposal acceptance.
- Why: Marks transition into active client status.

##### OnboardingTaskFinished

- Description: Records that an onboarding task has been resolved, either by completion or by being skipped.
- Why: Provides a single, consistent event type for tracking task progress.

### 4. Sessions Events

##### SessionScheduled

- Description: A session is created.
- Why: Core part of client progress.

##### SessionRescheduled

- Description: Session time is changed.
- Why: Shows adjustments and planning changes.

##### SessionCancelled

- Description: Session is cancelled.
- Why: Important for understanding disruptions.

##### SessionCompleted

- Description: Session is completed.
- Why: Major progress event.

### 5. Reminders (Hangfire) Events

##### SessionReminderScheduled

- Description: Reminder job is scheduled (Hangfire).
- Why: Shows system automation.

##### SessionReminderRescheduled

- Description: Records that a previously scheduled reminder for a client’s upcoming session has been changed to a new time or delivery method.
- Why: Provides visibility into adjustments made to reminder workflows.

##### SessionReminderCancelled

- Description: Records that a previously scheduled reminder for a client’s upcoming session has been cancelled.

- Why: Provides visibility into reminder management.

##### SessionReminderSent

- Description: Reminder is actually sent.
- Why: Helps consultants verify reminders.

### 6. Consultant Notes Events

##### ConsultantNoteAdded

- Description: Consultant adds a note.
- Why: Notes are essential for tracking progress.

##### ConsultantNoteUpdated

- Description: Consultant edits a note.
- Why: Shows refinement or corrections.

### 7. Consultant Documents Events

##### ConsultantDocumentUploaded

- Description: Consultant uploads a document.
- Why: Dcouments are essential for recording files.

##### ConsultantDocumentUpdated

- Description: Consultant changes a document.
- Why: Shows refinement or corrections.

##### ConsultantDocumentDeleted

- Description: Consultant deletes a document.
- Why: Important lifecycle event.

### 8. Client Profile & Status Events

##### ClientProfileUpdated

- Description: Client profile fields updated.
- Why: Shows changes in client information.

##### ClientStatusChanged

- Description: Client status changed.
- Why: Important lifecycle event.

---

## Enum

```csharp
public enum TimelineEventType
{
    // Lead & Discovery
    LeadCaptured = 1,
    DiscoveryCallScheduled = 2,
    DiscoveryCallOutcome = 3,

    // Packages & Proposals
    PackageCreated = 10,
    PackageUpdated = 11,
    PackageDeleted = 12,
    ProposalSent = 13,
    ProposalAccepted = 14,
    ProposalRejected = 15,

    // Onboarding
    OnboardingStarted = 20,
    OnboardingTaskFinished = 21,

    // Sessions
    SessionScheduled = 30,
    SessionRescheduled = 31,
    SessionCancelled = 32,
    SessionCompleted = 33,

    // Reminders
    SessionReminderScheduled = 40,
    SessionReminderRescheduled = 41,
    SessionReminderCancelled = 42,
    SessionReminderSent = 43,

    // Notes
    ConsultantNoteAdded = 50,
    ConsultantNoteUpdated = 51,

    // Documents
    ConsultantDocumentUploaded = 60,
    ConsultantDocumentUpdated = 61,
    ConsultantDocumentDeleted = 62,

    // Client profile
    ClientProfileUpdated = 70,
    ClientStatusChanged = 71
}
```

This enum is:

- grouped logically
- easy to map to UI icons
- easy to extend when App adds other events later

**Note:**
`ClientTimeline.EventType` is stored as `INT` because it represents a predefined system enum used for fast filtering, sorting, and indexing in a high‑volume event log table.
While `Client.Status`, for example, is stored as `NVARCHAR` because it represents a human‑readable business state that appears directly in the UI.

---

## Should consultants see events from other consultants?

Yes. Because the timeline is a **client‑centric history**, not a consultant‑centric history.

The timeline answers:
_What has happened with this client over time?_
Not:
_What has this consultant done?_

**You can add optional filtering:**

- Show only my events
- Show only onboarding events
- Show only sales events
- Show only session events

---

## Workflow

All major modules, including Lead Capture, Discovery Calls, Packages, Proposals, Onboarding, Sessions, Reminders, Notes, and Client Profile updates, automatically add events to the `ClientTimeline` table through their respective handlers.
Each handler records a `TimelineEventType` along with a structured `MetadataJson` payload describing the action (such as IDs, timestamps, and relevant details).
