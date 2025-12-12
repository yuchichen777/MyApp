namespace MyApp.Application.DTOs;

public class RegisterUserDto
{
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;
}

public class LoginRequestDto
{
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class LoginResultDto
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Role { get; set; } = default!;
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = default!;
}
