    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string? Username { get; set; }
        public string? Email { get; set; }
    }