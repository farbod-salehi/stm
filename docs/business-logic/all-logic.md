# STM – Calendar Integration

## Overview

Calendar Integration synchronizes App **Sessions** with the consultant’s external calendar (Google or Outlook).
It ensures that every scheduled session **automatically** appears in the consultant’s personal calendar, stays updated when edited, and is removed when cancelled.

Calendar Integration is only allowed when the consultant has an active, valid calendar connection

---

## Workflow

### 0. Precondition: Consultant Connects Calendar

Before any session can sync:

- Consultant connects Google/Outlook calendar
- App receives an access token + refresh token
- App stores them securely
- App marks consultant as "Calendar Connected"

**All future calendar operations require:**

- consultant has calendar integration enabled
- token is valid (or refreshed automatically)

### 1. Session Created => Validate Access => Create Calendar Event

**When a new session is created:**

1. Check consultant has calendar integration enabled

2. Validate token
   - If expired => refresh
   - If refresh fails => disable integration

3. Create event in external calendar
4. Store the returned event ID in the `Sessions` table (`ExternalEventId`)

5. Session and calendar event are now linked

**Event includes:**

- title
- description
- StartTimeUtc / EndTimeUtc
- meeting link or location

### 2. Session Updated => Validate Access => Update Calendar Event

**When a session is edited:**

- Check consultant has calendar integration enabled
- Validate token
- Use `ExternalEventId` (from `Sessions` table) to update the event in the external calendar
- Save updated event metadata

**Fields that trigger updates:**

- title
- description
- start/end time
- meeting link
- location

### 3. Session Cancelled => Validate Access => Delete Calendar Event

**When a session is cancelled:**

- Check consultant has calendar integration enabled
- Validate token
- Use `ExternalEventId` (from `Sessions` table) to delete the event
- Set `ExternalEventId = NULL` (in `Sessions` table)

**This prevents outdated events from remaining in the consultant’s calendar.**

### 4. Session Completed

Completed events stay in the consultant’s calendar for history.

### 5. Session Materials Uploaded

Uploading materials does not affect calendar integration.

---

# STM – Clients

## Overview

A **Client** represents any person who enters the system. Every client begins as a **Lead** when they submit a public form or are added manually, then progresses through discovery, proposals, and onboarding, eventually becomeing an **Active Client** who receives sessions and ongoing support.

---

## Status field

### Purpose

The **Status** defines where a client currently stands in the business workflow. It is the backbone of client lifecycle management.

### Statuses Descriptions

- **Lead** - A potential client who has shown interest but has not yet started working with the consultant. Leads typically have minimal data: name, email/phone, source.
- **Active** - A client who is currently receiving coaching or consulting services. This is the main operational state.
- **Puased** - A client who temporarily stopped working with the consultant. Common reasons: vacation, budget pause, personal issues.
- **Completed** - A client who has finished their coaching program or engagement. Completed clients remain in the system for analytics and history.

### Implementation

To ensure clarity, safety, and consistency across code and database layers, the following design is used:

- **Enum in Code**:  
  In C#, the status is represented as a strongly‑typed `enum` (`ClientStatus`). This prevents typos and invalid assignments at compile time.

  ```csharp
  public enum ClientStatus
  {
      Lead,
      Active,
      Paused,
      Completed
  }

  public class Clinet
  {
      ...
      public ClientStatus Status { get; set; }
  }
  ```

- **NVARCHAR in Database:**
  The enum is stored as a string (NVARCHAR(30)) for readability and reporting. EF Core maps the enum values to their string names:

  ```csharp
  modelBuilder.Entity<Client>()
      .Property(c => c.Status)
      .HasConversion<string>()   // store enum as NVARCHAR
      .HasMaxLength(30);
  ```

- **CHECK Constraint for Integrity:**
  A CHECK constraint is added via EF Core to enforce that only valid status values are stored:

  ```csharp
  modelBuilder.Entity<Client>()
      .HasCheckConstraint("CK_Clients_Status", "Status IN ('Lead','Active','Paused', 'Completed')");
  ```

**Note:** Localization of status names should be handled in the frontend/UI layer. The database always stores canonical English values.

---

## Source field

### Purpose

**Source** tells the business where a client originally came from. It is a simple but extremely valuable field for lead tracking, marketing analytics, and business growth decisions.

### Sources Descriptions

- **Manual:** Clients created directly by a consultant inside App
- **LinkedIn:** Clients acquired through LinkedIn posts, messages, profile link, ...
- **Website:** Clients who came through website contact form, landing page, booking page, ...
- **Referral:** Clients referred by previous clients, friends, colleagues, ...
- **Other:** Fallback category for Instagram, WhatsApp, Telegram, ...

### Frontend Implementation

When staff adds a new client, they select the source. If they don’t know the source, they leave it empty and backend stores `NULL`.

### Backend Implementation

- **Validation**:
  Backend ensures the value is one of the allowed options:

  ```csharp
  if (prefs.Source != null &&
      prefs.Source is not ("Manual" or "LinkedIn" or "Website" or "Referral" or "Other"))
  {
      throw new ValidationException("Invalid client source.");
  }
  ```

- **Enum in Code**:  
  In C#, the source is represented as a strongly‑typed `enum` (`ClientSource`). This prevents typos and invalid assignments at compile time.

  ```csharp
  public enum ClientSource
  {
      Manual,
      LinkedIn,
      Website,
      Referral,
      Other
  }

  public class Clinet
  {
      ...
      public ClientStatus? Source { get; set; }
  }
  ```

- **NVARCHAR in Database:**
  The enum is stored as a string (NVARCHAR(30)) for readability and reporting. EF Core maps the enum values to their string names:

  ```csharp
  modelBuilder.Property(x => x.Source)
    .HasConversion<string>()
    .HasMaxLength(30)
    .IsRequired(false);
  ```

- **CHECK Constraint for Integrity:**
  A CHECK constraint is added via EF Core to enforce that only valid source values are stored:

  ```csharp
  modelBuilder.HasCheckConstraint(
    "CK_Clients_Source",
    "[Source] IS NULL OR [Source] IN ('Manual', 'LinkedIn', 'Website', 'Referral', 'Other')");
  ```

**Note:** Localization of source names should be handled in the frontend/UI layer. The database always stores canonical English values.

---

## Manual Lead Creation

Consultants can manually add a lead directly into App when a potential client comes through personal communication, referrals, messaging apps, or offline interactions.
Manual lead creation follows the same lifecycle as public form submissions but is initiated from the consultant interface.

### Frontend Workflow

**1.Consultant opens the "Add Lead" form**

- Accessible from the Leads list or Client creation menu.

**2. Consultant enters basic lead information**

- FullName
- Email
- Phone
- Timezone

**3. Consultant submits the form**

- The frontend sends the lead data to the backend via `POST /clients`.

**4. Lead appears in the Lead list**

- `Status` is shown as `Lead`.
- Consultant can immediately continue with discovery, notes, or proposal creation.

**5. Timeline is displayed**

- The UI shows a `LeadCaptured` event with `Source = Manual`.

### Backend Workflow

**1. Create Client Record**

- Backend inserts a new row into `Clients`.
- Sets:
  - `Status = Lead`
  - `Source = Manual`
  - `CreatedAt = now`

**2. Record Timeline Event**
Backend writes a `LeadCaptured` event into `ClientTimeline`:

```json
{
  "ClientId": "guid",
  "Source": "Manual",
  "CapturedBy": "consultantId"
}
```

**3. Return Client Response**

- Backend returns the newly created client object.
- Frontend updates the Lead list.

**4. Lifecycle continues normally**

- Discovery scheduling
- Proposal creation
- Onboarding
- Sessions
- Notes

---

## Client Profile Management

It refers to maintaining and updating a client's information after they have been captured as a lead. Profile management ensures that the client's details remain accurate and relevant throughout their lifecycle.
Updates may include changes to contact information or preferences, and are always performed by the consultant.

### Workflow

**1. Update Client Profile**

Consultant updates client’s profile fields (`Fullname`, `Email`, `Phone`, `Timezone`, `Source`).

Backend receives:  
`PUT /api/clients/{clientId}/profile`

**Backend:**

- validates fields (e.g., email format, phone number format, timezone validity)
- checks that at least one field is being updated
- sets `UpdatedAt`
- applies changes to the `Clients` table
- records an event in the `ClientTimeline` table:

  ```csharp
  clientTimeline.EventType = (int)TimelineEventType.ClientProfileUpdated;
  ```

  `MetadataJson`:

  ```json
  {
    "UpdatedAt": "2026-07-30T18:59:00Z",
    "UpdatedBy": "Consultant",
    "UpdatedFields": {
      "FullName": { "Old": "John Doe", "New": "Jonathan Doe" },
      "Email": { "Old": "john@example.com", "New": "jonathan@example.com" },
      "Phone": { "Old": "+1-555-1234", "New": "+1-555-5678" },
      "Timezone": { "Old": "UTC-5", "New": "UTC+1" },
      "Source": { "Old": "Referral", "New": "LinkedIn" }
    }
  }
  ```

**2. Load Client Profile**
Frontend requests updated client profile for:

- consultant dashboard
- client timeline history
- proposal details

Backend:

- fetches latest profile from `Clients` table
- fetches timeline events from `ClientTimeline` table

---

## Client Status Management

Consultants should be able to **manually** set a client to:

- **Completed** from any status (`Lead`, `Active`, `Paused`), and this covers cases where the engagement ends naturally or is closed early.
- **Paused** only from `Active`. This reflects the real‑world scenario where only ongoing clients can be temporarily suspended.

### Workflow

Consultant changes a client’s status manually.

Backend receives:  
`PUT /api/clients/{clientId}/status`

Backend:

- validates requested status against allowed transitions
  - `Lead` | `Active` | `Paused` => `Completed` (allowed)
  - `Active` => `Paused` (allowed)
- sets `UpdatedAt`
- updates client `Status`
- records an event in the `ClientTimeline` table:
  ```csharp
  clientTimeline.EventType = (int)TimelineEventType.ClientStatusChanged;
  ```
  `MetadataJson`:
  ```json
  {
    "OldStatus": "Active", // or other options
    "NewStatus": "Paused", // or other options
    "ChangedAt": "2026-07-31T12:35:00Z",
    "ChangedBy": "Consultant",
    "TriggerEvent": "ManualStatusChange"
  }
  ```

