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
