# STM - OTPs Table

## Schema

| Column     | Type          | Description                                                                         |
| ---------- | ------------- | ----------------------------------------------------------------------------------- |
| Id         | GUID          | Primary key;                                                                        |
| Email      | nvarchar(256) | Email address the OTP was sent to; (Required, Indexed)                              |
| UserId     | GUID          | Foreign key referencing Users table. Linked user if exists; (Not Required, Indexed) |
| HashedCode | NVARCHAR(256) | Securely hashed OTP code; (Required)                                                |
| ExpiresAt  | DATETIME2(0)  | Expiration timestamp for OTP validity; (Required)                                   |
| Attempts   | INT           | Number of verification attempts used; (Required)                                    |
| Purpose    | nvarchar(32)  | Signup, ForgotPassword, ResetPassword, ChangeEmail; (Required)                      |

---

## Indexes

- `IX_OTPs_Email`
- `IX_OTPs_UserId`
- `IX_OTPs_ExpiresAt`
- `IX_OTPs_Email_Purpose`

---

## Checkes

```SQL
ALTER TABLE OTPs
ADD CONSTRAINT CK_OTPs_Purpose
CHECK (Purpose IN ('Signup', 'ForgotPassword', 'ResetPassword', 'ChangeEmail'));
```
