using Microsoft.EntityFrameworkCore;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;
using MyApp.Domain.Entities;
using MyApp.Infrastructure.Data;

namespace MyApp.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _db;

    public CustomerService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await _db.Customers.AsNoTracking().ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _db.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer?> UpdateAsync(int id, Customer customer)
    {
        var exist = await _db.Customers.FindAsync(id);
        if (exist == null) return null;

        exist.Code = customer.Code;
        exist.Name = customer.Name;
        exist.Contact = customer.Contact;
        exist.Phone = customer.Phone;

        await _db.SaveChangesAsync();
        return exist;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Customers.FindAsync(id);
        if (entity == null) return false;

        _db.Customers.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<Customer>> GetPagedAsync(PagedQueryDto query)
    {
        var q = _db.Customers.AsNoTracking();

        // 關鍵字：Code 或 Name 模糊
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            q = q.Where(p =>
                p.Code.Contains(kw) ||
                p.Name.Contains(kw) ||
                (p.Phone != null && p.Phone.Contains(kw)));
        }

        // 排序
        q = query.SortBy?.ToLower() switch
        {
            "code" => query.Desc ? q.OrderByDescending(p => p.Code) : q.OrderBy(p => p.Code),
            "name" => query.Desc ? q.OrderByDescending(p => p.Name) : q.OrderBy(p => p.Name),
            "contact" => query.Desc ? q.OrderByDescending(p => p.Contact) : q.OrderBy(p => p.Contact),
            "phone" => query.Desc ? q.OrderByDescending(p => p.Phone) : q.OrderBy(p => p.Phone),
            _ => query.Desc ? q.OrderByDescending(p => p.Id) : q.OrderBy(p => p.Id),
        };

        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

        var total = await q.CountAsync();

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Customer>
        {
            Items = items,
            TotalCount = total
        };
    }

    public async Task<bool> IsCodeUniqueAsync(string code)
    {
        return !await _db.Customers.AnyAsync(c => c.Code == code);
    }

    public async Task<bool> IsCodeUniqueAsync(int id, string code)
    {
        return !await _db.Customers.AnyAsync(c => c.Code == code && c.Id != id);
    }

    public async Task<List<Customer>> GetAllIncludingDeletedAsync()
    {
        return await _db.Customers
            .IgnoreQueryFilters()
            .ToListAsync();
    }

    public async Task<List<Customer>> GetDeletedAsync()
    {
        return await _db.Customers
            .IgnoreQueryFilters()
            .Where(c => c.IsDeleted)
            .ToListAsync();
    }

    public async Task<bool> RestoreAsync(int id)
    {
        var entity = await _db.Customers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null || !entity.IsDeleted)
            return false;

        entity.IsDeleted = false;
        entity.DeletedAt = null;
        entity.DeletedBy = null;

        _db.Customers.Update(entity);
        await _db.SaveChangesAsync();
        return true;
    }
}