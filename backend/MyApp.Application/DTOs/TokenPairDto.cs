namespace MyApp.Application.DTOs
{
    public class TokenPairDto
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
    }
}
