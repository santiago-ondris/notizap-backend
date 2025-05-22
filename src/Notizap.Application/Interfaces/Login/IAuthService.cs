    public interface IAuthService
    {
        Task<LoginResponseDto> AuthenticateAsync(LoginRequestDto request);
        Task<UserDto> RegisterAsync(CreateUserDto dto);
    }