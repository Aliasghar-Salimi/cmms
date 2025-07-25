using AutoMapper;
using FluentAssertions;
using IdentityService.Application.Features.Roles.Commands.CreateRole;
using IdentityService.Application.Features.Roles.DTOs;
using IdentityService.Application.Common;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Tests.Common;
using IdentityService.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace IdentityService.Tests.Features.Roles.Commands.CreateRole;

public class CreateRoleCommandHandlerTests : TestBase
{
    private readonly Mock<RoleManager<ApplicationRole>> _mockRoleManager;
    private readonly Mock<IMapper> _mockMapper;
    private readonly TestDbContext _testDbContext;
    private readonly CreateRoleCommandHandler _handler;

    public CreateRoleCommandHandlerTests()
    {
        _testDbContext = new TestDbContext();
        
        var roleStore = new Mock<IRoleStore<ApplicationRole>>();
        _mockRoleManager = new Mock<RoleManager<ApplicationRole>>(
            roleStore.Object, null, null, null, null);
        
        _mockMapper = new Mock<IMapper>();
        
        _handler = new CreateRoleCommandHandler(
            _mockRoleManager.Object,
            _testDbContext.Context,
            _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WithValidRoleData_ShouldCreateRoleSuccessfully()
    {
        // Arrange
        var command = new CreateRoleCommand
        {
            Name = "TestRole",
            Description = "Test role for unit testing",
            TenantId = Guid.NewGuid().ToString(),
            PermissionIds = new List<string>()
        };

        var role = TestData.Roles.CreateValidRole();
        role.Name = command.Name;
        role.TenantId = Guid.Parse(command.TenantId);

        _mockRoleManager.Setup(x => x.FindByNameAsync(command.Name))
            .ReturnsAsync((ApplicationRole?)null);
        
        _mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        var roleDto = new RoleDto { Id = role.Id.ToString(), Name = role.Name, Description = role.Description };
        _mockMapper.Setup(x => x.Map<RoleDto>(It.IsAny<ApplicationRole>()))
            .Returns(roleDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(command.Name);
        result.Data.Description.Should().Be(command.Description);
    }

    [Fact]
    public async Task Handle_WithExistingRoleInSameTenant_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateRoleCommand
        {
            Name = "ExistingRole",
            Description = "Test role for unit testing",
            TenantId = tenantId.ToString(),
            PermissionIds = new List<string>()
        };

        var existingRole = TestData.Roles.CreateValidRole();
        existingRole.Name = command.Name;
        existingRole.TenantId = tenantId;

        _mockRoleManager.Setup(x => x.FindByNameAsync(command.Name))
            .ReturnsAsync(existingRole);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be($"Role '{command.Name}' already exists in this tenant.");
    }

    [Fact]
    public async Task Handle_WithExistingRoleInDifferentTenant_ShouldCreateRoleSuccessfully()
    {
        // Arrange
        var command = new CreateRoleCommand
        {
            Name = "TestRole",
            Description = "Test role for unit testing",
            TenantId = Guid.NewGuid().ToString(),
            PermissionIds = new List<string>()
        };

        var existingRole = TestData.Roles.CreateValidRole();
        existingRole.Name = command.Name;
        existingRole.TenantId = Guid.NewGuid(); // Different tenant

        _mockRoleManager.Setup(x => x.FindByNameAsync(command.Name))
            .ReturnsAsync(existingRole);
        
        _mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        var roleDto = new RoleDto { Id = Guid.NewGuid().ToString(), Name = command.Name, Description = command.Description };
        _mockMapper.Setup(x => x.Map<RoleDto>(It.IsAny<ApplicationRole>()))
            .Returns(roleDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(command.Name);
    }

    [Fact]
    public async Task Handle_WithInvalidRoleData_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateRoleCommand
        {
            Name = "", // Invalid name
            Description = "Test role for unit testing",
            TenantId = Guid.NewGuid().ToString(),
            PermissionIds = new List<string>()
        };

        _mockRoleManager.Setup(x => x.FindByNameAsync(command.Name))
            .ReturnsAsync((ApplicationRole?)null);
        
        var identityErrors = new List<IdentityError>
        {
            new() { Description = "Role name cannot be empty" }
        };
        
        _mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to create role");
        result.Error.Should().Contain("Role name cannot be empty");
    }

    [Fact]
    public async Task Handle_WithValidPermissions_ShouldAssignPermissionsSuccessfully()
    {
        // Arrange
        var permissionId = Guid.NewGuid();
        var command = new CreateRoleCommand
        {
            Name = "TestRole",
            Description = "Test role for unit testing",
            TenantId = Guid.NewGuid().ToString(),
            PermissionIds = new List<string> { permissionId.ToString() }
        };

        var role = TestData.Roles.CreateValidRole();
        role.Name = command.Name;

        _mockRoleManager.Setup(x => x.FindByNameAsync(command.Name))
            .ReturnsAsync((ApplicationRole?)null);
        
        _mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        var roleDto = new RoleDto { Id = role.Id.ToString(), Name = role.Name, Description = role.Description };
        _mockMapper.Setup(x => x.Map<RoleDto>(It.IsAny<ApplicationRole>()))
            .Returns(roleDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        // Verify role permissions were created
        var rolePermissions = await _testDbContext.Context.RolePermissions
            .Where(rp => rp.RoleId == role.Id && rp.PermissionId == permissionId)
            .ToListAsync();
        
        rolePermissions.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithMultiplePermissions_ShouldAssignAllPermissions()
    {
        // Arrange
        var permissionIds = new List<string> 
        { 
            Guid.NewGuid().ToString(), 
            Guid.NewGuid().ToString() 
        };
        
        var command = new CreateRoleCommand
        {
            Name = "TestRole",
            Description = "Test role for unit testing",
            TenantId = Guid.NewGuid().ToString(),
            PermissionIds = permissionIds
        };

        var role = TestData.Roles.CreateValidRole();
        role.Name = command.Name;

        _mockRoleManager.Setup(x => x.FindByNameAsync(command.Name))
            .ReturnsAsync((ApplicationRole?)null);
        
        _mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        var roleDto = new RoleDto { Id = role.Id.ToString(), Name = role.Name, Description = role.Description };
        _mockMapper.Setup(x => x.Map<RoleDto>(It.IsAny<ApplicationRole>()))
            .Returns(roleDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        // Verify all role permissions were created
        var rolePermissions = await _testDbContext.Context.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .ToListAsync();
        
        rolePermissions.Should().HaveCount(2);
        rolePermissions.Should().OnlyContain(rp => permissionIds.Contains(rp.PermissionId.ToString()));
    }

    [Fact]
    public async Task Handle_WithNullPermissionIds_ShouldCreateRoleWithoutPermissions()
    {
        // Arrange
        var command = new CreateRoleCommand
        {
            Name = "TestRole",
            Description = "Test role for unit testing",
            TenantId = Guid.NewGuid().ToString(),
            PermissionIds = null
        };

        var role = TestData.Roles.CreateValidRole();
        role.Name = command.Name;

        _mockRoleManager.Setup(x => x.FindByNameAsync(command.Name))
            .ReturnsAsync((ApplicationRole?)null);
        
        _mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        var roleDto = new RoleDto { Id = role.Id.ToString(), Name = role.Name, Description = role.Description };
        _mockMapper.Setup(x => x.Map<RoleDto>(It.IsAny<ApplicationRole>()))
            .Returns(roleDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        
        // Verify no role permissions were created
        var rolePermissions = await _testDbContext.Context.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .ToListAsync();
        
        rolePermissions.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithEmptyPermissionIds_ShouldCreateRoleWithoutPermissions()
    {
        // Arrange
        var command = new CreateRoleCommand
        {
            Name = "TestRole",
            Description = "Test role for unit testing",
            TenantId = Guid.NewGuid().ToString(),
            PermissionIds = new List<string>()
        };

        var role = TestData.Roles.CreateValidRole();
        role.Name = command.Name;

        _mockRoleManager.Setup(x => x.FindByNameAsync(command.Name))
            .ReturnsAsync((ApplicationRole?)null);
        
        _mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        var roleDto = new RoleDto { Id = role.Id.ToString(), Name = role.Name, Description = role.Description };
        _mockMapper.Setup(x => x.Map<RoleDto>(It.IsAny<ApplicationRole>()))
            .Returns(roleDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        
        // Verify no role permissions were created
        var rolePermissions = await _testDbContext.Context.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .ToListAsync();
        
        rolePermissions.Should().BeEmpty();
    }

    public void Dispose()
    {
        _testDbContext.Dispose();
    }
} 