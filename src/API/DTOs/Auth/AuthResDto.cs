namespace API.DTOs.Auth
{
    public class AuthResDto
    {
        public string FullName { get; init; }
        public string UserName { get; init; }
        public string Email { get; init; }
        public string Role { get; init; }
        public string Token { get; init; }
        public DateTime TokenExpiration { get; init; }
        public string RefreshToken { get; init; }
        public DateTime RefreshTokenExpiration { get; init; }
    }
}
