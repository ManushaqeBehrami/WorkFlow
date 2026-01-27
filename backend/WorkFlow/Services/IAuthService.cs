using WorkFlow.DTOs;

namespace WorkFlow.Services
{
    public interface IAuthService
    {
        Task Register(RegisterDto dto);
        Task<AuthResponseDto> Login(LoginDto dto);
    }
}
