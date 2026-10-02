# STM – JWT Token

## Overview

STM uses a dual‑token authentication model:

- **Access Token:** short‑lived, stored in memory, used for API authorization

- **Refresh Token:** long‑lived, stored in an HttpOnly, Secure cookie, used to obtain new access tokens

Refresh tokens are tied to **DeviceId**, enabling multi‑device login, per‑device logout, and accurate session tracking.

Signin is fully passwordless. PasswordHash is stored for future privileged actions but is not used in JWT authentication.

---

## Access Token

### Purpose

Authorize API calls. Never stored in cookies or localStorage.

### Storage

- In-memory (Angular service)

### Lifetime

- 15 minutes

### Contents

- `UserId`
- `TenantId`
- `Role`
- `permissions`
- `exp` (expiration timestamp)
- `iat` (issued at)

### Behavior

- Sent in `Authorization: Bearer <token>` header
- When expired => frontend automatically calls `/auth/refresh-token`

---

## Refresh Token

### Purpose

Issue new access tokens and maintain persistent login.

### Storage

- HttpOnly cookie (backend sets it)
- Secure (HTTPS only)
- SameSite=Strict (CSRF protection)

### Lifetime

- 30 days

### Device Binding

Refresh tokens are tied to:

- `UserId`
- `DeviceId`
- `TokenHash`

This ensures each device has its own independent session.

---

## Refresh Token Rotation

Every refresh request performs:

- Validate old refresh token
- Insert new refresh token (new TokenHash)
- Delete old refresh token
- Update `UserSessions.LastActivityAt`
- Return new access token
- Set new HttpOnly cookie

This prevents token theft and replay attacks.

---

## UserSessions Integration

Each refresh token belongs to a session, represented in `UserSessions`.

### When fields update in UserSessions table

- `CreatedAt` => on login
- `LastActivityAt` => on any authenticated API call or refresh
- `IsActive = 0` => on logout, password change, suspicious activity, or token expiration

---

### Security Considerations

- Refresh token stored in HttpOnly cookie => not accessible by JS
- Access token stored in memory => safe from XSS
- `DeviceId` prevents token replay
- `TokenHash` prevents token theft
- `IpAddress` + `UserAgent` stored for auditing
- `UserLoginAttempts` logs signin attempts for auditing and future security features, but is **not used** for authentication decisions in the current OTP‑based signin model.
