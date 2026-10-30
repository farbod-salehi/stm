# STM – Tenant

## Overview

A Tenant represents a single customer business that subscribes to the platform.
Each tenant owns its own isolated workspace, including users, clients, workflows, automations, sessions, deliverables, and analytics.

---

## Domain

### Purpose

Domain is the unique workspace address for each tenant:

` https://{domain}.stmapp.com`

This value is chosen by the customer (e.g., careercoach). This field enables:

- Public pages before login (e.g., Landing page, Pricing page, Public booking page, etc.)
- Custom domains. STM later will support `https://careercoach.com` instead of `https://careercoach.stmapp.com`.

### Characteristics

- Must be unique across tenants
- Lowercase only
- Allowed characters: a-z, 0-9, hyphen (-)
- Length: 3-50 characters
- No leading/trailing hyphens
- Reserved words blocked: www, api, admin, login, support, stmapp

### Tenant isolation

The middleware will look like:

```csharp
 var domain = context.Request.Host.Host; //careercoach.stmapp.com
 var tenant = await _tenantService.GetTenantByDomain(domain);
 context.Items["Tenant"] = tenant;
```

Then every repository uses:

```csharp
 ... WHERE TenantId = context.Items["Tenant"].Id
```
