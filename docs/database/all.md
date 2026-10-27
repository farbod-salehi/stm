# STM - ClientDocuments Table

## Schema

| Column     | Type           | Description                                                                               |
| ---------- | -------------- | ----------------------------------------------------------------------------------------- |
| Id         | GUID           | Primary key;                                                                              |
| ClientId   | GUID           | Foreign key referencing Clients table; (Required, Indexed)                                |
| FileUrl    | NVARCHAR(500)  | URL pointing to the stored file; (Required)                                               |
| Type       | NVARCHAR(50)   | Discovery, Proposal, Onboarding, Session, Contract, Invoice, Receipt, General; (Required) |
| UploadedAt | DATETIMEOFFSET | Upload timestamp; (Required)                                                              |
| UploadedBy | GUID           | Foreign key referencing Users table; (Required)                                           |
| UpdatedAt  | DATETIMEOFFSET | Update timestamp; (Required)                                                              |

---

## Indexes

- `IX_ClientDocuments_ClientId`
- `IX_ClientDocuments_ClientId_Type`

---

## Check

```SQL
ALTER TABLE ClientDocuments
ADD CONSTRAINT CK_ClientDocuments_Type
CHECK (Type IN ('Discovery', 'Proposal', 'Onboarding', 'Session', 'Contract', 'Invoice', 'Receipt', 'General'));
```

---

# STM - ClientNotes Table

## Schema

| Column       | Type           | Description                                                                             |
| ------------ | -------------- | --------------------------------------------------------------------------------------- |
| Id           | GUID           | Primary key;                                                                            |
| ClientId     | GUID           | Foreign key referencing Clients table; (Required, Indexed)                              |
| ConsultantId | GUID           | Foreign key referencing Users table, staff user who wrote the note; (Required, Indexed) |
| Content      | NVARCHAR(MAX)  | The actual note text; (Required)                                                        |
| CreatedAt    | DATETIMEOFFSET | Creation timestamp; (Required)                                                          |
| UpdatedAt    | DATETIMEOFFSET | Updating timestamp; (Not Required)                                                      |

---

## Indexes

- `IX_ClientNotes_ClientId`
- `IX_ClientNotes_ConsultantId`

---

# STM - Clients Table

## Schema

| Column    | Type           | Description                                                                   |
| --------- | -------------- | ----------------------------------------------------------------------------- |
| Id        | GUID           | Primary key;                                                                  |
| TenantId  | GUID           | Foreign key referencing Tenants table; (Required, Indexed)                    |
| FullName  | NVARCHAR(200)  | Client’s full name (Required)                                                 |
| Email     | NVARCHAR(320)  | Client’s email; (Not Required)                                                |
| Phone     | NVARCHAR(50)   | Client's phone number; (Not Required)                                         |
| Status    | NVARCHAR(30)   | Lead, Active, Paused, Completed; (Required)                                   |
| Timezone  | NVARCHAR(100)  | Client's preferred time zone; (Required, Default: "UTC")                      |
| Source    | NVARCHAR(30)   | Manual, LinkedIn, Website, Referral, Other; (Not Required)                    |
| CreatedAt | DATETIMEOFFSET | Creation timestamp; (Required)                                                |
| UpdatedAt | DATETIMEOFFSET | Updating timestamp; (Not Required)                                            |
| IsDeleted | BIT            | Indicates whether the record is logically deleted; (Required, Default: false) |
| DeletedAt | DATETIMEOFFSET | Deleting timestamp; (Not Required)                                            |
| DeletedBy | GUID           | Foreign key referencing Users table; (Not Required)                           |

---

## Indexes

- `IX_Clients_TenantId WHERE IsDeleted = 0`
- `IX_Clients_TenantId_Status WHERE IsDeleted = 0`
- `IX_Clients_TenantId_Phone WHERE IsDeleted = 0` (unique)
- `IX_Clients_TenantId_Email WHERE IsDeleted = 0` (Unique)

---

## Checkes

```SQL
ALTER TABLE Clients
ADD CONSTRAINT CK_Clients_Status
CHECK (Status IN ('Lead', 'Active', 'Paused', 'Completed'));
```

