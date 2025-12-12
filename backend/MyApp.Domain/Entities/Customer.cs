namespace MyApp.Domain.Entities;

public class Customer : AuditableEntity
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Contact { get; set; }

    public string? Phone { get; set; }
}