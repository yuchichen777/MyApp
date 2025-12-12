using FluentValidation;
using MyApp.Application.DTOs;

namespace MyApp.Application.Validation;

public class ProductCreateDtoValidator : AbstractValidator<ProductCreateDto>
{
    public ProductCreateDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("產品代碼必填")
            .MaximumLength(50);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("產品名稱必填")
            .MaximumLength(200);

        RuleFor(x => x.Price)
            .NotNull().WithMessage("價格必填")
            .GreaterThanOrEqualTo(0).WithMessage("價格不可為負數");
    }
}

public class ProductUpdateDtoValidator : AbstractValidator<ProductUpdateDto>
{
    public ProductUpdateDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("產品代碼必填")
            .MaximumLength(50);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("產品名稱必填")
            .MaximumLength(200);

        RuleFor(x => x.Price)
            .NotNull().WithMessage("價格必填")
            .GreaterThanOrEqualTo(0).WithMessage("價格不可為負數");
    }
}
