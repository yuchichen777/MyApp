using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyApp.Application.Auth;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;
using MyApp.Domain.Entities;
using MyApp.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MyApp.Application.Services;

public class TokenService : ITokenService
{
    private readonly AppDbContext _db;
    private readonly JwtSettings _jwt;

    public TokenService(AppDbContext db, IOptions<JwtSettings> jwtOptions)
    {
        _db = db;
        _jwt = jwtOptions.Value;
    }

    public TokenPairDto GenerateTokens(AppUser user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        var refreshEntity = new UserRefreshToken
        {
            UserId = user.Id,
            TokenHash = Hash(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays),
            IsRevoked = false
        };

        _db.UserRefreshTokens.Add(refreshEntity);
        _db.SaveChanges();

        return new TokenPairDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<TokenPairDto?> RefreshTokensAsync(string refreshToken)
    {
        var tokenHash = Hash(refreshToken);

        var existing = await _db.UserRefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

        if (existing == null) return null;
        if (existing.IsRevoked) return null;
        if (existing.ExpiresAt < DateTime.UtcNow) return null;

        var user = existing.User;
        if (!user.IsActive) return null;

        // Token Rotation：舊的標記 revoked，再發一組新的 RefreshToken
        existing.IsRevoked = true;

        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        var newRefreshEntity = new UserRefreshToken
        {
            UserId = user.Id,
            TokenHash = Hash(newRefreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays),
            IsRevoked = false,
            ReplacedByTokenId = existing.Id
        };

        _db.UserRefreshTokens.Add(newRefreshEntity);
        await _db.SaveChangesAsync();

        return new TokenPairDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }

    public ParsedUserInfoDto ParseUserFromAccessToken(string accessToken)
    {
        var handler = new JwtSecurityTokenHandler();

        JwtSecurityToken? token;
        try
        {
            token = handler.ReadJwtToken(accessToken);
        }
        catch
        {
            return new ParsedUserInfoDto();
        }

        var userName =
            token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ??
            token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value ??
            string.Empty;

        // 你前端 decodeRoleFromToken 用 payload["role"]，所以這裡一定要有 "role"
        var role =
            token.Claims.FirstOrDefault(c => c.Type == "role")?.Value ??
            token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ??
            string.Empty;

        return new ParsedUserInfoDto
        {
            UserName = userName,
            Role = role
        };
    }

    // ----------------- 私有 Helper -----------------

    private string GenerateAccessToken(AppUser user)
    {
        if (string.IsNullOrWhiteSpace(_jwt.Key))
        {
            throw new InvalidOperationException("JwtSettings.Key 未設定，請在 appsettings.json 的 JwtSettings:Key 設定金鑰。");
        }

        var keyBytes = Encoding.UTF8.GetBytes(_jwt.Key);
        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException("JwtSettings.Key 長度太短，請設定至少 32 字元的金鑰（>= 32 chars）。");
        }

        var key = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new("role", user.Role), 
            new(JwtRegisteredClaimNames.Sub, user.UserName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string Hash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
