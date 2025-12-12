using FluentValidation;
using FluentValidation.Results;
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
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    private readonly IValidator<ProductCreateDto> _createValidator;
    private readonly IValidator<ProductUpdateDto> _updateValidator;

    public ProductsController(
        IProductService service,
        IValidator<ProductCreateDto> createValidator,
        IValidator<ProductUpdateDto> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [SwaggerOperation(OperationId = "getApiProducts")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        var list = await _service.GetAllAsync();
        var dto = list.ToDtoList();

        return Ok(dto);
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(OperationId = "getApiProduct")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity == null)
        {
            return NotFound(new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "找不到產品",
                TraceId = HttpContext.TraceIdentifier,
                Errors = null
            });
        }

        var dto = entity.ToDto();

        return Ok(dto);
    }

    [HttpPost]
    [SwaggerOperation(OperationId = "postApiProducts")]
    public async Task<ActionResult<ProductDto>> Create(ProductCreateDto dto)
    {
        ValidationResult result = await _createValidator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            var errorResponse = new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "驗證失敗",
                TraceId = HttpContext.TraceIdentifier,
                Errors = errors
            };

            return BadRequest(errorResponse);
        }

        var entity = dto.ToEntity();
        var created = await _service.CreateAsync(entity);
        var resultDto = created.ToDto();

        return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
    }

    [HttpPut("{id:int}")]
    [SwaggerOperation(OperationId = "putApiProducts")]
    public async Task<ActionResult<ProductDto>> Update(int id, ProductUpdateDto dto)
    {
        ValidationResult result = await _updateValidator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            var errorResponse = new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "驗證失敗",
                TraceId = HttpContext.TraceIdentifier,
                Errors = errors
            };

            return BadRequest(errorResponse);
        }

        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound(new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "找不到產品",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        dto.UpdateEntity(existing);

        var updated = await _service.UpdateAsync(id, existing);
        if (updated == null)
        {
            return NotFound(new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "找不到產品",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        var resultDto = updated.ToDto();

        return Ok(resultDto);
    }

    [HttpGet("paged")]
    [SwaggerOperation(OperationId = "getApiProductsPaged")]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetPaged([FromQuery] PagedQueryDto query)
    {
        var result = await _service.GetPagedAsync(query);
        var dtoItems = result.Items.ToDtoList();

        var dtoPaged = new PagedResult<ProductDto>
        {
            Items = dtoItems,
            TotalCount = result.TotalCount
        };

        return Ok(dtoPaged);
    }

    [HttpDelete("{id:int}")]
    [SwaggerOperation(OperationId = "deleteApiProducts")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id);
        if (!ok)
        {
            return NotFound(new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "找不到產品",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        var response = new ApiResponse<object?>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "刪除成功",
            TraceId = HttpContext.TraceIdentifier,
            Data = null
        };

        return Ok(response);
    }
}
