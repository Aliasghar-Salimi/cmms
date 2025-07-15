# CMMS Identity Service

A microservice for handling user authentication, authorization, and multi-tenant user management using ASP.NET Core Identity and JWT tokens.

## Features

- 🔐 JWT-based authentication
- 👥 Multi-tenant user management
- 🛡️ Role-based authorization
- 📊 Audit logging
- 🔄 Refresh token support
- 🐳 Docker support

## Prerequisites

- .NET 8.0 SDK
- Docker and Docker Compose
- SQL Server (or use the provided Docker setup)

## Quick Start with Docker

### 1. Build the Docker Image

```bash
docker build -t cmms-identity-service:latest .
```

### 2. Run with Docker Compose (Recommended)

```bash
docker-compose up -d
```

This will start:
- SQL Server on port 1433
- Identity Service on port 5000

### 3. Run Standalone Container

```bash
# Make sure SQL Server is running first
docker run -d \
  --name cmms-identity-service \
  -p 5000:80 \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;Database=CMMSIdentityService;User=sa;Password=Ali@1234;TrustServerCertificate=True" \
  cmms-identity-service:latest
```

## Development

### Local Development

1. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

2. **Run database migrations:**
   ```bash
   dotnet ef database update
   ```

3. **Run the application:**
   ```bash
   dotnet run
   ```

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | SQL Server connection string | `Server=localhost;Database=CMMSIdentityService;User=sa;Password=Ali@1234;TrustServerCertificate=True` |
| `Jwt__Key` | JWT signing key | `default-key-for-development` |
| `Jwt__Issuer` | JWT issuer | `cmms-identity-service` |
| `Jwt__Audience` | JWT audience | `cmms-clients` |

## API Endpoints

- `GET /health` - Health check endpoint
- `GET /swagger` - API documentation (Development only)

## Docker Files

- `Dockerfile` - Standard Docker build
- `Dockerfile.prod` - Production-optimized build with security improvements
- `docker-compose.yml` - Complete stack with SQL Server

## Production Deployment

For production, use the optimized Dockerfile:

```bash
docker build -f Dockerfile.prod -t cmms-identity-service:prod .
```

## Health Checks

The service includes a health check endpoint at `/health` that returns:

```json
{
  "status": "Healthy",
  "timestamp": "2025-07-13T03:46:42.123Z"
}
```

## Troubleshooting

### Common Issues

1. **Database Connection Failed**
   - Ensure SQL Server is running
   - Check connection string in appsettings.json
   - Verify database exists and migrations are applied

2. **JWT Configuration Missing**
   - Set proper JWT configuration in appsettings.json
   - Ensure JWT key is provided for production

3. **Docker Build Fails**
   - Ensure you're in the correct directory (where .csproj file is located)
   - Check that all required files are present

### Logs

View container logs:
```bash
docker logs cmms-identity-service
```

View application logs:
```bash
docker exec -it cmms-identity-service cat /app/logs/app.log
``` 