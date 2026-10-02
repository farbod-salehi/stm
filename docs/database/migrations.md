**Note:** _Entries are listed in chronological order. New migrations must be appended at the end._
<br />

## Migration: Add Soft Delete Fields to Clients

**Date:** 2026-08-02  
**Author:** Farbod  
**Version:** DB v1.4

### Summary

Introduce logical delete support for Clients table.

### Changes

- Added column `IsDeleted` (bit, default false, not nullable)
- Added column `DeletedAt` (datetime, nullable)
- Added column `DeletedBy` (uniqueidentifier, nullable)
- Added global query filter: `IsDeleted = false`

### Reason

To support safe deletion, auditability, and error recovery without losing historical client data.

### Dependencies

- Backend must update delete workflow to set these fields.
- ClientTimeline must support `TriggerEvent = LogicalDelete`.
- Frontend must add “Deleted Clients” view.
- EF Core model updated with global query filter.

### Rollback

- Remove `IsDeleted`, `DeletedAt`, `DeletedBy` columns.
- Remove global query filter from EF Core configuration.

### Testing Notes

- Verify deleted clients no longer appear in default queries.
- Verify `.IgnoreQueryFilters()` returns deleted clients.
- Verify delete and restore workflows update fields correctly.

---

# New Changes
