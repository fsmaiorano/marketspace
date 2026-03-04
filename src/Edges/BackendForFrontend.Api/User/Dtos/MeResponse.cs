namespace BackendForFrontend.Api.User.Dtos;

public class MeResponse
{
  public string? UserId { get; set; }
  public string? UserName { get; set; }
  public string? Email { get; set; }
  public string? FirstName { get; set; }
  public string? LastName { get; set; }
  public string? UserType { get; set; }
}