using Builder;
using User.Data.Models;

namespace User.Test.Models;

public class RefreshTokenUnitTest
{
    [Fact]
    public void IsExpired_WhenExpiresInFuture_ShouldReturnFalse()
    {
        // Arrange
        RefreshToken token = UserBuilder.CreateRefreshToken("user-id", expires: DateTime.UtcNow.AddDays(1));

        // Act & Assert
        Assert.False(token.IsExpired);
    }

    [Fact]
    public void IsExpired_WhenExpiresInPast_ShouldReturnTrue()
    {
        // Arrange
        RefreshToken token = UserBuilder.CreateRefreshToken("user-id", expires: DateTime.UtcNow.AddDays(-1));

        // Act & Assert
        Assert.True(token.IsExpired);
    }

    [Fact]
    public void IsExpired_WhenExpiresNow_ShouldReturnTrue()
    {
        // Arrange
        RefreshToken token = UserBuilder.CreateRefreshToken("user-id", expires: DateTime.UtcNow);

        // Act & Assert
        Assert.True(token.IsExpired);
    }

    [Fact]
    public void IsActive_WhenNotRevokedAndNotExpired_ShouldReturnTrue()
    {
        // Arrange
        RefreshToken token = UserBuilder.CreateRefreshToken("user-id", expires: DateTime.UtcNow.AddDays(1), isRevoked: false);

        // Act & Assert
        Assert.True(token.IsActive);
    }

    [Fact]
    public void IsActive_WhenRevoked_ShouldReturnFalse()
    {
        // Arrange
        RefreshToken token = UserBuilder.CreateRefreshToken("user-id", expires: DateTime.UtcNow.AddDays(1), isRevoked: true);

        // Act & Assert
        Assert.False(token.IsActive);
    }

    [Fact]
    public void IsActive_WhenExpired_ShouldReturnFalse()
    {
        // Arrange
        RefreshToken token = UserBuilder.CreateRefreshToken("user-id", expires: DateTime.UtcNow.AddDays(-1), isRevoked: false);

        // Act & Assert
        Assert.False(token.IsActive);
    }

    [Fact]
    public void IsActive_WhenRevokedAndExpired_ShouldReturnFalse()
    {
        // Arrange
        RefreshToken token = UserBuilder.CreateRefreshToken("user-id", expires: DateTime.UtcNow.AddDays(-1), isRevoked: true);

        // Act & Assert
        Assert.False(token.IsActive);
    }

    [Fact]
    public void RefreshToken_ShouldHaveCorrectProperties()
    {
        // Arrange
        string userId = "test-user-id";
        string tokenString = "test-token";
        DateTime expires = DateTime.UtcNow.AddDays(30);
        string createdByIp = "192.168.1.1";

        // Act
        RefreshToken token = new RefreshToken
        {
            Token = tokenString,
            UserId = userId,
            Expires = expires,
            Created = DateTime.UtcNow,
            CreatedByIp = createdByIp
        };

        // Assert
        Assert.Equal(tokenString, token.Token);
        Assert.Equal(userId, token.UserId);
        Assert.Equal(expires, token.Expires);
        Assert.Equal(createdByIp, token.CreatedByIp);
        Assert.Null(token.Revoked);
        Assert.Null(token.RevokedByIp);
    }
}