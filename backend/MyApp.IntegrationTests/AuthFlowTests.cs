using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MyApp.IntegrationTests
{
    public class AuthFlowTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthFlowTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_Success_Should_Return_Tokens_And_Profile()
        {
            var resp = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                userName = "admin",
                password = "Admin123!"
            });

            resp.EnsureSuccessStatusCode();

            var body = await resp.Content.ReadFromJsonAsync<ApiResponse<LoginResultDto>>();
            Assert.NotNull(body);
            Assert.NotNull(body!.Data);

            Assert.False(string.IsNullOrWhiteSpace(body.Data!.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(body.Data.RefreshToken));
            Assert.Equal("admin", body.Data.UserName);
            Assert.Equal("Admin", body.Data.Role);

            Assert.False(string.IsNullOrWhiteSpace(body.TraceId));
            Assert.True(body.StatusCode == 200 || body.StatusCode == 201);
        }

        [Fact]
        public async Task Login_WrongPassword_Should_Return_401()
        {
            var resp = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                userName = "admin",
                password = "WrongPassword!"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact]
        public async Task Refresh_Success_Should_Return_New_AccessToken()
        {
            // 1) Login 取得 refresh token
            var login = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                userName = "admin",
                password = "Admin123!"
            });
            login.EnsureSuccessStatusCode();

            var loginBody = await login.Content.ReadFromJsonAsync<ApiResponse<LoginResultDto>>();
            Assert.NotNull(loginBody);
            Assert.NotNull(loginBody!.Data);

            var oldAccessToken = loginBody.Data!.AccessToken!;
            var refreshToken = loginBody.Data.RefreshToken!;

            // 2) Refresh
            var refreshResp = await _client.PostAsJsonAsync("/api/auth/refresh", new
            {
                refreshToken
            });

            refreshResp.EnsureSuccessStatusCode();

            var refreshBody = await refreshResp.Content.ReadFromJsonAsync<ApiResponse<LoginResultDto>>();
            Assert.NotNull(refreshBody);
            Assert.NotNull(refreshBody!.Data);

            var newAccessToken = refreshBody.Data!.AccessToken!;
            Assert.False(string.IsNullOrWhiteSpace(newAccessToken));

            // 很多系統 refresh 會換新 access token（通常不會相同）
            Assert.NotEqual(oldAccessToken, newAccessToken);
        }

        [Fact]
        public async Task Refresh_InvalidToken_Should_Return_401()
        {
            var refreshResp = await _client.PostAsJsonAsync("/api/auth/refresh", new
            {
                refreshToken = "this-is-not-a-valid-refresh-token"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, refreshResp.StatusCode);
        }

        [Fact]
        public async Task AccessToken_Should_Be_Usable_On_Protected_Endpoints()
        {
            // login
            var login = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                userName = "admin",
                password = "Admin123!"
            });
            login.EnsureSuccessStatusCode();

            var loginBody = await login.Content.ReadFromJsonAsync<ApiResponse<LoginResultDto>>();
            Assert.NotNull(loginBody);
            Assert.NotNull(loginBody!.Data);

            var token = loginBody.Data!.AccessToken!;

            // call protected endpoint
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await _client.GetAsync("/api/customers?page=1&pageSize=10");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }
    }
}
