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
