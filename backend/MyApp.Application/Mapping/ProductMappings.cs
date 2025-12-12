using MyApp.Application.DTOs;
using MyApp.Domain.Entities;

namespace MyApp.Application.Mapping;

public static class ProductMappings
{
    public static ProductDto ToDto(this Product entity)
    {
        return new ProductDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Price = entity.Price,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static IEnumerable<ProductDto> ToDtoList(this IEnumerable<Product> entities)
    {
        return entities.Select(e => e.ToDto());
    }

    public static Product ToEntity(this ProductCreateDto dto)
    {
        return new Product
        {
            Code = dto.Code ?? string.Empty,
            Name = dto.Name ?? string.Empty,
            Price = dto.Price
            // CreatedAt/By 由 DbContext Audit 自動填
        };
    }

    public static void UpdateEntity(this ProductUpdateDto dto, Product entity)
    {
        entity.Code = dto.Code ?? string.Empty;
        entity.Name = dto.Name ?? string.Empty;
        entity.Price = dto.Price;
        // UpdatedAt/By 一樣由 DbContext Audit 自動填
    }
}
