 # CMMS Microservices Platform

A comprehensive microservices platform for Computerized Maintenance Management System (CMMS) with event-driven architecture using Kafka, featuring API composition and distributed transaction management with the Saga pattern.

## 🏗️ Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Identity      │    │   Asset         │    │   Audit Log     │
│   Service       │    │   Service       │    │   Service       │
│   (Port 5000)   │    │   (Port 5002)   │    │   (Port 5001)   │
└─────────┬───────┘    └─────────┬───────┘    └─────────┬───────┘
          │                      │                      │
          │                      │                      │
          └──────────────────────┼──────────────────────┘
                                 │
                   ┌─────────────┴─────────────┐
                   │          Kafka            │
                   │       (Port 9092)         │
                   └─────────────┬─────────────┘
                                 │
                   ┌─────────────┴─────────────┐
                   │        SQL Server         │
                   │       (Port 1433)         │
                   └───────────────────────────┘
```

## 🚀 Quick Start

### Prerequisites
- Docker and Docker Compose
- .NET 8.0 SDK (for local development)

### Start All Services
```bash
# Start all services
docker-compose up -d

# Start with monitoring tools
docker-compose --profile monitoring up -d
```

### Stop All Services
```bash
docker-compose down
```

## 📋 Services Overview

### Core Services

| Service | Port | Description | Health Check |
|---------|------|-------------|--------------|
| **Identity Service** | 5000 | User authentication, authorization, and management | `http://localhost:5000/health` |
| **Asset Service** | 5002 | Asset lifecycle management with Saga pattern | `http://localhost:5002/health` |
| **Audit Log Service** | 5001 | Audit log collection and querying | `http://localhost:5001/health` |

### Infrastructure Services

| Service | Port | Description | Health Check |
|---------|------|-------------|--------------|
| **SQL Server** | 1433 | Database for all services | `sqlcmd -S localhost -U sa -P Ali@1234 -Q 'SELECT 1'` |
| **Kafka** | 9092 | Message broker for event streaming | `kafka-topics --bootstrap-server localhost:9092 --list` |
| **Zookeeper** | 2181 | Kafka coordination service | `echo ruok \| nc localhost 2181` |

### Monitoring Tools (Optional)

| Service | Port | Description |
|---------|------|-------------|
| **Kafka UI** | 8080 | Web interface for Kafka monitoring |
| **Adminer** | 8081 | Database management interface |

## 🔧 Configuration

### Environment Variables

#### Identity Service
```yaml
ASPNETCORE_ENVIRONMENT: Development
ConnectionStrings__DefaultConnection: Server=sqlserver;Database=CMMSIdentityService;User=sa;Password=Ali@1234;TrustServerCertificate=True
Kafka__BootstrapServers: kafka:29092
Kafka__Topic: cmms-identity-service-topic
Kafka__AuditTopic: cmms-audit-logs
```

#### Asset Service
```yaml
ASPNETCORE_ENVIRONMENT: Development
ConnectionStrings__DefaultConnection: Server=sqlserver;Database=CMMSAssetService;User=sa;Password=Ali@1234;TrustServerCertificate=True
Kafka__BootstrapServers: kafka:29092
Kafka__AuditTopic: cmms-audit-logs
IdentityService__BaseUrl: http://identity-service:5000
IdentityService__TimeoutSeconds: 30
IdentityService__MaxRetries: 3
IdentityService__RetryDelayMilliseconds: 1000
```

#### Audit Log Service
```yaml
ASPNETCORE_ENVIRONMENT: Development
ConnectionStrings__DefaultConnection: Server=sqlserver;Database=CMMSAuditLogService;User=sa;Password=Ali@1234;TrustServerCertificate=True
Kafka__BootstrapServers: kafka:29092
Kafka__AuditTopic: cmms-audit-logs
```

## 🆕 New Features

### API Composition
The Asset Service implements API composition to integrate with the Identity Service for user authentication and permission validation. This allows the Asset Service to:

- **Validate user permissions** before performing asset operations
- **Retrieve user context** for audit logging and authorization
- **Maintain service independence** while providing seamless user experience
- **Handle cross-service communication** through HTTP clients with retry policies