```SQL
ALTER TABLE Clients
ADD CONSTRAINT CK_Clients_Source
CHECK (Source IS NULL OR Source IN ('Manual', 'LinkedIn', 'Website', 'Referral', 'Other'));
```

---

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

---

# STM - DiscoveryCalls Table

## Schema

| Column          | Type           | Description                                                                                |
| --------------- | -------------- | ------------------------------------------------------------------------------------------ |
| Id              | GUID           | Primary key;                                                                               |
| ClientId        | GUID           | Foreign key referencing Clients table; (Required, Indexed)                                 |
| ScheduledAt     | DATETIMEOFFSET | Calling timestamp; (Required, Indexed)                                                     |
| DurationMinutes | INT            | 15, 30, 45, 60, etc. ; (Required)                                                          |
| Status          | NVARCHAR(20)   | Scheduled, Completed, No-Show; (Required, Indexed, Default: 'Scheduled')                   |
| Outcome         | NVARCHAR(20)   | Qualified, NotQualified, NoShow; (Not Required)                                            |
| Notes           | NVARCHAR(MAX)  | Consultant notes about the call; (Not Required)                                            |
| ExternalEventId | NVARCHAR(200)  | ID of the event in Google/Outlook calendar, used for updates/cancellations; (Not Required) |
| CreatedAt       | DATETIMEOFFSET | Creation timestamp; (Required)                                                             |
| UpdatedAt       | DATETIMEOFFSET | Updating timestamp; (Not Required)                                                         |
| IsDeleted       | BIT            | Indicates whether the record is logically deleted; (Required, Default: false)              |
| DeletedAt       | DATETIMEOFFSET | Deleting timestamp; (Not Required)                                                         |
| DeletedBy       | GUID           | Foreign key referencing Users table; (Not Required)                                        |

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

---

# STM - LeadFormSubmissions Table

## Schema

| Column      | Type           | Description                                                  |
| ----------- | -------------- | ------------------------------------------------------------ |
| Id          | GUID           | Primary key;                                                 |
| LeadFormId  | GUID           | Foreign key referencing LeadForms table; (Required, Indexed) |
| FullName    | NVARCHAR(200)  | Lead's name; (Required)                                      |
| Email       | NVARCHAR(320)  | Lead's email; (Not Required)                                 |
| Phone       | NVARCHAR(50)   | Lead's phone; (Not Required)                                 |
| AnswersJson | NVARCHAR(MAX)  | String of JSON object mapping answers to fields; (Required)  |
| SubmittedAt | DATETIMEOFFSET | Submission timestamp; (Required, Indexed)                    |

---

## Indexes

- `IX_LeadFormSubmissions_LeadFormId`
- `IX_LeadFormSubmissions_SubmittedAt`

---

# STM - LeadForms Table

## Schema

| Column      | Type           | Description                                                                   |
| ----------- | -------------- | ----------------------------------------------------------------------------- |
| Id          | GUID           | Primary key;                                                                  |
| TenantId    | GUID           | Foreign key referencing Tenants table; (Required, Indexed)                    |
| Title       | NVARCHAR(200)  | Form name; (Required)                                                         |
| Description | NVARCHAR(500)  | Form description; (Not Required)                                              |
| FieldsJson  | NVARCHAR(MAX)  | JSON array string defining dynamic fields; (Required, Default: '[]')          |
| IsActive    | bit            | Controls public availability; (Required, Default: true)                       |
| CreatedAt   | DATETIMEOFFSET | Creation timestamp; (Required)                                                |
| UpdatedAt   | DATETIMEOFFSET | Updating timestamp; (Not Required)                                            |
| IsDeleted   | BIT            | Indicates whether the record is logically deleted; (Required, Default: false) |
| DeletedAt   | DATETIMEOFFSET | Deleting timestamp; (Not Required)                                            |
| DeletedBy   | GUID           | Foreign key referencing Users table; (Not Required)                           |

---

## Indexes

- `IX_LeadForms_TenantId WHERE IsDeleted = 0`
- `IX_LeadForms_TenantId_IsActive WHERE IsDeleted = 0`

---

# STM - OnboardingTaskTemplates Table

## Schema

