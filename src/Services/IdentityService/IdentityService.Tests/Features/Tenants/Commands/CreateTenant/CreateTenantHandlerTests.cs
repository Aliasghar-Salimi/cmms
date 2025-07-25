using AutoMapper;
using FluentAssertions;
using IdentityService.Application.Features.Tenants.Commands.CreateTenant;
using IdentityService.Application.Features.Tenants.DTOs;
using IdentityService.Application.Features.Tenants.Handlers;
using IdentityService.Application.Common;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Tests.Common;
using IdentityService.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace IdentityService.Tests.Features.Tenants.Commands.CreateTenant;

public class CreateTenantHandlerTests : TestBase
{
    private readonly Mock<IMapper> _mockMapper;
    private readonly TestDbContext _testDbContext;
    private readonly CreateTenantHandler _handler;

    public CreateTenantHandlerTests()
    {
        _testDbContext = new TestDbContext();
        _mockMapper = new Mock<IMapper>();
        
        _handler = new CreateTenantHandler(
            _testDbContext.Context,
            _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WithValidTenantData_ShouldCreateTenantSuccessfully()
    {
        // Arrange
        var command = new CreateTenantCommand
        {
            Name = "TestTenant",
            Description = "Test tenant for unit testing",
            IsActive = true
        };

        var tenant = TestData.Tenants.CreateValidTenant();
        tenant.Name = command.Name;
        tenant.Description = command.Description;

        var tenantDto = new TenantDto 
        { 
            Id = tenant.Id.ToString(), 
            Name = tenant.Name, 
            Description = tenant.Description,
            IsActive = tenant.IsActive
        };
        
        _mockMapper.Setup(x => x.Map<TenantDto>(It.IsAny<Tenant>()))
            .Returns(tenantDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(command.Name);
        result.Data.Description.Should().Be(command.Description);
        result.Data.IsActive.Should().Be(command.IsActive);
    }

    [Fact]
    public async Task Handle_WithExistingTenantName_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateTenantCommand
        {
            Name = "ExistingTenant",
            Description = "Test tenant for unit testing",
            IsActive = true
        };

        var existingTenant = TestData.Tenants.CreateValidTenant();
        existingTenant.Name = command.Name;

        await _testDbContext.Context.Tenants.AddAsync(existingTenant);
        await _testDbContext.Context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Tenant name already exists");
    }

    [Fact]
    public async Task Handle_WithInactiveTenant_ShouldCreateTenantSuccessfully()
    {
        // Arrange
        var command = new CreateTenantCommand
        {
            Name = "InactiveTenant",
            Description = "Test inactive tenant",
            IsActive = false
        };

        var tenant = TestData.Tenants.CreateValidTenant();
        tenant.Name = command.Name;
        tenant.IsActive = false;

        var tenantDto = new TenantDto 
        { 
            Id = tenant.Id.ToString(), 
            Name = tenant.Name, 
            Description = tenant.Description,
            IsActive = tenant.IsActive
        };
        
        _mockMapper.Setup(x => x.Map<TenantDto>(It.IsAny<Tenant>()))
            .Returns(tenantDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(command.Name);
        result.Data.IsActive.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task Handle_WithInvalidTenantName_ShouldCreateTenantSuccessfully(string name)
    {
        // Arrange
        var command = new CreateTenantCommand
        {
            Name = name,
            Description = "Test tenant for unit testing",
            IsActive = true
        };

        var tenant = TestData.Tenants.CreateValidTenant();
        tenant.Name = name;

        var tenantDto = new TenantDto 
        { 
            Id = tenant.Id.ToString(), 
            Name = tenant.Name, 
            Description = tenant.Description,
            IsActive = tenant.IsActive
        };
        
        _mockMapper.Setup(x => x.Map<TenantDto>(It.IsAny<Tenant>()))
            .Returns(tenantDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(name);
    }

    [Fact]
    public async Task Handle_ShouldSetCorrectTimestamps()
    {
        // Arrange
        var command = new CreateTenantCommand
        {
            Name = "TestTenant",
            Description = "Test tenant for unit testing",
            IsActive = true
        };

        var tenant = TestData.Tenants.CreateValidTenant();
        tenant.Name = command.Name;

        var tenantDto = new TenantDto 
        { 
            Id = tenant.Id.ToString(), 
            Name = tenant.Name, 
            Description = tenant.Description,
            IsActive = tenant.IsActive
        };
        
        _mockMapper.Setup(x => x.Map<TenantDto>(It.IsAny<Tenant>()))
            .Returns(tenantDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        // Verify timestamps were set correctly
        var savedTenant = await _testDbContext.Context.Tenants
            .FirstOrDefaultAsync(t => t.Name == command.Name);
        
        savedTenant.Should().NotBeNull();
        savedTenant!.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        savedTenant.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldGenerateUniqueId()
    {
        // Arrange
        var command = new CreateTenantCommand
        {
            Name = "TestTenant",
            Description = "Test tenant for unit testing",
            IsActive = true
        };

        var tenant = TestData.Tenants.CreateValidTenant();
        tenant.Name = command.Name;

        var tenantDto = new TenantDto 
        { 
            Id = tenant.Id.ToString(), 
            Name = tenant.Name, 
            Description = tenant.Description,
            IsActive = tenant.IsActive
        };
        
        _mockMapper.Setup(x => x.Map<TenantDto>(It.IsAny<Tenant>()))
            .Returns(tenantDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        // Verify unique ID was generated
        var savedTenant = await _testDbContext.Context.Tenants
            .FirstOrDefaultAsync(t => t.Name == command.Name);
        
        savedTenant.Should().NotBeNull();
        savedTenant!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithDatabaseException_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateTenantCommand
        {
            Name = "TestTenant",
            Description = "Test tenant for unit testing",
            IsActive = true
        };

        // Simulate database exception by disposing context
        _testDbContext.Dispose();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("An error occurred while creating the tenant");
    }

    [Fact]
    public async Task Handle_WithMultipleTenants_ShouldCreateAllSuccessfully()
    {
        // Arrange
        var commands = new List<CreateTenantCommand>
        {
            new() { Name = "Tenant1", Description = "First tenant", IsActive = true },
            new() { Name = "Tenant2", Description = "Second tenant", IsActive = true },
            new() { Name = "Tenant3", Description = "Third tenant", IsActive = false }
        };

        var results = new List<Result<TenantDto>>();

        // Act
        foreach (var command in commands)
        {
            var tenant = TestData.Tenants.CreateValidTenant();
            tenant.Name = command.Name;

            var tenantDto = new TenantDto 
            { 
                Id = tenant.Id.ToString(), 
                Name = tenant.Name, 
                Description = tenant.Description,
                IsActive = tenant.IsActive
            };
            
            _mockMapper.Setup(x => x.Map<TenantDto>(It.IsAny<Tenant>()))
                .Returns(tenantDto);

            var result = await _handler.Handle(command, CancellationToken.None);
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(3);
        results.Should().OnlyContain(r => r.IsSuccess);
        
        // Verify all tenants were created
        var savedTenants = await _testDbContext.Context.Tenants.ToListAsync();
        savedTenants.Should().HaveCount(3);
        savedTenants.Should().OnlyContain(t => commands.Any(c => c.Name == t.Name));
    }

    public void Dispose()
    {
        _testDbContext.Dispose();
    }
} 