# CMMS Identity Service

[![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)](https://github.com/cmms-platform/identity-service)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)]()

A professional Identity and Access Management Service for the CMMS Platform, built with ASP.NET Core 8.0, featuring multi-tenant support, JWT authentication, SMS-based MFA, and comprehensive RBAC operations.

## 🚀 Features

### 🔐 Authentication & Security
- **JWT Authentication**: Secure token-based authentication with refresh tokens
- **SMS-based MFA**: Multi-factor authentication using Kavenegar SMS provider
- **Password Management**: Forgot password, reset password, and change password flows
- **Session Management**: Secure logout and token invalidation
- **JWT in Swagger UI**: Interactive authentication testing directly in Swagger

### 👥 User & Access Management
- **Multi-tenant Architecture**: Complete tenant isolation and management
- **User Management**: Full CRUD operations with role-based access control
- **Role-Based Access Control (RBAC)**: Comprehensive permission system
- **Permission Management**: Auto-generation and manual permission management
- **Current User Info**: JWT-based current user information retrieval
- **Admin User Creation**: Database seeding and CLI command for creating admin users
- **Default Tenant Setup**: Automatic creation of default tenant and admin role

### 🏗️ Architecture & Development
- **Clean Architecture**: CQRS pattern with MediatR
- **API Versioning**: Professional API versioning support (v1.0)
- **Validation**: Comprehensive input validation with FluentValidation
- **AutoMapper**: Efficient object mapping
- **Professional Versioning**: Semantic versioning with management tools

### 📚 Documentation & Testing
- **Swagger Documentation**: Interactive API documentation with JWT support
- **XML Documentation**: Enhanced API documentation with detailed descriptions
- **Health Checks**: Built-in health monitoring
- **Testing Support**: Comprehensive HTTP testing files

### 🚀 Deployment & Infrastructure
- **Docker Support**: Containerized deployment with SQL Server
- **Database Migrations**: Entity Framework migrations
- **Configuration Management**: Environment-based configuration
- **Nullable Reference Types**: Enhanced type safety

## 📋 Table of Contents

- [Quick Start](#quick-start)
- [Admin User Setup](#admin-user-setup)
- [Authentication Flow](#authentication-flow)
- [API Documentation](#api-documentation)
- [RBAC System](#rbac-system)
- [SMS Integration](#sms-integration)
- [Version Management](#version-management)
- [Architecture](#architecture)
- [Configuration](#configuration)
- [Deployment](#deployment)
- [Development](#development)
- [Database Seeding](#database-seeding)

## 🏃‍♂️ Quick Start

### Prerequisites

- .NET 8.0 SDK
- Docker and Docker Compose
- SQL Server (or use Docker)
- Kavenegar SMS API key (for SMS features)

### Running with Docker

```bash
# Clone the repository
git clone <repository-url>
cd cmms/src/Services/IdentityService/IdentityService

# Start the services
docker-compose up -d

# The service will be available at:
# - API: http://localhost:5000
# - Swagger: http://localhost:5000/swagger
# - Health: http://localhost:5000/health
```

### Running Locally

```bash
# Restore dependencies
dotnet restore

# Run database migrations
dotnet ef database update

# Start the service
dotnet run

# The service will be available at:
# - API: http://localhost:5226
# - Swagger: http://localhost:5226/swagger
# - Health: http://localhost:5226/health
```

## 👤 Admin User Setup

### Automatic Database Seeding

The application automatically creates essential system entities on startup:

1. **Default Tenant**: Creates a "Default" tenant for system operations
2. **Admin Role**: Creates an "Admin" role with full system permissions
3. **Admin User**: Creates a default admin user (admin@cmms.com)

This seeding runs automatically when the application starts if these entities don't exist.

### Manual Admin User Creation

You can also manually create admin users using the CLI command:

```bash
# Create admin user via CLI command
dotnet run -- CreateAdminUser

# The command will prompt for:
# - Username (default: admin)
# - Email (default: admin@cmms.com)
# - Password (default: Admin@123)
```

### Default Admin Credentials

After seeding, you can log in with:
- **Email**: admin@cmms.com
- **Password**: Admin@123
- **Role**: Admin (with full system permissions)

### Customizing Admin Setup

You can modify the default values in:
- `Infrastructure/Persistence/DatabaseSeeder.cs` - For automatic seeding
- `Commands/CreateAdminUser.cs` - For CLI command defaults

## 🔐 Authentication Flow

### JWT Authentication in Swagger UI

1. **Access Swagger UI**: Navigate to `/swagger`
2. **Login**: Use `POST /api/v1/auth/login` to get JWT token
3. **Authorize**: Click "Authorize" and enter `Bearer YOUR_JWT_TOKEN`
4. **Test Protected Endpoints**: All protected endpoints will now include your token

### Authentication Endpoints

```http
# Public Endpoints (No Auth Required)
POST   /api/v1/auth/login              # Login with username/password
POST   /api/v1/auth/mfa-login          # Complete MFA login
POST   /api/v1/auth/refresh            # Refresh JWT token
POST   /api/v1/auth/forgot-password    # Request password reset via SMS
POST   /api/v1/auth/reset-password     # Reset password with SMS code
POST   /api/v1/auth/resend-otp         # Resend OTP for MFA/password reset

# Protected Endpoints (Auth Required)
POST   /api/v1/auth/logout             # Logout and invalidate tokens
POST   /api/v1/auth/change-password    # Change user password
POST   /api/v1/auth/enable-mfa         # Enable MFA for user
POST   /api/v1/auth/verify-mfa         # Verify MFA code
GET    /api/v1/auth/me                 # Get current user information
```

### MFA Flow

1. **Login**: User logs in with username/password
2. **MFA Challenge**: If MFA is enabled, receive session ID
3. **SMS Code**: User receives SMS with verification code
4. **MFA Login**: Complete login with session ID and verification code
5. **JWT Tokens**: Receive access token and refresh token

## 📚 API Documentation

### Base URL
- **Production**: `https://api.cmms-platform.com/api/v1`
- **Development**: `http://localhost:5226/api/v1`
- **Docker**: `http://localhost:5000/api/v1`

### Authentication
All protected API endpoints require JWT Bearer token authentication:
```http
Authorization: Bearer <your-jwt-token>
```

### Version Information
```http
GET /api/v1/version
GET /api/v1/version/simple
GET /api/v1/version/build
GET /api/v1/version/health
```

### Users Management
```http
GET    /api/v1/users                    # Get all users (with filtering/pagination)
GET    /api/v1/users/{id}              # Get user by ID
POST   /api/v1/users                   # Create new user
PUT    /api/v1/users/{id}              # Update user
DELETE /api/v1/users/{id}              # Delete user
PATCH  /api/v1/users/{id}/toggle-status # Toggle user status
```

### Roles Management
```http
GET    /api/v1/roles                    # Get all roles (with filtering/pagination)
GET    /api/v1/roles/{id}              # Get role by ID
POST   /api/v1/roles                   # Create new role
PUT    /api/v1/roles/{id}              # Update role
DELETE /api/v1/roles/{id}              # Delete role
PATCH  /api/v1/roles/{id}/toggle-status # Toggle role status
GET    /api/v1/roles/tenant/{tenantId} # Get roles by tenant
GET    /api/v1/roles/active            # Get active roles
```

### Permissions Management
```http
GET    /api/v1/permissions                    # Get all permissions
GET    /api/v1/permissions/{id}              # Get permission by ID
POST   /api/v1/permissions                   # Create new permission
PUT    /api/v1/permissions/{id}              # Update permission
DELETE /api/v1/permissions/{id}              # Delete permission
POST   /api/v1/permissions/auto-generate     # Auto-generate permissions
GET    /api/v1/permissions/tenant/{tenantId} # Get permissions by tenant
GET    /api/v1/permissions/resource/{resource} # Get permissions by resource
GET    /api/v1/permissions/active            # Get active permissions
GET    /api/v1/permissions/templates         # Get permission templates
```

### Tenants Management
```http
GET    /api/v1/tenants                    # Get all tenants (with filtering/pagination)
GET    /api/v1/tenants/{id}              # Get tenant by ID
POST   /api/v1/tenants                   # Create new tenant
PUT    /api/v1/tenants/{id}              # Update tenant
DELETE /api/v1/tenants/{id}              # Delete tenant
PATCH  /api/v1/tenants/{id}/toggle-status # Toggle tenant status
GET    /api/v1/tenants/{id}/statistics   # Get tenant statistics
```

### Interactive Documentation
Visit `/swagger` for interactive API documentation with JWT authentication support.

## 🛡️ RBAC System

### Role-Based Access Control
- **Roles**: Group-based permissions for users
- **Permissions**: Granular access control for resources and actions
- **Auto-Generation**: Automatic permission generation from templates
- **Tenant Isolation**: Role and permission isolation per tenant

### Permission Templates
Pre-configured permission templates for common resources:
- **Users**: Create, Read, Update, Delete, List, ToggleStatus
- **Tenants**: Create, Read, Update, Delete, List, ToggleStatus, Statistics
- **Roles**: Create, Read, Update, Delete, List, ToggleStatus, AssignPermissions
- **Permissions**: Create, Read, Update, Delete, List, AutoGenerate
- **System**: Admin, Monitor, Configure
- **Reports**: Generate, View, Export, Schedule
- **Audit**: View, Export, Analyze

### Custom Authorization
```csharp
[RequirePermission("Users", "Read")]
public async Task<IActionResult> GetUsers()
{
    // Only users with "Users.Read" permission can access
}
```

## 📱 SMS Integration

### Kavenegar SMS Provider
- **SMS Verification**: OTP delivery for MFA and password reset
- **Multi-purpose OTP**: Support for different OTP purposes
- **Rate Limiting**: Built-in rate limiting for SMS requests
- **Error Handling**: Comprehensive error handling and fallbacks

### SMS Configuration
```json
{
  "Kavenegar": {
    "ApiKey": "your-kavenegar-api-key",
    "TemplateName": "cmms-verification",
    "DefaultCountryCode": "+98"
  }
}
```

### SMS Endpoints
```http
POST /api/v1/auth/forgot-password    # Send password reset SMS
POST /api/v1/auth/reset-password     # Reset password with SMS code
POST /api/v1/auth/enable-mfa         # Send MFA setup SMS
POST /api/v1/auth/verify-mfa         # Verify MFA with SMS code
POST /api/v1/auth/resend-otp         # Resend OTP for any purpose
```

## 🔢 Version Management

### Current Version
- **Version**: 1.0.0
- **API Version**: v1.0
- **Build**: 1.0.0.0

### Version Scripts

```bash
# Get current version
./version.sh get

# Set specific version
./version.sh set 1.2.3

# Bump version
./version.sh bump major    # 1.0.0 → 2.0.0
./version.sh bump minor    # 1.0.0 → 1.1.0
./version.sh bump patch    # 1.0.0 → 1.0.1

# Show version information
./version.sh info
```

### Version Format
- **Major.Minor.Patch** (e.g., 1.0.0)
- **Major**: Breaking changes
- **Minor**: New features, backward compatible
- **Patch**: Bug fixes, backward compatible

### API Versioning
- **URL Versioning**: `/api/v1/users`
- **Header Versioning**: `X-API-Version: 1.0`
- **Media Type Versioning**: `application/json;version=1.0`

## 🏗️ Architecture

### Clean Architecture
```
IdentityService/
├── Application/                 # Application layer (CQRS, DTOs, Validators)
│   ├── Common/                  # Shared application concerns
│   │   ├── Authorization/       # RBAC authorization handlers
│   │   ├── Services/            # JWT, SMS, and other services
│   │   └── ApiVersioning/       # API versioning configuration
│   ├── Features/                # Feature-based organization
│   │   ├── Auth/                # Authentication features
│   │   ├── Users/               # User management features
│   │   ├── Roles/               # Role management features
│   │   ├── Permissions/         # Permission management features
│   │   └── Tenants/             # Tenant management features
│   └── Mapping/                 # AutoMapper profiles
├── Domain/                      # Domain layer (Entities, Interfaces)
│   └── Entities/                # Domain entities
├── Infrastructure/              # Infrastructure layer (Database, External services)
│   └── Persistence/             # Entity Framework configuration
└── Controllers/                 # API Controllers
```

### CQRS Pattern
- **Commands**: Create, Update, Delete operations
- **Queries**: Read operations with filtering and pagination
- **Handlers**: Business logic implementation
- **Validators**: Input validation using FluentValidation

### Multi-tenant Support
- Tenant isolation at the database level
- Tenant-specific user and role management
- Tenant statistics and monitoring

## ⚙️ Configuration

### Environment Variables
```bash
# Database
ConnectionStrings__DefaultConnection=Server=localhost;Database=CMMSIdentityService;Trusted_Connection=true;TrustServerCertificate=true;

# JWT
Jwt__Key=your-super-secret-key-here
Jwt__Issuer=cmms-platform
Jwt__Audience=cmms-users
Jwt__ExpiryInMinutes=60

# SMS (Kavenegar)
Kavenegar__ApiKey=your-kavenegar-api-key
Kavenegar__TemplateName=cmms-verification
Kavenegar__DefaultCountryCode=+98

# Application
ASPNETCORE_ENVIRONMENT=Development
```

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CMMSIdentityService;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "Jwt": {
    "Key": "your-super-secret-key-here",
    "Issuer": "cmms-platform",
    "Audience": "cmms-users",
    "ExpiryInMinutes": 60
  },
  "Kavenegar": {
    "ApiKey": "your-kavenegar-api-key",
    "TemplateName": "cmms-verification",
    "DefaultCountryCode": "+98"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## 🚀 Deployment

### Docker Deployment
```bash
# Build and run with Docker Compose
docker-compose up -d

# Build custom image
docker build -t cmms-identity-service:latest .

# Run container
docker run -d -p 5000:80 cmms-identity-service:latest
```

### Production Deployment
1. Set environment variables
2. Run database migrations
3. Deploy application
4. Configure reverse proxy (nginx/Apache)
5. Set up SSL certificates
6. Configure monitoring and logging

## 🛠️ Development

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- SQL Server (or Docker)
- Kavenegar SMS API key (for SMS features)

### Project Structure
```
IdentityService/
├── Application/                 # Application layer (CQRS, DTOs, Validators)
│   ├── Common/                  # Shared application concerns
│   │   ├── Authorization/       # RBAC authorization handlers
│   │   ├── Services/            # JWT, SMS, and other services
│   │   └── ApiVersioning/       # API versioning configuration
│   ├── Features/                # Feature-based organization
│   │   ├── Auth/                # Authentication features
│   │   ├── Users/               # User management features
│   │   ├── Roles/               # Role management features
│   │   ├── Permissions/         # Permission management features
│   │   └── Tenants/             # Tenant management features
│   └── Mapping/                 # AutoMapper profiles
├── Domain/                      # Domain layer (Entities, Interfaces)
│   └── Entities/                # Domain entities
├── Infrastructure/              # Infrastructure layer (Database, External services)
│   └── Persistence/             # Entity Framework configuration
├── Controllers/                 # API Controllers
├── Commands/                    # CLI commands (CreateAdminUser)
└── Migrations/                  # Entity Framework migrations
```

### Development Setup
```bash
# Clone repository
git clone <repository-url>
cd cmms/src/Services/IdentityService/IdentityService

# Install dependencies
dotnet restore

# Run database migrations
dotnet ef database update

# Start development server
dotnet run

# The service will be available at:
# - API: http://localhost:5226/api/v1
# - Swagger: http://localhost:5226/swagger
# - Health: http://localhost:5226/health
```

### Database Seeding

The application includes automatic database seeding that creates:
- **Default Tenant**: "Default" tenant for system operations
- **Admin Role**: "Admin" role with full system permissions
- **Admin User**: Default admin user (admin@cmms.com)

This seeding runs automatically on application startup if the entities don't exist.

### Testing
```bash
# Run unit tests
dotnet test

# Run integration tests
dotnet test --filter Category=Integration

# Test JWT authentication in Swagger
# 1. Start the service
# 2. Navigate to /swagger
# 3. Use the Swagger_JWT_Testing.http file for testing
```

### Database Migrations
```bash
# Create migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

## 📊 Health Monitoring

### Health Check Endpoints
- `/health` - Basic health check
- `/api/v1/version/health` - Health check with version info

### Health Check Response
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-01T12:00:00Z",
  "service": "CMMS Identity Service",
  "version": "1.0.0",
  "environment": "Production"
}
```

## 🔒 Security

### JWT Configuration
- Secure key generation
- Token expiration
- Issuer and audience validation
- Role-based authorization
- Refresh token support

### SMS Security
- Rate limiting for SMS requests
- Secure OTP generation
- Time-based expiration
- Audit logging

### Input Validation
- FluentValidation for all inputs
- SQL injection prevention
- XSS protection
- CSRF protection
- Nullable reference types

### RBAC Security
- Permission-based authorization
- Role-based access control
- Tenant isolation
- Audit logging

## 📝 Changelog

### Version 1.0.0 (Latest)
- ✅ JWT authentication in Swagger UI
- ✅ SMS-based MFA with Kavenegar integration
- ✅ Comprehensive RBAC system
- ✅ Professional API versioning
- ✅ Multi-tenant support
- ✅ Complete CRUD operations for all entities
- ✅ Password reset and MFA flows
- ✅ Current user information endpoint
- ✅ Enhanced documentation and testing support
- ✅ Nullable reference types implementation
- ✅ **Database seeding for admin user creation**
- ✅ **CLI command for manual admin user creation**
- ✅ **Automatic default tenant and role setup**
- ✅ **Fixed DTO mapping and namespace consistency**
- ✅ **Improved error handling and validation**

### Recent Improvements (Latest Update)
- 🔧 **Fixed Build Errors**: Resolved missing using directives for UserStore and RoleStore
- 🔧 **DTO Consistency**: Unified DTO usage across features and removed duplicate mappings
- 🔧 **Admin User Creation**: Added both automatic seeding and manual CLI command
- 🔧 **Port Configuration**: Updated development port to 5226 for consistency
- 🔧 **Documentation**: Enhanced README with new features and setup instructions

See [CHANGELOG.md](CHANGELOG.md) for detailed version history and changes.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Update documentation
6. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- **Email**: support@cmms-platform.com
- **Documentation**: `/swagger` endpoint
- **Health Check**: `/health` endpoint
- **Version Info**: `/api/v1/version` endpoint
- **JWT Guide**: See `SWAGGER_JWT_GUIDE.md`
- **Testing**: See `Swagger_JWT_Testing.http`

## 🔧 Troubleshooting

### Common Issues

**Build Errors**
```bash
# If you get UserStore/RoleStore errors:
# Add missing using directive in CreateAdminUser.cs:
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
```

**Port Already in Use**
```bash
# Kill existing dotnet processes
pkill -f dotnet

# Or use a different port
dotnet run --urls="http://localhost:5227"
```

**Database Connection Issues**
```bash
# Check connection string in appsettings.json
# Ensure SQL Server is running
# For Docker: docker-compose up -d
```

**Admin User Already Exists**
```bash
# This is normal - the seeding checks for existing users
# You can still use the CLI command to create additional admin users
```

## 🔗 Related Projects

- [CMMS Platform](https://github.com/cmms-platform)
- [CMMS API Gateway](https://github.com/cmms-platform/api-gateway)
- [CMMS Frontend](https://github.com/cmms-platform/frontend)

---

**CMMS Identity Service v1.0.0** - Professional Identity and Access Management with JWT, SMS MFA, and RBAC 