| Column       | Type           | Description                                                                   |
| ------------ | -------------- | ----------------------------------------------------------------------------- |
| Id           | GUID           | Primary key;                                                                  |
| ConsultantId | GUID           | Foreign key referencing Users table; (Required, Indexed)                      |
| PackageId    | GUID           | Foreign key referencing Packages table; (Not Required, Indexed)               |
| Title        | NVARCHAR(200)  | Short title describing the onboarding task; (Required)                        |
| Description  | NVARCHAR(MAX)  | Detailed instructions for the task; (Not Required)                            |
| Order        | INT            | Display order for the task in the onboarding workflow; (Required, Default: 0) |
| CreatedAt    | DATETIMEOFFSET | Creation timestamp; (Required)                                                |
| UpdatedAt    | DATETIMEOFFSET | Updating timestamp; (Not Required)                                            |
| IsDeleted    | BIT            | Indicates whether the record is logically deleted; (Required, Default: false) |
| DeletedAt    | DATETIMEOFFSET | Deleting timestamp; (Not Required)                                            |
| DeletedBy    | GUID           | Foreign key referencing Users table; (Not Required)                           |

---

## Indexes

- `IX_OnboardingTaskTemplates_ConsultantId WHERE IsDeleted = 0`
- `IX_OnboardingTaskTemplates_ConsultantId_PackageId WHERE IsDeleted = 0`

---

## Checkes

```SQL
ALTER TABLE OnboardingTaskTemplates
ADD CONSTRAINT CK_OnboardingTaskTemplates_Order
CHECK (Order >= 0);
```

---

# STM - OnboardingTasks Table

## Schema

| Column      | Type           | Description                                                                   |
| ----------- | -------------- | ----------------------------------------------------------------------------- |
| Id          | GUID           | Primary key;                                                                  |
| ProposalId  | GUID           | Foreign key referencing Proposals table; (Required, Indexed)                  |
| Title       | NVARCHAR(200)  | Short title describing the task (e.g., "Sign Coaching Agreement");(Required)  |
| Description | NVARCHAR(MAX)  | Detailed instructions or notes for the task; (Not Required)                   |
| Status      | NVARCHAR(20)   | Pending, Completed, Skipped; (Required, Default: 'Pending')                   |
| Order       | INT            | Display order for tasks in the onboarding workflow; (Required, Default: 0)    |
| DueDate     | DATETIMEOFFSET | Optional deadline for completing the task; (Not Required)                     |
| CompletedAt | DATETIMEOFFSET | Timestamp when the task was completed; (Not Required)                         |
| CreatedAt   | DATETIMEOFFSET | Timestamp when the task was created; (Required)                               |
| UpdatedAt   | DATETIMEOFFSET | Timestamp when the task was last updated; (Not Required)                      |
| IsDeleted   | BIT            | Indicates whether the record is logically deleted; (Required, Default: false) |
| DeletedAt   | DATETIMEOFFSET | Deleting timestamp; (Not Required)                                            |
| DeletedBy   | GUID           | Foreign key referencing Users table; (Not Required)                           |

---

## Indexes

- `IX_OnboardingTasks_ProposalId WHERE IsDeleted = 0`
- `IX_OnboardingTasks_ProposalId_Order WHERE IsDeleted = 0` (unique)

---

## Checkes

```SQL
ALTER TABLE OnboardingTasks
ADD CONSTRAINT CK_OnboardingTasks_Status
CHECK (Status IN ('Pending', 'Completed', 'Skipped'));
```

---

# STM - Packages Table

## Schema

| Column           | Type           | Description                                                                   |
| ---------------- | -------------- | ----------------------------------------------------------------------------- |
| Id               | GUID           | Primary key;                                                                  |
| TenantId         | GUID           | Foreign key referencing Tenants table; (Required, Indexed)                    |
| Name             | NVARCHAR(200)  | Package name (e.g., "Career Accelerator"); (Required)                         |
| Description      | NVARCHAR(MAX)  | Detailed description of the package; (Not Required)                           |
| Price            | DECIMAL(10,2)  | Price in tenant’s currency; (Required)                                        |
| SessionsIncluded | INT            | Number of coaching sessions included; (Required, Default: 0)                  |
| CreatedAt        | DATETIMEOFFSET | Creation timestamp; (Required)                                                |
| UpdatedAt        | DATETIMEOFFSET | Updating timestamp; (Not Required)                                            |
| IsDeleted        | BIT            | Indicates whether the record is logically deleted; (Required, Default: false) |
| DeletedAt        | DATETIMEOFFSET | Deleting timestamp; (Not Required)                                            |
| DeletedBy        | GUID           | Foreign key referencing Users table; (Not Required)                           |

