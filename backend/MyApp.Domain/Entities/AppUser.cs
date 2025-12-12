namespace MyApp.Domain.Entities;

public class AppUser : AuditableEntity
{
    public int Id { get; set; }
    public string UserName { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; }
}