### Saga Pattern for Distributed Transactions
The Asset Service implements the Saga pattern to manage complex, multi-step operations that span multiple services and ensure data consistency:

#### Saga Orchestrator
- **Centralized coordination** of distributed transactions
- **Step-by-step execution** with rollback capabilities
- **Compensation logic** for handling failures
- **State persistence** for transaction recovery

#### Supported Saga Types
1. **Asset Creation Saga**
   - Permission validation
   - Asset creation
   - Event publishing
   - Compensation: Asset deletion on failure

2. **Asset Update Saga**
   - Permission validation
   - Asset update
   - Event publishing
   - Compensation: Asset rollback on failure

3. **Asset Deletion Saga**
   - Permission validation
   - Asset deletion
   - Event publishing
   - Compensation: Asset restoration on failure

#### Saga States
- **Pending**: Initial state when saga starts
- **InProgress**: Saga is executing steps
- **Completed**: All steps completed successfully
- **Failed**: Saga failed, compensation executed
- **Compensated**: Rollback completed

## 📊 API Endpoints

### Identity Service (`http://localhost:5000`)

#### Authentication
- `POST /api/v1/auth/login` - User login
- `POST /api/v1/auth/logout` - User logout
- `POST /api/v1/auth/refresh-token` - Refresh access token
- `POST /api/v1/auth/forgot-password` - Forgot password
- `POST /api/v1/auth/reset-password` - Reset password

#### User Management
- `GET /api/v1/users` - List users
- `POST /api/v1/users` - Create user
- `GET /api/v1/users/{id}` - Get user by ID
- `PUT /api/v1/users/{id}` - Update user
- `DELETE /api/v1/users/{id}` - Delete user

#### Role Management
- `GET /api/v1/roles` - List roles
- `POST /api/v1/roles` - Create role
- `GET /api/v1/roles/{id}` - Get role by ID
- `PUT /api/v1/roles/{id}` - Update role
- `DELETE /api/v1/roles/{id}` - Delete role

#### Permission Management
- `GET /api/v1/permissions` - List permissions
- `POST /api/v1/permissions` - Create permission
- `GET /api/v1/permissions/{id}` - Get permission by ID
- `PUT /api/v1/permissions/{id}` - Update permission
- `DELETE /api/v1/permissions/{id}` - Delete permission

### Asset Service (`http://localhost:5002`)

#### Asset Management
- `GET /api/v1/assets` - List assets with filtering and pagination
- `POST /api/v1/assets` - Create asset (triggers Asset Creation Saga)
- `GET /api/v1/assets/{id}` - Get asset by ID
- `PUT /api/v1/assets/{id}` - Update asset (triggers Asset Update Saga)
- `DELETE /api/v1/assets/{id}` - Delete asset (triggers Asset Deletion Saga)

#### Saga Management
- `GET /api/v1/sagas/{id}` - Get saga status and details
- `POST /api/v1/sagas/{id}/compensate` - Manually trigger saga compensation

#### Query Parameters for Assets
- `page` - Page number (default: 1)
- `pageSize` - Items per page (default: 20)
- `assetType` - Filter by asset type
- `manufacturer` - Filter by manufacturer
- `status` - Filter by asset status
- `location` - Filter by location

### Audit Log Service (`http://localhost:5001`)

#### Audit Logs
- `GET /api/auditlogs` - List audit logs with filtering and pagination
- `GET /api/auditlogs/{id}` - Get specific audit log
- `GET /api/auditlogs/stats` - Get audit log statistics

#### Query Parameters
- `page` - Page number (default: 1)
- `pageSize` - Items per page (default: 20)
- `action` - Filter by action (e.g., "LOGIN", "LOGOUT", "ASSET_CREATED")
- `userName` - Filter by username
- `fromDate` - Filter from date (ISO format)
- `toDate` - Filter to date (ISO format)

## 🔍 Monitoring and Debugging

### Service Health Checks
```bash
# Check all services health
curl http://localhost:5000/health  # Identity Service
curl http://localhost:5002/health  # Asset Service
curl http://localhost:5001/health  # Audit Log Service
```

