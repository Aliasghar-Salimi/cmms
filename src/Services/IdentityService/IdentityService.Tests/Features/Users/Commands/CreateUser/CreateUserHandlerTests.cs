using AutoMapper;
using FluentAssertions;
using IdentityService.Application.Features.Users.Commands.CreateUser;
using IdentityService.Application.Features.Users.DTOs;
using IdentityService.Application.Features.Users.Handlers;
using IdentityService.Application.Common;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Tests.Common;
using IdentityService.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace IdentityService.Tests.Features.Users.Commands.CreateUser;

public class CreateUserHandlerTests : TestBase
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<RoleManager<ApplicationRole>> _mockRoleManager;
    private readonly Mock<IMapper> _mockMapper;
    private readonly TestDbContext _testDbContext;
    private readonly CreateUserHandler _handler;

    public CreateUserHandlerTests()
    {
        _testDbContext = new TestDbContext();
        
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null, null, null, null, null, null, null, null);
        
        var roleStore = new Mock<IRoleStore<ApplicationRole>>();
        _mockRoleManager = new Mock<RoleManager<ApplicationRole>>(
            roleStore.Object, null, null, null, null);
        
        _mockMapper = new Mock<IMapper>();
        
        _handler = new CreateUserHandler(
            _mockUserManager.Object,
            _mockRoleManager.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WithValidUserData_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            PhoneNumber = "+1234567890",
            TenantId = Guid.NewGuid(),
            RoleIds = new List<Guid>()
        };

        var user = TestData.Users.CreateValidUser();
        user.UserName = command.UserName;
        user.Email = command.Email;

        _mockUserManager.Setup(x => x.FindByNameAsync(command.UserName))
            .ReturnsAsync((ApplicationUser?)null);
        
        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null);
        
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);
        
        _mockUserManager.Setup(x => x.Users)
            .Returns(_testDbContext.Context.Users);
        
        _mockUserManager.Setup(x => x.Users.Include(It.IsAny<string>()))
            .Returns(_testDbContext.Context.Users);

        var userDto = new UserDto { Id = user.Id.ToString(), UserName = user.UserName, Email = user.Email };
        _mockMapper.Setup(x => x.Map<UserDto>(It.IsAny<ApplicationUser>()))
            .Returns(userDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.UserName.Should().Be(command.UserName);
        result.Data.Email.Should().Be(command.Email);
    }

    [Fact]
    public async Task Handle_WithExistingUsername_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            UserName = "existinguser",
            Email = "test@example.com",
            Password = "Password123!",
            PhoneNumber = "+1234567890",
            TenantId = Guid.NewGuid(),
            RoleIds = new List<Guid>()
        };

        var existingUser = TestData.Users.CreateValidUser();
        existingUser.UserName = command.UserName;

        _mockUserManager.Setup(x => x.FindByNameAsync(command.UserName))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Username already exists");
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            UserName = "newuser",
            Email = "existing@example.com",
            Password = "Password123!",
            PhoneNumber = "+1234567890",
            TenantId = Guid.NewGuid(),
            RoleIds = new List<Guid>()
        };

        var existingUser = TestData.Users.CreateValidUser();
        existingUser.Email = command.Email;

        _mockUserManager.Setup(x => x.FindByNameAsync(command.UserName))
            .ReturnsAsync((ApplicationUser?)null);
        
        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email already exists");
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "weak",
            PhoneNumber = "+1234567890",
            TenantId = Guid.NewGuid(),
            RoleIds = new List<Guid>()
        };

        _mockUserManager.Setup(x => x.FindByNameAsync(command.UserName))
            .ReturnsAsync((ApplicationUser?)null);
        
        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null);
        
        var identityErrors = new List<IdentityError>
        {
            new() { Description = "Password is too short" },
            new() { Description = "Password must contain uppercase letters" }
        };
        
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Password is too short");
        result.Error.Should().Contain("Password must contain uppercase letters");
    }

    [Fact]
    public async Task Handle_WithValidRoles_ShouldAssignRolesSuccessfully()
    {
        // Arrange
        var roleId = Guid.NewGuid().ToString();
        var command = new CreateUserCommand
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            PhoneNumber = "+1234567890",
            TenantId = Guid.NewGuid(),
            RoleIds = new List<Guid> { Guid.Parse(roleId) }
        };

        var role = TestData.Roles.CreateValidRole();
        role.Id = Guid.Parse(roleId);

        _mockUserManager.Setup(x => x.FindByNameAsync(command.UserName))
            .ReturnsAsync((ApplicationUser?)null);
        
        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null);
        
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);
        
        _mockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _mockUserManager.Setup(x => x.Users)
            .Returns(_testDbContext.Context.Users);

        _mockRoleManager.Setup(x => x.Roles)
            .Returns(_testDbContext.Context.Roles);

        var userDto = new UserDto { Id = Guid.NewGuid().ToString(), UserName = command.UserName, Email = command.Email };
        _mockMapper.Setup(x => x.Map<UserDto>(It.IsAny<ApplicationUser>()))
            .Returns(userDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        _mockUserManager.Verify(x => x.AddToRolesAsync(
            It.IsAny<ApplicationUser>(), 
            It.Is<IEnumerable<string>>(roles => roles.Contains(role.Name))), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithRoleAssignmentFailure_ShouldReturnFailure()
    {
        // Arrange
        var roleId = Guid.NewGuid().ToString();
        var command = new CreateUserCommand
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            PhoneNumber = "+1234567890",
            TenantId = Guid.NewGuid(),
            RoleIds = new List<Guid> { Guid.Parse(roleId) }
        };

        var role = TestData.Roles.CreateValidRole();
        role.Id = Guid.Parse(roleId);

        _mockUserManager.Setup(x => x.FindByNameAsync(command.UserName))
            .ReturnsAsync((ApplicationUser?)null);
        
        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null);
        
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);
        
        var roleErrors = new List<IdentityError>
        {
            new() { Description = "Role does not exist" }
        };
        
        _mockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Failed(roleErrors.ToArray()));

        _mockRoleManager.Setup(x => x.Roles)
            .Returns(_testDbContext.Context.Roles);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Role does not exist");
    }

    [Fact]
    public async Task Handle_WithException_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            PhoneNumber = "+1234567890",
            TenantId = Guid.NewGuid(),
            RoleIds = new List<Guid>()
        };

        _mockUserManager.Setup(x => x.FindByNameAsync(command.UserName))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("An error occurred while creating the user");
        result.Error.Should().Contain("Database connection failed");
    }

    public void Dispose()
    {
        _testDbContext.Dispose();
    }
} 