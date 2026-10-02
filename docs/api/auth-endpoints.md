# STM – Authentication API Endpoints

<br>

## Base URL

### `/api/auth`

---

## POST `/register`

Registers a new user.

### Request

```
{
    "email": "user@example.com",
    "password": "StrongPass123!"
}
```

### Responses

- 200 → OTP sent

- 400 → Email already exists

- 429 → Too many OTP requests

---

## POST `/verify-otp`

Verifies the OTP code.

### Request

```
{
    "email": "user@example.com",
    "code": "123456"
}
```

### Responses

- 200 → Account activated

- 400 → Invalid or expired code

- 403 → Too many attempts

---

## POST `/login`

Authenticates user.

### Request

```
{
    "email": "user@example.com",
    "password": "StrongPass123!"
}
```

### Response

```
{
    "accessToken": "...",
    "refreshToken": "..."
}
```

---

## POST `/refresh-token`

Refreshes access token.

### Request

```
{
    "refreshToken": "..."
}
```

### Response

```
{
    "accessToken": "...",
    "refreshToken": "..."
}
```

---

## POST `/resend-otp`

Resends OTP to user.

### Response

- 200 → OTP resent
- 429 → Too many requests
