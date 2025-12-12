using MyApp.Domain.Entities;

namespace MyApp.Application.DTOs
{
    public class UserDto : AuditableEntity
    {
        public int Id { get; set; }
        public string UserName { get; set; } = default!;
        public string Role { get; set; } = default!;
        public bool IsActive { get; set; }
    }

    public class UserCreateDto
    {
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;   // 之後在 Service 裡雜湊
        public string Role { get; set; } = "User";
    }

    public class UserUpdateDto
    {
        public int Id { get; set; }
        public string? Password { get; set; }   // 選填，若有填表示要改密碼
        public string Role { get; set; } = "User";
        public bool IsActive { get; set; } = true;
    }
}
