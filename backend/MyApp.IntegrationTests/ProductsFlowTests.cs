// MyApp.IntegrationTests/ProductsFlowTests.cs
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

public class ProductsFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsFlowTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Products_CRUD_Flow_Should_Work()
    {
        // 1) login
        var token = await TestApiModels.LoginAndGetAccessTokenAsync(_client, "admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var code = "P" + Guid.NewGuid().ToString("N")[..8];

        // 2) create
        var createResp = await _client.PostAsJsonAsync("/api/products", new
        {
            code,
            name = "Product 001",
            price = 12.3456m
        });

        // 你目前 create 回 201 很正常，也兼容 200
        Assert.True(
            createResp.StatusCode == HttpStatusCode.Created ||
            createResp.StatusCode == HttpStatusCode.OK
        );

        var created = await createResp.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        Assert.NotNull(created);
        Assert.NotNull(created!.Data);
        Assert.True(created.Data.Id > 0);

        var id = created.Data.Id;

        // 3) read
        var getResp = await _client.GetAsync($"/api/products/{id}");
        getResp.EnsureSuccessStatusCode();

        var got = await getResp.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        Assert.Equal(code, got!.Data!.Code);

        // 4) update
        var updateResp = await _client.PutAsJsonAsync($"/api/products/{id}", new
        {
            code,
            name = "Product 001 - Updated",
            price = 99.0001m
        });
        updateResp.EnsureSuccessStatusCode();

        // 5) paging
        var listResp = await _client.GetAsync("/api/products?page=1&pageSize=10");
        listResp.EnsureSuccessStatusCode();

        // 6) delete (soft delete)
        var delResp = await _client.DeleteAsync($"/api/products/{id}");
        delResp.EnsureSuccessStatusCode();

        // 7) read again -> should be 404 (QueryFilter 過濾 IsDeleted=true)
        var getAfterDel = await _client.GetAsync($"/api/products/{id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDel.StatusCode);
    }

    [Fact]
    public async Task GetProducts_WithoutToken_Should_Return_401()
    {
        var resp = await _client.GetAsync("/api/products");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
}

public class ProductDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
}
