# STM - UserSettings Table

## Schema

| Column                   | Type           | Description                                                                          |
| ------------------------ | -------------- | ------------------------------------------------------------------------------------ |
| Id                       | GUID           | Primary key;                                                                         |
| UserId                   | GUID           | Foreign key referencing Users table; (Required, Unique)                              |
| Timezone                 | NVARCHAR(100)  | User’s preferred time zone; (Required, Default: "UTC")                               |
| NotificationPreferences  | NVARCHAR(max)  | User's notification configuration as a JSON string; (Required, Default: JSON string) |
| CalendarIntegrationToken | NVARCHAR(max)  | Connection to an external calendar provider, as a JSON string; (Not Required)        |
| CreatedAt                | DATETIMEOFFSET | Creation timestamp (Required)                                                        |
| UpdatedAt                | DATETIMEOFFSET | Updating timestamp (Not Required)                                                    |

---

## Indexes

- `IX_UserSettings_UserId` (unique)
