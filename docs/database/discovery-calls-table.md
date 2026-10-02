# STM - DiscoveryCalls Table

## Schema

| Column          | Type          | Description                                                                                |
| --------------- | ------------- | ------------------------------------------------------------------------------------------ |
| Id              | GUID          | Primary key;                                                                               |
| ClientId        | GUID          | Foreign key referencing Clients table; (Required, Indexed)                                 |
| ScheduledAt     | DATETIME2(0)  | Calling timestamp; (Required, Indexed)                                                     |
| DurationMinutes | INT           | 15, 30, 45, 60, etc. ; (Required)                                                          |
| Status          | NVARCHAR(20)  | Scheduled, Completed, No-Show; (Required, Indexed, Default: 'Scheduled')                   |
| Outcome         | NVARCHAR(20)  | Qualified, NotQualified, NoShow; (Not Required)                                            |
| Notes           | NVARCHAR(MAX) | Consultant notes about the call; (Not Required)                                            |
| ExternalEventId | NVARCHAR(200) | ID of the event in Google/Outlook calendar, used for updates/cancellations; (Not Required) |
| CreatedAt       | DATETIME2(0)  | Creation timestamp; (Required)                                                             |
| UpdatedAt       | DATETIME2(0)  | Updating timestamp; (Not Required)                                                         |
| IsDeleted       | BIT           | Indicates whether the record is logically deleted; (Required, Default: false)              |
| DeletedAt       | DATETIME2(0)  | Deleting timestamp; (Not Required)                                                         |
| DeletedBy       | GUID          | Foreign key referencing Users table; (Not Required)                                        |

---

## Indexes

- `IX_DiscoveryCalls_ClientId WHERE IsDeleted = 0`
- `IX_DiscoveryCalls_ScheduledAt WHERE IsDeleted = 0`
- `IX_DiscoveryCalls_Status WHERE IsDeleted = 0`

---

## Checkes

```SQL
ALTER TABLE DiscoveryCalls
ADD CONSTRAINT CK_DiscoveryCalls_Status
CHECK (Status IN ('Scheduled', 'Completed', 'No-Show'));
```

```SQL
ALTER TABLE DiscoveryCalls
ADD CONSTRAINT CK_DiscoveryCalls_Outcome
CHECK (Outcome IN ('Qualified', 'NotQualified', 'NoShow'));
```

```SQL
ALTER TABLE DiscoveryCalls
ADD CONSTRAINT CK_DiscoveryCalls_DurationMinutes
CHECK (DurationMinutes > 0);
```
