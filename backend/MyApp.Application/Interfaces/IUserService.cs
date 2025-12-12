using MyApp.Application.DTOs;
using MyApp.Domain.Entities;

namespace MyApp.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<AppUser>> GetAllAsync();
        Task<AppUser?> GetByIdAsync(int id);
        Task<AppUser> CreateAsync(UserCreateDto dto, string currentUser);
        Task<AppUser?> UpdateAsync(int id, UserUpdateDto dto, string currentUser);
        Task<bool> DeleteAsync(int id);

        Task<PagedResult<AppUser>> GetPagedAsync(PagedQueryDto query, string role);

        Task<bool> IsUserNameUniqueAsync(string userName);
        Task<bool> IsUserNameUniqueAsync(int id, string userName);

        Task<List<AppUser>> GetAllIncludingDeletedAsync();
        Task<List<AppUser>> GetDeletedAsync();
        Task<bool> RestoreAsync(int id);
    }
}