---

# STM - Client Notes

## Overview

**Client Notes** allow consultants to record important information throughout the client’s lifecycle, including discovery insights, onboarding details, and session outcomes. All notes are stored in the `ClientNotes` table.

## Frontend Workflow

**1- Consultant opens the client profile**

- Notes are visible in a dedicated "Notes" section

**2- Consultant adds a new note**

- Writes text
- Saves the note

**3- Consultant edits an existing note**

- Opens the note
- Updates the content
- Saves changes

**4- Notes appear immediately in the UI**

- Sorted by newest first
- Displayed in the timeline
- Linked to sessions when applicable

## Backend Workflow

**1- Add Note**

- Insert a new row into `ClientNotes`
- Fields include:
  - `ClientId`
  - `ConsultantId`
  - `Content`
  - `CreatedAt`

**2- Record Timeline Event**
Backend writes a timeline entry:

- `ConsultantNoteAdded`
- Metadata includes:

  ```json
  {
    "NoteId": "guid",
    "By": "ConsultantId"
    "Preview": "string" // (first 50–100 chars)
  }
  ```

**3- Edit Note**

- Update the row in `ClientNotes`
- Write a timeline event:
  - `ConsultantNoteUpdated`
  - Metadata includes:
    ```json
    {
     "NoteId": "guid",
     "By": "ConsultantId"
     "Preview": "string" // (first 50–100 chars)
    }
    ```

**4- Return updated notes list**

- Backend returns the updated notes
- Frontend refreshes the notes section and timeline

---

# STM – Client Documents

## Overview

**Client Documents** allow consultants to upload, store, and manage files related to a client throughout their lifecycle. These may include discovery materials, signed proposals, onboarding forms, session worksheets, or any supporting documents needed for ongoing work. All documents are stored in the `ClientDocuments` table. A client can have multiple documents of the same `Type`.

## Type field

### Purpose

The **Type** column categorizes each uploaded document. So the App can:
group documents in the UI (tabs or filters), help consultants quickly find the right file, support workflows.

### Types Descriptions

- **Discovery:** Documents created or used during the discovery stage, such as worksheets, notes, or initial assessments.
- **Proposal:** Files related to packages and proposals, including drafts, attachments, and signed versions.
- **Onboarding:** Documents required for onboarding, such as forms, checklists, or client‑submitted information.
- **Session:** Materials used or produced during coaching sessions, including worksheets, recordings, and session attachments.
- **Contract:** Formal agreements between the consultant and the client, including signed contracts or service terms.
- **Invoice:** Billing documents issued to the client for services or packages.
- **Receipt:** Proof of payment provided to the client after an invoice is paid.
- **General:** Any other client‑related documents that do not belong to a specific lifecycle stage or category.

### Implementation

To ensure clarity, safety, and consistency across code and database layers, the following design is used:

- **Enum in Code**:  
  In C#, the type is represented as a strongly‑typed `enum` (`ClientDocumentType`). This prevents typos and invalid assignments at compile time.

  ```csharp
  public enum ClientDocumentType
  {
      Dicovery,
      Proposal,
      Onboarding,
      Session,
      Contract,
      Invoice,
      Receipt,
      General
  }

  public class ClinetDocument
  {
      ...
      public ClientDocumentType Type { get; set; }
  }
  ```

- **NVARCHAR in Database:**
  The enum is stored as a string (NVARCHAR(50)) for readability and reporting. EF Core maps the enum values to their string names:

  ```csharp
  modelBuilder.Entity<ClientDocument>()
      .Property(cd => cd.Type)
      .HasConversion<string>()   // store enum as NVARCHAR
      .HasMaxLength(50);
  ```

- **CHECK Constraint for Integrity:**
  A CHECK constraint is added via EF Core to enforce that only valid type values are stored:

  ```csharp
  modelBuilder.Entity<ClientDocument>()
      .HasCheckConstraint("CK_ClientDocuments_Type", "Type IN ('Dicovery', 'Proposal', 'Onboarding', 'Session', 'Contract', 'Invoice', 'Receipt', 'General')");
  ```

**Note:** Localization of type names should be handled in the frontend/UI layer. The database always stores canonical English values.

## Frontend Workflow

**1- Consultant opens the client profile**

- The "Documents" section displays all files previously uploaded for the client.

**2- Consultant uploads a new document**

- Select `Type` of file
- Selects a file
- Submits the upload form

**3- Document appears in the client’s document list**

- Shows filename, upload date, consultant name, and description
- Can be downloaded or previewed depending on file type

**4- Consultant updates or replaces a document**

- Opens the document
- Uploads a new version or change the `Type`
- Saves changes

**5- Consultant deletes a document**

- Removes the file from the client's document list
- Timeline reflects the deletion event

## Backend Workflow

**1- Upload Document**

- Stores the file in the configured storage (local or cloud)

- Creates a row in `ClientDocuments` with:
  - `Type`
  - `ClientId`
  - `FileUrl`
  - `UploadedAt`

**2- Record Timeline Event**
Writes a timeline entry:

- ConsultantDocumentUploaded
- Metadata includes:
  ```json
  {
    "DocumentId": "guid",
    "Type": "string"
  }
  ```

**3- Update Document**

- Updates `Type` or replaces the stored file
- Writes a timeline event:
  - `ConsultantDocumentUpdated`

**4- Delete Document**

- Removes the file from storage
- Deletes the row from `ClientDocuments`
- Writes a timeline event:
  - `ConsultantDocumentDeleted`

**5- Return updated document list**

- Backend returns the full list of documents
- Frontend refreshes the documents section and timeline

---

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

---

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

---

# STM – JWT Token

## Overview

STM uses a dual‑token authentication model:

- **Access Token:** short‑lived, stored in memory, used for API authorization

- **Refresh Token:** long‑lived, stored in an HttpOnly, Secure cookie, used to obtain new access tokens

Refresh tokens are tied to **DeviceId**, enabling multi‑device login, per‑device logout, and accurate session tracking.

Signin is fully passwordless. PasswordHash is stored for future privileged actions but is not used in JWT authentication.

---

## Access Token

### Purpose

Authorize API calls. Never stored in cookies or localStorage.

### Storage

- In-memory (Angular service)

### Lifetime

- 15 minutes

### Contents

- `UserId`
- `TenantId`
- `Role`
- `permissions`
- `exp` (expiration timestamp)
- `iat` (issued at)

### Behavior

- Sent in `Authorization: Bearer <token>` header
- When expired => frontend automatically calls `/auth/refresh-token`

---

## Refresh Token

### Purpose

Issue new access tokens and maintain persistent login.

### Storage

- HttpOnly cookie (backend sets it)
- Secure (HTTPS only)
- SameSite=Strict (CSRF protection)

### Lifetime

- 30 days

### Device Binding

Refresh tokens are tied to:

- `UserId`
- `DeviceId`
- `TokenHash`

This ensures each device has its own independent session.

---

## Refresh Token Rotation

Every refresh request performs:

- Validate old refresh token
- Insert new refresh token (new TokenHash)
- Delete old refresh token
- Update `UserSessions.LastActivityAt`
- Return new access token
- Set new HttpOnly cookie

This prevents token theft and replay attacks.

---

## UserSessions Integration

Each refresh token belongs to a session, represented in `UserSessions`.

### When fields update in UserSessions table

- `CreatedAt` => on login
- `LastActivityAt` => on any authenticated API call or refresh
- `IsActive = 0` => on logout, password change, suspicious activity, or token expiration

---

### Security Considerations

- Refresh token stored in HttpOnly cookie => not accessible by JS
- Access token stored in memory => safe from XSS
- `DeviceId` prevents token replay
- `TokenHash` prevents token theft
- `IpAddress` + `UserAgent` stored for auditing
- `UserLoginAttempts` logs signin attempts for auditing and future security features, but is **not used** for authentication decisions in the current OTP‑based signin model.

---

# STM – Lead Capture

## Overview

Lead Capture is the entry point of the client lifecycle. It is the process through which potential clients provide their initial information to the consultant using a customizable intake form.

Lead Capture consists of two main components:

1. **Lead Forms:** Customizable intake forms created by the `Admin` or `Consultant`, and stored in the `LeadForms` table.
2. **Lead Form Submissions:** Every time a potential client fills out a Lead Form, and a submission record will be created. They are stored in the `LeadFormSubmissions` table.

---

## Workflow Summary

1. **Consultant creates a Lead Form**
   - The consultant defines: title, description, dynamic fields, active/inactive status.
   - App generates a public URL for the form.

2. **Consultant shares the form**
   - The form can be:
     - embedded on a website
     - shared on LinkedIn
     - sent via email
     - included in booking flows
   - This becomes the consultant’s "front door" for new clients.

3. **Lead submits the form**
   - A potential client fills out the form and submits it.
   - App:
     - stores the submission
     - triggers notifications
     - shows the lead in the dashboard

4. **Consultant reviews the submission**
   - The consultant reads:
     - contact details
     - answers
     - resume (if uploaded)
5. **Consultant schedules a Discovery Call**
   - If the lead looks promising:
     - a Discovery Call is created
     - calendar integration syncs the event
     - the lead receives confirmation

---

## 1) Creating a Lead Form

A Lead Form is created by a user with the role of `Consultant` or `Admin`. This form becomes the public intake form that potential clients use to submit their information.

**1-1) Angular UI Flow for Creating Lead Forms**

- The `LeadFormCreateComponent` contains metadata fields:
  - title (input, required)
  - description (textarea, optional)
  - isActive (toggle, default: true)

- The `LeadFormFieldBuilderComponent` is a dynamic field builder. Consultant or admin:
  - Add new fields
  - Choose type for each field: text, textarea, number, select, checkbox, file upload
  - Set for each field: label, placeholder, required flag, validation rules
  - Fields are stored in a local array:
    ```typescript
     formFields: LeadFormField[] = [];
    ```
    Example:
    ```typescript
    [
      {
        id: "fullName",
        label: "Full Name",
        type: "text",
        required: true,
      },
      {
        id: "email",
        label: "Email",
        type: "email",
        required: true,
      },
    ];
    ```
