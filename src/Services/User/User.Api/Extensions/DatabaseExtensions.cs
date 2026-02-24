namespace User.Api.Extensions;

public static class DatabaseExtensions
{
    public static Task InitialiseDatabaseAsync(this WebApplication app)
        => Infrastructure.Data.Extensions.DatabaseExtensions.InitialiseDatabaseAsync(app);
}