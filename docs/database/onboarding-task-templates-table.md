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
