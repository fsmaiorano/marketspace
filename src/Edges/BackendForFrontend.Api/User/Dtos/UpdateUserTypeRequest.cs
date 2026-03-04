namespace BackendForFrontend.Api.User.Dtos;

public class UpdateUserTypeRequest
{
  public string UserId { get; set; } = null!;
  public int UserType { get; set; }
}