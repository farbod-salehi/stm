# STM - Signup Flow

## Overview

Signup in App is **exclusively for new tenants**. A successful signup creates:

1. A **Tenant** record
2. A **User** record with role Admin (`IsVerified = 0` until OTP validated)
3. An **OTP** record for verification (`Purpose = 'Signup'`)
4. A **RefreshToken**
5. A **UserSession**

Consultants and Assistants **cannot sign up**. They are added later by the Admin inside the tenant dashboard.

---

## Rules

- **Admin email must be globally unique** across all users.
- Signup requires verifying the email using OTP. OTPs are stored in the `OTPs` table and deleted after successful verification.
- Signup attempts are tracked in the `OTPs.Attempts` field.
- Signup initializes a refresh token, an access token, and a user session tied to `DeviceId`.

---

## Validations

### Signup Request:

- **FullName:** Required, minimum length 2 characters.
- **BusinessName:** Required, minimum length 2 characters.
- **Domain:** Required, must be 3-50 characters, alphanumeric with optional hyphens, and cannot already exist in the `Tenants` table.
- **Email:** Required, valid email format, must not already exist in `Users` table with `IsVerified = 1`.
- **Passwrod:** Required, minimum length 8 chareacters, strong password recommended.
- **DeviceId:** Required, must be a valid client-generated identifier.

### OTP:

- **Email:** Must match the email used during signup.
- **Code:** Must match hashed code in `OTPs` table.
- **ExpiresAt:** Must be in the future.
- **Attempts:** Must be less than max allowed. (e.g., 3 per OTP)

---

## Workflow

### Step 1: User submits signup form

Payload:

```json
{
  fullName,
  businessName,
  domain,
  email,
  password,
  deviceId
}
```

System actions:

- Validate fields
- Check if `Users.Email` exists:
  - If `IsVerified = 1` => stop signup, return "Email already registered."
  - If `IsVerified = 0`:
    - If valid OTP exists => stop signup, return "OTP already sent, please verify."
    - If OTP expired => update Tenant/User with new values, create new OTP.
    - If attempts exceeded => stop signup, return "Too many attempts."
  - If not found => continue normal signup.

### Step 2: Create or Update Tenant

- If new email => insert Tenant.
  - `Name = businessName`
  - `Domain = domain`
  - `CreatedAt = now`
- If existing unverified email => update Tenant with new values.

### Step 3: Create or Update User (Admin)

- If new email => insert User.
  - `TenantId = newly created TenantId`
  - `Email = email`
  - `FullName = fullName`
  - `PasswordHash = hashed(password)`
  - `Role = 'Admin'`
  - `IsVerified = 0`
  - `IsActive = 1`
  - `CreatedAt = now`
- If existing unverified email => update User with new values.
- Always set:
  - `IsVerified = 0`
  - `IsActive = 1`

### Step 4: Generate OTP

**Insert** / **replace** in **OTPs**:

- `Email = email`
- `UserId = newly created UserId`
- `HashedCode = hash(otp)`
- `ExpiresAt = now + 5 minutes`
- `Attempts = 0`
- `Purpose = 'Signup'`

**Insert:**
Creates a brand‑new OTP record for the given `Email + Purpose`. Used when the email is starting a new signup flow, meaning no active (unverified and unexpired) OTP exists for `Email + Purpose = Signup`.

**Replace:**  
Updates the existing pending OTP record for the same `Email + Purpose`. Used when a previous OTP has expired or the user retries signup and submits new tenant/user information.
The old record is overwritten with the new code, reset `Attempts = 0`, and updated `ExpiresAt`.

Send OTP via email.

### Step 5: Verify OTP

User submits:

```json
{
  email
  otpCode
  deviceId
}
```

System actions:

- Fetch OTP by (`Email = email`, `Purpose = 'Signup'`)
- Validate expiration, attempts, and hashed code.
- If incorrect:
  - Increment `Attempts`
  - Reject with error message
- If correct:
  - Delete OTP record
  - Update `Users.IsVerified = 1`
  - Continue signup

### Step 6: Create Refresh Token

Insert into **RefreshTokens**:

- `UserId`
- `DeviceId`
- `TokenHash`
- `CreatedAt`
- `ExpiresAt = now + 30 days`
- `UserAgent`
- `IpAddress`

Backend sets HttpOnly cookie.

### Step 7: Create UserSession

Insert into **UserSessions**:

- `UserId`
- `DeviceId`
- `DeviceName` (parsed from UserAgent)
- `CreatedAt = now`
- `LastActivityAt = now`
- `IsActive = 1`

### Step 8: Generate Access Token (JWT)

After the refresh token and session are created, the backend generates a short‑lived access token.

**Access Token Contents:**

- `UserId`
- `TenantId`
- `Role` (always `'Admin'` for signup)
- `iat` (issued at)
- `exp` (expires in 15 minutes)

**Access Token Behavior:**

- Returned in the response body
- Stored **only in memory** on the frontend
- Used for all authenticated API calls
- Automatically refreshed using the refresh token when expired
