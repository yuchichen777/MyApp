namespace MyApp.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public decimal? Price { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class ProductCreateDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
}

public class ProductUpdateDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
}