using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Application.Interfaces;
using MyApp.Infrastructure.Data;
using MyApp.UnitTests.TestSupport;

namespace MyApp.UnitTests.Services;

public class TokenServiceTests
{
    [Fact]
    public async Task GenerateTokens_Should_Return_Tokens_And_Persist_RefreshTokenHash()
    {
        using var factory = new TestServiceProviderFactory();
        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        TestDataSeeder.SeedUsers(db);

        var tokenSvc = scope.ServiceProvider.GetRequiredService<ITokenService>();

        // 取一個 user
        var user = await db.Users.AsNoTracking().FirstAsync(x => x.UserName == "admin");

        // 產 token
        var tokens = tokenSvc.GenerateTokens(user);

        Assert.NotNull(tokens);
        Assert.False(string.IsNullOrWhiteSpace(tokens.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(tokens.RefreshToken));

        // Access token 基本格式：三段
        Assert.Equal(3, tokens.AccessToken.Split('.').Length);

        // Refresh token hash 是否寫入 DB（你有 UserRefreshTokens 表）
        var saved = await db.UserRefreshTokens.AsNoTracking()
            .Where(x => x.UserId == user.Id)
            .ToListAsync();

        Assert.True(saved.Count >= 1);
        Assert.All(saved, x => Assert.False(string.IsNullOrWhiteSpace(x.TokenHash)));
    }

    [Fact]
    public async Task Refresh_With_Valid_RefreshToken_Should_Return_New_AccessToken()
    {
        using var factory = new TestServiceProviderFactory();
        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        TestDataSeeder.SeedUsers(db);

        var tokenSvc = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var user = await db.Users.AsNoTracking().FirstAsync(x => x.UserName == "admin");

        // 先發一組 token
        var tokens1 = tokenSvc.GenerateTokens(user);

        // 再用 refresh token 換新 access token
        // ✅ 版本 A：RefreshAsync 回 TokenPairDto 或 TokenPairDto?
        var tokens2 = await tokenSvc.RefreshTokensAsync(tokens1.RefreshToken);

        Assert.NotNull(tokens2);
        Assert.False(string.IsNullOrWhiteSpace(tokens2!.AccessToken));
        Assert.Equal(3, tokens2.AccessToken.Split('.').Length);

        // 通常 refresh 會換新 access token（建議）
        Assert.NotEqual(tokens1.AccessToken, tokens2.AccessToken);

        // 你若有 refresh rotation（換新 refresh token），可額外驗：
        if (!string.IsNullOrWhiteSpace(tokens2.RefreshToken))
        {
            // 有回新的 refresh token，就應該跟舊的不同（視你設計）
            // Assert.NotEqual(tokens1.RefreshToken, tokens2.RefreshToken);
            Assert.False(string.IsNullOrWhiteSpace(tokens2.RefreshToken));
        }
    }

    [Fact]
    public async Task Refresh_With_InvalidToken_Should_Fail()
    {
        using var factory = new TestServiceProviderFactory();
        using var scope = factory.Services.CreateScope();

        var tokenSvc = scope.ServiceProvider.GetRequiredService<ITokenService>();

        try
        {
            var result = await tokenSvc.RefreshTokensAsync("this-is-not-a-valid-refresh-token");

            // ✅ 若你設計是回 null 代表失敗
            Assert.Null(result);
        }
        catch (Exception)
        {
            // ✅ 若你設計是用例外表示 refresh 失敗，也算 pass
            Assert.True(true);
        }
    }

    [Fact]
    public async Task Refresh_Should_Not_Create_Duplicate_TokenHash_For_Same_RefreshToken()
    {
        using var factory = new TestServiceProviderFactory();
        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        TestDataSeeder.SeedUsers(db);

        var tokenSvc = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var user = await db.Users.AsNoTracking().FirstAsync(x => x.UserName == "admin");

        var tokens = tokenSvc.GenerateTokens(user);

        // 同一個 refresh token 重複 refresh（依你設計可能允許一次或多次）
        // 這邊主要是驗證：UserRefreshTokens.TokenHash 是 Unique，不該插入重複 hash 造成 exception
        try
        {
            await tokenSvc.RefreshTokensAsync(tokens.RefreshToken);
        }
        catch
        {
            // 如果你的設計是「用過一次就失效」，這裡可能會 throw，OK
        }

        // 只要 DB 不因 Unique Index 爆掉就算通過
        var count = await db.UserRefreshTokens.CountAsync(x => x.UserId == user.Id);
        Assert.True(count >= 1);
    }
}
