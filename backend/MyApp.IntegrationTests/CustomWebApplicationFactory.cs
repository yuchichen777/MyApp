// MyApp.IntegrationTests/CustomWebApplicationFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyApp.Domain;
using MyApp.Domain.Entities;
using MyApp.Infrastructure.Data;
using Testcontainers.MsSql;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _db = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Your_strong_Password123!")
        .Build();

    public string ConnectionString => _db.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _db.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _db.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 1) 移除原本 AppDbContext 設定
            var dbContextDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            // 2) 用 Testcontainer SQL Server 取代
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlServer(ConnectionString));

            // 3) 確保 ICurrentUser 有註冊（測試用）
            services.RemoveAll<ICurrentUser>();
            services.AddScoped<ICurrentUser>(_ => new TestCurrentUser());

            // 4) 建好 scope 跑 migration + seed
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();

            SeedUsers(db);
        });
    }

    private static void SeedUsers(AppDbContext db)
    {
        if (db.Users.Any()) return;

        var admin = new AppUser
        {
            UserName = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = "Admin",
            IsActive = true
        };

        var user = new AppUser
        {
            UserName = "user",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
            Role = "User",
            IsActive = true
        };

        db.Users.AddRange(admin, user);
        db.SaveChanges();
    }
}