---

## Indexes

- `IX_Packages_TenantId WHERE IsDeleted = 0`

---

## Checkes

```SQL
ALTER TABLE Packages
ADD CONSTRAINT CK_Packages_Price
CHECK (Price >= 0);
```

```SQL
ALTER TABLE Packages
ADD CONSTRAINT CK_Packages_SessionsIncluded
CHECK (SessionsIncluded >= 0);
```

---

# STM - Proposals Table

## Schema

| Column                  | Type           | Description                                                                                      |
| ----------------------- | -------------- | ------------------------------------------------------------------------------------------------ |
| Id                      | GUID           | Primary key;                                                                                     |
| ClientId                | GUID           | Foreign key referencing Clients table; (Required, Indexed)                                       |
| PackageId               | GUID           | Foreign key referencing Packages table; (Required, Indexed)                                      |
| PackageName             | NVARCHAR(200)  | Snapshot of package name at creation; (Required)                                                 |
| PackageDescription      | NVARCHAR(MAX)  | Snapshot of package description at creation; (Not Required)                                      |
| PackagePrice            | DECIMAL(10,2)  | Snapshot of package price at creation; (Required)                                                |
| PackageSessionsIncluded | INT            | Snapshot of package included session at creation; (Required)                                     |
| Status                  | NVARCHAR(20)   | Draft, Sent, Accepted, Rejected; (Required, Default: 'Draft')                                    |
| IsActive                | BIT            | Only one accepted proposal can be "active" for each client at a time; (Required, Default: false) |
| SentAt                  | DATETIMEOFFSET | When the proposal was sent; (Not Required)                                                       |
| RespondedAt             | DATETIMEOFFSET | When the client accepted or rejected; (Not Required)                                             |
| Notes                   | NVARCHAR(MAX)  | Internal consultant notes; (Not Required)                                                        |
| CreatedAt               | DATETIMEOFFSET | Creation timestamp; (Required)                                                                   |
| UpdatedAt               | DATETIMEOFFSET | Updating timestamp; (Not Required)                                                               |
| IsDeleted               | BIT            | Indicates whether the record is logically deleted; (Required, Default: false)                    |
| DeletedAt               | DATETIMEOFFSET | Deleting timestamp; (Not Required)                                                               |
| DeletedBy               | GUID           | Foreign key referencing Users table; (Not Required)                                              |

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

---

# STM - SessionMaterials Table

## Schema

| Column      | Type           | Description                                                 |
| ----------- | -------------- | ----------------------------------------------------------- |
| Id          | GUID           | Primary key;                                                |
| SessionId   | GUID           | Foreign key referencing Sessions table; (Required, Indexed) |
| Title       | NVARCHAR(200)  | Name of the material; (Required)                            |
| Description | NVARCHAR(MAX)  | Optional description; (Not Required)                        |
| FileUrl     | NVARCHAR(500)  | URL to the uploaded file (PDF, docx, etc.); (Required)      |
| FileType    | NVARCHAR(50)   | File type: pdf, docx, pptx, image, etc.; (Required)         |
| UploadedAt  | DATETIMEOFFSET | When the material was uploaded; (Required)                  |
| UploadedBy  | GUID           | Consultant who uploaded the material; (Required)            |
| UpdatedAt   | DATETIMEOFFSET | Updating timestamp; (Not Required)                          |

---

## Indexes

- `IX_SessionMaterials_SessionId`

---

# STM - Sessions Table

## Schema

