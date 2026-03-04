namespace BackendForFrontend.Api.User.Dtos;

public class RegisterRequest
{
  public string Email { get; set; } = null!;
  public string Password { get; set; } = null!;
  public string? UserName { get; set; }
  public string? Name { get; set; }
  public int UserType { get; set; }
}