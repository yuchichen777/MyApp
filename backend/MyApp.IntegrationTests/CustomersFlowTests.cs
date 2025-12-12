// MyApp.IntegrationTests/CustomersFlowTests.cs
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

public class CustomersFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CustomersFlowTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Customers_CRUD_Flow_Should_Work()
    {
        // 1) login
        var token = await TestApiModels.LoginAndGetAccessTokenAsync(_client, "admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var code = "C" + Guid.NewGuid().ToString("N")[..8];

        // 2) create
        var createResp = await _client.PostAsJsonAsync("/api/customers", new
        {
            code,
            name = "Customer 001",
            contact = "Bill",
            phone = "0912-000-000"
        });
        Assert.True(
            createResp.StatusCode == HttpStatusCode.OK ||
            createResp.StatusCode == HttpStatusCode.Created
        );

        var created = await createResp.Content.ReadFromJsonAsync<ApiResponse<CustomerDto>>();
        Assert.True(created!.Data!.Id > 0);

        var id = created.Data.Id;

        // 3) read
        var getResp = await _client.GetAsync($"/api/customers/{id}");
        getResp.EnsureSuccessStatusCode();

        var got = await getResp.Content.ReadFromJsonAsync<ApiResponse<CustomerDto>>();
        Assert.Equal(code, got!.Data!.Code);

        // 4) update
        var updateResp = await _client.PutAsJsonAsync($"/api/customers/{id}", new
        {
            code,
            name = "Customer 001 - Updated",
            contact = "Bill2",
            phone = "0912-111-111"
        });
        updateResp.EnsureSuccessStatusCode();

        // 5) paging
        var listResp = await _client.GetAsync("/api/customers?page=1&pageSize=10");
        listResp.EnsureSuccessStatusCode();

        // 6) delete (soft delete)
        var delResp = await _client.DeleteAsync($"/api/customers/{id}");
        delResp.EnsureSuccessStatusCode();

        // 7) read again -> should be 404 (QueryFilter filter IsDeleted=true)
        var getAfterDel = await _client.GetAsync($"/api/customers/{id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDel.StatusCode);
    }

    [Fact]
    public async Task GetCustomers_WithoutToken_Should_Return_401()
    {
        var resp = await _client.GetAsync("/api/customers");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
}

public class CustomerDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Contact { get; set; }
    public string? Phone { get; set; }
}