| Column          | Type           | Description                                                                               |
| --------------- | -------------- | ----------------------------------------------------------------------------------------- |
| Id              | GUID           | Primary key;                                                                              |
| ConsultantId    | GUID           | Foreign key referencing Users table; (Required, Indexed)                                  |
| ProposalId      | GUID           | Foreign key referencing Proposals table; (Required, Indexed)                              |
| Title           | NVARCHAR(200)  | Short title of the session; (Required)                                                    |
| Description     | NVARCHAR(MAX)  | Detailed description of the session; (Not Required)                                       |
| StartTime       | DATETIMEOFFSET | Scheduled start time; (Required)                                                          |
| EndTime         | DATETIMEOFFSET | Scheduled end time; (Required)                                                            |
| Status          | NVARCHAR(50)   | Scheduled, Completed, Cancelled; (Required, Default: 'Scheduled')                         |
| ExternalEventId | NVARCHAR(200)  | The event ID returned by Google/Outlook for syncing updates and deletions; (Not Required) |
| ReminderJobId   | NVARCHAR(100)  | The Hangfire job identifier for the scheduled reminder; (Not Required)                    |
| MeetingUrl      | NVARCHAR(500)  | Link for online sessions (Zoom, Teams, etc.); (Not Required)                              |
| Location        | NVARCHAR(300)  | Physical location for in‑person sessions; (Not Required)                                  |
| IsOnline        | BIT            | Whether the session is online; (Required, Default: 0)                                     |
| CreatedAt       | DATETIMEOFFSET | Creation timestamp; (Required)                                                            |
| UpdatedAt       | DATETIMEOFFSET | Updating timestamp; (Not Required)                                                        |
| IsDeleted       | BIT            | Indicates whether the record is logically deleted; (Required, Default: false)             |
| DeletedAt       | DATETIMEOFFSET | Deleting timestamp; (Not Required)                                                        |
| DeletedBy       | GUID           | Foreign key referencing Users table; (Not Required)                                       |

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

---

# STM – Tenants Table

## Schema

| Column    | Type           | Description                                  |
| --------- | -------------- | -------------------------------------------- |
| Id        | GUID           | Primary key                                  |
| Name      | NVARCHAR(200)  | Business name (Required)                     |
| Domain    | NVARCHAR(50)   | Custom subdomain (Required, Unique, Indexed) |
| CreatedAt | DATETIMEOFFSET | Creation timestamp (Required)                |
| UpdatedAt | DATETIMEOFFSET | Updating timestamp (Not Required)            |

---

## Indexes

- `IX_Tenant_Domain` (unique)

---

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

---

# STM - Users Table

## Schema

| Column       | Type           | Description                                                                                               |
| ------------ | -------------- | --------------------------------------------------------------------------------------------------------- |
| Id           | GUID           | Primary key;                                                                                              |
| TenantId     | GUID           | Foreign key referencing Tenants table; (Required, Indexed)                                                |
| Email        | NVARCHAR(320)  | User’s login email; (Required, Indexed)                                                                   |
| PasswordHash | NVARCHAR(500)  | Securely stored password hash; (Required)                                                                 |
| FullName     | NVARCHAR(200)  | User’s full name (Required)                                                                               |
| Role         | NVARCHAR(50)   | Admin, Consultant, Assistant; (Required)                                                                  |
| IsVerified   | BIT            | Indicates whether the user’s email has been successfully verified through OTP; (Required, Default: false) |
| IsActive     | BIT            | Admins can deactivate consultants or assistants. Inactive users cannot log in ;(Required, Default: true)  |
| UpdatedAt    | DATETIMEOFFSET | Updating timestamp; (Not Required)                                                                        |
| CreatedAt    | DATETIMEOFFSET | Creation timestamp; (Required)                                                                            |
| LastLoginAt  | DATETIMEOFFSET | Last login timestamp; (Not Required)                                                                      |

---

## Indexes

- `IX_Users_TenantId`
- `IX_Users_Email` (unique)

---

## Check

```SQL
ALTER TABLE Users
ADD CONSTRAINT CK_Users_Role
CHECK (Role IN ('Admin', 'Consultant', 'Assistant'));
```

---

# STM - OTPs Table

## Schema

| Column     | Type           | Description                                                                         |
| ---------- | -------------- | ----------------------------------------------------------------------------------- |
| Id         | GUID           | Primary key;                                                                        |
| Email      | nvarchar(256)  | Email address the OTP was sent to; (Required, Indexed)                              |
| UserId     | GUID           | Foreign key referencing Users table. Linked user if exists; (Not Required, Indexed) |
| HashedCode | NVARCHAR(256)  | Securely hashed OTP code; (Required)                                                |
| ExpiresAt  | DATETIMEOFFSET | Expiration timestamp for OTP validity; (Required)                                   |
| Attempts   | INT            | Number of verification attempts used; (Required)                                    |
| Purpose    | nvarchar(32)   | Signup, ForgotPassword, ResetPassword, ChangeEmail; (Required)                      |

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

