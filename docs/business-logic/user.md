# STM – Users

## Overview

The **Users** table stores all staff accounts within a tenant (e.g., admins, consultants, assistants), and every user belongs to exactly one tenant.

---

## Role field

### Purpose

The **Role** defines the user’s function within a tenant (e.g., Admin, Consultant, Assistant).

### Roles Descriptions

- **Admin:** Created only through signup, and represents the tenant owner. They have full access to tenant settings, and management of staff, client, proposal and session.
- **Consultant:** Added by Admin and belongs to the same tenant. They can create and edit clients, proposals, sessions and documents.
- **Assistant:** Added by Admin and belongs to the same tenant. They support consultants and have limited permissions: view clients, view proposals, schedule sessions, and upload documents.

### Implementation

To ensure clarity, safety, and consistency across code and database layers, the following design is used:

- **Enum in Code**:  
  In C#, the role is represented as a strongly‑typed `enum` (`UserRole`). This prevents typos and invalid assignments at compile time.

```csharp
  public enum UserRole
  {
      Admin,
      Consultant,
      Assistant
  }

  public class User
  {
      ...
      public UserRole Role { get; set; }
  }
```

- **NVARCHAR in Database:**
  The enum is stored as a string (NVARCHAR(50)) for readability and reporting. EF Core maps the enum values to their string names:

  ```csharp
  modelBuilder.Entity<User>()
      .Property(u => u.Role)
      .HasConversion<string>()   // store enum as NVARCHAR
      .HasMaxLength(50);

  ```

- **CHECK Constraint for Integrity:**
  A CHECK constraint is added via EF Core to enforce that only valid role values are stored:

  ```csharp
  modelBuilder.Entity<User>()
      .HasCheckConstraint("CK_Users_Role", "Role IN ('Admin','Consultant','Assistant')");

  ```

**Note:** Localization of role names should be handled in the frontend/UI layer. The database always stores canonical English values.

---

# STM – UserSettings

## Overview

The **UserSettings** table stores per‑user preferences and configuration options within a tenant.  
Each user must have a corresponding `UserSettings` record to ensure consistent behavior across scheduling, notifications, and localization features.

---

## Timezone field

### Purpose

The **Timezone** field defines the user’s local time zone, ensuring that all scheduling, reminders, and timestamp displays are correctly converted from UTC into the user’s preferred local time.

### Backend

- Stores timezone (Asia/Tehran)

- Stores timestamps in UTC

- Returns timestamps in UTC

- Uses timezone only for business logic (reminders, scheduling)

### Frontend

- Reads timezone and UTC from backend

- Converts UTC to local time

- Formats for display

- Applies local rules

### Backend Implementation (C# / EF Core)

**Model:**

```csharp
public class UserSettings
{
    ...
    public string Timezone { get; set; } = "UTC"; // default
}
```

**EF Core Configuration:**

```csharp
 modelBuilder.Entity<UserSettings>()
    .Property(u => u.Timezone)
    .IsRequired()
    .HasMaxLength(100); // NVARCHAR(100)

```

**Validation:**
Implement the timezone validator as a validation service in the Application layer, and use it there whenever user input for timezone is accepted or updated. The Application layer must enforce that only valid IANA timezone identifiers are stored. If the provided timezone is missing or invalid, default to "UTC".

```csharp
// Application/Interfaces/Services/ITimezoneValidationService.cs
public interface ITimezoneValidationService
{
    bool IsValidTimezone(string timezone);
}

// Application/Services/TimezoneValidationService.cs
public class TimezoneValidationService : ITimezoneValidationService
{
    public bool IsValidTimezone(string timezone)
    {
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
    }
}
```

**How to use it:**
Even though backend does not display time, it still needs timezone for scheduling jobs, calculating deadlines, and so on.

```csharp
 var utcNow = DateTime.UtcNow;
 var userTz = TimeZoneInfo.FindSystemTimeZoneById(userSettings.Timezone);
 var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, userTz);
```

### Frontend Implementation (Angular/TypeScript)

**UI Selection**
Provide a dropdown of IANA timezone IDs (e.g., "Asia/Tehran", "America/New_York") for the user to choose.

**Conversion with moment‑timezone:**
Handle localization and display using moment‑timezone library, inside a shared DateTime utility service (e.g., datetime.service.ts). It must not be placed inside components.

```typescript
import * as moment from "moment-timezone";
import { Injectable } from "@angular/core";

@Injectable({ providedIn: "root" })
export class DateTimeService {
  formatUserTime(utcDate: string, timezone: string): string {
    return moment.utc(utcDate).tz(timezone).format("YYYY-MM-DD HH:mm");
  }
}
```

**Usage in a component**
Use this service whenever displaying localized timestamps, converting UTC to the user's timezone. This keeps time conversion logic centralized, reusable, and consistent across the application.

```typescript
ngOnInit() {
  this.localTime = this.dateTime.formatUserTime(
    this.session.startUtc,     // session is an example here
    this.userSettings.timezone
  );
}
```

---

## NotificationPreferences field

### Purpose

The **NotificationPreferences** field stores the user's notification configuration as JSON in an `NVARCHAR(MAX)` column. It defines which channels the user wants to receive notifications through (email, SMS, in-app, push), which events they want alerts for, and how early reminders should be sent.
The frontend sends these preferences as JSON, the backend validates and stores them, and uses them for scheduling reminders and delivering notifications.

