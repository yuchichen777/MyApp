using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;
using MyApp.Domain.Entities;
using MyApp.Infrastructure.Data;
using MyApp.UnitTests.TestSupport;

namespace MyApp.UnitTests.Services;

public class CustomerServiceTests
{
    [Fact]
    public async Task CreateAsync_Should_Set_AuditFields_And_CanBeRead()
    {
        using var factory = new TestServiceProviderFactory();
        using var scope = factory.Services.CreateScope();

        var svc = scope.ServiceProvider.GetRequiredService<ICustomerService>();

        var c = new Customer
        {
            Code = "C001",
            Name = "Customer 1",
            Contact = "Bill",
            Phone = "0912"
        };

        var created = await svc.CreateAsync(c);

        Assert.True(created.Id > 0);

        // QueryFilter 不會影響新增後讀取
        var got = await svc.GetByIdAsync(created.Id);
        Assert.NotNull(got);
        Assert.Equal("C001", got!.Code);

        // Audit：AppDbContext 會從 ICurrentUser 寫入 CreatedBy
        Assert.False(string.IsNullOrWhiteSpace(got.CreatedBy));
        Assert.Equal("utest", got.CreatedBy);
        Assert.NotEqual(default, got.CreatedAt);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_Should_Return_Null()
    {
        using var factory = new TestServiceProviderFactory();
        using var scope = factory.Services.CreateScope();

        var svc = scope.ServiceProvider.GetRequiredService<ICustomerService>();

        var result = await svc.UpdateAsync(9999, new Customer
        {
            Code = "C999",
            Name = "NotExists"
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_And_Set_UpdatedBy()
    {
        using var factory = new TestServiceProviderFactory();
        using var scope = factory.Services.CreateScope();

        var svc = scope.ServiceProvider.GetRequiredService<ICustomerService>();

        var created = await svc.CreateAsync(new Customer
        {
            Code = "C002",
            Name = "Customer 2",
            Contact = "A",
            Phone = "100"
        });

        var updated = await svc.UpdateAsync(created.Id, new Customer
        {
            Code = "C002",
            Name = "Customer 2 - Updated",
            Contact = "B",
            Phone = "200"
        });

        Assert.NotNull(updated);
        Assert.Equal("Customer 2 - Updated", updated!.Name);

        // 重新查一次（AsNoTracking）
        var got = await svc.GetByIdAsync(created.Id);
        Assert.NotNull(got);
        Assert.Equal("Customer 2 - Updated", got!.Name);

        Assert.NotNull(got.UpdatedAt);
        Assert.Equal("utest", got.UpdatedBy);
    }

    [Fact]
    public async Task DeleteAsync_Should_SoftDelete_And_FilteredOut_By_QueryFilter()
    {
        using var factory = new TestServiceProviderFactory();
        using var scope = factory.Services.CreateScope();

        var svc = scope.ServiceProvider.GetRequiredService<ICustomerService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var created = await svc.CreateAsync(new Customer
        {
            Code = "C003",
            Name = "Customer 3"
        });

        var ok = await svc.DeleteAsync(created.Id);
        Assert.True(ok);

        // 一般查詢（受 QueryFilter 影響）應該找不到
        var filtered = await svc.GetByIdAsync(created.Id);
        Assert.Null(filtered);

        // IgnoreQueryFilters 才能看到實際資料
        var raw = await db.Customers.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == created.Id);
        Assert.NotNull(raw);
        Assert.True(raw!.IsDeleted);

        // 你在 CustomerService.DeleteAsync 目前是寫死 "system"
        // （這點其實跟 AppDbContext 的 ApplyAuditInformation 重複，建議之後統一）
        Assert.True(raw.DeletedAt != null);
        Assert.False(string.IsNullOrWhiteSpace(raw.DeletedBy));
    }

    [Fact]
    public async Task RestoreAsync_Should_UnDelete_And_BeVisibleAgain()
    {
        using var factory = new TestServiceProviderFactory();
        using var scope = factory.Services.CreateScope();

        var svc = scope.ServiceProvider.GetRequiredService<ICustomerService>();

        var created = await svc.CreateAsync(new Customer
        {
            Code = "C004",
            Name = "Customer 4"
        });

        var delOk = await svc.DeleteAsync(created.Id);
        Assert.True(delOk);

        var restoreOk = await svc.RestoreAsync(created.Id);
        Assert.True(restoreOk);

        var got = await svc.GetByIdAsync(created.Id);
        Assert.NotNull(got);
        Assert.Equal("C004", got!.Code);
        Assert.False(got.IsDeleted);
        Assert.Null(got.DeletedAt);
        Assert.Null(got.DeletedBy);
    }

    [Fact]
    public async Task IsCodeUniqueAsync_Should_Work()
    {
        using var factory = new TestServiceProviderFactory();
        using var scope = factory.Services.CreateScope();

        var svc = scope.ServiceProvider.GetRequiredService<ICustomerService>();

        await svc.CreateAsync(new Customer { Code = "C005", Name = "Customer 5" });

        var unique1 = await svc.IsCodeUniqueAsync("C005");
        Assert.False(unique1);

        var unique2 = await svc.IsCodeUniqueAsync("C006");
        Assert.True(unique2);
    }

    [Fact]
    public async Task GetPagedAsync_Should_Filter_Sort_And_Page()
    {
        using var factory = new TestServiceProviderFactory();
        using var scope = factory.Services.CreateScope();

        var svc = scope.ServiceProvider.GetRequiredService<ICustomerService>();

        // 建 25 筆
        for (int i = 1; i <= 25; i++)
        {
            await svc.CreateAsync(new Customer
            {
                Code = $"C{i:000}",
                Name = $"Customer {i:000}",
                Phone = i % 2 == 0 ? "0912" : "0988"
            });
        }

        var result = await svc.GetPagedAsync(new PagedQueryDto
        {
            Page = 2,
            PageSize = 10,
            SortBy = "code",
            Desc = false
        });

        Assert.Equal(25, result.TotalCount);
        Assert.Equal(10, result.Items.Count());

        // Page2, sort code asc：應該是 C011 ~ C020
        Assert.Equal("C011", result.Items.Where(x => x.Code == "C011").First().Code);
        Assert.Equal("C020", result.Items.Where(x => x.Code == "C020").First().Code);

        // keyword filter（phone contains 0912）
        var filtered = await svc.GetPagedAsync(new PagedQueryDto
        {
            Page = 1,
            PageSize = 50,
            Keyword = "0912"
        });
        Assert.True(filtered.TotalCount > 0);
        Assert.All(filtered.Items, x => Assert.Contains("0912", x.Phone));
    }
}
