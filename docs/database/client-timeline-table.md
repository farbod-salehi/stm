# STM - ClientTimeline Table

## Schema

| Column       | Type           | Description                                                |
| ------------ | -------------- | ---------------------------------------------------------- |
| Id           | GUID           | Primary key;                                               |
| ClientId     | GUID           | Foreign key referencing Clients table; (Required, Indexed) |
| ConsultantId | GUID           | Foreign key referencing Users table; (Not Required)        |
| EventType    | INT            | Type of event; (Required)                                  |
| OccurredAt   | DATETIMEOFFSET | When event occurred; (Required)                            |
| MetadataJson | NVARCHAR(MAX)  | JSON metadata describing the event; (Not Required)         |
| CreatedAt    | DATETIMEOFFSET | Insert timestamp; (Required)                               |

---

## Indexes

- `IX_ClientTimeline_ClientId_OccurredAt`
