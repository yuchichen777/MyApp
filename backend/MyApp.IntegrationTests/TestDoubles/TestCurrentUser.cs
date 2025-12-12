// MyApp.IntegrationTests/TestDoubles/TestCurrentUser.cs
using MyApp.Domain;

public class TestCurrentUser : ICurrentUser
{
    public string? UserName { get; set; } = "itest";
    public string? Role { get; set; } = "Admin";
    public int? UserId { get; set; } = 1;
}