- Save and send to backend:
  - Build DTO:
    ```typescript
    const dto = {
      title: this.form.title,
      description: this.form.description,
      isActive: this.form.isActive,
      fieldsJson: JSON.stringify(this.formFields),
    };
    ```
  - Call API
- UX Considerations:
  - Auto-focus on title field
  - Drag-and-drop ordering of fields
  - Preview mode for form
  - Copy-to-clipboard for public URL
  - Toggle to activate/deactivate form

**1-2) Backend Flow for creating Lead Form**

- Validate:
  - Title is required
  - FieldsJson is a valid JSON array
  - TenantId is extracted from JWT
- Create a new LeadForms record
- Generate a public URL using the LeadForm's `Id` and return it:
  ```csharp
  https://stm.app/form/{LeadFormId}
  ```

## 2) Public Form Access

Public Form Access allows potential clients to open and submit Lead Forms without authentication. This is the entry point of the Lead Capture workflow. App exposes a secure public endpoint that serves the form definition and accepts submissions.

### 2-1) Backend Workflow

Public Form Access is handled through two backend endpoints:

1. **GET /api/public/lead-forms/{id}**
   Returns the form definition (metadata + dynamic fields).

2. **POST /api/public/lead-forms/{id}**  
   Accepts and stores lead submissions.

Both endpoints operate without authentication and rely on the LeadForm’s `Id` and `IsActive` status for access control.

**GET Public Form Definition**
Serve the Lead Form definition to the frontend so it can render the form dynamically.

- Validate that:
  - The Lead Form exists.
  - `IsActive = true`.
  - The tenant owning the form is active.

- Load the form:
  - Title
  - Description
  - FieldsJson (dynamic fields)
  - TenantId

- Return JSON response:
  ```json
  {
    "id": "GUID",
    "title": "Career Coaching Intake Form",
    "description": "Please fill out the following information.",
    "fieldsJson": [ ... ]
  }
  ```

**POST Public Form Submission**
Store lead submissions and trigger notifications.

- Validate:
  - Form exists and is active.
  - Required fields match FieldsJson.
  - Email format (if provided).
  - File uploads (if any).
- Create a new LeadFormSubmissions record
- Trigger notifications:
  - Email to consultant.
  - In-app notification.
- Record an event in the `ClientTimeline` table
  - `clientTimeline.EventType = (int)TimelineEventType.LeadCaptured;`
  - MetadataJson:

    ```json
    {
      "ClientId": "guid",
      "Source": "LeadForm",
      "CapturedBy": "System",
      "FormName": "string"
    }
    ```

    - **Source:** Where did this event come from?
    - **CapturedBy: "System"** means the client submitted the form not a consultant
    - **FormName:** Comes from `leadForm.Title`

- Return:
  ```json
  {
    "submissionId": "GUID",
    "status": "success"
  }
  ```

### 2-2) Frontend Workflow

The frontend renders public Lead Forms using the form definition returned by the backend. It dynamically builds the UI based on `FieldsJson` and submits answers back to the backend.

Public Form Access is handled through a dedicated route:

```code
/form/:id
```

**Angular Route Definition**

```typescript
{
  path: 'form/:id',
  component: PublicLeadFormComponent
}
```

**Component: PublicLeadFormComponent**

- Fetch form definition from backend.
  - Extract `id` from route.
  - Call `GET /api/public/lead-forms/{id}`
- If form is inactive or not found, show error page.
- Render dynamic fields.
- Handle validation.
- Submit form data and call: `POST /api/public/lead-forms/{id}`
- Show success or error messages.

## 3) Trigger Notifications

When a potential client submits a Lead Form, App automatically triggers notifications to inform the consultant. Notifications ensure that consultants respond quickly to new leads and maintain a smooth intake workflow.

### 3-1) Backend Workflow

After a Lead Form submission is stored in `LeadFormSubmissions`, the backend triggers one or more notifications. Notifications are triggered asynchronously to avoid slowing down the submission process.

**Load Consultant Notification Preferences**
This ensures notifications are personalized, and fetches:

- Consultant’s email
- Notification settings from `UserSettings`
- Consultant timezone for formatting timestamps from `UserSettings`
- Tenant name from `Tenants`

**Build Notification Payload**
The backend constructs a payload containing:

```json
{
  "consultantId": "GUID",
  "tenantId": "GUID",
  "leadFormSubmissionId": "GUID",
  "leadName": "John Doe",
  "leadEmail": "john@example.com",
  "submittedAt": "2026-07-14T15:23:00Z",
  "summary": {
    "careerGoals": "Become a senior developer",
    "experience": "3 years"
  }
}
```

**Send Email Notification**

- Render email template
- Inject tenant branding (e.g., name)
- Send email via SMTP or email provider
- Log notification event

**Create In-App Notification**
Backend inserts a record into `Notifications` table. It appears in the consultant’s notification center.

### 3-2) Frontend Workflow

The frontend does not send notifications itself. Instead, it reacts to backend-triggered notifications:

- Shows success message after submission
- Displays in-app notifications in the consultant dashboard
- Updates notification badge count

**Public Form Submission**
After submitting the form, frontend shows: "Thank you for your submission".

**Consultant Dashboard Notification Badge**
When consultant logs in, frontend updates:

- Notification bell icon count
- Notification dropdown list

**Notification Center**
It displays:

- Title: "New Lead Submission"
- Message: "John Doe submitted a lead form.""
- Timestamp
- "View Submission" button

## 4) Lead Appears in Dashboard

After a potential client submits a Lead Form, App must display the new lead inside the consultant’s dashboard.
The consultant opens a specific lead submission inside App to evaluate the lead, read their answers, and decide the next action (e.g., scheduling a Discovery Call).

### 4-1) Backend Workflow

Once a submission is stored in `LeadFormSubmissions`, the backend exposes endpoints that allow the consultant to view:

- A list of all leads
- Details of a specific lead
- Submission metadata
- Dynamic answers mapped to field definitions

**Dashboard Loads Lead List**

- Extract `tenantId` from JWT.
- Query all submissions belonging to that tenant
- Return a lightweight list. For example:
  ```json
  [
    {
      "id": "GUID",
      "fullName": "John Doe",
      "email": "john@example.com",
      "submittedAt": "2026-07-14T15:23:00Z",
      "leadFormTitle": "Career Coaching Intake Form",
      "status": "New"
    }
  ]
  ```
  **Notes**
  - Backend does not return full answers and only summary fields are returned for performance.
  - The backend derives `status` and returns it to the frontend.

**Dashboard Loads Lead Details**

- Validate:
  - Submission exists.
  - Submission belongs to the consultant’s tenant.
- Load:
  - Submission record
  - LeadForm metadata
  - FieldsJson (dynamic field definitions)
- Map dynamic answers and return it:

  ```json
  {
    "id": "GUID"
    "fullName": "John Doe",
    "email": "john@example.com",
    "answers": [
      { "label": "Career Goals", "value": "Become a senior developer" },
      { "label": "Experience", "value": "3 years" }
    ],
    "status": "New",
     "availableActions": ["ScheduleDiscoveryCall"], // This also could be ["MarkQualified", "MarkNotQualified","MarkNoShow"] or []
  }
  ```

  **Note:** `availableActions` should be returned because the backend is the only reliable source of truth for a lead’s lifecycle state, and the frontend uses it to decide which action buttons should be shown or hidden.

**Lead Status Management**
The lead’s status is computed dynamically and appears in the dashboard list based on:

- whether a DiscoveryCall exists
- whether the DiscoveryCall has an outcome

When the dashboard loads leads, the backend checks:

- **Does the submission have a `DiscoveryCall`?**
  - If yes: status = `DiscoveryScheduled`
  - If no: status = `New`

- **Does the `DiscoveryCall` have an Outcome?**
  - Qualified: `Qualified`
  - NotQualified: `NotQualified`
  - NoShow: `NoShow`

When frontend calls `POST /api/lead-submissions/{id}/status`, while the received status is:

- Schedule Discovery Call:
  Backend creates a new `DiscoveryCalls` record: status becomes DiscoveryScheduled (derived)
- Mark Qualified:
  Backend updates `DiscoveryCalls.Outcome = 'Qualified'`: status becomes Qualified (derived)
- Mark Not Qualified:
  Backend updates `DiscoveryCalls.Outcome = 'NotQualified'`: status becomes NotQualified (derived)
- Mark No Show:
  Backend updates `DiscoveryCalls.Outcome = 'NoShow'`: status becomes NoShow (derived)

### 4-2) Frontend Workflow

The Angular dashboard displays leads using two components:

- `LeadListComponent`
- `LeadDetailsComponent`

The frontend fetches data from backend endpoints and renders:

- Lead list
- Lead details
- Dynamic answers
- Status indicators

**Load Lead List**
On the init of the `LeadListComponent` component, fetch the leads list from backend and:

- Display
  - Full name
  - Email
  - Submission date
  - Form title
  - Status (derived by backend)

- Sorting
  - Default: newest first
  - Optional: sort by name or form

- Filtering
  - By form
  - By status
  - By date range

**Load Lead Details**
Clicking a lead in the list navigates to details page: `/leads/submissions/{id}`

In the `LeadDetailsComponent` component:

- Extract `id` from route, and call: `GET /api/lead-submissions/{id}`
- Render:
  - Contact info
  - Submission timestamp
  - Form title
  - Dynamic answers (loop through mapped fields)
  - Lead actions (buttons):
    - Schedule Discovery Call (only available if NO Discovery Call exists yet)
    - Mark as Qualified (only available If a Discovery Call exists AND Outcome is NULL)
    - Mark as Not Qualified (only available If a Discovery Call exists AND Outcome is NULL)

**Status Updates**
When consultant performs actions (click on a button), frontend calls: `POST /api/lead-submissions/{id}/status`, and dashboard list updates automatically.

## 5) Consultant Schedules Discovery Call

This is the action a consultant performs after reviewing a lead submission, and a transition from Lead Capture to Discovery Stage.

### 5-1) Frontend Workflow

