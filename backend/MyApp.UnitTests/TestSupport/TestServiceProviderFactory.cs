using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyApp.Application.Auth;
using MyApp.Application.Interfaces;
using MyApp.Application.Services;
using MyApp.Domain;
using MyApp.Infrastructure.Data;

namespace MyApp.UnitTests.TestSupport;

public sealed class TestServiceProviderFactory : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly ServiceProvider _sp;

    public IServiceProvider Services => _sp;

    public TestServiceProviderFactory(Action<IServiceCollection>? overrideServices = null)
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();

        var services = new ServiceCollection();

        services.AddLogging();

        services.Configure<JwtSettings>(options =>
        {
            options.Issuer = "TestIssuer";
            options.Audience = "TestAudience";
            options.Key = "THIS_IS_A_TEST_KEY_123456789012345"; // >= 32 chars
            options.AccessTokenExpiryMinutes = 60;
            options.RefreshTokenExpiryDays = 7;
        });

        // 1) DbContext: SQLite in-memory
        services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(_conn));

        // 2) 測試用 CurrentUser（讓 Audit/SoftDelete 有 userName）
        services.RemoveAll(typeof(ICurrentUser));
        services.AddScoped<ICurrentUser>(_ => new TestCurrentUser { UserName = "utest", Role = "Admin" });

        // 3) 你專案的 Services 註冊：
        //    ⚠️ 這裡要你依實際情況選一種（下面會說怎麼改）
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();

        // 4) 允許你在單一測試覆寫某些依賴（例如 JwtSettings）
        overrideServices?.Invoke(services);

        _sp = services.BuildServiceProvider();

        // 5) 建 DB schema
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _sp.Dispose();
        _conn.Close();
        _conn.Dispose();
    }
}

public sealed class TestCurrentUser : ICurrentUser
{
    public int? UserId { get; set; } = 1;
    public string? UserName { get; set; } = "utest";
    public string? Role { get; set; } = "Admin";
}
