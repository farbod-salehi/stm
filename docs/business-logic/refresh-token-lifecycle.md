# STM – Refresh Tokens

## Overview

Refresh tokens are long‑lived credentials that allow users to obtain new access tokens without logging in again.
This App uses a secure, database‑backed refresh‑token system with:

- Physical deletion of invalid tokens
- Rotation on every use
- Device‑bound tokens
- Strict expiration rules
- Background cleanup jobs

This ensures strong security, prevents replay attacks, and keeps the authentication system fast and predictable.

---

## Refresh Token Creation

### Purpose

A refresh token is issued to allow the user to renew their session after the access token expires.

**Characteristics**

- Lifetime: 7–30 days
- Stored in the database (hashed)
- Bound to:
  - User ID
  - Device
- Issued only after OTP verification.

**When created**

- User logs in
- User completes OTP verification
- User re-authenticates after session expiration

---

## Refresh Token Rotation

### What is rotation?

Every time a refresh token is used:

1.  Backend validates the token
2.  A new access token is issued
3.  A new refresh token is generated
4.  The old refresh token is physically deleted

### Why rotation?

- Prevents replay attacks
- Limits the window of token theft
- Ensures only one valid refresh token per device

---

## Refresh Token Validation

### Validation checks

When the frontend calls: `POST /auth/refresh-token`

Backend verifies:

- Token exists in DB
- Token is not expired
- Token belongs to the correct user
- Token matches the device
- Token hash matches the stored hash

### Failure cases

If any check fails:

- Token is deleted (if expired)
- Request is rejected
- User must log in again

---

## Refresh Token Deletion

### Purpose

App uses physical deletion to ensure invalid tokens never remain in the database.

#### When tokens are deleted

**1. On rotation**
Old token is deleted immediately after issuing a new one.

**2. On expiration**
If `ExpiresAt < now`, token is deleted:

- during validation
- during background cleanup job

**3. On logout (single device)**
Delete the refresh token for that device.

**4. On logout from all devices**
Delete all refresh tokens for the user.

**5. On suspicious activity**
Delete all tokens for the user to force full re-authentication.

**6. On email change**
Delete all tokens when sensitive account information changes (e.g., email change, future privileged actions)

#### Why physical deletion?

- Keeps the table small
- Avoids index bloat
- Ensures only valid tokens exist in DB

---

## Background Cleanup Job

### Purpose

Some tokens expire without ever being used again.
To prevent database growth, App runs a scheduled cleanup job.

### Daily job

- Deletes all tokens where `ExpiresAt < now`
- Removes tokens older than a retention threshold (optional)

### Benefits

- Keeps DB small and fast
- Prevents stale tokens

---

## Logout Flow

### Single-device logout

- Delete refresh token for that device
- Access token naturally expires in 15 minutes

### Logout from all devices

- Delete all refresh tokens for the user
- Access tokens expire automatically

---

## Security Considerations

### 1. Store refresh tokens hashed

- Never store raw tokens.
- Use a secure hashing algorithm (SHA256 or stronger).

### 2. Use HttpOnly cookies

It is stored in an HttpOnly, Secure cookie set by the backend:

- The frontend cannot read or modify this cookie
- The browser automatically sends it with requests

### 3. Enforce strict expiration

- Expired tokens must be deleted immediately.

### 4. Invalidate tokens on sensitive changes

Delete all tokens when:

- Email changes
- Suspicious login detected

### 5. Device binding

Each device receives its own refresh token. This enables:

- Per-device logout
- Per-device revocation
- Better security auditing
