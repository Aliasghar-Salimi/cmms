using AutoMapper;
using FluentAssertions;
using IdentityService.Application.Features.Permissions.Commands.CreatePermission;
using IdentityService.Application.Features.Permissions.DTOs;
using IdentityService.Application.Common;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Tests.Common;
using IdentityService.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace IdentityService.Tests.Features.Permissions.Commands.CreatePermission;

public class CreatePermissionCommandHandlerTests : TestBase
{
    private readonly Mock<IMapper> _mockMapper;
    private readonly TestDbContext _testDbContext;
    private readonly CreatePermissionCommandHandler _handler;

    public CreatePermissionCommandHandlerTests()
    {
        _testDbContext = new TestDbContext();
        _mockMapper = new Mock<IMapper>();
        
        _handler = new CreatePermissionCommandHandler(
            _testDbContext.Context,
            _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WithValidPermissionData_ShouldCreatePermissionSuccessfully()
    {
        // Arrange
        var command = new CreatePermissionCommand
        {
            Name = "CreateUser",
            Description = "Permission to create users",
            Resource = "User",
            Action = "Create",
            TenantId = Guid.NewGuid().ToString()
        };

        var permission = TestData.Permissions.CreateValidPermission();
        permission.Name = command.Name;
        permission.Resource = command.Resource;
        permission.Action = command.Action;
        permission.TenantId = Guid.Parse(command.TenantId);

        var permissionDto = new PermissionDto 
        { 
            Id = permission.Id.ToString(), 
            Name = permission.Name, 
            Description = permission.Description,
            Resource = permission.Resource,
            Action = permission.Action
        };
        
        _mockMapper.Setup(x => x.Map<PermissionDto>(It.IsAny<Permission>()))
            .Returns(permissionDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(command.Name);
        result.Data.Description.Should().Be(command.Description);
        result.Data.Resource.Should().Be(command.Resource);
        result.Data.Action.Should().Be(command.Action);
    }

    [Fact]
    public async Task Handle_WithExistingPermissionInSameTenant_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreatePermissionCommand
        {
            Name = "CreateUser",
            Description = "Permission to create users",
            Resource = "User",
            Action = "Create",
            TenantId = tenantId.ToString()
        };

        var existingPermission = TestData.Permissions.CreateValidPermission();
        existingPermission.Resource = command.Resource;
        existingPermission.Action = command.Action;
        existingPermission.TenantId = tenantId;

        await _testDbContext.Context.Permissions.AddAsync(existingPermission);
        await _testDbContext.Context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be($"Permission '{command.Resource}.{command.Action}' already exists in this tenant.");
    }

    [Fact]
    public async Task Handle_WithExistingPermissionInDifferentTenant_ShouldCreatePermissionSuccessfully()
    {
        // Arrange
        var command = new CreatePermissionCommand
        {
            Name = "CreateUser",
            Description = "Permission to create users",
            Resource = "User",
            Action = "Create",
            TenantId = Guid.NewGuid().ToString()
        };

        var existingPermission = TestData.Permissions.CreateValidPermission();
        existingPermission.Resource = command.Resource;
        existingPermission.Action = command.Action;
        existingPermission.TenantId = Guid.NewGuid(); // Different tenant

        await _testDbContext.Context.Permissions.AddAsync(existingPermission);
        await _testDbContext.Context.SaveChangesAsync();

        var permissionDto = new PermissionDto 
        { 
            Id = Guid.NewGuid().ToString(), 
            Name = command.Name, 
            Description = command.Description,
            Resource = command.Resource,
            Action = command.Action
        };
        
        _mockMapper.Setup(x => x.Map<PermissionDto>(It.IsAny<Permission>()))
            .Returns(permissionDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(command.Name);
        result.Data.Resource.Should().Be(command.Resource);
        result.Data.Action.Should().Be(command.Action);
    }

    [Fact]
    public async Task Handle_WithDifferentResourceSameAction_ShouldCreatePermissionSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreatePermissionCommand
        {
            Name = "CreateRole",
            Description = "Permission to create roles",
            Resource = "Role", // Different resource
            Action = "Create", // Same action
            TenantId = tenantId.ToString()
        };

        var existingPermission = TestData.Permissions.CreateValidPermission();
        existingPermission.Resource = "User"; // Different resource
        existingPermission.Action = command.Action;
        existingPermission.TenantId = tenantId;

        await _testDbContext.Context.Permissions.AddAsync(existingPermission);
        await _testDbContext.Context.SaveChangesAsync();

        var permissionDto = new PermissionDto 
        { 
            Id = Guid.NewGuid().ToString(), 
            Name = command.Name, 
            Description = command.Description,
            Resource = command.Resource,
            Action = command.Action
        };
        
        _mockMapper.Setup(x => x.Map<PermissionDto>(It.IsAny<Permission>()))
            .Returns(permissionDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(command.Name);
    }

    [Fact]
    public async Task Handle_WithSameResourceDifferentAction_ShouldCreatePermissionSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreatePermissionCommand
        {
            Name = "UpdateUser",
            Description = "Permission to update users",
            Resource = "User", // Same resource
            Action = "Update", // Different action
            TenantId = tenantId.ToString()
        };

        var existingPermission = TestData.Permissions.CreateValidPermission();
        existingPermission.Resource = command.Resource;
        existingPermission.Action = "Create"; // Different action
        existingPermission.TenantId = tenantId;

        await _testDbContext.Context.Permissions.AddAsync(existingPermission);
        await _testDbContext.Context.SaveChangesAsync();

        var permissionDto = new PermissionDto 
        { 
            Id = Guid.NewGuid().ToString(), 
            Name = command.Name, 
            Description = command.Description,
            Resource = command.Resource,
            Action = command.Action
        };
        
        _mockMapper.Setup(x => x.Map<PermissionDto>(It.IsAny<Permission>()))
            .Returns(permissionDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(command.Name);
    }

    [Theory]
    [InlineData("", "Create", "User")]
    [InlineData("CreateUser", "", "User")]
    [InlineData("CreateUser", "Create", "")]
    [InlineData(null, "Create", "User")]
    [InlineData("CreateUser", null, "User")]
    [InlineData("CreateUser", "Create", null)]
    public async Task Handle_WithInvalidData_ShouldCreatePermissionSuccessfully(string name, string action, string resource)
    {
        // Arrange
        var command = new CreatePermissionCommand
        {
            Name = name,
            Description = "Permission to create users",
            Resource = resource,
            Action = action,
            TenantId = Guid.NewGuid().ToString()
        };

        var permissionDto = new PermissionDto 
        { 
            Id = Guid.NewGuid().ToString(), 
            Name = command.Name, 
            Description = command.Description,
            Resource = command.Resource,
            Action = command.Action
        };
        
        _mockMapper.Setup(x => x.Map<PermissionDto>(It.IsAny<Permission>()))
            .Returns(permissionDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldSetCorrectDefaultValues()
    {
        // Arrange
        var command = new CreatePermissionCommand
        {
            Name = "TestPermission",
            Description = "Test permission",
            Resource = "Test",
            Action = "Test",
            TenantId = Guid.NewGuid().ToString()
        };

        var permissionDto = new PermissionDto 
        { 
            Id = Guid.NewGuid().ToString(), 
            Name = command.Name, 
            Description = command.Description,
            Resource = command.Resource,
            Action = command.Action
        };
        
        _mockMapper.Setup(x => x.Map<PermissionDto>(It.IsAny<Permission>()))
            .Returns(permissionDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        // Verify the permission was saved with correct default values
        var savedPermission = await _testDbContext.Context.Permissions
            .FirstOrDefaultAsync(p => p.Name == command.Name);
        
        savedPermission.Should().NotBeNull();
        savedPermission!.IsActive.Should().BeTrue();
        savedPermission.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        savedPermission.TenantId.Should().Be(Guid.Parse(command.TenantId));
    }

    public void Dispose()
    {
        _testDbContext.Dispose();
    }
} 