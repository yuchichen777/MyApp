using FluentValidation;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;

namespace MyApp.Application.Validation;

public class CustomerCreateDtoValidator : AbstractValidator<CustomerCreateDto>
{
    public CustomerCreateDtoValidator(ICustomerService customerService)
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("客戶代碼必填")
            .MaximumLength(50)
            .MustAsync(async (code, ct) =>
            {
                if (string.IsNullOrWhiteSpace(code))
                    return true; // 交給 NotEmpty 處理

                return await customerService.IsCodeUniqueAsync(code);
            })
            .WithMessage("客戶代碼已存在");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("客戶名稱必填")
            .MaximumLength(200);

        RuleFor(x => x.Phone)
            .MaximumLength(50);
    }
}

public class CustomerUpdateDtoValidator : AbstractValidator<CustomerUpdateDto>
{
    public CustomerUpdateDtoValidator(ICustomerService customerService)
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("客戶代碼必填")
            .MaximumLength(50)
            .MustAsync(async (dto, code, ct) =>
            {
                if (string.IsNullOrWhiteSpace(code))
                    return true;

                return await customerService.IsCodeUniqueAsync(dto.Id, code);
            })
            .WithMessage("客戶代碼已存在");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("客戶名稱必填")
            .MaximumLength(200);

        RuleFor(x => x.Phone)
            .MaximumLength(50);
    }
}
