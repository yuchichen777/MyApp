using Microsoft.EntityFrameworkCore;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;
using MyApp.Domain.Entities;
using MyApp.Infrastructure.Data;

namespace MyApp.Application.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _db.Products.AsNoTracking().ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> UpdateAsync(int id, Product product)
    {
        var exist = await _db.Products.FindAsync(id);
        if (exist == null) return null;

        exist.Code = product.Code;
        exist.Name = product.Name;
        exist.Price = product.Price;

        await _db.SaveChangesAsync();
        return exist;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Products.FindAsync(id);
        if (entity == null) return false;

        _db.Products.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<Product>> GetPagedAsync(PagedQueryDto query)
    {
        var q = _db.Products.AsNoTracking();

        // 關鍵字：Code 或 Name 模糊
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            q = q.Where(p =>
                p.Code.Contains(kw) ||
                p.Name.Contains(kw));
        }

        // 排序
        q = query.SortBy?.ToLower() switch
        {
            "code" => query.Desc ? q.OrderByDescending(p => p.Code) : q.OrderBy(p => p.Code),
            "name" => query.Desc ? q.OrderByDescending(p => p.Name) : q.OrderBy(p => p.Name),
            "price" => query.Desc ? q.OrderByDescending(p => p.Price) : q.OrderBy(p => p.Price),
            _ => query.Desc ? q.OrderByDescending(p => p.Id) : q.OrderBy(p => p.Id),
        };

        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

        var total = await q.CountAsync();

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = total
        };
    }
}
