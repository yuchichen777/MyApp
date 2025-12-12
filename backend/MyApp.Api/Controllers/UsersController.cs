using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Api.Models;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;
using MyApp.Application.Mapping;
using Swashbuckle.AspNetCore.Annotations;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // 使用者管理只給 Admin
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("paged")]
        [SwaggerOperation(OperationId = "getUsersPaged", Summary = "取得使用者分頁列表")]
        public async Task<ActionResult<PagedResult<UserDto>>> GetPaged(
        [FromQuery] PagedQueryDto query, string? role)
        {
            var result = await _userService.GetPagedAsync(query, role ?? "");

            var dtoItems = result.Items.ToDtoList();

            var resultDto = new PagedResult<UserDto>
            {
                Items = dtoItems,
                TotalCount = result.TotalCount,
            };

            return Ok(resultDto);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(OperationId = "getApiUser")]
        public async Task<ActionResult<UserDto>> GetById(int id)
        {
            var entity = await _userService.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound(new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "找不到使用者",
                    TraceId = HttpContext.TraceIdentifier,
                });
            }

            var dto = entity.ToDto();

            return Ok(dto);
        }

        [HttpPost]
        [SwaggerOperation(OperationId = "postApiUsers")]
        public async Task<ActionResult<UserDto>> Create(
        UserCreateDto dto,
        [FromServices] IValidator<UserCreateDto> validator)
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

                var errorResponse = new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "驗證失敗",
                    TraceId = HttpContext.TraceIdentifier,
                    Errors = errors
                };

                return BadRequest(errorResponse);
            }

            var currentUser = User.Identity?.Name ?? "system";
            var created = await _userService.CreateAsync(dto, currentUser);
            var resultDto = created.ToDto();

            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }

        [HttpPut("{id:int}")]
        [SwaggerOperation(OperationId = "putApiUsers")]
        public async Task<ActionResult<UserDto>> Update(
        int id,
        UserUpdateDto dto,
        [FromServices] IValidator<UserUpdateDto> validator)
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

                var errorResponse = new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "驗證失敗",
                    TraceId = HttpContext.TraceIdentifier,
                    Errors = errors
                };

                return BadRequest(errorResponse);
            }

            var existing = await _userService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "找不到使用者",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            var currentUser = User.Identity?.Name ?? "system";
            var updated = await _userService.UpdateAsync(id, dto, currentUser);
            if (updated == null)
            {
                return NotFound(new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "找不到使用者",
                    TraceId = HttpContext.TraceIdentifier,
                });
            }

            var resultDto = updated.ToDto();

            return Ok(resultDto);
        }

        [HttpDelete("{id:int}")]
        [SwaggerOperation(OperationId = "deleteApiUsers")]
        public async Task<ActionResult<ApiResponse<object?>>> Delete(int id)
        {
            var ok = await _userService.DeleteAsync(id);
            if (!ok)
            {
                return NotFound(new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "找不到使用者",
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

        [HttpGet("me")]
        [AllowAnonymous]
        public IActionResult Me()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(claims);
        }
    }
}
