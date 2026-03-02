namespace User.Api.Models;

public class RegisterRequest
{
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string? UserName { get; init; }
    public string? Name { get; init; }
    public UserTypeEnum? UserType { get; init; }
}