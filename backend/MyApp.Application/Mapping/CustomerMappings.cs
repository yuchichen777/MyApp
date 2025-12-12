using MyApp.Application.DTOs;
using MyApp.Domain.Entities;

namespace MyApp.Application.Mapping
{
    public static class CustomerMappings
    {
        public static CustomerDto ToDto(this Customer entity)
        {
            return new CustomerDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Contact = entity.Contact,
                Phone = entity.Phone,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedAt = entity.UpdatedAt,
                UpdatedBy = entity.UpdatedBy
            };
        }

        public static IEnumerable<CustomerDto> ToDtoList(this IEnumerable<Customer> entities)
        {
            return entities.Select(e => e.ToDto());
        }

        public static Customer ToEntity(this CustomerCreateDto dto)
        {
            return new Customer
            {
                Code = dto.Code ?? string.Empty,
                Name = dto.Name ?? string.Empty,
                Contact = dto.Contact,
                Phone = dto.Phone
                // CreatedAt/By 由 DbContext Audit 自動填
            };
        }

        public static void UpdateEntity(this CustomerUpdateDto dto, Customer entity)
        {
            entity.Code = dto.Code ?? string.Empty;
            entity.Name = dto.Name ?? string.Empty;
            entity.Contact = dto.Contact;
            entity.Phone = dto.Phone;
            // UpdatedAt/By 一樣由 DbContext Audit 自動填
        }
    }
}