**Consultant clicks "Schedule Discovery Call"**
A new form (modal or page) should open. It is a Discovery Call scheduling form. Scheduling a Discovery Call requires input from the consultant, such as:

- date (Date picker)
- time (Time picker)
- duration (Duration selector)
- optional notes
- optional attendee email (lead’s email)

The consultant must fill them.

**Frontend builds DTO:**

```json
{
  "leadFormSubmissionId": "GUID",
  "scheduledAt": "2026-07-20T14:00:00Z",
  "durationMinutes": 45
}
```

And sends request: `POST /api/discovery-calls`

**Frontend receives updated lifecycle state:**

```json
{
  "status": "DiscoveryScheduled",
  "availableActions": ["MarkQualified", "MarkNotQualified"]
}
```

and updates UI:

- Status badge changes
- New buttons appear
- Scheduling button disappears

### 5-2) Backend Workflow

**Validation steps:**

- Submission exists
- Submission belongs to consultant’s tenant
- No existing Discovery Call for this submission
- Scheduled time is valid
- Consultant has calendar integration configured

**Creating Discovery Call record:**
Insert into `DiscoveryCalls`. This marks the lead as `DiscoveryScheduled` (derived status).

**Syncing with calendar provider:**

- Build calendar event payload:
  - Title: "Discovery Call with John Doe"
  - Start time
  - End time
  - Consultant timezone
  - Lead email (optional attendee)
- Send event to Google/Otulook Calendar
- Store external event ID in `DiscoveryCalls` table for future updates/cancellations.

**Record an event in the `ClientTimeline` table**

```csharp
clientTimeline.EventType = (int)TimelineEventType.DiscoveryCallScheduled;
```

`MetadataJson`:

```json
{
  "DiscoveryCallId": "guid",
  "ScheduledBy": "Consultant",
  "ScheduledAt": "2025-01-01T10:00:00Z",
  "ClientTimezone": "America/New_York",
  "ConsultantTimezone": "Asia/Tehran",
  "Method": "Online",
  "Notes": "string"
}
```

---

# STM – Onboarding Task Templates

## Overview

**Onboarding Task Templates** define reusable onboarding steps that consultants can apply automatically when a proposal is accepted.
They allow each consultant to design their own onboarding workflow, independent of other consultants in the same tenant.

---

## Workflow

### 1. Load Templates

Consultant opens the template configuration page.

Frontend requests: `GET /api/onboarding-task-templates`

Backend:

- Reads consultantId from JWT.
- Returns all templates for that consultant, ordered by `Order ASC`.

### 2. Bulk Update Template List

Consultant can:

- add new templates
- delete existing templates
- reorder templates

Frontend sends the entire updated list: `PUT /api/onboarding-task-templates/bulk-update`

Backend:

- inserts templates with `id = null`
- deletes templates missing from payload
- updates `Order` for all templates
- returns updated list
- performs all changes in a single transaction

Bulk update handles list-level changes only. Editing template details is separate.

### 3. Edit a Specific Template

Consultant clicks a template to modify its details.

Frontend sends: `PUT /api/onboarding-task-templates/{templateId}`

Backend updates:

- Title
- Description
- PackageId
- UpdatedAt

---

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

---

# STM – Packages

## Overview

The **Packages** table stores the coaching/consulting offerings defined by each tenant (consultant).
Packages are the foundation of App's sales and onboarding workflow. They define what consultants sell, how proposals are created, and how clients are onboarded.
Packages are used when creating Proposals, which are sent to clients after they are **Qualified**.

---

## Workflow

### 1. Consultant Creates a Package

In Frontend, consultant opens **Settings => Packages** and clicks **Create Package**.

Form fields:

- Name
- Description
- Price
- SessionsIncluded

Frontend sends `POST /api/packages`. Payload is:

```json
{
  "name": "Career Accelerator",
  "description": "A 6‑week coaching program focused on career transitions.",
  "price": 250,
  "sessionsIncluded": 6
}
```

Backend:

- Validates fields
- Ensures Price >= 0
- Ensures SessionsIncluded >= 0
- Creates the package
- Record an event in the `ClientTimeline` table
  ```csharp
   clientTimeline.EventType = (int)TimelineEventType.PackageCreated;
  ```
  `MetadataJson`:
  ```json
  {
    "PackageId": "guid",
    "CreatedBy": "Consultant",
    "CreatedAt": "2025-01-01T12:00:00Z",
    "PackageName": "string",
    "Price": 0
  }
  ```
- Returns the new package

### 2. Consultant Views Packages

Frontend loads the list of packages for the tenant by calling: `GET /api/packages`.

Backend returns all packages belonging to the tenant, sorted by `CreatedAt DESC`.

Frontend displays:

- Name
- Price
- SessionsIncluded
- Edit/Delete actions

### 3. Consultant Edits a Package

In Frontend, consultant clicks **Edit**.

Frontend sends:
`PUT /api/packages/{id}`.
Payload includes updated fields.

Backend:

- Validates changes
- Updates the record
- Record an event in the `ClientTimeline` table
  ```csharp
   clientTimeline.EventType = (int)TimelineEventType.PackageUpdated;
  ```
  `MetadataJson`:
  ```json
  {
    "PackageId": "guid",
    "UpdatedBy": "Consultant",
    "UpdatedAt": "2025-01-01T12:00:00Z",
    "PackageName": "string"
  }
  ```
- Returns updated package

**Note**: Editing a package does not affect existing proposals. Proposals store the package ID but rely on the package’s current values only at creation time.

### 4. Consultant Deletes a Package

In frontend, consultant clicks **Delete**.

Frontend sends:
`DELETE /api/packages/{id}`.

Backend:

- Checks if the package is used in any proposals
- If used => reject deletion
- If unused => delete the record
  - Record an event in the `ClientTimeline` table
    ```csharp
    clientTimeline.EventType = (int)TimelineEventType.PackageDeleted;
    ```
    `MetadataJson`:
    ```json
    {
      "PackageId": "guid",
      "DeletedBy": "Consultant",
      "DeletedAt": "2025-01-01T12:00:00Z",
      "PackageName": "string"
    }
    ```

### 5. Using Packages in Proposals

When creating a proposal, the frontend loads available packages:
`GET /api/packages`

Consultant selects one package. Frontend sends:
`POST /api/proposals`

Payload:

```json
{
  "clientId": "...",
  "packageId": "...",
  "notes": "Recommended based on your goals."
}
```

Backend:

- Validates the package exists
- Validates the client belongs to the tenant
- Creates a proposal with `Status = Draft`
- Returns the proposal

---

# STM – Proposals

## Overview

The **Proposals** table represents the formal offer sent to a client after they are `Qualified` in the Discovery Call workflow.
A proposal contains a snapshot of the selected package at the moment of creation, ensuring that future edits to the package do not affect previously created proposals.

---

## Status field

### Purpose

It represents the lifecycle of a proposal. Proposals move through a simple lifecycle:

- **Draft:** created but not sent
- **Sent:** delivered to the client
- **Accepted:** client agrees to the offer
- **Rejected:** client declines the offer

### Implementation

**Status** is implemented as a C# enum (`ProposalStatus`) and persisted as an `nvarchar(20)` column using EF Core value conversion.
This follows the same pattern used for the `Status` field in the `Clients` table.
The database stores human‑readable values (`Draft`, `Sent`, `Accepted`, `Rejected`) and enforces validity using a SQL CHECK constraint.

---

## Packages' Snapshot fields

### Purpose

Snapshot fields represent the **exact offer** the client received at that moment. They must NOT be editable after the proposal is created.

When creating a proposal, the backend loads the package and **copies** them into the proposal. This is the default behavior.

Consultant can override snapshot fields during creation:

- Consultant wants to give a discount
- Consultant wants to rename the package for this client
- Consultant wants to customize the description
- Consultant wants to add bonus sessions
- Consultant wants to create a special offer

**Notes:**

- It only modifies the proposal snapshot.
- Snapshot fields cannot change after creation.

---

## IsActive field

### Purpose

**IsActive** field is meaningful _only_ for proposals whose status is `Accepted`, and all other statuses must have `IsActive = false`.

A client can have:

- multiple **Draft** proposals
- multiple **Sent** proposals
- multiple **Accepted** proposals (e.g., renewals)
- multiple **Rejected** proposals

Because:

- A client may receive **different package offers** over time.
- A client may reject one proposal and later accept another.
- A consultant may send a **new proposal** after updating their packages.
- A client may upgrade from a smaller package to a larger one.
- A consultant may send a proposal for **renewal** after the first engagement ends.

So a client can have multiple accepted proposals historically, but **the last accepted proposal is the active one.**

When a new proposal is accepted:

- backend sets the new proposal → `IsActive = true`
- backend sets all previous accepted proposals → `IsActive = false`

---

## Backend Workflow

### 1. Consultant creates a proposal

Backend receives:

```json
{
  "clientId": "GUID",
  "packageId": "GUID",
  "notes": "Optional notes"
}
```

Backend performs:

- Validate client belongs to tenant
- Validate package belongs to tenant
- Load package
- Copy package snapshot fields
- Create proposal with: `Status = Draft`
- Record an event in the `ClientTimeline` table
  ```csharp
   clientTimeline.EventType = (int)TimelineEventType.ProposalCreated;
  ```
  `MetadataJson`:
  ```json
  {
    "ProposalId": "guid",
    "CreatedBy": "Consultant",
    "CreatedAt": "2025-01-01T13:00:00Z",
    "PackageId": "guid",
    "PackageName": "string",
    "PackagePrice": 0,
    "Notes": "string"
  }
  ```

### 2. Consultant sends the proposal

Frontend calls: `POST /api/proposals/{id}/send`

Backend:

- sets `Status = Sent`
- sets `SentAt`
- Record an event in the `ClientTimeline` table
  ```csharp
   clientTimeline.EventType = (int)TimelineEventType.ProposalSent;
  ```
  `MetadataJson`:
  ```json
  {
    "ProposalId": "guid",
    "SentAt": "2025-01-01T13:00:00Z"
  }
  ```

### 3. Client accepts the proposal

Frontend calls: `POST /api/proposals/{id}/accept`

Backend:

