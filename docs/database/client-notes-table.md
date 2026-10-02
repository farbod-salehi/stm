# STM - ClientNotes Table

## Schema

| Column       | Type          | Description                                                                             |
| ------------ | ------------- | --------------------------------------------------------------------------------------- |
| Id           | GUID          | Primary key;                                                                            |
| ClientId     | GUID          | Foreign key referencing Clients table; (Required, Indexed)                              |
| ConsultantId | GUID          | Foreign key referencing Users table, staff user who wrote the note; (Required, Indexed) |
| Content      | NVARCHAR(MAX) | The actual note text; (Required)                                                        |
| CreatedAt    | DATETIME2(0)  | Creation timestamp; (Required)                                                          |
| UpdatedAt    | DATETIME2(0)  | Updating timestamp; (Not Required)                                                      |

---

## Indexes

- `IX_ClientNotes_ClientId`
- `IX_ClientNotes_ConsultantId`
