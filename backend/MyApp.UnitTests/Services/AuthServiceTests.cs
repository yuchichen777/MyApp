using Microsoft.Extensions.DependencyInjection;
using MyApp.Application.Interfaces;
using MyApp.Infrastructure.Data;
using MyApp.UnitTests.TestSupport;

namespace MyApp.UnitTests.Services
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task ValidateUserAsync_CorrectPassword_Should_Return_Success_And_User()
        {
            using var factory = new TestServiceProviderFactory();
            using var scope = factory.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            TestDataSeeder.SeedUsers(db);

            var svc = scope.ServiceProvider.GetRequiredService<IAuthService>();

            var result = await svc.ValidateUserAsync("admin", "Admin123!");

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.User);
            Assert.Equal("admin", result.User!.UserName);
            Assert.True(string.IsNullOrWhiteSpace(result.ErrorMessage));
        }

        [Fact]
        public async Task ValidateUserAsync_WrongPassword_Should_Return_Fail_And_ErrorMessage()
        {
            using var factory = new TestServiceProviderFactory();
            using var scope = factory.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            TestDataSeeder.SeedUsers(db);

            var svc = scope.ServiceProvider.GetRequiredService<IAuthService>();

            var result = await svc.ValidateUserAsync("admin", "Wrong!");

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.User);
            Assert.Equal("密碼不正確", result.ErrorMessage);
        }

        [Fact]
        public async Task ValidateUserAsync_InactiveUser_Should_Return_Fail_And_ErrorMessage()
        {
            using var factory = new TestServiceProviderFactory();
            using var scope = factory.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            TestDataSeeder.SeedUsers(db);

            var svc = scope.ServiceProvider.GetRequiredService<IAuthService>();

            var result = await svc.ValidateUserAsync("inactive", "Inactive123!");

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.User);
            Assert.Equal("帳號已被停用", result.ErrorMessage);
        }
    }
}
