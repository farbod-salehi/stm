# STM – Lead Capture

## Overview

Lead Capture is the entry point of the client lifecycle. It is the process through which potential clients provide their initial information to the consultant using a customizable intake form.

Lead Capture consists of two main components:

1. **Lead Forms:** Customizable intake forms created by the `Admin` or `Consultant`, and stored in the `LeadForms` table.
2. **Lead Form Submissions:** Every time a potential client fills out a Lead Form, and a submission record will be created. They are stored in the `LeadFormSubmissions` table.

---

## Workflow Summary

1. **Consultant creates a Lead Form**
   - The consultant defines: title, description, dynamic fields, active/inactive status.
   - App generates a public URL for the form.

2. **Consultant shares the form**
   - The form can be:
     - embedded on a website
     - shared on LinkedIn
     - sent via email
     - included in booking flows
   - This becomes the consultant’s "front door" for new clients.

3. **Lead submits the form**
   - A potential client fills out the form and submits it.
   - App:
     - stores the submission
     - triggers notifications
     - shows the lead in the dashboard

4. **Consultant reviews the submission**
   - The consultant reads:
     - contact details
     - answers
     - resume (if uploaded)
5. **Consultant schedules a Discovery Call**
   - If the lead looks promising:
     - a Discovery Call is created
     - calendar integration syncs the event
     - the lead receives confirmation

---

## 1) Creating a Lead Form

A Lead Form is created by a user with the role of `Consultant` or `Admin`. This form becomes the public intake form that potential clients use to submit their information.

**1-1) Angular UI Flow for Creating Lead Forms**

- The `LeadFormCreateComponent` contains metadata fields:
  - title (input, required)
  - description (textarea, optional)
  - isActive (toggle, default: true)

- The `LeadFormFieldBuilderComponent` is a dynamic field builder. Consultant or admin:
  - Add new fields
  - Choose type for each field: text, textarea, number, select, checkbox, file upload
  - Set for each field: label, placeholder, required flag, validation rules
  - Fields are stored in a local array:
    ```typescript
     formFields: LeadFormField[] = [];
    ```
    Example:
    ```typescript
    [
      {
        id: "fullName",
        label: "Full Name",
        type: "text",
        required: true,
      },
      {
        id: "email",
        label: "Email",
        type: "email",
        required: true,
      },
    ];
    ```
- Save and send to backend:
  - Build DTO:
    ```typescript
    const dto = {
      title: this.form.title,
      description: this.form.description,
      isActive: this.form.isActive,
      fieldsJson: JSON.stringify(this.formFields),
    };
    ```
  - Call API
- UX Considerations:
  - Auto-focus on title field
  - Drag-and-drop ordering of fields
  - Preview mode for form
  - Copy-to-clipboard for public URL
  - Toggle to activate/deactivate form

**1-2) Backend Flow for creating Lead Form**

- Validate:
  - Title is required
  - FieldsJson is a valid JSON array
  - TenantId is extracted from JWT
- Create a new LeadForms record
- Generate a public URL using the LeadForm's `Id` and return it:
  ```csharp
  https://stm.app/form/{LeadFormId}
  ```

## 2) Public Form Access

Public Form Access allows potential clients to open and submit Lead Forms without authentication. This is the entry point of the Lead Capture workflow. App exposes a secure public endpoint that serves the form definition and accepts submissions.

### 2-1) Backend Workflow

Public Form Access is handled through two backend endpoints:

1. **GET /api/public/lead-forms/{id}**
   Returns the form definition (metadata + dynamic fields).

2. **POST /api/public/lead-forms/{id}**  
   Accepts and stores lead submissions.

Both endpoints operate without authentication and rely on the LeadForm’s `Id` and `IsActive` status for access control.

**GET Public Form Definition**
Serve the Lead Form definition to the frontend so it can render the form dynamically.

- Validate that:
  - The Lead Form exists.
  - `IsActive = true`.
  - The tenant owning the form is active.

- Load the form:
  - Title
  - Description
  - FieldsJson (dynamic fields)
  - TenantId

- Return JSON response:
  ```json
  {
    "id": "GUID",
    "title": "Career Coaching Intake Form",
    "description": "Please fill out the following information.",
    "fieldsJson": [ ... ]
  }
  ```

**POST Public Form Submission**
Store lead submissions and trigger notifications.

- Validate:
  - Form exists and is active.
  - Required fields match FieldsJson.
  - Email format (if provided).
  - File uploads (if any).
- Create a new LeadFormSubmissions record
- Trigger notifications:
  - Email to consultant.
  - In-app notification.
- Record an event in the `ClientTimeline` table
  - `clientTimeline.EventType = (int)TimelineEventType.LeadCaptured;`
  - MetadataJson:

    ```json
    {
      "ClientId": "guid",
      "Source": "LeadForm",
      "CapturedBy": "System",
      "FormName": "string"
    }
    ```

    - **Source:** Where did this event come from?
    - **CapturedBy: "System"** means the client submitted the form not a consultant
    - **FormName:** Comes from `leadForm.Title`

