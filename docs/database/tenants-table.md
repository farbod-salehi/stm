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
