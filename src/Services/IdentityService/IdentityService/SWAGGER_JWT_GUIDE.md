# JWT Authentication in Swagger UI - CMMS Identity Service

## Overview

The CMMS Identity Service now supports JWT Bearer Token authentication in Swagger UI, allowing you to test protected endpoints directly from the Swagger interface.

## Features

- **JWT Bearer Token Authentication**: All protected endpoints require a valid JWT token
- **Swagger UI Integration**: Easy-to-use authentication interface in Swagger
- **API Versioning**: Support for multiple API versions with separate documentation
- **XML Documentation**: Enhanced API documentation with detailed descriptions
- **Response Type Documentation**: Clear response type definitions for all endpoints

## How to Use JWT Authentication in Swagger UI

### 1. Access Swagger UI

1. Start the Identity Service
2. Navigate to: `https://localhost:5001/swagger` or `http://localhost:5000/swagger`
3. You'll see the Swagger UI with all available API endpoints

### 2. Authentication Flow

#### Step 1: Login to Get JWT Token

1. Find the **POST /api/v1/auth/login** endpoint
2. Click "Try it out"
3. Enter your credentials:
```json
{
  "userName": "admin@example.com",
  "password": "Admin123!"
}
```
4. Click "Execute"
5. Copy the `accessToken` from the response

#### Step 2: Authenticate in Swagger UI

1. Click the **"Authorize"** button at the top of the Swagger UI
2. In the authorization dialog, enter your JWT token:
   - **Value**: `Bearer YOUR_JWT_TOKEN_HERE`
   - Example: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
3. Click **"Authorize"**
4. Click **"Close"**

#### Step 3: Test Protected Endpoints

Now you can test any protected endpoint:
- All endpoints with a lock icon 🔒 require authentication
- Your JWT token will be automatically included in requests
- You can test endpoints like:
  - `GET /api/v1/auth/me` - Get current user info
  - `GET /api/v1/users` - Get all users
  - `POST /api/v1/roles` - Create a new role
  - etc.

### 3. MFA Authentication Flow

If MFA is enabled for your user:

#### Step 1: Initial Login
1. Use the **POST /api/v1/auth/login** endpoint
2. You'll receive an MFA challenge response instead of tokens

#### Step 2: Complete MFA
1. Use the **POST /api/v1/auth/mfa-login** endpoint
2. Enter your verification code and session ID
3. You'll receive the JWT tokens

#### Step 3: Authenticate in Swagger UI
1. Use the `accessToken` from the MFA login response
2. Follow the same authentication steps as above

## Protected Endpoints

### Authentication Endpoints (No Auth Required)
- `POST /api/v1/auth/login` - Login
- `POST /api/v1/auth/mfa-login` - MFA login
- `POST /api/v1/auth/refresh` - Refresh token
- `POST /api/v1/auth/forgot-password` - Request password reset
- `POST /api/v1/auth/reset-password` - Reset password
- `POST /api/v1/auth/resend-otp` - Resend OTP

### Protected Endpoints (Auth Required)
- `POST /api/v1/auth/logout` - Logout
- `POST /api/v1/auth/change-password` - Change password
- `POST /api/v1/auth/enable-mfa` - Enable MFA
- `POST /api/v1/auth/verify-mfa` - Verify MFA
- `GET /api/v1/auth/me` - Get current user

### User Management (Auth Required)
- `GET /api/v1/users` - Get all users
- `GET /api/v1/users/{id}` - Get user by ID
- `POST /api/v1/users` - Create user
- `PUT /api/v1/users/{id}` - Update user
- `DELETE /api/v1/users/{id}` - Delete user
- `PATCH /api/v1/users/{id}/toggle-status` - Toggle user status

