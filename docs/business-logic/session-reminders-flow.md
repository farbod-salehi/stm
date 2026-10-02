# STM – Session Reminders

## Overview

**Session reminders** notify consultants before a session starts.
The reminder system uses **Hangfire** as a background job scheduler to schedule, reschedule, and cancel reminder jobs reliably.

The `ReminderJobId` field in the `Sessions` table stores the Hangfire job identifier for the scheduled reminder.

**Why a background job is required:**

- Reminders must fire at a specific future time
- They must run even if the user is offline
- They must survive app restarts
- They must be cancellable and reschedulable
- They must run independently of API requests

---

## Hangfire Library

### Overview

**Hangfire** is a background job processing library for .NET that provides:

- precise scheduling
- persistent job storage
- automatic retries
- distributed execution
- a built‑in monitoring dashboard

Hangfire stores scheduled jobs in its own internal tables (SQL Server, PostgreSQL, Redis), ensuring reminders are reliable even if the application restarts.

This makes Hangfire ideal for session reminders.

### Installation & Configuration

#### 1. Install Hangfire

Install the required NuGet packages:

```bash
dotnet add package Hangfire
dotnet add package Hangfire.AspNetCore
dotnet add package Hangfire.SqlServer
```

#### 2. Configure Hangfire in Program.cs

a) Add Hangfire services:

```csharp
builder.Services.AddHangfire(config =>
{
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"));
});
```

b) Add Hangfire server:

```csharp
builder.Services.AddHangfireServer();
```

c) Add Hangfire dashboard (optional but recommended):

```csharp
app.UseHangfireDashboard("/hangfire");
```

This gives you a UI to monitor reminders, retries, failures, and scheduled jobs.

#### 3. Create Database

You create an empty database: STM_Hangfire for example, and Hangfire automatically creates all required tables. This rule applies to both development and production.

When your app starts and Hangfire connects to the database, it checks:

- whether the Hangfire schema exists
- whether the required tables exist
- whether the schema version matches

If not, Hangfire **automatically creates** them.

#### 4. Add Hangfire Connection String

In `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "HangfireConnection": "Server=.;Database=STM_Hangfire;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

---

### Workflow

#### 1. Schedule Reminder Jobs

This method is defined in `ReminderService.cs`, and is called after a session is created:

```csharp
public void ScheduleSessionReminder(Session session, NotificationPreferences prefs)
{
    var reminderTimeUtc = session.StartTime
        .AddMinutes(-prefs.SessionReminderMinutes);

    var jobId = BackgroundJob.Schedule(
        () => SendSessionReminder(session.Id),
        reminderTimeUtc
    );

    session.ReminderJobId = jobId;
}
```

**Behavior:**

- Calculate reminder time: `ReminderTime = StartTime - ConsultantReminderOffset`
- Schedule a Hangfire job to run `SendSessionReminder(session.Id)` at `ReminderTime`
- Hangfire stores the job internally (in its own tables)
- The job ID is returned by Hangfire
- The returned `jobId` is stored in: `session.ReminderJobId`
- record an event in the `ClientTimeline` table:
  ```csharp
  clientTimeline.EventType = (int)TimelineEventType.SessionReminderScheduled;
  ```
  `MetadataJson`:
  ```json
  {
    "ProposalId": "guid",
    "SessionId": "guid",
    "ReminderScheduledAt": "2025-01-09T10:00:00Z",
    "ReminderFor": "2025-01-10T14:00:00Z",
    "ReminderMinutesBefore": 60,
    "ReminderType": "Email", // or other options
    "ScheduledBy": "Consultant"
  }
  ```

#### 2. Reschedule Reminder Jobs

This method is defined in `ReminderService.cs`, and is called when a session is updated:

```csharp
public void RescheduleSessionReminder(Session session, NotificationPreferences prefs)
{
    if (!string.IsNullOrEmpty(session.ReminderJobId))
        BackgroundJob.Delete(session.ReminderJobId);

    ScheduleSessionReminder(session, prefs);
}
```

**Steps:**

- Load `ReminderJobId` from the session
- Delete the existing Hangfire job
- Call `ScheduleSessionReminder` to
  - Recalculate `ReminderTime`
  - Schedule a new Hangfire job
  - Store the new `ReminderJobId`
- record an event in the `ClientTimeline` table:
  ```csharp
  clientTimeline.EventType = (int)TimelineEventType.SessionReminderRescheduled;
  ```
  `MetadataJson`:
  ```json
  {
    "ProposalId": "guid",
    "SessionId": "guid",
    "OldReminderAt": "2025-01-09T10:00:00Z",
    "NewReminderAt": "2025-01-09T12:00:00Z",
    "ReminderMinutesBefore": 120,
    "ReminderType": "SMS", // or other options
    "RescheduledBy": "Consultant",
    "RescheduledAt": "2025-01-08T09:30:00Z"
  }
  ```

#### 3. Cancel Reminder Jobs

This method is defined in `ReminderService.cs`, and is called when a session is cancelled:

```csharp
public void CancelSessionReminder(Session session)
{
    if (!string.IsNullOrEmpty(session.ReminderJobId))
        BackgroundJob.Delete(session.ReminderJobId);

    session.ReminderJobId = null;
}
```

**Steps**

- Load `ReminderJobId` from the session
- Delete the Hangfire job
- Set `ReminderJobId = NULL`
- record an event in the `ClientTimeline` table:
  ```csharp
  clientTimeline.EventType = (int)TimelineEventType.SessionReminderCancelled;
  ```
  `MetadataJson`:
  ```json
  {
    "ProposalId": "guid",
    "SessionId": "guid",
    "ReminderScheduledAt": "2025-01-09T10:00:00Z",
    "CancelledAt": "2025-01-08T09:30:00Z",
    "CancelledBy": "Consultant"
  }
  ```

#### 4 Execute Reminder Job

This method is defined in `ReminderService.cs`, and is called by Hangfire at the scheduled time:

```csharp
public async Task SendSessionReminder(Guid sessionId)
{
    var session = await _sessionRepository.Get(sessionId);

    if (session == null || session.Status != SessionStatus.Scheduled)
        return;

    var prefs = await _notificationPreferencesRepository.Get(session.ConsultantId);

    await _notificationService.SendReminder(session, prefs);
}
```

**Steps:**

- Load session by ID
- Verify:
  - Session exists
  - `session.Status == Scheduled`
- Load consultant `NotificationPreferences`
- Send reminder notification (push/email/SMS)
- Hangfire marks job as completed
- record an event in the `ClientTimeline` table:
  ```csharp
  clientTimeline.EventType = (int)TimelineEventType.SessionReminderSent;
  ```
  `MetadataJson`:
  ```json
  {
    "ProposalId": "guid",
    "SessionId": "guid",
    "ReminderScheduledAt": "2025-01-09T10:00:00Z",
    "ReminderSentAt": "2025-01-09T10:01:00Z",
    "ReminderType": "Email", // or other options
    "SentBy": "System"
  }
  ```
