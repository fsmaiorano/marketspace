using Microsoft.AspNetCore.Identity;

namespace User.Data.Models;

public class ApplicationUser : IdentityUser
{
    public bool EnableNotifications { get; set; }
}