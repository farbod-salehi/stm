# STM – Onboarding Task Templates

## Overview

**Onboarding Task Templates** define reusable onboarding steps that consultants can apply automatically when a proposal is accepted.
They allow each consultant to design their own onboarding workflow, independent of other consultants in the same tenant.

---

## Workflow

### 1. Load Templates

Consultant opens the template configuration page.

Frontend requests: `GET /api/onboarding-task-templates`

Backend:

- Reads consultantId from JWT.
- Returns all templates for that consultant, ordered by `Order ASC`.

### 2. Bulk Update Template List

Consultant can:

- add new templates
- delete existing templates
- reorder templates

Frontend sends the entire updated list: `PUT /api/onboarding-task-templates/bulk-update`

Backend:

- inserts templates with `id = null`
- deletes templates missing from payload
- updates `Order` for all templates
- returns updated list
- performs all changes in a single transaction

Bulk update handles list-level changes only. Editing template details is separate.

### 3. Edit a Specific Template

Consultant clicks a template to modify its details.

Frontend sends: `PUT /api/onboarding-task-templates/{templateId}`

Backend updates:

- Title
- Description
- PackageId
- UpdatedAt