---

# STM - RefreshTokens Table

## Schema

| Column    | Type           | Description                                                                                              |
| --------- | -------------- | -------------------------------------------------------------------------------------------------------- |
| Id        | GUID           | Primary key;                                                                                             |
| UserId    | GUID           | Foreign key referencing Users table; (Required, Indexed)                                                 |
| TokenHash | NVARCHAR(256)  | Hashed refresh token value. Never store raw tokens; (Required, Indexed)                                  |
| ExpiresAt | DATETIMEOFFSET | Expiration timestamp for the refresh token; (Required, Indexed)                                          |
| CreatedAt | DATETIMEOFFSET | Timestamp when the token was created; (Required)                                                         |
| DeviceId  | NVARCHAR(64)   | A unique, stable identifier generated on the client and used to distinguish devices/browsers; (Required) |
| IpAddress | NVARCHAR(64)   | IP address used when the token was created; (Not Required)                                               |
| UserAgent | NVARCHAR(256)  | Browser or device user agent string; (Not Required)                                                      |

---

## Indexes

- `IX_RefreshTokens_UserId`
- `IX_RefreshTokens_TokenHash` (unique)
- `IX_RefreshTokens_ExpiresAt`
- `IX_RefreshTokens_UserId_DeviceId`

---

# STM - UserLoginAttempts Table

## Schema

| Column       | Type           | Description                                                                                                        |
| ------------ | -------------- | ------------------------------------------------------------------------------------------------------------------ |
| Id           | GUID           | Primary key;                                                                                                       |
| UserId       | GUID           | Foreign key referencing Users table, NULL when the email does not match any existing user; (Not Required, Indexed) |
| Email        | NVARCHAR(256)  | The identifier the user attempted to log in with. Stored even if no matching user exists; (Required, Indexed)      |
| IsSuccessful | BIT            | Indicates whether the login attempt succeeded; (Required)                                                          |
| IpAddress    | NVARCHAR(64)   | IP address of the login attempt; (Not Required, Indexed)                                                           |
| UserAgent    | NVARCHAR(512)  | Raw browser user agent string; (Not Required)                                                                      |
| DeviceId     | NVARCHAR(64)   | Client‑generated device identifie; (Not Required, Indexed)                                                         |
| CreatedAt    | DATETIMEOFFSET | Timestamp of the login attempt; (Required, Indexed)                                                                |

---

## Indexes

- `IX_UserLoginAttempts_UserId`
- `IX_UserLoginAttempts_Email`
- `IX_UserLoginAttempts_IpAddress`
- `IX_UserLoginAttempts_DeviceId`
- `IX_UserLoginAttempts_CreatedAt`

---

# STM - UserSessions Table

## Schema

| Column         | Type           | Description                                                                                                                 |
| -------------- | -------------- | --------------------------------------------------------------------------------------------------------------------------- |
| Id             | GUID           | Primary key;                                                                                                                |
| UserId         | GUID           | Foreign key referencing Users table; (Required, Indexed)                                                                    |
| DeviceId       | NVARCHAR(64)   | Unique device/browser identifier. Matches the DeviceId stored in RefreshTokens; (Required)                                  |
| DeviceName     | NVARCHAR(128)  | Human‑readable device label, derived from UserAgent and used only for UI display; (Not Required)                            |
| CreatedAt      | DATETIMEOFFSET | Timestamp when the token was created; (Required)                                                                            |
| IsActive       | BIT            | Set 0 on log out from this device or all devices, password/email change, expiration/revocation of refresh-token; (Required) |
| LastActivityAt | DATETIMEOFFSET | Updated on login, refresh-token rotation, any authenticated API call; (Not Required)                                        |

---

## Indexes

- `IX_UserSessions_UserId`
- `IX_UserSessions_UserId_DeviceId`
- `IX_UserSessions_DeviceId`
