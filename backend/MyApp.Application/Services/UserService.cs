using Microsoft.EntityFrameworkCore;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;
using MyApp.Application.Mapping;
using MyApp.Domain.Entities;
using MyApp.Infrastructure.Data;

namespace MyApp.Application.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<AppUser>> GetAllAsync()
    {
        return await _db.Users.AsNoTracking().ToListAsync();
    }

    public async Task<AppUser?> GetByIdAsync(int id)
    {
        return await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<AppUser> CreateAsync(UserCreateDto dto, string currentUser)
    {
        // 這裡簡單示範：你可以注入 IPasswordHasher<User> 來做 Hash
        var entity = dto.ToEntity();

        // TODO: 用真正的 PasswordHasher
        entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        entity.CreatedAt = DateTime.Now;
        entity.CreatedBy = currentUser;

        _db.Users.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<AppUser?> UpdateAsync(int id, UserUpdateDto dto, string currentUser)
    {
        var entity = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        entity.Role = dto.Role;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.Now;
        entity.UpdatedBy = currentUser;

        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Users.FindAsync(id);
        if (entity == null) return false;

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.Now;
        entity.DeletedBy ??= "system";

        _db.Users.Update(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<AppUser>> GetPagedAsync(PagedQueryDto query, string role)
    {
        var q = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(role))
        {
            q = q.Where(x => x.Role == role);
        }

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            q = q.Where(u => u.UserName.Contains(kw));
        }

        q = query.SortBy?.ToLower() switch
        {
            "username" => query.Desc ? q.OrderByDescending(u => u.UserName) : q.OrderBy(u => u.UserName),
            "role" => query.Desc ? q.OrderByDescending(u => u.Role) : q.OrderBy(u => u.Role),
            _ => query.Desc ? q.OrderByDescending(u => u.Id) : q.OrderBy(u => u.Id),
        };

        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

        var total = await q.CountAsync();
        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<AppUser>
        {
            Items = items,
            TotalCount = total
        };
    }

    public async Task<bool> IsUserNameUniqueAsync(string userName)
    {
        return !await _db.Users.AnyAsync(u => u.UserName == userName);
    }

    public async Task<bool> IsUserNameUniqueAsync(int id, string userName)
    {
        return !await _db.Users.AnyAsync(u => u.UserName == userName && u.Id != id);
    }

    public async Task<List<AppUser>> GetAllIncludingDeletedAsync()
    {
        return await _db.Users
            .IgnoreQueryFilters()
            .ToListAsync();
    }

    public async Task<List<AppUser>> GetDeletedAsync()
    {
        return await _db.Users
            .IgnoreQueryFilters()
            .Where(u => u.IsDeleted)
            .ToListAsync();
    }

    public async Task<bool> RestoreAsync(int id)
    {
        var entity = await _db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null || !entity.IsDeleted)
            return false;

        entity.IsDeleted = false;
        entity.DeletedAt = null;
        entity.DeletedBy = null;

        _db.Users.Update(entity);
        await _db.SaveChangesAsync();
        return true;
    }
}
