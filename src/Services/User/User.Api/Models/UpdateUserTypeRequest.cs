namespace User.Api.Models;

public class UpdateUserTypeRequest
{
    public string UserId { get; init; } = null!;
    public UserTypeEnum UserType { get; init; }
}

