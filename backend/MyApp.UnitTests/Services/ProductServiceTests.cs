using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Application.Interfaces;
using MyApp.Domain.Entities;
using MyApp.Infrastructure.Data;
using MyApp.UnitTests.TestSupport;

namespace MyApp.UnitTests.Services
{
    

    public class ProductServiceTests
    {
        [Fact]
        public async Task DeleteAsync_Should_SoftDelete_Product()
        {
            using var factory = new TestServiceProviderFactory();
            using var scope = factory.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var p = new Product { Code = "P001", Name = "Prod", Price = 10m };
            db.Products.Add(p);
            db.SaveChanges();

            var svc = scope.ServiceProvider.GetRequiredService<IProductService>();

            // ⚠️ 依你的 method 名稱調整（例如 DeleteAsync / RemoveAsync）
            await svc.DeleteAsync(p.Id);

            // QueryFilter 會把 IsDeleted=true 的資料過濾掉 → 所以用 IgnoreQueryFilters 查真實狀態
            var raw = db.Products.IgnoreQueryFilters().Single(x => x.Id == p.Id);

            Assert.True(raw.IsDeleted);
            Assert.NotNull(raw.DeletedAt);
            Assert.Equal("utest", raw.DeletedBy);
        }
    }
}
