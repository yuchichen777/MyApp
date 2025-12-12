// MyApp.UnitTests/Validators/ProductValidatorTests.cs
using FluentValidation.TestHelper;
using MyApp.Application.DTOs;
using MyApp.Application.Validation;

public class ProductValidatorTests
{
    private readonly ProductCreateDtoValidator _validator = new();

    [Fact]
    public void Name_Is_Required()
    {
        var model = new ProductCreateDto { Name = "", Code = "P01", Price = 10 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
