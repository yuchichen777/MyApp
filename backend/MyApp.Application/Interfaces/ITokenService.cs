using MyApp.Application.DTOs;
using MyApp.Domain.Entities;

namespace MyApp.Application.Interfaces
{
    public interface ITokenService
    {
        TokenPairDto GenerateTokens(AppUser user);
        Task<TokenPairDto?> RefreshTokensAsync(string refreshToken);
        /// <summary>
        /// 從 AccessToken 解析使用者資訊（UserName / Role）
        /// </summary>
        ParsedUserInfoDto ParseUserFromAccessToken(string accessToken);
    }
}