- Return:
  ```json
  {
    "submissionId": "GUID",
    "status": "success"
  }
  ```

### 2-2) Frontend Workflow

The frontend renders public Lead Forms using the form definition returned by the backend. It dynamically builds the UI based on `FieldsJson` and submits answers back to the backend.

Public Form Access is handled through a dedicated route:

```code
/form/:id
```

**Angular Route Definition**

```typescript
{
  path: 'form/:id',
  component: PublicLeadFormComponent
}
```

**Component: PublicLeadFormComponent**

- Fetch form definition from backend.
  - Extract `id` from route.
  - Call `GET /api/public/lead-forms/{id}`
- If form is inactive or not found, show error page.
- Render dynamic fields.
- Handle validation.
- Submit form data and call: `POST /api/public/lead-forms/{id}`
- Show success or error messages.

## 3) Trigger Notifications

When a potential client submits a Lead Form, App automatically triggers notifications to inform the consultant. Notifications ensure that consultants respond quickly to new leads and maintain a smooth intake workflow.

### 3-1) Backend Workflow

After a Lead Form submission is stored in `LeadFormSubmissions`, the backend triggers one or more notifications. Notifications are triggered asynchronously to avoid slowing down the submission process.

**Load Consultant Notification Preferences**
This ensures notifications are personalized, and fetches:

- Consultant’s email
- Notification settings from `UserSettings`
- Consultant timezone for formatting timestamps from `UserSettings`
- Tenant name from `Tenants`

**Build Notification Payload**
The backend constructs a payload containing:

```json
{
  "consultantId": "GUID",
  "tenantId": "GUID",
  "leadFormSubmissionId": "GUID",
  "leadName": "John Doe",
  "leadEmail": "john@example.com",
  "submittedAt": "2026-07-14T15:23:00Z",
  "summary": {
    "careerGoals": "Become a senior developer",
    "experience": "3 years"
  }
}
```

**Send Email Notification**

- Render email template
- Inject tenant branding (e.g., name)
- Send email via SMTP or email provider
- Log notification event

**Create In-App Notification**
Backend inserts a record into `Notifications` table. It appears in the consultant’s notification center.

### 3-2) Frontend Workflow

The frontend does not send notifications itself. Instead, it reacts to backend-triggered notifications:

- Shows success message after submission
- Displays in-app notifications in the consultant dashboard
- Updates notification badge count

**Public Form Submission**
After submitting the form, frontend shows: "Thank you for your submission".

**Consultant Dashboard Notification Badge**
When consultant logs in, frontend updates:

- Notification bell icon count
- Notification dropdown list

**Notification Center**
It displays:

- Title: "New Lead Submission"
- Message: "John Doe submitted a lead form.""
- Timestamp
- "View Submission" button

## 4) Lead Appears in Dashboard

After a potential client submits a Lead Form, App must display the new lead inside the consultant’s dashboard.
The consultant opens a specific lead submission inside App to evaluate the lead, read their answers, and decide the next action (e.g., scheduling a Discovery Call).

### 4-1) Backend Workflow

Once a submission is stored in `LeadFormSubmissions`, the backend exposes endpoints that allow the consultant to view:

- A list of all leads
- Details of a specific lead
- Submission metadata
- Dynamic answers mapped to field definitions

**Dashboard Loads Lead List**

- Extract `tenantId` from JWT.
- Query all submissions belonging to that tenant
- Return a lightweight list. For example:
  ```json
  [
    {
      "id": "GUID",
      "fullName": "John Doe",
      "email": "john@example.com",
      "submittedAt": "2026-07-14T15:23:00Z",
      "leadFormTitle": "Career Coaching Intake Form",
      "status": "New"
    }
  ]
  ```
  **Notes**
  - Backend does not return full answers and only summary fields are returned for performance.
  - The backend derives `status` and returns it to the frontend.

**Dashboard Loads Lead Details**

- Validate:
  - Submission exists.
  - Submission belongs to the consultant’s tenant.
- Load:
  - Submission record
  - LeadForm metadata
  - FieldsJson (dynamic field definitions)
- Map dynamic answers and return it:

  ```json
  {
    "id": "GUID"
    "fullName": "John Doe",
    "email": "john@example.com",
    "answers": [
      { "label": "Career Goals", "value": "Become a senior developer" },
      { "label": "Experience", "value": "3 years" }
    ],
    "status": "New",
     "availableActions": ["ScheduleDiscoveryCall"], // This also could be ["MarkQualified", "MarkNotQualified","MarkNoShow"] or []
  }
  ```

  **Note:** `availableActions` should be returned because the backend is the only reliable source of truth for a lead’s lifecycle state, and the frontend uses it to decide which action buttons should be shown or hidden.

