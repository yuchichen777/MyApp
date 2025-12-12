using FluentValidation;
using MyApp.Application.DTOs;
using MyApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MyApp.Application.Validation;

public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserDtoValidator(AppDbContext db)
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("帳號必填")
            .MaximumLength(50)
            .MustAsync(async (userName, ct) =>
            {
                // 🔍 檢查帳號是否唯一
                if (string.IsNullOrWhiteSpace(userName))
                    return true;

                return !await db.Users.AnyAsync(u => u.UserName == userName, ct);
            })
            .WithMessage("帳號已存在");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密碼必填")
            .MinimumLength(6).WithMessage("密碼長度至少 6 碼");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("兩次輸入的密碼不一致");
    }
}
