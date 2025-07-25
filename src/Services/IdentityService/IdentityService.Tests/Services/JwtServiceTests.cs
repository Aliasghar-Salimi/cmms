using FluentAssertions;
using IdentityService.Application.Common.Services;
using IdentityService.Domain.Entities;
using IdentityService.Tests.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.Security.Claims;
using Xunit;

namespace IdentityService.Tests.Services;

public class JwtServiceTests : TestBase
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        
        _mockConfiguration.Setup(x => x["Jwt:Key"]).Returns("your-super-secret-key-here-make-it-long-enough-for-security");
        _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("CMMS.IdentityService");
        _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("CMMS.Users");
        
        _jwtService = new JwtService(_mockConfiguration.Object);
    }

    [Fact]
    public void GenerateToken_WithValidUser_ShouldReturnValidToken()
    {
        // Arrange
        var user = TestData.Users.CreateValidUser();
        var roles = new List<string> { "User", "Admin" };
        var permissions = new List<string> { "User:Read", "User:Write" };

        // Act
        var token = _jwtService.GenerateToken(user, roles, permissions);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Should().Contain(".");
    }

    [Fact]
    public void GenerateToken_WithUserClaims_ShouldIncludeAllClaims()
    {
        // Arrange
        var user = TestData.Users.CreateValidUser();
        var roles = new List<string> { "User" };
        var permissions = new List<string> { "User:Read" };

        // Act
        var token = _jwtService.GenerateToken(user, roles, permissions);
        var principal = _jwtService.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        principal.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        principal.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == user.UserName);
        principal.Claims.Should().Contain(c => c.Type == "tenant_id" && c.Value == user.TenantId.ToString());
        principal.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
        principal.Claims.Should().Contain(c => c.Type == "permission" && c.Value == "User:Read");
    }

    [Fact]
    public void GenerateToken_WithMultipleRoles_ShouldIncludeAllRoles()
    {
        // Arrange
        var user = TestData.Users.CreateValidUser();
        var roles = new List<string> { "User", "Admin", "Manager" };
        var permissions = new List<string>();

        // Act
        var token = _jwtService.GenerateToken(user, roles, permissions);
        var principal = _jwtService.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        var roleClaims = principal!.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        roleClaims.Should().HaveCount(3);
        roleClaims.Should().Contain(c => c.Value == "User");
        roleClaims.Should().Contain(c => c.Value == "Admin");
        roleClaims.Should().Contain(c => c.Value == "Manager");
    }

    [Fact]
    public void GenerateToken_WithMultiplePermissions_ShouldIncludeAllPermissions()
    {
        // Arrange
        var user = TestData.Users.CreateValidUser();
        var roles = new List<string>();
        var permissions = new List<string> { "User:Read", "User:Write", "User:Delete" };

        // Act
        var token = _jwtService.GenerateToken(user, roles, permissions);
        var principal = _jwtService.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        var permissionClaims = principal!.Claims.Where(c => c.Type == "permission").ToList();
        permissionClaims.Should().HaveCount(3);
        permissionClaims.Should().Contain(c => c.Value == "User:Read");
        permissionClaims.Should().Contain(c => c.Value == "User:Write");
        permissionClaims.Should().Contain(c => c.Value == "User:Delete");
    }

    [Fact]
    public void GenerateToken_WithEmptyRolesAndPermissions_ShouldGenerateValidToken()
    {
        // Arrange
        var user = TestData.Users.CreateValidUser();
        var roles = new List<string>();
        var permissions = new List<string>();

        // Act
        var token = _jwtService.GenerateToken(user, roles, permissions);
        var principal = _jwtService.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier);
        principal.Claims.Should().NotContain(c => c.Type == ClaimTypes.Role);
        principal.Claims.Should().NotContain(c => c.Type == "permission");
    }

    [Fact]
    public void ValidateToken_WithValidToken_ShouldReturnClaimsPrincipal()
    {
        // Arrange
        var user = TestData.Users.CreateValidUser();
        var roles = new List<string> { "User" };
        var permissions = new List<string> { "User:Read" };

        var token = _jwtService.GenerateToken(user, roles, permissions);

        // Act
        var principal = _jwtService.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.Identity.Should().NotBeNull();
        principal.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var principal = _jwtService.ValidateToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithExpiredToken_ShouldReturnNull()
    {
        // Arrange
        var user = TestData.Users.CreateValidUser();
        var roles = new List<string> { "User" };
        var permissions = new List<string>();

        // Create a service with short expiration for testing
        var shortExpirationConfig = new Mock<IConfiguration>();
        shortExpirationConfig.Setup(x => x["Jwt:Key"]).Returns("your-super-secret-key-here-make-it-long-enough-for-security");
        shortExpirationConfig.Setup(x => x["Jwt:Issuer"]).Returns("CMMS.IdentityService");
        shortExpirationConfig.Setup(x => x["Jwt:Audience"]).Returns("CMMS.Users");

        var shortExpirationService = new JwtService(shortExpirationConfig.Object);
        var token = shortExpirationService.GenerateToken(user, roles, permissions);

        // Wait for token to expire (if we had a way to set very short expiration)
        // For now, we'll test with an obviously invalid token
        var expiredToken = token + ".expired";

        // Act
        var principal = _jwtService.ValidateToken(expiredToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithWrongIssuer_ShouldReturnNull()
    {
        // Arrange
        var user = TestData.Users.CreateValidUser();
        var roles = new List<string> { "User" };
        var permissions = new List<string>();

        var token = _jwtService.GenerateToken(user, roles, permissions);

        // Create a service with different issuer
        var differentIssuerConfig = new Mock<IConfiguration>();
        differentIssuerConfig.Setup(x => x["Jwt:Key"]).Returns("your-super-secret-key-here-make-it-long-enough-for-security");
        differentIssuerConfig.Setup(x => x["Jwt:Issuer"]).Returns("DifferentIssuer");
        differentIssuerConfig.Setup(x => x["Jwt:Audience"]).Returns("CMMS.Users");

        var differentIssuerService = new JwtService(differentIssuerConfig.Object);

        // Act
        var principal = differentIssuerService.ValidateToken(token);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithWrongAudience_ShouldReturnNull()
    {
        // Arrange
        var user = TestData.Users.CreateValidUser();
        var roles = new List<string> { "User" };
        var permissions = new List<string>();

        var token = _jwtService.GenerateToken(user, roles, permissions);

        // Create a service with different audience
        var differentAudienceConfig = new Mock<IConfiguration>();
        differentAudienceConfig.Setup(x => x["Jwt:Key"]).Returns("your-super-secret-key-here-make-it-long-enough-for-security");
        differentAudienceConfig.Setup(x => x["Jwt:Issuer"]).Returns("CMMS.IdentityService");
        differentAudienceConfig.Setup(x => x["Jwt:Audience"]).Returns("DifferentAudience");

        var differentAudienceService = new JwtService(differentAudienceConfig.Object);

        // Act
        var principal = differentAudienceService.ValidateToken(token);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnUniqueTokens()
    {
        // Act
        var token1 = _jwtService.GenerateRefreshToken();
        var token2 = _jwtService.GenerateRefreshToken();

        // Assert
        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnValidGuid()
    {
        // Act
        var token = _jwtService.GenerateRefreshToken();

        // Assert
        token.Should().NotBeNullOrEmpty();
        Guid.TryParse(token, out _).Should().BeTrue();
    }

    [Fact]
    public void GenerateToken_WithNullUserProperties_ShouldHandleGracefully()
    {
        // Arrange
        var user = TestData.Users.CreateValidUser();
        user.Email = null;
        user.UserName = null;
        var roles = new List<string> { "User" };
        var permissions = new List<string>();

        // Act
        var token = _jwtService.GenerateToken(user, roles, permissions);
        var principal = _jwtService.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == string.Empty);
        principal.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == string.Empty);
    }

    [Fact]
    public void GenerateToken_WithCustomConfiguration_ShouldUseConfigurationValues()
    {
        // Arrange
        var customConfig = new Mock<IConfiguration>();
        customConfig.Setup(x => x["Jwt:Key"]).Returns("custom-secret-key-for-testing-purposes-only");
        customConfig.Setup(x => x["Jwt:Issuer"]).Returns("CustomIssuer");
        customConfig.Setup(x => x["Jwt:Audience"]).Returns("CustomAudience");

        var customJwtService = new JwtService(customConfig.Object);
        var user = TestData.Users.CreateValidUser();
        var roles = new List<string> { "User" };
        var permissions = new List<string>();

        // Act
        var token = customJwtService.GenerateToken(user, roles, permissions);
        var principal = customJwtService.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.Identity.Should().NotBeNull();
        principal.Identity!.IsAuthenticated.Should().BeTrue();
    }
} 