### Kafka Monitoring
```bash
# List topics
docker exec cmms-kafka kafka-topics --bootstrap-server localhost:9092 --list

# Monitor audit logs topic
docker exec cmms-kafka kafka-console-consumer --bootstrap-server localhost:9092 --topic cmms-audit-logs --from-beginning

# Monitor asset events topic
docker exec cmms-kafka kafka-console-consumer --bootstrap-server localhost:9092 --topic cmms-asset-events --from-beginning
```

### Database Access
```bash
# Connect to SQL Server
docker exec -it cmms-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Ali@1234

# Or use Adminer (if monitoring profile is enabled)
# Open http://localhost:8081 in browser
```

### Service Logs
```bash
# View all logs
docker-compose logs

# View specific service logs
docker-compose logs identity-service
docker-compose logs asset-service
docker-compose logs auditlog-service
docker-compose logs kafka
```

## 🧪 Testing

### Test the Complete Flow

1. **Start all services:**
   ```bash
   docker-compose up -d
   ```

2. **Wait for services to be healthy:**
   ```bash
   docker-compose ps
   ```

3. **Test login (generates audit log):**
   ```bash
   curl -X POST http://localhost:5000/api/v1/auth/login \
     -H "Content-Type: application/json" \
     -d '{"email":"admin@cmms.com","password":"Admin@123"}'
   ```

4. **Create an asset (triggers saga):**
   ```bash
   curl -X POST http://localhost:5002/api/v1/assets \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer YOUR_TOKEN" \
     -d '{"name":"Test Asset","assetType":"Equipment","manufacturer":"Test Corp","location":"Warehouse A","status":"Active"}'
   ```

5. **Check asset creation:**
   ```bash
   curl http://localhost:5002/api/v1/assets
   ```

6. **Check audit logs:**
   ```bash
   curl http://localhost:5001/api/auditlogs
   ```

7. **Monitor Kafka messages:**
   ```bash
   docker exec cmms-kafka kafka-console-consumer --bootstrap-server localhost:9092 --topic cmms-audit-logs --from-beginning
   docker exec cmms-kafka kafka-console-consumer --bootstrap-server localhost:9092 --topic cmms-asset-events --from-beginning
   ```

### Automated Testing Script
```bash
# Run the test setup script
cd src/Services/AuditLogService/AuditLogService
./test-setup.sh
```

## 🔧 Development

### Local Development
```bash
# Start only infrastructure services
docker-compose up -d zookeeper kafka sqlserver

# Run services locally
cd src/Services/IdentityService/IdentityService
dotnet run

cd src/Services/AssetService/AssetService
dotnet run

cd src/Services/AuditLogService/AuditLogService
dotnet run
```

### Adding New Services
1. Create service directory in `src/Services/`
2. Add service configuration to root `docker-compose.yml`
3. Update this README with service details

### Database Migrations
```bash
# Identity Service migrations
cd src/Services/IdentityService/IdentityService
dotnet ef migrations add InitialCreate
dotnet ef database update

# Asset Service migrations
cd src/Services/AssetService/AssetService
dotnet ef migrations add InitialCreate
dotnet ef database update

# Audit Log Service migrations
cd src/Services/AuditLogService/AuditLogService
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 🚨 Troubleshooting

### Common Issues

1. **Services not starting:**
   ```bash
   docker-compose logs [service-name]
   ```

2. **Database connection issues:**
   - Check SQL Server is running: `docker-compose ps sqlserver`
   - Verify connection string in environment variables

3. **Kafka connection issues:**
   - Check Kafka is running: `docker-compose ps kafka`
   - Verify bootstrap servers configuration

4. **Port conflicts:**
   - Check if ports are already in use: `netstat -tulpn | grep :5000`
   - Modify ports in docker-compose.yml if needed

5. **Saga failures:**
   - Check saga state in database: `SELECT * FROM SagaStates WHERE Status = 'Failed'`
   - Review compensation logic and retry mechanisms
   - Check cross-service communication logs

### Clean Start
```bash
# Stop and remove everything
docker-compose down -v
docker system prune -f

# Start fresh
docker-compose up -d
```

## 📝 License

This project is licensed under the MIT License.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request