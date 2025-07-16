# MFA Login Flow Documentation

## Overview

The Identity Service now supports Multi-Factor Authentication (MFA) during the login process. When a user has MFA enabled, they will be required to provide a second factor (SMS code) after successfully entering their username and password.

## Login Flow

### Step 1: Initial Login Request

**Endpoint:** `POST /api/v1/auth/login`

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "userpassword",
  "tenantId": "optional-tenant-id"
}
```

**Response (No MFA Required):**
```json
{
  "requiresMfa": false,
  "accessToken": "jwt-token-here",
  "refreshToken": "refresh-token-here",
  "expiresAt": "2024-01-01T12:00:00Z",
  "user": { /* user details */ },
  "roles": ["admin", "user"],
  "permissions": ["users:read", "users:write"]
}
```

**Response (MFA Required):**
```json
{
  "requiresMfa": true,
  "mfaToken": "session-token-for-mfa",
  "phoneNumber": "09******89",
  "mfaType": "sms",
  "expiresAt": "2024-01-01T12:05:00Z"
}
```

### Step 2: MFA Verification (if required)

**Endpoint:** `POST /api/v1/auth/mfa-login`

**Request Body:**
```json
{
  "mfaToken": "session-token-from-step-1",
  "verificationCode": "123456"
}
```

**Response (Success):**
```json
{
  "accessToken": "jwt-token-here",
  "refreshToken": "refresh-token-here",
  "expiresAt": "2024-01-01T12:00:00Z",
  "user": { /* user details */ },
  "roles": ["admin", "user"],
  "permissions": ["users:read", "users:write"]
}
```

## MFA Setup Flow

### Step 1: Enable MFA

**Endpoint:** `POST /api/v1/auth/enable-mfa`

**Request Body:**
```json
{
  "phoneNumber": "09123456789",
  "mfaType": "sms",
  "backupPhoneNumber": "optional-backup",
  "backupEmail": "optional-backup@example.com"
}
```

### Step 2: Verify MFA Setup

**Endpoint:** `POST /api/v1/auth/verify-mfa`

**Request Body:**
```json
{
  "phoneNumber": "09123456789",
  "verificationCode": "123456",
  "purpose": "mfa-setup"
}
```

## Security Features

1. **Phone Number Masking**: The phone number is masked in responses (e.g., "09******89")
2. **Session Tokens**: MFA sessions use temporary tokens that expire in 5 minutes
3. **Rate Limiting**: Built-in protection against brute force attacks
4. **Audit Trail**: All MFA attempts are logged for security monitoring

## Error Handling

### Common Error Responses

**Invalid Credentials:**
```json
{
  "error": "Invalid email or password."
}
```

**MFA Required but Not Set Up:**
```json
{
  "error": "MFA is required but not configured for this user."
}
```

**Invalid MFA Code:**
```json
{
  "error": "MFA verification failed: Invalid verification code"
}
```

**Expired MFA Session:**
```json
{
  "error": "Invalid or expired MFA session."
}
```

## Implementation Details

### Database Tables

- **SmsVerificationCodes**: Stores OTP codes and MFA sessions
- **UserMfas**: Stores user MFA settings and preferences

### Key Components

1. **LoginCommandHandler**: Checks for MFA requirement and initiates MFA flow
2. **MfaLoginCommandHandler**: Handles MFA verification during login
3. **SmsVerificationService**: Manages OTP generation and verification
4. **KavenegarSmsService**: Sends SMS via Kavenegar API

### Security Considerations

1. **Token Expiration**: MFA tokens expire after 5 minutes
2. **Attempt Limiting**: Maximum 3 attempts per OTP code
3. **Session Management**: MFA sessions are invalidated after use
4. **Phone Validation**: Iranian phone number format validation

## Testing

### Test Scenarios

1. **User without MFA**: Should receive immediate access token
2. **User with MFA**: Should receive MFA challenge
3. **Invalid MFA code**: Should receive error message
4. **Expired MFA session**: Should receive error message
5. **Multiple MFA attempts**: Should be rate limited

### Test Data

Create a user with MFA enabled:
1. Create user account
2. Enable MFA using `/api/v1/auth/enable-mfa`
3. Verify MFA setup using `/api/v1/auth/verify-mfa`
4. Test login flow with MFA challenge 