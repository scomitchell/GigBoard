using GigBoardBackend.Models;

namespace GigBoardBackend.Services
{
    public interface IAuthService
    {
        string GenerateToken(User user);
    }
}