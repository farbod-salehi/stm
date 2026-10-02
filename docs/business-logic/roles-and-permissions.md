# STM – Roles & Permissions

## Overview

App uses a role‑based authorization model with optional fine‑grained permissions. Authorization is enforced through:

- **Access Token (JWT)** => contains `Role`, `TenantId`, and `Permissions`
- Backend API policies
- Frontend route guards & feature flags

Roles define _who_ the user is.
Permissions define _what_ the user can do.

---

## Roles

App currently supports **3 roles**:

### 1. Admin

Tenant owner and full‑access user.

Admins can:

- Manage tenant settings
- Manage billing
- Create staff (Consultants, Assistants)
- Manage clients
- Manage sessions
- Manage documents
- View analytics
- Access all data within the tenant

Admins authenticate using OTP, not password.
PasswordHash is stored for future privileged actions.

### 2. Consultant

Primary service provider within the tenant.

Consultants can:

- Manage their assigned clients
- Create and manage sessions
- Upload and manage client documents
- View client progress
- View analytics related to their clients

Consultants **cannot**:

- Manage tenant settings
- Manage billing
- Create staff
- Access other consultants' clients (unless explicitly assigned)

### 3. Assistant

Support role with limited access.

Assistants can:

- View assigned clients
- Schedule sessions
- Upload documents
- Manage basic client information

Assistants **cannot**:

- Manage tenant settings
- Manage billing
- Create staff
- View analytics
- Access consultant‑only features

---

## Permissions

Permissions allow App to grow beyond simple role definitions.

Each permission is a boolean flag included in the JWT:

```csharp
public List<string> Permissions { get; set; }
```

It works perfectly with JWT and ASP. NET Authorization Policies:

```json
"permissions": ["ManageClients", "ViewSessions"]
```

```csharp
options.AddPolicy("ManageClients", policy =>
    policy.RequireClaim("permissions", "ManageClients"));
);
```

<br />

### Role / Permission Matrix

| Permission      | Description                                 | Admin | Consultant | Assistant |
| --------------- | ------------------------------------------- | :---: | :--------: | :-------: |
| ManageClients   | Create, update, delete clients              |   ✔   |     ✔      |    ❌     |
| ViewClients     | View client profiles                        |   ✔   |     ✔      |     ✔     |
| ManageSessions  | Create, update, delete sessions             |   ✔   |     ✔      |     ✔     |
| ViewSessions    | View session history                        |   ✔   |     ✔      |     ✔     |
| ManageDocuments | Upload, delete, and manage client documents |   ✔   |     ✔      |     ✔     |
| ViewDocuments   | View client documents                       |   ✔   |     ✔      |     ✔     |
| ManageStaff     | Create and manage consultants/assistants    |   ✔   |     ❌     |    ❌     |
| ManageTenant    | Update tenant settings                      |   ✔   |     ❌     |    ❌     |
| ManageBilling   | Access billing and subscription settings    |   ✔   |     ❌     |    ❌     |
| ViewAnalytics   | Access analytics dashboards                 |   ✔   |     ✔      |    ❌     |

---

## JWT Integration

The Access Token includes:

```json
{
  UserId,
  TenantId,
  Role,
  permissions: [...],
  exp,
  iat
}
```

Backend authorization uses:

- `[Authorize(Roles = "Admin")]`
- `[Authorize(Permissions = "ManageClients")]`

Frontend uses:

- Angular route guards
- Feature flags
- Conditional rendering

---

## Multi‑Tenant Isolation

All roles and permissions are scoped to a single tenant:

- A user belongs to exactly one tenant
- A user cannot access data from other tenants
- JWT contains `TenantId`
- Backend enforces tenant isolation on every request
