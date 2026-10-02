# STM – Tenants Table

## Schema

| Column    | Type          | Description                                  |
| --------- | ------------- | -------------------------------------------- |
| Id        | GUID          | Primary key                                  |
| Name      | NVARCHAR(200) | Business name (Required)                     |
| Domain    | NVARCHAR(50)  | Custom subdomain (Required, Unique, Indexed) |
| CreatedAt | DATETIME2(0)  | Creation timestamp (Required)                |
| UpdatedAt | DATETIME2(0)  | Updating timestamp (Not Required)            |

---

## Indexes

- `IX_Tenant_Domain` (unique)
