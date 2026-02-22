using WorkFlow.Models;

namespace WorkFlow.Services
{
    public interface ITokenService
    {
        string CreateAccessToken(User user);
        string CreateRefreshToken();
    }
}