# IdentityService Unit Tests

This directory contains comprehensive unit tests for the IdentityService, following professional testing best practices and industry standards.

## 🧪 Testing Framework

- **xUnit**: Primary testing framework
- **Moq**: Mocking framework for dependencies
- **FluentAssertions**: Human-readable assertions
- **AutoFixture**: Test data generation
- **coverlet**: Code coverage collection
- **ReportGenerator**: Coverage report generation

## 📁 Test Structure

```
IdentityService.Tests/
├── Common/
│   └── TestData.cs                 # Test data utilities
├── Infrastructure/
│   └── TestDbContext.cs            # In-memory database context
├── Features/
│   ├── Auth/
│   │   ├── Commands/
│   │   │   ├── Login/
│   │   │   └── MfaLogin/
│   │   └── Controllers/
│   ├── Users/
│   │   └── Commands/
│   ├── Roles/
│   │   └── Commands/
│   ├── Permissions/
│   │   └── Commands/
│   └── Tenants/
│       └── Commands/
├── Services/
│   ├── JwtServiceTests.cs
│   └── SmsVerificationServiceTests.cs
├── Controllers/
│   └── AuthControllerTests.cs
└── TestBase.cs                     # Base test class
```

## 🚀 Running Tests

### Prerequisites

- .NET 8.0 SDK
- Git

### Quick Start

1. **Clone and navigate to the project:**
   ```bash
   cd src/Services/IdentityService/IdentityService.Tests
   ```

2. **Run tests with coverage:**
   ```bash
   ./run-tests.sh
   ```

3. **Or run manually:**
   ```bash
   dotnet test --collect:"XPlat Code Coverage"
   ```

### Running Specific Tests

```bash
# Run specific test class
dotnet test --filter "FullyQualifiedName~LoginCommandHandlerTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~LoginCommandHandlerTests.Handle_WithValidCredentials_ShouldReturnSuccessResponse"

# Run tests by category
dotnet test --filter "Category=Integration"
```

## 📊 Coverage Reports

After running tests, coverage reports are generated in:
- **HTML**: `./TestResults/CoverageReport/index.html`
- **Cobertura**: `./TestResults/CoverageReport/Cobertura.xml`
- **Text Summary**: Console output

### Coverage Targets

- **Line Coverage**: > 90%
- **Branch Coverage**: > 85%
- **Function Coverage**: > 95%

## 🏗️ Test Architecture

### TestBase Class

All tests inherit from `TestBase` which provides:
- AutoFixture setup with Moq customization
- Common test utilities
- Mock logger setup
- Service collection helpers

### Test Data

`TestData` class provides factory methods for creating test entities:
- `TestData.Users.CreateValidUser()`
- `TestData.Roles.CreateValidRole()`
- `TestData.Permissions.CreateValidPermission()`
- `TestData.Tenants.CreateValidTenant()`

### In-Memory Database

Tests use Entity Framework's in-memory database provider for:
- Fast test execution
- No external dependencies
- Isolated test data

## 📝 Writing Tests

### Naming Convention

```
[MethodName]_[Scenario]_[ExpectedResult]
```

Examples:
- `Handle_WithValidCredentials_ShouldReturnSuccessResponse`
- `Handle_WithInvalidEmail_ShouldReturnFailure`
- `ValidateToken_WithExpiredToken_ShouldReturnNull`

### Test Structure (AAA Pattern)

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var command = new TestCommand();
    _mockService.Setup(x => x.Method()).ReturnsAsync(result);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
}
```

### Mocking Best Practices

```csharp
// Setup mocks in constructor or test setup
_mockUserManager.Setup(x => x.FindByEmailAsync(email))
    .ReturnsAsync(user);

// Verify interactions when needed
_mockUserManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>()), Times.Once);
```

## 🔧 CI/CD Integration

### GitHub Actions

Tests run automatically on:
- Push to `main` and `develop` branches
- Pull requests to `main` and `develop` branches

### Pipeline Steps

1. **Setup**: .NET 8.0 environment
2. **Restore**: NuGet packages
3. **Build**: Solution compilation
4. **Test**: Run tests with coverage
5. **Report**: Generate coverage reports
6. **Upload**: Artifacts and coverage to Codecov

### Local Development

```bash
# Run tests before committing
./run-tests.sh

# Check coverage locally
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

## 🧪 Test Categories

### Unit Tests
- Test individual components in isolation
- Mock external dependencies
- Fast execution (< 100ms per test)

### Integration Tests
- Test component interactions
- Use in-memory database
- Test real business logic flows

### Controller Tests
- Test HTTP endpoints
- Verify response codes and content
- Mock MediatR handlers

## 📋 Test Coverage Areas

### ✅ Covered
- **Authentication**: Login, MFA, Token refresh
- **User Management**: CRUD operations
- **Role Management**: CRUD operations
- **Permission Management**: CRUD operations
- **Tenant Management**: CRUD operations
- **Services**: JWT, SMS verification
- **Controllers**: All endpoints
- **Validation**: Command validators

### 🔄 In Progress
- **Authorization**: Permission-based access control
- **Audit Logging**: Security event tracking
- **Performance**: Load testing scenarios

## 🐛 Debugging Tests

### Visual Studio Code

1. Set breakpoints in test methods
2. Use `dotnet test --logger "console;verbosity=detailed"`
3. Check test output for detailed information

### Common Issues

1. **Mock Setup**: Ensure mocks are configured before use
2. **Async/Await**: Use `await` for async test methods
3. **Database State**: Clean up test data between tests
4. **Configuration**: Mock configuration values properly

## 📚 Best Practices

### Do's ✅
- Write descriptive test names
- Use AAA pattern (Arrange, Act, Assert)
- Mock external dependencies
- Test both success and failure scenarios
- Keep tests independent and isolated
- Use meaningful test data

### Don'ts ❌
- Don't test implementation details
- Don't create complex test setups
- Don't ignore failing tests
- Don't test framework code
- Don't create flaky tests

## 🔍 Code Quality

### Static Analysis

Tests are analyzed with:
- **StyleCop**: Code style enforcement
- **SonarQube**: Code quality metrics
- **Coverlet**: Coverage analysis

### Performance

- Test execution time: < 30 seconds total
- Individual test time: < 100ms
- Memory usage: < 100MB

## 📞 Support

For questions or issues:
1. Check existing test examples
2. Review test documentation
3. Create issue with test details
4. Contact development team

## 📈 Metrics

- **Total Tests**: 50+
- **Coverage**: > 90%
- **Execution Time**: < 30s
- **Success Rate**: > 99%

---

*Last updated: January 2025* 