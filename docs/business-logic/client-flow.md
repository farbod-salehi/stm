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