**Lead Status Management**
The lead’s status is computed dynamically and appears in the dashboard list based on:

- whether a DiscoveryCall exists
- whether the DiscoveryCall has an outcome

When the dashboard loads leads, the backend checks:

- **Does the submission have a `DiscoveryCall`?**
  - If yes: status = `DiscoveryScheduled`
  - If no: status = `New`

- **Does the `DiscoveryCall` have an Outcome?**
  - Qualified: `Qualified`
  - NotQualified: `NotQualified`
  - NoShow: `NoShow`

When frontend calls `POST /api/lead-submissions/{id}/status`, while the received status is:

- Schedule Discovery Call:
  Backend creates a new `DiscoveryCalls` record: status becomes DiscoveryScheduled (derived)
- Mark Qualified:
  Backend updates `DiscoveryCalls.Outcome = 'Qualified'`: status becomes Qualified (derived)
- Mark Not Qualified:
  Backend updates `DiscoveryCalls.Outcome = 'NotQualified'`: status becomes NotQualified (derived)
- Mark No Show:
  Backend updates `DiscoveryCalls.Outcome = 'NoShow'`: status becomes NoShow (derived)

### 4-2) Frontend Workflow

The Angular dashboard displays leads using two components:

- `LeadListComponent`
- `LeadDetailsComponent`

The frontend fetches data from backend endpoints and renders:

- Lead list
- Lead details
- Dynamic answers
- Status indicators

**Load Lead List**
On the init of the `LeadListComponent` component, fetch the leads list from backend and:

- Display
  - Full name
  - Email
  - Submission date
  - Form title
  - Status (derived by backend)

- Sorting
  - Default: newest first
  - Optional: sort by name or form

- Filtering
  - By form
  - By status
  - By date range

**Load Lead Details**
Clicking a lead in the list navigates to details page: `/leads/submissions/{id}`

In the `LeadDetailsComponent` component:

- Extract `id` from route, and call: `GET /api/lead-submissions/{id}`
- Render:
  - Contact info
  - Submission timestamp
  - Form title
  - Dynamic answers (loop through mapped fields)
  - Lead actions (buttons):
    - Schedule Discovery Call (only available if NO Discovery Call exists yet)
    - Mark as Qualified (only available If a Discovery Call exists AND Outcome is NULL)
    - Mark as Not Qualified (only available If a Discovery Call exists AND Outcome is NULL)

**Status Updates**
When consultant performs actions (click on a button), frontend calls: `POST /api/lead-submissions/{id}/status`, and dashboard list updates automatically.

## 5) Consultant Schedules Discovery Call

This is the action a consultant performs after reviewing a lead submission, and a transition from Lead Capture to Discovery Stage.

### 5-1) Frontend Workflow

**Consultant clicks "Schedule Discovery Call"**
A new form (modal or page) should open. It is a Discovery Call scheduling form. Scheduling a Discovery Call requires input from the consultant, such as:

- date (Date picker)
- time (Time picker)
- duration (Duration selector)
- optional notes
- optional attendee email (lead’s email)

The consultant must fill them.

**Frontend builds DTO:**

```json
{
  "leadFormSubmissionId": "GUID",
  "scheduledAt": "2026-07-20T14:00:00Z",
  "durationMinutes": 45
}
```

And sends request: `POST /api/discovery-calls`

**Frontend receives updated lifecycle state:**

```json
{
  "status": "DiscoveryScheduled",
  "availableActions": ["MarkQualified", "MarkNotQualified"]
}
```

and updates UI:

- Status badge changes
- New buttons appear
- Scheduling button disappears

### 5-2) Backend Workflow

**Validation steps:**

- Submission exists
- Submission belongs to consultant’s tenant
- No existing Discovery Call for this submission
- Scheduled time is valid
- Consultant has calendar integration configured

**Creating Discovery Call record:**
Insert into `DiscoveryCalls`. This marks the lead as `DiscoveryScheduled` (derived status).

**Syncing with calendar provider:**

- Build calendar event payload:
  - Title: "Discovery Call with John Doe"
  - Start time
  - End time
  - Consultant timezone
  - Lead email (optional attendee)
- Send event to Google/Otulook Calendar
- Store external event ID in `DiscoveryCalls` table for future updates/cancellations.

**Record an event in the `ClientTimeline` table**

```csharp
clientTimeline.EventType = (int)TimelineEventType.DiscoveryCallScheduled;
```

`MetadataJson`:

```json
{
  "DiscoveryCallId": "guid",
  "ScheduledBy": "Consultant",
  "ScheduledAt": "2025-01-01T10:00:00Z",
  "ClientTimezone": "America/New_York",
  "ConsultantTimezone": "Asia/Tehran",
  "Method": "Online",
  "Notes": "string"
}
```
