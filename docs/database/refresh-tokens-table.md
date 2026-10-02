# STM - RefreshTokens Table

## Schema

| Column    | Type          | Description                                                                                              |
| --------- | ------------- | -------------------------------------------------------------------------------------------------------- |
| Id        | GUID          | Primary key;                                                                                             |
| UserId    | GUID          | Foreign key referencing Users table; (Required, Indexed)                                                 |
| TokenHash | NVARCHAR(256) | Hashed refresh token value. Never store raw tokens; (Required, Indexed)                                  |
| ExpiresAt | DATETIME2(0)  | Expiration timestamp for the refresh token; (Required, Indexed)                                          |
| CreatedAt | DATETIME2(0)  | Timestamp when the token was created; (Required)                                                         |
| DeviceId  | NVARCHAR(64)  | A unique, stable identifier generated on the client and used to distinguish devices/browsers; (Required) |
| IpAddress | NVARCHAR(64)  | IP address used when the token was created; (Not Required)                                               |
| UserAgent | NVARCHAR(256) | Browser or device user agent string; (Not Required)                                                      |

---

## Indexes

- `IX_RefreshTokens_UserId`
- `IX_RefreshTokens_TokenHash` (unique)
- `IX_RefreshTokens_ExpiresAt`
- `IX_RefreshTokens_UserId_DeviceId`
