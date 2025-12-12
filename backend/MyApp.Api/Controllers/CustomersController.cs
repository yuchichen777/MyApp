using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Api.Models;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;
using MyApp.Application.Mapping;
using Swashbuckle.AspNetCore.Annotations;

namespace MyApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _service;

    public CustomersController(ICustomerService service)
    {
        _service = service;
    }

    /// <summary>
    /// 取得全部客戶列表
    /// </summary>
    [HttpGet]
    [SwaggerOperation(OperationId = "getApiCustomers")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
    {
        var list = await _service.GetAllAsync();
        var dto = list.ToDtoList();

        return Ok(dto);
    }

    /// <summary>
    /// 取得單一客戶
    /// </summary>
    [HttpGet("{id:int}")]
    [SwaggerOperation(OperationId = "getApiCustomer")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity == null)
        {
            return NotFound(new ApiErrorResponse
            {
                Success = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = "找不到客戶",
                TraceId = HttpContext.TraceIdentifier,
            });
        }

        return Ok(entity.ToDto());
    }

    /// <summary>
    /// 新增客戶
    /// </summary>
    [HttpPost]
    [SwaggerOperation(OperationId = "postApiCustomers")]
    public async Task<ActionResult<CustomerDto>> Create(
        CustomerCreateDto dto,
        [FromServices] IValidator<CustomerCreateDto> validator)
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return BadRequest(new ApiErrorResponse
            {
                Success = false,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "驗證失敗",
                TraceId = HttpContext.TraceIdentifier,
                Errors = errors
            });
        }

        var entity = dto.ToEntity();
        var created = await _service.CreateAsync(entity);
        var resultDto = created.ToDto();

        // ✅ 由 Filter 負責包成 ApiResponse<CustomerDto>，HTTP 201 維持不變
        return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
    }

    /// <summary>
    /// 更新客戶
    /// </summary>
    [HttpPut("{id:int}")]
    [SwaggerOperation(OperationId = "putApiCustomers")]
    public async Task<ActionResult<CustomerDto>> Update(
        int id,
        CustomerUpdateDto dto,
        [FromServices] IValidator<CustomerUpdateDto> validator)
    {
        dto.Id = id;

        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return BadRequest(new ApiErrorResponse
            {
                Success = false,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "驗證失敗",
                TraceId = HttpContext.TraceIdentifier,
                Errors = errors
            });
        }

        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound(new ApiErrorResponse
            {
                Success = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = "找不到客戶",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        dto.UpdateEntity(existing);
        var updated = await _service.UpdateAsync(id, existing);
        if (updated == null)
        {
            return NotFound(new ApiErrorResponse
            {
                Success = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = "找不到客戶",
                TraceId = HttpContext.TraceIdentifier,
            });
        }

        var resultDto = updated.ToDto();

        return Ok(resultDto);
    }

    /// <summary>
    /// 刪除客戶（僅 Admin）
    /// </summary>
    [HttpDelete("{id:int}")]
    [SwaggerOperation(OperationId = "deleteApiCustomers")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id);
        if (!ok)
        {
            // 小修正：訊息改成「客戶」而不是「產品」
            return NotFound(new ApiErrorResponse
            {
                Success = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = "找不到客戶",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return NoContent();
    }

    /// <summary>
    /// 客戶分頁查詢
    /// </summary>
    [HttpGet("paged")]
    [SwaggerOperation(OperationId = "getApiCustomersPaged")]
    public async Task<ActionResult<PagedResult<CustomerDto>>> GetPaged([FromQuery] PagedQueryDto query)
    {
        var paged = await _service.GetPagedAsync(query);
        var dtoItems = paged.Items.ToDtoList();

        var resultDto = new PagedResult<CustomerDto>
        {
            Items = dtoItems,
            TotalCount = paged.TotalCount,
        };

        return Ok(resultDto);
    }
}
