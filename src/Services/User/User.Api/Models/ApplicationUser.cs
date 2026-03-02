namespace User.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string? Name { get; set; }
    public UserTypeEnum UserType { get; set; }
    public bool EnableNotifications { get; set; }
}