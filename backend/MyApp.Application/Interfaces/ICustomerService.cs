using MyApp.Application.DTOs;
using MyApp.Domain.Entities;

namespace MyApp.Application.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer> CreateAsync(Customer customer);
    Task<Customer?> UpdateAsync(int id, Customer customer);
    Task<bool> DeleteAsync(int id);
    Task<PagedResult<Customer>> GetPagedAsync(PagedQueryDto query);
    Task<bool> IsCodeUniqueAsync(string code);
    Task<bool> IsCodeUniqueAsync(int id, string code);
    Task<List<Customer>> GetAllIncludingDeletedAsync();
    Task<List<Customer>> GetDeletedAsync();
    Task<bool> RestoreAsync(int id);
}