### Role Management (Auth Required)
- `GET /api/v1/roles` - Get all roles
- `GET /api/v1/roles/{id}` - Get role by ID
- `POST /api/v1/roles` - Create role
- `PUT /api/v1/roles/{id}` - Update role
- `DELETE /api/v1/roles/{id}` - Delete role
- `PATCH /api/v1/roles/{id}/toggle-status` - Toggle role status
- `GET /api/v1/roles/tenant/{tenantId}` - Get roles by tenant
- `GET /api/v1/roles/active` - Get active roles

### Permission Management (Auth Required)
- `GET /api/v1/permissions` - Get all permissions
- `GET /api/v1/permissions/{id}` - Get permission by ID
- `POST /api/v1/permissions` - Create permission
- `PUT /api/v1/permissions/{id}` - Update permission
- `DELETE /api/v1/permissions/{id}` - Delete permission
- `POST /api/v1/permissions/auto-generate` - Auto-generate permissions
- `GET /api/v1/permissions/tenant/{tenantId}` - Get permissions by tenant
- `GET /api/v1/permissions/resource/{resource}` - Get permissions by resource
- `GET /api/v1/permissions/active` - Get active permissions
- `GET /api/v1/permissions/templates` - Get permission templates

### Tenant Management (Auth Required)
- `GET /api/v1/tenants` - Get all tenants
- `GET /api/v1/tenants/{id}` - Get tenant by ID
- `POST /api/v1/tenants` - Create tenant
- `PUT /api/v1/tenants/{id}` - Update tenant
- `DELETE /api/v1/tenants/{id}` - Delete tenant
- `PATCH /api/v1/tenants/{id}/toggle-status` - Toggle tenant status
- `GET /api/v1/tenants/{id}/statistics` - Get tenant statistics

## JWT Token Structure

The JWT token contains the following claims:
- `sub` - User ID
- `email` - User email
- `name` - User name
- `role` - User roles (comma-separated)
- `tenant` - Tenant ID
- `permissions` - User permissions (comma-separated)
- `iat` - Issued at timestamp
- `exp` - Expiration timestamp
- `iss` - Issuer
- `aud` - Audience

## Token Refresh

When your JWT token expires:
1. Use the **POST /api/v1/auth/refresh** endpoint
2. Provide your `refreshToken`
3. You'll receive new `accessToken` and `refreshToken`
4. Update your Swagger UI authorization with the new `accessToken`

## Security Best Practices

1. **Never share your JWT tokens** - They provide access to your account
2. **Use HTTPS in production** - JWT tokens are sensitive data
3. **Set appropriate token expiration** - Configure reasonable token lifetimes
4. **Implement token refresh** - Use refresh tokens to maintain session
5. **Logout properly** - Use the logout endpoint to invalidate tokens
6. **Monitor token usage** - Track and audit token usage

## Troubleshooting

### Common Issues

1. **401 Unauthorized**
   - Check if your JWT token is valid and not expired
   - Ensure you're using the correct token format: `Bearer YOUR_TOKEN`
   - Verify the token was obtained from the correct login endpoint

2. **403 Forbidden**
   - Your user may not have the required permissions
   - Check if your user is active and not locked out
   - Verify your user belongs to the correct tenant

3. **Token Expired**
   - Use the refresh token endpoint to get a new access token
   - Update your Swagger UI authorization with the new token

4. **MFA Required**
   - If login returns an MFA challenge, complete the MFA flow first
   - Use the mfa-login endpoint with your verification code

### Getting Help

If you encounter issues:
1. Check the application logs for detailed error messages
2. Verify your JWT token using a tool like jwt.io
3. Ensure your user account is properly configured
4. Contact the development team with specific error details

## API Versioning

The service supports multiple API versions:
- **v1.0** - Current stable version
- Each version has its own Swagger documentation
- Use the version selector in Swagger UI to switch between versions
- URL format: `/api/v{version}/endpoint`

## Development Notes

- JWT tokens are signed using the key from `appsettings.json`
- Token validation includes issuer, audience, and lifetime checks
- Refresh tokens are stored in the database and can be revoked
- All protected endpoints require valid JWT authentication
- XML documentation is automatically generated and included in Swagger 