using Microsoft.AspNetCore.Identity;

namespace User.Api.Data.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool EnableNotifications { get; set; }
}