- sets `Status = Accepted`
- sets `AcceptedAt`
- Record an event in the `ClientTimeline` table
  ```csharp
   clientTimeline.EventType = (int)TimelineEventType.ProposalAccepted;
  ```
  `MetadataJson`:
  ```json
  {
    "ProposalId": "guid",
    "RespondAt": "2025-01-01T13:00:00Z"
  }
  ```
- Triggers onboarding workflow
- Record an event in the `ClientTimeline` table
  ```csharp
   clientTimeline.EventType = (int)TimelineEventType.OnboardingStarted;
  ```
  `MetadataJson`:
  ```json
  {
    "ProposalId": "guid",
    "StartedAt": "2025-01-01T13:00:00Z"
  }
  ```

### 4. Client rejects the proposal

Frontend calls: `POST /api/proposals/{id}/reject`

Backend:

- Sets `Status = Rejected`
- Record an event in the `ClientTimeline` table
  ```csharp
   clientTimeline.EventType = (int)TimelineEventType.ProposalRejected;
  ```
  `MetadataJson`:
  ```json
  {
    "ProposalId": "guid",
    "RespondAt": "2025-01-01T13:00:00Z"
  }
  ```

---

## Frontend Workflow

### 1. Consultant opens a Qualified client

Frontend loads:

```
GET /api/packages
GET /api/clients/{id}/proposals
```

UI shows:

- list of packages
- "Create Proposal" button
- existing proposals

### 2. Consultant creates a proposal

Frontend sends: `POST /api/proposals`

UI:

- shows the new proposal in "Draft" state
- displays snapshot fields
- enables "Send Proposal" button

### 3. Consultant sends the proposal

Frontend calls: `POST /api/proposals/{id}/send`

UI updates:

- status badge → Sent
- shows "Awaiting Client Decision"
- disables editing snapshot fields

### 4. Client accepts or rejects

Frontend calls:

```
POST /api/proposals/{id}/accept
or
POST /api/proposals/{id}/reject
```

UI updates:

- Accepted → "Next Step: Onboard Client"
- Rejected → "Proposal Declined"

---

# STM – Refresh Tokens

## Overview

Refresh tokens are long‑lived credentials that allow users to obtain new access tokens without logging in again.
This App uses a secure, database‑backed refresh‑token system with:

- Physical deletion of invalid tokens
- Rotation on every use
- Device‑bound tokens
- Strict expiration rules
- Background cleanup jobs

This ensures strong security, prevents replay attacks, and keeps the authentication system fast and predictable.

---

## Refresh Token Creation

### Purpose

A refresh token is issued to allow the user to renew their session after the access token expires.

**Characteristics**

- Lifetime: 7–30 days
- Stored in the database (hashed)
- Bound to:
  - User ID
  - Device
- Issued only after OTP verification.

**When created**

- User logs in
- User completes OTP verification
- User re-authenticates after session expiration

---

## Refresh Token Rotation

### What is rotation?

Every time a refresh token is used:

1.  Backend validates the token
2.  A new access token is issued
3.  A new refresh token is generated
4.  The old refresh token is physically deleted

### Why rotation?

- Prevents replay attacks
- Limits the window of token theft
- Ensures only one valid refresh token per device

---

## Refresh Token Validation

### Validation checks

When the frontend calls: `POST /auth/refresh-token`

Backend verifies:

- Token exists in DB
- Token is not expired
- Token belongs to the correct user
- Token matches the device
- Token hash matches the stored hash

### Failure cases

If any check fails:

- Token is deleted (if expired)
- Request is rejected
- User must log in again

---

## Refresh Token Deletion

### Purpose

App uses physical deletion to ensure invalid tokens never remain in the database.

#### When tokens are deleted

**1. On rotation**
Old token is deleted immediately after issuing a new one.

**2. On expiration**
If `ExpiresAt < now`, token is deleted:

- during validation
- during background cleanup job

**3. On logout (single device)**
Delete the refresh token for that device.

**4. On logout from all devices**
Delete all refresh tokens for the user.

**5. On suspicious activity**
Delete all tokens for the user to force full re-authentication.

**6. On email change**
Delete all tokens when sensitive account information changes (e.g., email change, future privileged actions)

#### Why physical deletion?

- Keeps the table small
- Avoids index bloat
- Ensures only valid tokens exist in DB

---

## Background Cleanup Job

### Purpose

Some tokens expire without ever being used again.
To prevent database growth, App runs a scheduled cleanup job.

### Daily job

- Deletes all tokens where `ExpiresAt < now`
- Removes tokens older than a retention threshold (optional)

### Benefits

- Keeps DB small and fast
- Prevents stale tokens

---

## Logout Flow

### Single-device logout

- Delete refresh token for that device
- Access token naturally expires in 15 minutes

### Logout from all devices

- Delete all refresh tokens for the user
- Access tokens expire automatically

---

## Security Considerations

### 1. Store refresh tokens hashed

- Never store raw tokens.
- Use a secure hashing algorithm (SHA256 or stronger).

### 2. Use HttpOnly cookies

It is stored in an HttpOnly, Secure cookie set by the backend:

- The frontend cannot read or modify this cookie
- The browser automatically sends it with requests

### 3. Enforce strict expiration

- Expired tokens must be deleted immediately.

### 4. Invalidate tokens on sensitive changes

Delete all tokens when:

- Email changes
- Suspicious login detected

### 5. Device binding

Each device receives its own refresh token. This enables:

- Per-device logout
- Per-device revocation
- Better security auditing

---

# STM – Roles & Permissions

## Overview

App uses a role‑based authorization model with optional fine‑grained permissions. Authorization is enforced through:

- **Access Token (JWT)** => contains `Role`, `TenantId`, and `Permissions`
- Backend API policies
- Frontend route guards & feature flags

Roles define _who_ the user is.
Permissions define _what_ the user can do.

---

## Roles

App currently supports **3 roles**:

### 1. Admin

Tenant owner and full‑access user.

Admins can:

- Manage tenant settings
- Manage billing
- Create staff (Consultants, Assistants)
- Manage clients
- Manage sessions
- Manage documents
- View analytics
- Access all data within the tenant

Admins authenticate using OTP, not password.
PasswordHash is stored for future privileged actions.

### 2. Consultant

Primary service provider within the tenant.

Consultants can:

- Manage their assigned clients
- Create and manage sessions
- Upload and manage client documents
- View client progress
- View analytics related to their clients

Consultants **cannot**:

- Manage tenant settings
- Manage billing
- Create staff
- Access other consultants' clients (unless explicitly assigned)

### 3. Assistant

Support role with limited access.

Assistants can:

- View assigned clients
- Schedule sessions
- Upload documents
- Manage basic client information

Assistants **cannot**:

- Manage tenant settings
- Manage billing
- Create staff
- View analytics
- Access consultant‑only features

---

## Permissions

Permissions allow App to grow beyond simple role definitions.

Each permission is a boolean flag included in the JWT:

```csharp
public List<string> Permissions { get; set; }
```

It works perfectly with JWT and ASP. NET Authorization Policies:

```json
"permissions": ["ManageClients", "ViewSessions"]
```

```csharp
options.AddPolicy("ManageClients", policy =>
    policy.RequireClaim("permissions", "ManageClients"));
);
```

<br />

### Role / Permission Matrix

| Permission      | Description                                 | Admin | Consultant | Assistant |
| --------------- | ------------------------------------------- | :---: | :--------: | :-------: |
| ManageClients   | Create, update, delete clients              |   ✔   |     ✔      |    ❌     |
| ViewClients     | View client profiles                        |   ✔   |     ✔      |     ✔     |
| ManageSessions  | Create, update, delete sessions             |   ✔   |     ✔      |     ✔     |
| ViewSessions    | View session history                        |   ✔   |     ✔      |     ✔     |
| ManageDocuments | Upload, delete, and manage client documents |   ✔   |     ✔      |     ✔     |
| ViewDocuments   | View client documents                       |   ✔   |     ✔      |     ✔     |
| ManageStaff     | Create and manage consultants/assistants    |   ✔   |     ❌     |    ❌     |
| ManageTenant    | Update tenant settings                      |   ✔   |     ❌     |    ❌     |
| ManageBilling   | Access billing and subscription settings    |   ✔   |     ❌     |    ❌     |
| ViewAnalytics   | Access analytics dashboards                 |   ✔   |     ✔      |    ❌     |

---

## JWT Integration

The Access Token includes:

```json
{
  UserId,
  TenantId,
  Role,
  permissions: [...],
  exp,
  iat
}
```

Backend authorization uses:

- `[Authorize(Roles = "Admin")]`
- `[Authorize(Permissions = "ManageClients")]`

Frontend uses:

- Angular route guards
- Feature flags
- Conditional rendering

---

## Multi‑Tenant Isolation

All roles and permissions are scoped to a single tenant:

- A user belongs to exactly one tenant
- A user cannot access data from other tenants
- JWT contains `TenantId`
- Backend enforces tenant isolation on every request

---

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

---

# STM – Session Reminders

## Overview

**Session reminders** notify consultants before a session starts.
The reminder system uses **Hangfire** as a background job scheduler to schedule, reschedule, and cancel reminder jobs reliably.

The `ReminderJobId` field in the `Sessions` table stores the Hangfire job identifier for the scheduled reminder.

**Why a background job is required:**

- Reminders must fire at a specific future time
- They must run even if the user is offline
- They must survive app restarts
- They must be cancellable and reschedulable
- They must run independently of API requests

---

## Hangfire Library

### Overview

**Hangfire** is a background job processing library for .NET that provides:

- precise scheduling
- persistent job storage
- automatic retries
- distributed execution
- a built‑in monitoring dashboard

Hangfire stores scheduled jobs in its own internal tables (SQL Server, PostgreSQL, Redis), ensuring reminders are reliable even if the application restarts.

This makes Hangfire ideal for session reminders.

### Installation & Configuration

#### 1. Install Hangfire

Install the required NuGet packages:

```bash
dotnet add package Hangfire
dotnet add package Hangfire.AspNetCore
dotnet add package Hangfire.SqlServer
```

#### 2. Configure Hangfire in Program.cs

a) Add Hangfire services:

```csharp
builder.Services.AddHangfire(config =>
{
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"));
});
```

b) Add Hangfire server:

```csharp
builder.Services.AddHangfireServer();
```

c) Add Hangfire dashboard (optional but recommended):

```csharp
app.UseHangfireDashboard("/hangfire");
```

This gives you a UI to monitor reminders, retries, failures, and scheduled jobs.

#### 3. Create Database

You create an empty database: STM_Hangfire for example, and Hangfire automatically creates all required tables. This rule applies to both development and production.

When your app starts and Hangfire connects to the database, it checks:

- whether the Hangfire schema exists
- whether the required tables exist
- whether the schema version matches

If not, Hangfire **automatically creates** them.

#### 4. Add Hangfire Connection String

In `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "HangfireConnection": "Server=.;Database=STM_Hangfire;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

---

### Workflow

#### 1. Schedule Reminder Jobs

This method is defined in `ReminderService.cs`, and is called after a session is created:

```csharp
public void ScheduleSessionReminder(Session session, NotificationPreferences prefs)
{
    var reminderTimeUtc = session.StartTime
        .AddMinutes(-prefs.SessionReminderMinutes);

    var jobId = BackgroundJob.Schedule(
        () => SendSessionReminder(session.Id),
        reminderTimeUtc
    );

    session.ReminderJobId = jobId;
}
```

**Behavior:**

- Calculate reminder time: `ReminderTime = StartTime - ConsultantReminderOffset`
- Schedule a Hangfire job to run `SendSessionReminder(session.Id)` at `ReminderTime`
- Hangfire stores the job internally (in its own tables)
- The job ID is returned by Hangfire
- The returned `jobId` is stored in: `session.ReminderJobId`
- record an event in the `ClientTimeline` table:
  ```csharp
  clientTimeline.EventType = (int)TimelineEventType.SessionReminderScheduled;
  ```
  `MetadataJson`:
  ```json
  {
    "ProposalId": "guid",
    "SessionId": "guid",
    "ReminderScheduledAt": "2025-01-09T10:00:00Z",
    "ReminderFor": "2025-01-10T14:00:00Z",
    "ReminderMinutesBefore": 60,
    "ReminderType": "Email", // or other options
    "ScheduledBy": "Consultant"
  }
  ```

#### 2. Reschedule Reminder Jobs

This method is defined in `ReminderService.cs`, and is called when a session is updated:

```csharp
public void RescheduleSessionReminder(Session session, NotificationPreferences prefs)
{
    if (!string.IsNullOrEmpty(session.ReminderJobId))
        BackgroundJob.Delete(session.ReminderJobId);

    ScheduleSessionReminder(session, prefs);
}
```

**Steps:**

- Load `ReminderJobId` from the session
- Delete the existing Hangfire job
- Call `ScheduleSessionReminder` to
  - Recalculate `ReminderTime`
  - Schedule a new Hangfire job
  - Store the new `ReminderJobId`
- record an event in the `ClientTimeline` table:
  ```csharp
  clientTimeline.EventType = (int)TimelineEventType.SessionReminderRescheduled;
  ```
  `MetadataJson`:
  ```json
  {
    "ProposalId": "guid",
    "SessionId": "guid",
    "OldReminderAt": "2025-01-09T10:00:00Z",
    "NewReminderAt": "2025-01-09T12:00:00Z",
    "ReminderMinutesBefore": 120,
    "ReminderType": "SMS", // or other options
    "RescheduledBy": "Consultant",
    "RescheduledAt": "2025-01-08T09:30:00Z"
  }
  ```

#### 3. Cancel Reminder Jobs

This method is defined in `ReminderService.cs`, and is called when a session is cancelled:

```csharp
public void CancelSessionReminder(Session session)
{
    if (!string.IsNullOrEmpty(session.ReminderJobId))
        BackgroundJob.Delete(session.ReminderJobId);

    session.ReminderJobId = null;
}
```

**Steps**

- Load `ReminderJobId` from the session
- Delete the Hangfire job
- Set `ReminderJobId = NULL`
- record an event in the `ClientTimeline` table:
  ```csharp
  clientTimeline.EventType = (int)TimelineEventType.SessionReminderCancelled;
  ```
  `MetadataJson`:
  ```json
  {
    "ProposalId": "guid",
    "SessionId": "guid",
    "ReminderScheduledAt": "2025-01-09T10:00:00Z",
    "CancelledAt": "2025-01-08T09:30:00Z",
    "CancelledBy": "Consultant"
  }
  ```

#### 4 Execute Reminder Job

This method is defined in `ReminderService.cs`, and is called by Hangfire at the scheduled time:

```csharp
public async Task SendSessionReminder(Guid sessionId)
{
    var session = await _sessionRepository.Get(sessionId);

    if (session == null || session.Status != SessionStatus.Scheduled)
        return;

    var prefs = await _notificationPreferencesRepository.Get(session.ConsultantId);

    await _notificationService.SendReminder(session, prefs);
}
```

**Steps:**

- Load session by ID
- Verify:
  - Session exists
  - `session.Status == Scheduled`
- Load consultant `NotificationPreferences`
- Send reminder notification (push/email/SMS)
- Hangfire marks job as completed
- record an event in the `ClientTimeline` table:
  ```csharp
  clientTimeline.EventType = (int)TimelineEventType.SessionReminderSent;
  ```
  `MetadataJson`:
  ```json
  {
    "ProposalId": "guid",
    "SessionId": "guid",
    "ReminderScheduledAt": "2025-01-09T10:00:00Z",
    "ReminderSentAt": "2025-01-09T10:01:00Z",
    "ReminderType": "Email", // or other options
    "SentBy": "System"
  }
  ```

---

# STM - Signin Flow

## Overview

Signin is used by **existing users** (Admins, Consultants, Assistants). A successful signin creates:

1. A **UserLoginAttempt** record
2. An **OTP** record (`Purpose = 'Signin'`)
3. A **RefreshToken**
4. A **UserSession**
5. A short‑lived **Access Token (JWT)**

Signin is **passwordless** and uses OTP verification tied to DeviceId.

---

## Password Role in Authentication

Although App stores a `PasswordHash` for every user, **signin does not use the password**. App uses a **passwordless OTP‑based login model**, and the password plays a different role in the system.

### 1. Current Role (Signup Identity Binding)

During signup, the password is:

- validated
- hashed
- stored in `Users.PasswordHash`

This ensures that tenant creation is initiated by a real human and not an automated bot. Bots can easily request OTPs using disposable emails, but they cannot reliably generate strong passwords that meet validation rules.

### 2. Not Used in Signin

Signin uses OTP only:

- No password check
- No password fallback
- No password authentication

This allows Consultants and Assistants (who never set passwords) to log in seamlessly.

### 3. Future Role (Privileged Actions)

The stored password will be used later for sensitive operations, such as:

- changing email
- deleting a tenant
- exporting data
- accessing billing settings
- enabling optional password login or 2FA
- verifying identity for external integrations

Keeping the password hash today avoids future schema migrations and supports advanced security features.

---

## Rules

- Email must already exist in the `Users` table and must be `IsVerified = 1`.
- Signin requires verifying the OTP sent to the user's email.
- OTP attempts are tracked in the `OTPs.Attempts` field.
- `UserLoginAttempts` logs **only signin attempts**, not signup attempts.
- A user may have multiple active sessions (different devices).
- Refresh tokens are device‑specific and stored in HttpOnly cookies.

---

## Note on UserLoginAttempts

App logs every signin attempt in `UserLoginAttempts`, but this table is not currently used for authentication decisions.  
Since App uses passwordless OTP login, brute‑force password protection is not required. The table exists for future features such as password login, 2FA, security auditing, anomaly detection, and compliance requirements.

---

## Validations

### Signin Request

- **Email:** Required, must exist in `Users` table.
- **DeviceId:** Required, client‑generated identifier.

### OTP

- **Email:** Must match the signin email.
- **Code:** Must match hashed code in `OTPs` table.
- **ExpiresAt:** Must be in the future.
- **Attempts:** Must be less than max allowed (e.g., 3).
- **Purpose:** Must be `'Signin'`.

---

## Workflow

### Step 1: User submits signin form

Payload:

```json
{
    email,
    deviceId
}
```

System actions:

- Fetch user by email.
- If user does not exist => reject.
- If `IsVerified = 0` => reject ("Email not verified").
- Log unsuccessful signin attempt. Insert into `UserLoginAttempts`:
  - `Email = email`
  - `IpAddress`
  - `UserAgent`
  - `DeviceId = deviceId`
  - `IsSuccessful = 0`
  - `CreatedAt = now`

### Step 2: Handle OTP (Insert or Replace)

Fetch latest OTP (`Email = email` AND `Purpose = 'Signin'`):

- Valid OTP exists (unexpired):
  - Reject signin
  - Return: "OTP already sent. Please verify."

- OTP expired:
  - Update existing OTP record with new code
  - Reset `Attempts = 0`
  - Update `ExpiresAt = now + 5 minutes`

- No OTP exists
  - Create new OTP record
  - `Attempts = 0`
  - `Purpose = 'Signin'`

Send OTP via email.

### Step 3: Verify OTP

User submits:

```json
{
  email,
  otpCode,
  deviceId
}
```

System actions:

- Fetch OTP (`Email + Purpose = 'Signin'`).
- Validate expiration, attempts, and hashed code.

**If incorrect:**

- Increment `Attempts`
- Log failed attempt in `UserLoginAttempts`
- Reject with error message

**If correct:**

- Delete OTP record
- Update `UserLoginAttempts.IsSuccessful = 1`
- Continue signin

### Step 4: Create Refresh Token

Insert into `RefreshTokens`:

- `UserId`
- `DeviceId = deviceId`
- `TokenHash`
- `CreatedAt = now`
- `ExpiresAt = now + 30 days`
- `UserAgent`
- `IpAddress`

Backend sets HttpOnly cookie.

### Step 5: Create UserSession

Insert into **UserSessions**:

- `UserId`
- `DeviceId`
- `DeviceName` (parsed from UserAgent)
- `CreatedAt = now`
- `LastActivityAt = now`
- `IsActive = 1`

### Step 6: Generate Access Token (JWT)

After refresh token and session are created, backend generates a short‑lived access token.

**Access Token Contents:**

- `UserId`
- `TenantId`
- `Role`
- `iat`
- `exp` (15 minutes)

**Access Token Behavior:**

- Returned in response body
- Stored only in memory on frontend
- Used for authenticated API calls
- Automatically refreshed using the refresh token

---

# STM - Signup Flow

## Overview

Signup in App is **exclusively for new tenants**. A successful signup creates:

1. A **Tenant** record
2. A **User** record with role Admin (`IsVerified = 0` until OTP validated)
3. An **OTP** record for verification (`Purpose = 'Signup'`)
4. A **RefreshToken**
5. A **UserSession**

Consultants and Assistants **cannot sign up**. They are added later by the Admin inside the tenant dashboard.

---

## Rules

- **Admin email must be globally unique** across all users.
- Signup requires verifying the email using OTP. OTPs are stored in the `OTPs` table and deleted after successful verification.
- Signup attempts are tracked in the `OTPs.Attempts` field.
- Signup initializes a refresh token, an access token, and a user session tied to `DeviceId`.

---

## Validations

### Signup Request:

- **FullName:** Required, minimum length 2 characters.
- **BusinessName:** Required, minimum length 2 characters.
- **Domain:** Required, must be 3-50 characters, alphanumeric with optional hyphens, and cannot already exist in the `Tenants` table.
- **Email:** Required, valid email format, must not already exist in `Users` table with `IsVerified = 1`.
- **Passwrod:** Required, minimum length 8 chareacters, strong password recommended.
- **DeviceId:** Required, must be a valid client-generated identifier.

### OTP:

- **Email:** Must match the email used during signup.
- **Code:** Must match hashed code in `OTPs` table.
- **ExpiresAt:** Must be in the future.
- **Attempts:** Must be less than max allowed. (e.g., 3 per OTP)

---

## Workflow

### Step 1: User submits signup form

Payload:

```json
{
  fullName,
  businessName,
  domain,
  email,
  password,
  deviceId
}
```

System actions:

- Validate fields
- Check if `Users.Email` exists:
  - If `IsVerified = 1` => stop signup, return "Email already registered."
  - If `IsVerified = 0`:
    - If valid OTP exists => stop signup, return "OTP already sent, please verify."
    - If OTP expired => update Tenant/User with new values, create new OTP.
    - If attempts exceeded => stop signup, return "Too many attempts."
  - If not found => continue normal signup.

### Step 2: Create or Update Tenant

- If new email => insert Tenant.
  - `Name = businessName`
  - `Domain = domain`
  - `CreatedAt = now`
- If existing unverified email => update Tenant with new values.

### Step 3: Create or Update User (Admin)

- If new email => insert User.
  - `TenantId = newly created TenantId`
  - `Email = email`
  - `FullName = fullName`
  - `PasswordHash = hashed(password)`
  - `Role = 'Admin'`
  - `IsVerified = 0`
  - `IsActive = 1`
  - `CreatedAt = now`
- If existing unverified email => update User with new values.
- Always set:
  - `IsVerified = 0`
  - `IsActive = 1`

### Step 4: Generate OTP

**Insert** / **replace** in **OTPs**:

- `Email = email`
- `UserId = newly created UserId`
- `HashedCode = hash(otp)`
- `ExpiresAt = now + 5 minutes`
- `Attempts = 0`
- `Purpose = 'Signup'`

**Insert:**
Creates a brand‑new OTP record for the given `Email + Purpose`. Used when the email is starting a new signup flow, meaning no active (unverified and unexpired) OTP exists for `Email + Purpose = Signup`.

**Replace:**  
Updates the existing pending OTP record for the same `Email + Purpose`. Used when a previous OTP has expired or the user retries signup and submits new tenant/user information.
The old record is overwritten with the new code, reset `Attempts = 0`, and updated `ExpiresAt`.

Send OTP via email.

### Step 5: Verify OTP

User submits:

```json
{
  email
  otpCode
  deviceId
}
```

System actions:

- Fetch OTP by (`Email = email`, `Purpose = 'Signup'`)
- Validate expiration, attempts, and hashed code.
- If incorrect:
  - Increment `Attempts`
  - Reject with error message
- If correct:
  - Delete OTP record
  - Update `Users.IsVerified = 1`
  - Continue signup

### Step 6: Create Refresh Token

Insert into **RefreshTokens**:

- `UserId`
- `DeviceId`
- `TokenHash`
- `CreatedAt`
- `ExpiresAt = now + 30 days`
- `UserAgent`
- `IpAddress`

Backend sets HttpOnly cookie.

### Step 7: Create UserSession

Insert into **UserSessions**:

- `UserId`
- `DeviceId`
- `DeviceName` (parsed from UserAgent)
- `CreatedAt = now`
- `LastActivityAt = now`
- `IsActive = 1`

### Step 8: Generate Access Token (JWT)

After the refresh token and session are created, the backend generates a short‑lived access token.

**Access Token Contents:**

- `UserId`
- `TenantId`
- `Role` (always `'Admin'` for signup)
- `iat` (issued at)
- `exp` (expires in 15 minutes)

**Access Token Behavior:**

- Returned in the response body
- Stored **only in memory** on the frontend
- Used for all authenticated API calls
- Automatically refreshed using the refresh token when expired

---

# STM – Tenant Staff Management

## Overview

Tenant Admins can create and manage staff members who work inside their tenant. App supports 2 staff roles:

- **Consultant:** primary service provider
- **Assistant:** support role with limited access

Staff users authenticate using OTP-only, not passwords. Their `PasswordHash` is automatically generated by the system.

Staff management is available only to **Admins** and requires the `ManageStaff` permission.

---

## Staff Creation Flow

### 1. Admin submits staff creation form

Payload:

```json
{
  "email": "staff@example.com",
  "role": "Consultant",
  "fullName": "John Doe"
}
```

### 2. Backend validations

- Email format is valid
- Email does not already exist in `Users`
- Role is one of: `Consultant`, `Assistant`
- Admin has `ManageStaff` permission
- Tenant is active

### 3. Create User record

Insert into **Users**:

- `Email`
- `FullName`
- `Role`
- `TenantId`
- `IsVerified = 1` (staff do not verify email manually)
- `PasswordHash = Hash("app-auto-" + Guid.NewGuid())`
- `CreatedAt = now`

#### Why dummy PasswordHash?

Staff users authenticate using OTP only. `PasswordHash` is required by schema and reserved for future privileged actions.

---

## Staff Invitation Email

After creation, App sends an invitation email:

- Contains OTP signin link
- Explains role and access
- No password setup required

Example:

```
"You've been added as a Consultant in STM. Sign in using OTP to access your dashboard."
```

---

## Staff Signin Flow

Staff signin uses the same OTP-based flow as Admins:

- Staff enters email
- System sends OTP
- Staff verifies OTP
- RefreshToken + UserSession created
- AccessToken issued

No password is ever required.

---

## Staff Permissions

Permissions are assigned automatically based on role.

### Consultant Permissions

- ManageClients
- ViewClients
- ManageSessions
- ViewSessions
- ManageDocuments
- ViewDocuments
- ViewAnalytics

### Assistant Permissions

- ViewClients
- ManageSessions
- ViewSessions
- ManageDocuments
- ViewDocuments

### Admin Permissions

Admins can manage staff and tenant settings.

---

## Staff Listing

Admins can view all staff in their tenant:

Fields returned:

- `UserId`
- `FullName`
- `Email`
- `Role`
- `CreatedAt`
- `LastActivityAt` (from UserSessions)
- `IsActive`

---

## Staff Update

Admins can update:

- Full name
- Role
- Assigned clients
- Status (active/inactive)

Updating role automatically updates permissions.

---

## Staff Deactivation

Admins can deactivate staff. Actions performed:

- Set `IsActive = 0`
- Delete all refresh tokens for that user
- Mark all sessions inactive
- Prevent future OTP signin

Deactivated staff cannot access App.

---

## Staff Deletion

Admins can permanently delete staff. Actions performed:

- Delete user record
- Delete all sessions
- Delete all refresh tokens
- Delete all client assignments
- Delete staff-related metadata

Deletion is irreversible.

---

## Security Considerations

- Only Admins with `ManageStaff` permission can manage staff
- Staff authenticate using OTP-only
- Staff passwords are system-generated and never used
- Staff sessions are device-bound
- Staff permissions are enforced via JWT
- Staff cannot access tenant-level settings or billing
- All staff actions are logged for auditing

---

# STM – Tenant

## Overview

A Tenant represents a single customer business that subscribes to the platform.
Each tenant owns its own isolated workspace, including users, clients, workflows, automations, sessions, deliverables, and analytics.

---

## Domain

### Purpose

Domain is the unique workspace address for each tenant:

` https://{domain}.stmapp.com`

