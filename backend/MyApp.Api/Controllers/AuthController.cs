using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Api.Models;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;

namespace MyApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly IValidator<RegisterUserDto> _registerValidator;

    public AuthController(
        IAuthService authService,
        ITokenService tokenService,
        IValidator<RegisterUserDto> registerValidator)
    {
        _authService = authService;
        _tokenService = tokenService;
        _registerValidator = registerValidator;
    }

    // ---------------- Login ----------------

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResultDto>>> Login(LoginRequestDto request)
    {
        var result = await _authService.ValidateUserAsync(request.UserName, request.Password);

        if (!result.Success || result.User is null)
        {
            var errorResponse = new ApiResponse<LoginResultDto>
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = $"登入失敗：{result.ErrorMessage ?? "帳號或密碼錯誤"}",
                TraceId = HttpContext.TraceIdentifier,
                Data = null
            };

            // 這裡用 Unauthorized(...) 回傳 401 + 統一格式的 ApiResponse
            return Unauthorized(errorResponse);
        }

        var user = result.User;
        var tokens = _tokenService.GenerateTokens(user);

        var response = new ApiResponse<LoginResultDto>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "登入成功",
            TraceId = HttpContext.TraceIdentifier,
            Data = new LoginResultDto
            {
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                UserName = user.UserName,
                Role = user.Role
            }
        };

        return Ok(response);
    }


    // ---------------- Register（順便自動登入） ----------------

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResultDto>>> Register(RegisterUserDto dto)
    {
        ValidationResult result = await _registerValidator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return BadRequest(new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "驗證失敗",
                TraceId = HttpContext.TraceIdentifier,
                Errors = errors
            });
        }

        var newUser = await _authService.RegisterUserAsync(dto);
        if (newUser == null)
        {
            return BadRequest(new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "帳號已存在",
                TraceId = HttpContext.TraceIdentifier,
                Errors = new Dictionary<string, string[]>
                {
                    ["UserName"] = ["帳號已存在"]
                }
            });
        }

        var tokens = _tokenService.GenerateTokens(newUser);

        var response = new ApiResponse<LoginResultDto>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "註冊成功",
            TraceId = HttpContext.TraceIdentifier,
            Data = new LoginResultDto
            {
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                UserName = newUser.UserName,
                Role = newUser.Role
            }
        };

        return Ok(response);
    }

    // ---------------- Refresh ----------------

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResultDto>>> Refresh(RefreshTokenRequestDto dto)
    {
        var tokens = await _tokenService.RefreshTokensAsync(dto.RefreshToken);
        if (tokens == null)
        {
            return Unauthorized(new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Refresh Token 無效或已過期",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        var userInfo = _tokenService.ParseUserFromAccessToken(tokens.AccessToken);

        var response = new ApiResponse<LoginResultDto>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Token 已續期",
            TraceId = HttpContext.TraceIdentifier,
            Data = new LoginResultDto
            {
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                UserName = userInfo.UserName,
                Role = userInfo.Role
            }
        };

        return Ok(response);
    }
}
