namespace User.Api.Models;

public class AuthRequest
{
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
}