This value is chosen by the customer (e.g., careercoach). This field enables:

- Public pages before login (e.g., Landing page, Pricing page, Public booking page, etc.)
- Custom domains. STM later will support `https://careercoach.com` instead of `https://careercoach.stmapp.com`.

### Characteristics

- Must be unique across tenants
- Lowercase only
- Allowed characters: a-z, 0-9, hyphen (-)
- Regex: `^[a-z0-9]+(-[a-z0-9]+)*$`
- Length: 3-50 characters
- No leading/trailing hyphens
- Reserved words blocked: www, api, admin, login, support, stmapp

### Tenant isolation

The middleware will look like:

```csharp
 var domain = context.Request.Host.Host; //careercoach.stmapp.com
 var tenant = await _tenantService.GetTenantByDomain(domain);
 context.Items["Tenant"] = tenant;
```

Then every repository uses:

```csharp
 ... WHERE TenantId = context.Items["Tenant"].Id
```

---

# STM – Users

## Overview

The **Users** table stores all staff accounts within a tenant (e.g., admins, consultants, assistants), and every user belongs to exactly one tenant.

---

## Role field

### Purpose

The **Role** defines the user’s function within a tenant (e.g., Admin, Consultant, Assistant).

### Roles Descriptions

- **Admin:** Created only through signup, and represents the tenant owner. They have full access to tenant settings, and management of staff, client, proposal and session.
- **Consultant:** Added by Admin and belongs to the same tenant. They can create and edit clients, proposals, sessions and documents.
- **Assistant:** Added by Admin and belongs to the same tenant. They support consultants and have limited permissions: view clients, view proposals, schedule sessions, and upload documents.

