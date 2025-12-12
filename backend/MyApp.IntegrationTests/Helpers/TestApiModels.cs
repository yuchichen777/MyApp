// MyApp.IntegrationTests/Helpers/AuthTestHelper.cs
using System.Net.Http.Json;

public static class TestApiModels
{
    public static async Task<string> LoginAndGetAccessTokenAsync(HttpClient client, string userName, string password)
    {
        var resp = await client.PostAsJsonAsync("/api/auth/login", new
        {
            userName,
            password
        });

        resp.EnsureSuccessStatusCode();

        // 你的回傳是 ApiResponse<LoginResultDto>，這邊用 dynamic/JsonDocument 都可
        var json = await resp.Content.ReadFromJsonAsync<ApiResponse<LoginResultDto>>();
        return json!.Data!.AccessToken!;
    }
}

// 下面兩個 model 你可以在 Tests 端自己建一份簡化版
public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public string? TraceId { get; set; }
    public T? Data { get; set; }
}
public class LoginResultDto
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? UserName { get; set; }
    public string? Role { get; set; }
}
