using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using User.Api.Data;
using User.Api.Data.Models;
using User.Api.Infrastructure.Data.Extensions;

namespace User.Api.Extensions;

public static class DatabaseExtensions
{
    public static Task InitialiseDatabaseAsync(this WebApplication app)
        => User.Api.Infrastructure.Data.Extensions.DatabaseExtensions.InitialiseDatabaseAsync(app);
}