### Implementation

To ensure clarity, safety, and consistency across code and database layers, the following design is used:

- **Enum in Code**:  
  In C#, the role is represented as a strongly‑typed `enum` (`UserRole`). This prevents typos and invalid assignments at compile time.

```csharp
  public enum UserRole
  {
      Admin,
      Consultant,
      Assistant
  }

  public class User
  {
      ...
      public UserRole Role { get; set; }
  }
```

- **NVARCHAR in Database:**
  The enum is stored as a string (NVARCHAR(50)) for readability and reporting. EF Core maps the enum values to their string names:

  ```csharp
  modelBuilder.Entity<User>()
      .Property(u => u.Role)
      .HasConversion<string>()   // store enum as NVARCHAR
      .HasMaxLength(50);

  ```

- **CHECK Constraint for Integrity:**
  A CHECK constraint is added via EF Core to enforce that only valid role values are stored:

  ```csharp
  modelBuilder.Entity<User>()
      .HasCheckConstraint("CK_Users_Role", "Role IN ('Admin','Consultant','Assistant')");

  ```

**Note:** Localization of role names should be handled in the frontend/UI layer. The database always stores canonical English values.

---

# STM – UserSettings

## Overview

The **UserSettings** table stores per‑user preferences and configuration options within a tenant.  
Each user must have a corresponding `UserSettings` record to ensure consistent behavior across scheduling, notifications, and localization features.

---

## Timezone field

### Purpose

The **Timezone** field defines the user’s local time zone, ensuring that all scheduling, reminders, and timestamp displays are correctly converted from UTC into the user’s preferred local time.

### Backend

- Stores timezone (Asia/Tehran)

- Stores timestamps in UTC

- Returns timestamps in UTC

- Uses timezone only for business logic (reminders, scheduling)

### Frontend

- Reads timezone and UTC from backend

- Converts UTC to local time

- Formats for display

- Applies local rules

### Backend Implementation (C# / EF Core)

**Model:**

```csharp
public class UserSettings
{
    ...
    public string Timezone { get; set; } = "UTC"; // default
}
```

**EF Core Configuration:**

```csharp
 modelBuilder.Entity<UserSettings>()
    .Property(u => u.Timezone)
    .IsRequired()
    .HasMaxLength(100); // NVARCHAR(100)

```

**Validation:**
Implement the timezone validator as a validation service in the Application layer, and use it there whenever user input for timezone is accepted or updated. The Application layer must enforce that only valid IANA timezone identifiers are stored. If the provided timezone is missing or invalid, default to "UTC".

```csharp
// Application/Interfaces/Services/ITimezoneValidationService.cs
public interface ITimezoneValidationService
{
    bool IsValidTimezone(string timezone);
}

// Application/Services/TimezoneValidationService.cs
public class TimezoneValidationService : ITimezoneValidationService
{
    public bool IsValidTimezone(string timezone)
    {
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
    }
}
```

**How to use it:**
Even though backend does not display time, it still needs timezone for scheduling jobs, calculating deadlines, and so on.

```csharp
 var utcNow = DateTime.UtcNow;
 var userTz = TimeZoneInfo.FindSystemTimeZoneById(userSettings.Timezone);
 var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, userTz);
```

### Frontend Implementation (Angular/TypeScript)

**UI Selection**
Provide a dropdown of IANA timezone IDs (e.g., "Asia/Tehran", "America/New_York") for the user to choose.

**Conversion with moment‑timezone:**
Handle localization and display using moment‑timezone library, inside a shared DateTime utility service (e.g., datetime.service.ts). It must not be placed inside components.

```typescript
import * as moment from "moment-timezone";
import { Injectable } from "@angular/core";

@Injectable({ providedIn: "root" })
export class DateTimeService {
  formatUserTime(utcDate: string, timezone: string): string {
    return moment.utc(utcDate).tz(timezone).format("YYYY-MM-DD HH:mm");
  }
}
```

**Usage in a component**
Use this service whenever displaying localized timestamps, converting UTC to the user's timezone. This keeps time conversion logic centralized, reusable, and consistent across the application.

```typescript
ngOnInit() {
  this.localTime = this.dateTime.formatUserTime(
    this.session.startUtc,     // session is an example here
    this.userSettings.timezone
  );
}
```

---

## NotificationPreferences field

### Purpose

The **NotificationPreferences** field stores the user's notification configuration as JSON in an `NVARCHAR(MAX)` column. It defines which channels the user wants to receive notifications through (email, SMS, in-app, push), which events they want alerts for, and how early reminders should be sent.
The frontend sends these preferences as JSON, the backend validates and stores them, and uses them for scheduling reminders and delivering notifications.

**Default Value:**

```csharp
{
    "email": true,
    "sms": false,
    "inApp": true,
    "push": false,
    "sessionReminderMinutes": 15,
    "dailySummary": false,
    "clientMessageAlerts": true,
    "taskAssignedAlerts": true
}
```

**Why JSON string?**

- Notification settings change over time, JSON avoids schema change.

- Different users may have different combinations of preferences.

- Easy to serialize/deserialize in frontend and backend.

### Backend (.NET) - Serialize & Deserialize

**Model:**

```csharp
public class NotificationPreferencesDto
{
    public bool Email { get; set; }
    public bool Sms { get; set; }
    public bool InApp { get; set; }
    public bool Push { get; set; }
    public int SessionReminderMinutes { get; set; }
    public bool DailySummary { get; set; }
    public bool ClientMessageAlerts { get; set; }
    public bool TaskAssignedAlerts { get; set; }
}
```

**Validation:**
Add a validation service in the Application layer, and use it in `UserSettingsService`, inside `UpdateUserSettingsAsync` method.

```csharp
public class NotificationPreferencesValidator
{
    public void Validate(NotificationPreferencesDto prefs)
    {
        if (prefs == null)
            throw new ValidationException("Notification preferences are required.");

        // Reminder must be within allowed range
        if (prefs.SessionReminderMinutes < 0 || prefs.SessionReminderMinutes > 1440)
            throw new ValidationException("SessionReminderMinutes must be between 0 and 1440.");

        // At least one channel must be enabled
        if (!prefs.Email && !prefs.Sms && !prefs.InApp && !prefs.Push)
            throw new ValidationException("At least one notification channel must be enabled.");

        // Daily summary is optional — no validation needed
        // ClientMessageAlerts and TaskAssignedAlerts are booleans — always valid
    }
}
```

**Serialize (store JSON in DB):**
The frontend sends NotificationPreferences as a structured JSON object, not as a string. The backend validates this object and serializes it into a JSON string.

```csharp
var prefsJson = JsonSerializer.Serialize(dto.NotificationPreferences);
userSettings.NotificationPreferencesJson = prefsJson;
```

**Deserialize:**
When reading from DB, backend gets a string, not an object. So it must convert it back:

```csharp
var prefs = JsonSerializer.Deserialize<NotificationPreferences>(userSettings.NotificationPreferencesJson);
```

### Frontend (Angular & TypeScript)

The frontend sends NotificationPreferences as a structured JSON object, not as a string.

**Receive JSON string from backend:**

```typescript
{
  "notificationPreferencesJson": "{"email":true,"sms":false,...}"
}
```

**Parse it:**

```typescript
this.notificationPreferences = JSON.parse(response.notificationPreferencesJson);
```

---

## CalendarIntegrationToken field

### Purpose

Staff users often rely on external calendars (Google/Outlook) to manage their daily schedule.
`CalendarIntegrationToken` enables STM to:

- Push sessions to the user’s external calendar

- Update external calendar events when sessions are rescheduled

- Delete external calendar events when sessions are cancelled

- Fetch external events (optional future feature)

- Maintain a persistent authenticated connection

Clients do not use this field. Only staff users need calendar integration.

**Why JSON string?**

- Tokens differ between providers (Google vs Outlook)

- Tokens may include multiple fields (access token, refresh token, expiry)

- JSON allows flexible schema changes without DB migrations

### Frontend Workflow

- **User connects their calendar (OAuth)**
  User clicks "Connect Google Calendar" or "Connect Outlook Calendar". Frontend redirects to OAuth provider, and user grants permission. OAuth provider returns authorization code, and frontend sends code to backend.
- **Frontend receives:**
  ```typescript
  {
   "calendarIntegrationEnabled": true
  }
  ```
- **Frontend shows “Connected” status**
  - Frontend does not store tokens.
  - Frontend does not handle refresh logic.
  - Frontend only shows UI state.
- **Frontend sends "Disconnect Calendar"**
  ```typescript
  {
    "disconnectCalendar": true
  }
  ```
  And backend clears the token:
  ```csharp
   userSettings.CalendarIntegrationToken = null;
  ```

### Backend Workflow

- **OAuth token Model**

  ```csharp
  public class CalendarIntegrationToken
  {
      public string Provider { get; set; } // google | outlook
      public string AccessToken { get; set; }
      public string RefreshToken { get; set; }
      public DateTime ExpiresAt { get; set; }
  }
  ```

- **Receives OAuth tokens**

  ```csharp
  {
      "provider": "google",
      "accessToken": "...",
      "refreshToken": "...",
      "expiresAt": "2026-07-09T18:00:00Z"
  }
  ```

- **Validates the token**
  - Checks provider is allowed
  - Checks required fields exist
  - Checks expiry format

- **Serializes JSON into string**

  ```csharp
  userSettings.CalendarIntegrationToken = JsonSerializer.Serialize(dto.CalendarIntegrationToken);
  ```

  And stores it in DB

- **Uses the token for calendar operations**
  - When a session is created, pushes event to Google/Outlook

  - When a session is rescheduled, updates external event

  - When a session is cancelled, deletes external event

  - When token expires, refreshes using refreshToke

- **Deserializes when needed**
  ```csharp
  var token = JsonSerializer.Deserialize<CalendarIntegrationToken>(userSettings.CalendarIntegrationToken);
  ```
