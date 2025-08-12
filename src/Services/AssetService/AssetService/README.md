# Asset Service

A microservice for managing assets in the CMMS (Computerized Maintenance Management System) with full CRUD operations, role-based authorization, and API versioning.

## Features

- **Full CRUD Operations**: Create, Read, Update, and Delete assets
- **Role-Based Authorization**: Restricted to SystemAdmin and TenantAdmin roles
- **API Versioning**: Support for v1.0 and v2.0 API versions
- **Advanced Filtering**: Search and filter assets by multiple criteria
- **Pagination**: Built-in pagination support for large datasets
- **Validation**: Comprehensive input validation using FluentValidation
- **Swagger Documentation**: Auto-generated API documentation
- **Global Exception Handling**: Consistent error responses across all endpoints

## Architecture

The service follows Clean Architecture principles with:

- **Domain Layer**: Asset entities and business logic
- **Application Layer**: Commands, Queries, and Handlers using MediatR pattern
- **Infrastructure Layer**: Database context and persistence
- **API Layer**: Controllers with role-based authorization

## Prerequisites

- .NET 8.0
- SQL Server
- Visual Studio 2022 or VS Code

## Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd cmms/src/Services/AssetService/AssetService
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure database connection**
   Update `appsettings.json` with your SQL Server connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=CMMS_Assets;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

4. **Run database migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the service**
   ```bash
   dotnet run
   ```

## API Endpoints

### Base URL
```
http://localhost:5000/api/v{version}/assets
```

### Available Versions
- **v1.0**: Current stable version
- **v2.0**: Future version (currently mirrors v1.0)

### Authentication & Authorization

All endpoints require authentication and one of the following roles:
- `SystemAdmin`: Full system-wide access
- `TenantAdmin`: Tenant-scoped access

### Endpoints

#### 1. Create Asset
```http
POST /api/v1.0/assets
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Server Rack A1",
  "assetType": "Server Hardware",
  "manufacturer": "Dell",
  "location": "Data Center - Row A",
  "status": "Operational",
  "warrantyExpirationDate": "2025-12-31T23:59:59Z"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Asset created successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "name": "Server Rack A1",
    "assetType": "Server Hardware",
    "manufacturer": "Dell",
    "location": "Data Center - Row A",
    "status": "Operational",
    "warrantyExpirationDate": "2025-12-31T23:59:59Z",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  },
  "timestamp": "2024-01-15T10:30:00Z"
}
```

#### 2. Get Asset by ID
```http
GET /api/v1.0/assets/{id}
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Asset retrieved successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "name": "Server Rack A1",
    "assetType": "Server Hardware",
    "manufacturer": "Dell",
    "location": "Data Center - Row A",
    "status": "Operational",
    "warrantyExpirationDate": "2025-12-31T23:59:59Z",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  },
  "timestamp": "2024-01-15T10:30:00Z"
}
```

#### 3. Get Assets List (with filtering and pagination)
```http
GET /api/v1.0/assets?name=Server&assetType=Hardware&page=1&pageSize=10&sortBy=Name&sortDescending=false
Authorization: Bearer {token}
```

**Query Parameters:**
- `name`: Filter by asset name (partial match)
- `assetType`: Filter by asset type
- `manufacturer`: Filter by manufacturer
- `location`: Filter by location
- `status`: Filter by status
- `warrantyExpirationDateStart`: Filter by warranty start date
- `warrantyExpirationDateEnd`: Filter by warranty end date
- `createdAtStart`: Filter by creation start date
- `createdAtEnd`: Filter by creation end date
- `updatedAtStart`: Filter by update start date
- `updatedAtEnd`: Filter by update end date
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 10, max: 100)
- `sortBy`: Sort field (default: Name)
- `sortDescending`: Sort direction (default: false)

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Assets retrieved successfully",
  "data": {
    "assets": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "name": "Server Rack A1",
        "assetType": "Server Hardware",
        "manufacturer": "Dell",
        "location": "Data Center - Row A",
        "status": "Operational",
        "warrantyExpirationDate": "2025-12-31T23:59:59Z",
        "createdAt": "2024-01-15T10:30:00Z",
        "updatedAt": "2024-01-15T10:30:00Z"
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 10,
    "totalPages": 1
  },
  "timestamp": "2024-01-15T10:30:00Z"
}
```

#### 4. Update Asset
```http
PUT /api/v1.0/assets/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Server Rack A1 - Updated",
  "assetType": "Server Hardware",
  "manufacturer": "Dell Inc.",
  "location": "Data Center - Row A - Updated",
  "status": "Maintenance",
  "warrantyExpirationDate": "2025-12-31T23:59:59Z"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Asset updated successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "name": "Server Rack A1 - Updated",
    "assetType": "Server Hardware",
    "manufacturer": "Dell Inc.",
    "location": "Data Center - Row A - Updated",
    "status": "Maintenance",
    "warrantyExpirationDate": "2025-12-31T23:59:59Z",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:35:00Z"
  },
  "timestamp": "2024-01-15T10:35:00Z"
}
```

#### 5. Delete Asset
```http
DELETE /api/v1.0/assets/{id}
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Asset deleted successfully",
  "timestamp": "2024-01-15T10:40:00Z"
}
```

#### 6. Health Check
```http
GET /api/v1.0/assets/health
```

**Response (200 OK):**
```json
{
  "service": "Asset Service",
  "status": "Healthy",
  "timestamp": "2024-01-15T10:30:00Z",
  "version": "1.0"
}
```

## Data Models

### Asset Entity
```csharp
public class Asset
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime WarrantyExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### Validation Rules
- **Name**: Required, max 100 characters, must be unique
- **AssetType**: Required, max 50 characters
- **Manufacturer**: Required, max 100 characters
- **Location**: Required, max 200 characters
- **Status**: Required, max 50 characters
- **WarrantyExpirationDate**: Must be in the future

