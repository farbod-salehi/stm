# STM - Proposals Table

## Schema

| Column                  | Type          | Description                                                                                      |
| ----------------------- | ------------- | ------------------------------------------------------------------------------------------------ |
| Id                      | GUID          | Primary key;                                                                                     |
| ClientId                | GUID          | Foreign key referencing Clients table; (Required, Indexed)                                       |
| PackageId               | GUID          | Foreign key referencing Packages table; (Required, Indexed)                                      |
| PackageName             | NVARCHAR(200) | Snapshot of package name at creation; (Required)                                                 |
| PackageDescription      | NVARCHAR(MAX) | Snapshot of package description at creation; (Not Required)                                      |
| PackagePrice            | DECIMAL(10,2) | Snapshot of package price at creation; (Required)                                                |
| PackageSessionsIncluded | INT           | Snapshot of package included session at creation; (Required)                                     |
| Status                  | NVARCHAR(20)  | Draft, Sent, Accepted, Rejected; (Required, Default: 'Draft')                                    |
| IsActive                | BIT           | Only one accepted proposal can be "active" for each client at a time; (Required, Default: false) |
| SentAt                  | DATETIME2(0)  | When the proposal was sent; (Not Required)                                                       |
| RespondedAt             | DATETIME2(0)  | When the client accepted or rejected; (Not Required)                                             |
| Notes                   | NVARCHAR(MAX) | Internal consultant notes; (Not Required)                                                        |
| CreatedAt               | DATETIME2(0)  | Creation timestamp; (Required)                                                                   |
| UpdatedAt               | DATETIME2(0)  | Updating timestamp; (Not Required)                                                               |
| IsDeleted               | BIT           | Indicates whether the record is logically deleted; (Required, Default: false)                    |
| DeletedAt               | DATETIME2(0)  | Deleting timestamp; (Not Required)                                                               |
| DeletedBy               | GUID          | Foreign key referencing Users table; (Not Required)                                              |

---

## Indexes

- `IX_Proposals_ClientId WHERE IsDeleted = 0`
- `IX_Proposals_PackageId WHERE IsDeleted = 0`
- `IX_Proposals_Status WHERE IsDeleted = 0`
- `IX_Proposals_ClientId_IsActive WHERE IsActive = 1 AND IsDeleted = 0` (unique)

---

## Checkes

```SQL
ALTER TABLE Proposals
ADD CONSTRAINT CK_Proposals_Status
CHECK (Status IN ('Draft', 'Sent', 'Accepted', 'Rejected''));
```

```SQL
ALTER TABLE Proposals
ADD CONSTRAINT CK_Proposals_Status_IsActive
CHECK (
    (Status = 'Accepted' AND IsActive = 1)
    OR
    (Status <> 'Accepted' AND IsActive = 0)
);
```
