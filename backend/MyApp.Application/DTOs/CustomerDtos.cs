namespace MyApp.Application.DTOs;

public class CustomerDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Contact { get; set; }
    public string? Phone { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class CustomerCreateDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Contact { get; set; }
    public string? Phone { get; set; }
}

public class CustomerUpdateDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Contact { get; set; }
    public string? Phone { get; set; }
}