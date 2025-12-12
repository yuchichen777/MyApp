using MyApp.Application.DTOs;
using MyApp.Domain.Entities;

namespace MyApp.Application.Mapping
{
    public static class UserMappings
    {
        public static UserDto ToDto(this AppUser entity)
        {
            return new UserDto
            {
                Id = entity.Id,
                UserName = entity.UserName,
                Role = entity.Role,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedAt = entity.UpdatedAt,
                UpdatedBy = entity.UpdatedBy
            };
        }

        public static IEnumerable<UserDto> ToDtoList(this IEnumerable<AppUser> entities)
        {
            return entities.Select(e => e.ToDto());
        }

        public static AppUser ToEntity(this UserCreateDto dto)
        {
            return new AppUser
            {
                UserName = dto.UserName,
                // PasswordHash 交給 Service 去 Hash
                Role = dto.Role,
                IsActive = true
            };
        }

        public static void UpdateEntity(this UserUpdateDto dto, AppUser entity)
        {
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                // 密碼 Hash 同樣交給 Service 處理；這裡先不動 PasswordHash
            }

            entity.Role = dto.Role;
            entity.IsActive = dto.IsActive;
            // UpdatedAt/By 一樣由 DbContext Audit 自動填
        }
    }
}
