using Microsoft.AspNetCore.Identity;

namespace User.Api.Data.Models;

public class ApplicationUser : IdentityUser
{
    public bool EnableNotifications { get; set; }
}