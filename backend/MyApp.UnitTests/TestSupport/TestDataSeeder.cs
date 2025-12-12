using MyApp.Domain.Entities;
using MyApp.Infrastructure.Data;

namespace MyApp.UnitTests.TestSupport;

public static class TestDataSeeder
{
    public static void SeedUsers(AppDbContext db)
    {
        if (db.Users.Any()) return;

        db.Users.Add(new AppUser
        {
            UserName = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = "Admin",
            IsActive = true
        });

        db.Users.Add(new AppUser
        {
            UserName = "inactive",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Inactive123!"),
            Role = "User",
            IsActive = false
        });

        db.SaveChanges();
    }
}