**Default Value:**

```csharp
{
    "email": true,
    "sms": false,
    "inApp": true,
    "push": false,
    "sessionReminderMinutes": 15,
    "dailySummary": false,
    "clientMessageAlerts": true,
    "taskAssignedAlerts": true
}
```

**Why JSON string?**

- Notification settings change over time, JSON avoids schema change.

- Different users may have different combinations of preferences.

- Easy to serialize/deserialize in frontend and backend.

### Backend (.NET) - Serialize & Deserialize

**Model:**

```csharp
public class NotificationPreferencesDto
{
    public bool Email { get; set; }
    public bool Sms { get; set; }
    public bool InApp { get; set; }
    public bool Push { get; set; }
    public int SessionReminderMinutes { get; set; }
    public bool DailySummary { get; set; }
    public bool ClientMessageAlerts { get; set; }
    public bool TaskAssignedAlerts { get; set; }
}
```

**Validation:**
Add a validation service in the Application layer, and use it in `UserSettingsService`, inside `UpdateUserSettingsAsync` method.

```csharp
public class NotificationPreferencesValidator
{
    public void Validate(NotificationPreferencesDto prefs)
    {
        if (prefs == null)
            throw new ValidationException("Notification preferences are required.");

        // Reminder must be within allowed range
        if (prefs.SessionReminderMinutes < 0 || prefs.SessionReminderMinutes > 1440)
            throw new ValidationException("SessionReminderMinutes must be between 0 and 1440.");

        // At least one channel must be enabled
        if (!prefs.Email && !prefs.Sms && !prefs.InApp && !prefs.Push)
            throw new ValidationException("At least one notification channel must be enabled.");

        // Daily summary is optional — no validation needed
        // ClientMessageAlerts and TaskAssignedAlerts are booleans — always valid
    }
}
```

**Serialize (store JSON in DB):**
The frontend sends NotificationPreferences as a structured JSON object, not as a string. The backend validates this object and serializes it into a JSON string.

```csharp
var prefsJson = JsonSerializer.Serialize(dto.NotificationPreferences);
userSettings.NotificationPreferencesJson = prefsJson;
```

**Deserialize:**
When reading from DB, backend gets a string, not an object. So it must convert it back:

```csharp
var prefs = JsonSerializer.Deserialize<NotificationPreferences>(userSettings.NotificationPreferencesJson);
```

### Frontend (Angular & TypeScript)

The frontend sends NotificationPreferences as a structured JSON object, not as a string.

**Receive JSON string from backend:**

```typescript
{
  "notificationPreferencesJson": "{"email":true,"sms":false,...}"
}
```

**Parse it:**

```typescript
this.notificationPreferences = JSON.parse(response.notificationPreferencesJson);
```

---

## CalendarIntegrationToken field

### Purpose

Staff users often rely on external calendars (Google/Outlook) to manage their daily schedule.
`CalendarIntegrationToken` enables STM to:

- Push sessions to the user’s external calendar

- Update external calendar events when sessions are rescheduled

- Delete external calendar events when sessions are cancelled

- Fetch external events (optional future feature)

- Maintain a persistent authenticated connection

Clients do not use this field. Only staff users need calendar integration.

**Why JSON string?**

- Tokens differ between providers (Google vs Outlook)

- Tokens may include multiple fields (access token, refresh token, expiry)

- JSON allows flexible schema changes without DB migrations

### Frontend Workflow

- **User connects their calendar (OAuth)**
  User clicks "Connect Google Calendar" or "Connect Outlook Calendar". Frontend redirects to OAuth provider, and user grants permission. OAuth provider returns authorization code, and frontend sends code to backend.
- **Frontend receives:**
  ```typescript
  {
   "calendarIntegrationEnabled": true
  }
  ```
- **Frontend shows “Connected” status**
  - Frontend does not store tokens.
  - Frontend does not handle refresh logic.
  - Frontend only shows UI state.
- **Frontend sends "Disconnect Calendar"**
  ```typescript
  {
    "disconnectCalendar": true
  }
  ```
  And backend clears the token:
  ```csharp
   userSettings.CalendarIntegrationToken = null;
  ```

### Backend Workflow

- **OAuth token Model**

  ```csharp
  public class CalendarIntegrationToken
  {
      public string Provider { get; set; } // google | outlook
      public string AccessToken { get; set; }
      public string RefreshToken { get; set; }
      public DateTime ExpiresAt { get; set; }
  }
  ```

- **Receives OAuth tokens**

  ```csharp
  {
      "provider": "google",
      "accessToken": "...",
      "refreshToken": "...",
      "expiresAt": "2026-07-09T18:00:00Z"
  }
  ```

- **Validates the token**
  - Checks provider is allowed
  - Checks required fields exist
  - Checks expiry format

- **Serializes JSON into string**

  ```csharp
  userSettings.CalendarIntegrationToken = JsonSerializer.Serialize(dto.CalendarIntegrationToken);
  ```

  And stores it in DB

- **Uses the token for calendar operations**
  - When a session is created, pushes event to Google/Outlook

  - When a session is rescheduled, updates external event

  - When a session is cancelled, deletes external event

  - When token expires, refreshes using refreshToke

- **Deserializes when needed**
  ```csharp
  var token = JsonSerializer.Deserialize<CalendarIntegrationToken>(userSettings.CalendarIntegrationToken);
  ```
