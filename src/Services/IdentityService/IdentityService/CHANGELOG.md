# Changelog

All notable changes to the CMMS Identity Service will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Professional versioning system with semantic versioning
- API versioning support (v1.0)
- Version management scripts and tools
- Comprehensive version information endpoints
- Assembly versioning and metadata

### Changed
- Updated API routes to include versioning (e.g., `/api/v1/users`)
- Enhanced Swagger documentation with version support
- Improved health check endpoints with version information

## [1.0.0] - 2024-01-01

### Added
- Complete CRUD operations for Users with CQRS pattern
- Complete CRUD operations for Tenants with CQRS pattern
- Professional architecture with Clean Architecture principles
- MediatR for CQRS implementation
- FluentValidation for input validation
- AutoMapper for object mapping
- ASP.NET Core Identity integration
- Multi-tenant support
- JWT authentication support
- Comprehensive error handling
- Professional DTOs and validation
- Docker support with SQL Server
- Health check endpoints
- Swagger API documentation

### Technical Features
- **Users Management**: Full CRUD with role assignment, filtering, pagination
- **Tenants Management**: Full CRUD with user/role counting, status management
- **API Versioning**: Support for multiple API versions
- **Database**: Entity Framework Core with SQL Server
- **Authentication**: JWT Bearer tokens
- **Validation**: Comprehensive input validation with FluentValidation
- **Mapping**: AutoMapper for entity-DTO mapping
- **Architecture**: Clean Architecture with CQRS pattern

### API Endpoints
- `GET /api/v1/users` - Get all users with filtering/pagination
- `GET /api/v1/users/{id}` - Get user by ID
- `POST /api/v1/users` - Create new user
- `PUT /api/v1/users/{id}` - Update user
- `DELETE /api/v1/users/{id}` - Delete user
- `PATCH /api/v1/users/{id}/toggle-status` - Toggle user status

- `GET /api/v1/tenants` - Get all tenants with filtering/pagination
- `GET /api/v1/tenants/{id}` - Get tenant by ID
- `POST /api/v1/tenants` - Create new tenant
- `PUT /api/v1/tenants/{id}` - Update tenant
- `DELETE /api/v1/tenants/{id}` - Delete tenant
- `PATCH /api/v1/tenants/{id}/toggle-status` - Toggle tenant status
- `GET /api/v1/tenants/{id}/statistics` - Get tenant statistics

- `GET /api/v1/version` - Get detailed version information
- `GET /api/v1/version/simple` - Get simple version string
- `GET /api/v1/version/build` - Get build information
- `GET /api/v1/version/health` - Get health check with version

### Dependencies
- .NET 8.0
- ASP.NET Core Identity
- Entity Framework Core
- MediatR
- FluentValidation
- AutoMapper
- JWT Bearer Authentication
- Swagger/OpenAPI

---

## Version Management

### Version Format
- **Major.Minor.Patch** (e.g., 1.0.0)
- **Major**: Breaking changes
- **Minor**: New features, backward compatible
- **Patch**: Bug fixes, backward compatible

### Version Scripts
```bash
./version.sh get          # Get current version
./version.sh set 1.2.3    # Set specific version
./version.sh bump minor   # Bump minor version
./version.sh info         # Show version information
```

### API Versioning
- Current API version: v1.0
- Version specified in URL: `/api/v1/`
- Version specified in header: `X-API-Version: 1.0`
- Version specified in media type: `application/json;version=1.0`

---

## Migration Guide

### From Unversioned to v1.0
- Update API calls to include version: `/api/v1/` instead of `/api/`
- Add version headers if needed: `X-API-Version: 1.0`
- Update Swagger documentation URLs

---

## Support

For support and questions:
- Email: support@cmms-platform.com
- Documentation: `/swagger` endpoint
- Health Check: `/health` endpoint
- Version Info: `/api/v1/version` endpoint 