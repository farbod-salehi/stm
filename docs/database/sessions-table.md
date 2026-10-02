# STM - Sessions Table

## Schema

| Column          | Type          | Description                                                                               |
| --------------- | ------------- | ----------------------------------------------------------------------------------------- |
| Id              | GUID          | Primary key;                                                                              |
| ConsultantId    | GUID          | Foreign key referencing Users table; (Required, Indexed)                                  |
| ProposalId      | GUID          | Foreign key referencing Proposals table; (Required, Indexed)                              |
| Title           | NVARCHAR(200) | Short title of the session; (Required)                                                    |
| Description     | NVARCHAR(MAX) | Detailed description of the session; (Not Required)                                       |
| StartTime       | DATETIME2(0)  | Scheduled start time; (Required)                                                          |
| EndTime         | DATETIME2(0)  | Scheduled end time; (Required)                                                            |
| Status          | NVARCHAR(50)  | Scheduled, Completed, Cancelled; (Required, Default: 'Scheduled')                         |
| ExternalEventId | NVARCHAR(200) | The event ID returned by Google/Outlook for syncing updates and deletions; (Not Required) |
| ReminderJobId   | NVARCHAR(100) | The Hangfire job identifier for the scheduled reminder; (Not Required)                    |
| MeetingUrl      | NVARCHAR(500) | Link for online sessions (Zoom, Teams, etc.); (Not Required)                              |
| Location        | NVARCHAR(300) | Physical location for in‑person sessions; (Not Required)                                  |
| IsOnline        | BIT           | Whether the session is online; (Required, Default: 0)                                     |
| CreatedAt       | DATETIME2(0)  | Creation timestamp; (Required)                                                            |
| UpdatedAt       | DATETIME2(0)  | Updating timestamp; (Not Required)                                                        |
| IsDeleted       | BIT           | Indicates whether the record is logically deleted; (Required, Default: false)             |
| DeletedAt       | DATETIME2(0)  | Deleting timestamp; (Not Required)                                                        |
| DeletedBy       | GUID          | Foreign key referencing Users table; (Not Required)                                       |

---

## Indexes

- `IX_Sessions_ConsultantId_StartTime WHERE IsDeleted = 0`
- `IX_Sessions_ProposalId WHERE IsDeleted = 0`
- `IX_Sessions_ProposalId_StartTime WHERE IsDeleted = 0` (unique)

---

## Checkes

```SQL
ALTER TABLE Sessions
ADD CONSTRAINT CK_Sessions_TimeRange
CHECK (StartTimeUtc < EndTimeUtc);
```

```SQL
ALTER TABLE Sessions
ADD CONSTRAINT CK_Sessions_Status
CHECK (Status IN ('Scheduled','Completed','Cancelled'));
```

```SQL
ALTER TABLE Sessions
ADD CONSTRAINT CK_Sessions_IsOnline
CHECK (IsOnline IN (0,1));
```
