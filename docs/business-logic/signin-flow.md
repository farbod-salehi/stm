# STM - Signin Flow

## Overview

Signin is used by **existing users** (Admins, Consultants, Assistants). A successful signin creates:

1. A **UserLoginAttempt** record
2. An **OTP** record (`Purpose = 'Signin'`)
3. A **RefreshToken**
4. A **UserSession**
5. A short‑lived **Access Token (JWT)**

Signin is **passwordless** and uses OTP verification tied to DeviceId.

---

## Password Role in Authentication

Although App stores a `PasswordHash` for every user, **signin does not use the password**. App uses a **passwordless OTP‑based login model**, and the password plays a different role in the system.

### 1. Current Role (Signup Identity Binding)

During signup, the password is:

- validated
- hashed
- stored in `Users.PasswordHash`

This ensures that tenant creation is initiated by a real human and not an automated bot. Bots can easily request OTPs using disposable emails, but they cannot reliably generate strong passwords that meet validation rules.

### 2. Not Used in Signin

Signin uses OTP only:

- No password check
- No password fallback
- No password authentication

This allows Consultants and Assistants (who never set passwords) to log in seamlessly.

### 3. Future Role (Privileged Actions)

The stored password will be used later for sensitive operations, such as:

- changing email
- deleting a tenant
- exporting data
- accessing billing settings
- enabling optional password login or 2FA
- verifying identity for external integrations

Keeping the password hash today avoids future schema migrations and supports advanced security features.

---

## Rules

- Email must already exist in the `Users` table and must be `IsVerified = 1`.
- Signin requires verifying the OTP sent to the user's email.
- OTP attempts are tracked in the `OTPs.Attempts` field.
- `UserLoginAttempts` logs **only signin attempts**, not signup attempts.
- A user may have multiple active sessions (different devices).
- Refresh tokens are device‑specific and stored in HttpOnly cookies.

---

## Note on UserLoginAttempts

App logs every signin attempt in `UserLoginAttempts`, but this table is not currently used for authentication decisions.  
Since App uses passwordless OTP login, brute‑force password protection is not required. The table exists for future features such as password login, 2FA, security auditing, anomaly detection, and compliance requirements.

---

## Validations

### Signin Request

- **Email:** Required, must exist in `Users` table.
- **DeviceId:** Required, client‑generated identifier.

### OTP

- **Email:** Must match the signin email.
- **Code:** Must match hashed code in `OTPs` table.
- **ExpiresAt:** Must be in the future.
- **Attempts:** Must be less than max allowed (e.g., 3).
- **Purpose:** Must be `'Signin'`.

---

## Workflow

### Step 1: User submits signin form

Payload:

```json
{
    email,
    deviceId
}
```

System actions:

- Fetch user by email.
- If user does not exist => reject.
- If `IsVerified = 0` => reject ("Email not verified").
- Log unsuccessful signin attempt. Insert into `UserLoginAttempts`:
  - `Email = email`
  - `IpAddress`
  - `UserAgent`
  - `DeviceId = deviceId`
  - `IsSuccessful = 0`
  - `CreatedAt = now`

### Step 2: Handle OTP (Insert or Replace)

Fetch latest OTP (`Email = email` AND `Purpose = 'Signin'`):

- Valid OTP exists (unexpired):
  - Reject signin
  - Return: "OTP already sent. Please verify."

- OTP expired:
  - Update existing OTP record with new code
  - Reset `Attempts = 0`
  - Update `ExpiresAt = now + 5 minutes`

- No OTP exists
  - Create new OTP record
  - `Attempts = 0`
  - `Purpose = 'Signin'`

Send OTP via email.

### Step 3: Verify OTP

User submits:

```json
{
  email,
  otpCode,
  deviceId
}
```

System actions:

- Fetch OTP (`Email + Purpose = 'Signin'`).
- Validate expiration, attempts, and hashed code.

**If incorrect:**

- Increment `Attempts`
- Log failed attempt in `UserLoginAttempts`
- Reject with error message

**If correct:**

- Delete OTP record
- Update `UserLoginAttempts.IsSuccessful = 1`
- Continue signin

### Step 4: Create Refresh Token

Insert into `RefreshTokens`:

- `UserId`
- `DeviceId = deviceId`
- `TokenHash`
- `CreatedAt = now`
- `ExpiresAt = now + 30 days`
- `UserAgent`
- `IpAddress`

Backend sets HttpOnly cookie.

### Step 5: Create UserSession

Insert into **UserSessions**:

- `UserId`
- `DeviceId`
- `DeviceName` (parsed from UserAgent)
- `CreatedAt = now`
- `LastActivityAt = now`
- `IsActive = 1`

### Step 6: Generate Access Token (JWT)

After refresh token and session are created, backend generates a short‑lived access token.

**Access Token Contents:**

- `UserId`
- `TenantId`
- `Role`
- `iat`
- `exp` (15 minutes)

**Access Token Behavior:**

- Returned in response body
- Stored only in memory on frontend
- Used for authenticated API calls
- Automatically refreshed using the refresh token
