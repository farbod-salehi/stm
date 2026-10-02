# STM - UserLoginAttempts Table

## Schema

| Column       | Type          | Description                                                                                                        |
| ------------ | ------------- | ------------------------------------------------------------------------------------------------------------------ |
| Id           | GUID          | Primary key;                                                                                                       |
| UserId       | GUID          | Foreign key referencing Users table, NULL when the email does not match any existing user; (Not Required, Indexed) |
| Email        | NVARCHAR(256) | The identifier the user attempted to log in with. Stored even if no matching user exists; (Required, Indexed)      |
| IsSuccessful | BIT           | Indicates whether the login attempt succeeded; (Required)                                                          |
| IpAddress    | NVARCHAR(64)  | IP address of the login attempt; (Not Required, Indexed)                                                           |
| UserAgent    | NVARCHAR(512) | Raw browser user agent string; (Not Required)                                                                      |
| DeviceId     | NVARCHAR(64)  | Client‑generated device identifie; (Not Required, Indexed)                                                         |
| CreatedAt    | DATETIME2(0)  | Timestamp of the login attempt; (Required, Indexed)                                                                |

---

## Indexes

- `IX_UserLoginAttempts_UserId`
- `IX_UserLoginAttempts_Email`
- `IX_UserLoginAttempts_IpAddress`
- `IX_UserLoginAttempts_DeviceId`
- `IX_UserLoginAttempts_CreatedAt`