## Error Handling

The service provides consistent error responses:

```json
{
  "success": false,
  "message": "Error description",
  "errors": ["Detailed error 1", "Detailed error 2"],
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### Common HTTP Status Codes
- **200**: Success
- **201**: Created
- **400**: Bad Request (validation errors)
- **401**: Unauthorized
- **403**: Forbidden (insufficient permissions)
- **404**: Not Found
- **500**: Internal Server Error

## Development

### Project Structure
```
AssetService/
├── Application/
│   ├── Common/
│   │   ├── ApiResponse.cs
│   │   └── GlobalExceptionHandler.cs
│   ├── Features/
│   │   └── Asset/
│   │       ├── Commands/
│   │       │   ├── CreateAsset/
│   │       │   ├── UpdateAsset/
│   │       │   └── DeleteAsset/
│   │       ├── Queries/
│   │       │   ├── GetAssetById/
│   │       │   └── GetAssets/
│   │       ├── DTOs/
│   │       └── Validators/
│   └── Mapping/
├── Controllers/
├── Domain/
├── Infrastructure/
└── Program.cs
```

### Adding New Features

1. **Create Command/Query**: Add new command/query classes in the appropriate folder
2. **Create Handler**: Implement the handler logic
3. **Create Validator**: Add validation rules using FluentValidation
4. **Update Controller**: Add new endpoint with proper authorization
5. **Add Tests**: Create unit tests for the new functionality

### Running Tests
```bash
dotnet test
```

## Configuration

### Environment Variables
- `ASPNETCORE_ENVIRONMENT`: Set to `Development`, `Staging`, or `Production`
- `ConnectionStrings__DefaultConnection`: Database connection string

### Swagger Configuration
Swagger is automatically configured with:
- API versioning support
- Role-based authorization documentation
- Request/response examples
- Comprehensive endpoint documentation

## Monitoring & Health

### Health Endpoints
- `/health`: Public health check
- `/api/v{version}/assets/health`: Service-specific health check

### Logging
The service uses structured logging with:
- Request/response logging
- Exception logging
- Performance metrics

## Security

### Authentication
- JWT token-based authentication
- Token validation on all protected endpoints

### Authorization
- Role-based access control (RBAC)
- Permission-based policies
- Tenant isolation support

### Data Protection
- Input validation and sanitization
- SQL injection prevention
- XSS protection

## Performance

### Optimization Features
- Entity Framework query optimization
- Pagination for large datasets
- Efficient filtering and sorting
- Connection pooling

### Caching
- Response caching for read operations
- Database query result caching

## Troubleshooting

### Common Issues

1. **Database Connection Failed**
   - Verify connection string in `appsettings.json`
   - Ensure SQL Server is running
   - Check firewall settings

2. **Authorization Failed**
   - Verify JWT token is valid
   - Check user has required role (SystemAdmin or TenantAdmin)
   - Ensure token hasn't expired

3. **Validation Errors**
   - Check request payload matches required schema
   - Verify all required fields are provided
   - Ensure data types are correct

### Debug Mode
Enable debug logging in `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

## Contributing

1. Follow the existing code structure and patterns
2. Add comprehensive tests for new features
3. Update documentation for any API changes
4. Ensure all endpoints have proper authorization
5. Follow C# coding conventions

## License

This project is part of the CMMS system and follows the same licensing terms.

## Support

For issues and questions:
1. Check the troubleshooting section
2. Review API documentation in Swagger
3. Contact the development team
4. Create an issue in the project repository 