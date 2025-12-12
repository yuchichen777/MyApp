using MyApp.Application.DTOs;
using MyApp.Application.DTOs.AuthResult;
using MyApp.Domain.Entities;

namespace MyApp.Application.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// 驗證帳號密碼，成功回傳使用者，失敗回傳 null
    /// </summary>
    Task<ValidateUserResult> ValidateUserAsync(string userName, string password);
    Task<AppUser?> RegisterUserAsync(RegisterUserDto dto);
}
