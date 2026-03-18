namespace BuildingBlocks.Services.CurrentUser;

public interface ICurrentUserService
{
    /// <summary>
    /// The authenticated user's ID (ClaimTypes.NameIdentifier).
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// The authenticated user's username (ClaimTypes.Name).
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// The authenticated user's email (ClaimTypes.Email).
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Returns true when a user is authenticated in the current request.
    /// </summary>
    bool IsAuthenticated { get; }
}
