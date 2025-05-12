    public interface IAuthService
    {
        LoginResponseDto Authenticate(LoginRequestDto request);
    }