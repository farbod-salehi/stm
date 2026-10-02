# STM - LeadForms Table

## Schema

| Column      | Type          | Description                                                                   |
| ----------- | ------------- | ----------------------------------------------------------------------------- |
| Id          | GUID          | Primary key;                                                                  |
| TenantId    | GUID          | Foreign key referencing Tenants table; (Required, Indexed)                    |
| Title       | NVARCHAR(200) | Form name; (Required)                                                         |
| Description | NVARCHAR(500) | Form description; (Not Required)                                              |
| FieldsJson  | NVARCHAR(MAX) | JSON array string defining dynamic fields; (Required, Default: '[]')          |
| IsActive    | bit           | Controls public availability; (Required, Default: true)                       |
| CreatedAt   | DATETIME2(0)  | Creation timestamp; (Required)                                                |
| UpdatedAt   | DATETIME2(0)  | Updating timestamp; (Not Required)                                            |
| IsDeleted   | BIT           | Indicates whether the record is logically deleted; (Required, Default: false) |
| DeletedAt   | DATETIME2(0)  | Deleting timestamp; (Not Required)                                            |
| DeletedBy   | GUID          | Foreign key referencing Users table; (Not Required)                           |

---

## Indexes

- `IX_LeadForms_TenantId WHERE IsDeleted = 0`
- `IX_LeadForms_TenantId_IsActive WHERE IsDeleted = 0`
