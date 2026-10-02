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
