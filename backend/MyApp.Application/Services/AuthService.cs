
using Microsoft.EntityFrameworkCore;
using MyApp.Application.DTOs;
using MyApp.Application.DTOs.AuthResult;
using MyApp.Application.Interfaces;
using MyApp.Domain.Entities;
using MyApp.Infrastructure.Data;

namespace MyApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;

    public AuthService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 驗證帳號密碼，成功回傳使用者資訊與 Success=true，失敗回傳錯誤訊息
    /// </summary>
    public async Task<ValidateUserResult> ValidateUserAsync(string userName, string password)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userName);

        if (user == null)
            return ValidateUserResult.Fail("使用者不存在");

        var passwordHash = HashPassword(password);

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return ValidateUserResult.Fail($"密碼不正確");

        if (!user.IsActive)
            return ValidateUserResult.Fail("帳號已被停用");

        return ValidateUserResult.Ok(user);
    }


    public async Task<AppUser?> RegisterUserAsync(RegisterUserDto dto)
    {
        // 安全起見再檢查一次（Validator 已經檢查過一次唯一性）
        var exists = await _db.Users.AnyAsync(u => u.UserName == dto.UserName);
        if (exists)
            return null;

        var user = new AppUser
        {
            UserName = dto.UserName,
            PasswordHash = HashPassword(dto.Password),
            Role = "User",
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return user;
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
