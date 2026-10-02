# STM – Calendar Integration

## Overview

Calendar Integration synchronizes App **Sessions** with the consultant’s external calendar (Google or Outlook).
It ensures that every scheduled session **automatically** appears in the consultant’s personal calendar, stays updated when edited, and is removed when cancelled.

Calendar Integration is only allowed when the consultant has an active, valid calendar connection

---

## Workflow

### 0. Precondition: Consultant Connects Calendar

Before any session can sync:

- Consultant connects Google/Outlook calendar
- App receives an access token + refresh token
- App stores them securely
- App marks consultant as "Calendar Connected"

**All future calendar operations require:**

- consultant has calendar integration enabled
- token is valid (or refreshed automatically)

### 1. Session Created => Validate Access => Create Calendar Event

**When a new session is created:**

1. Check consultant has calendar integration enabled

2. Validate token
   - If expired => refresh
   - If refresh fails => disable integration

3. Create event in external calendar
4. Store the returned event ID in the `Sessions` table (`ExternalEventId`)

5. Session and calendar event are now linked

**Event includes:**

- title
- description
- StartTimeUtc / EndTimeUtc
- meeting link or location

### 2. Session Updated => Validate Access => Update Calendar Event

**When a session is edited:**

- Check consultant has calendar integration enabled
- Validate token
- Use `ExternalEventId` (from `Sessions` table) to update the event in the external calendar
- Save updated event metadata

**Fields that trigger updates:**

- title
- description
- start/end time
- meeting link
- location

### 3. Session Cancelled => Validate Access => Delete Calendar Event

**When a session is cancelled:**

- Check consultant has calendar integration enabled
- Validate token
- Use `ExternalEventId` (from `Sessions` table) to delete the event
- Set `ExternalEventId = NULL` (in `Sessions` table)

**This prevents outdated events from remaining in the consultant’s calendar.**

### 4. Session Completed

Completed events stay in the consultant’s calendar for history.

### 5. Session Materials Uploaded

Uploading materials does not affect calendar integration.
