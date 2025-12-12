using MyApp.Domain.Entities;

namespace MyApp.Application.DTOs.AuthResult
{
    public class ValidateUserResult
    {
        public bool Success { get; set; }
        public AppUser? User { get; set; }
        public string? ErrorMessage { get; set; }

        public static ValidateUserResult Fail(string error)
        {
            return new ValidateUserResult
            {
                Success = false,
                User = null,
                ErrorMessage = error
            };
        }

        public static ValidateUserResult Ok(AppUser user)
        {
            return new ValidateUserResult
            {
                Success = true,
                User = user,
                ErrorMessage = null
            };
        }
    }

}
