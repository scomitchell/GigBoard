using GigBoardBackend.Models;

namespace GigBoardBackend.Services
{
    public interface IUserService
    {
        Task<TokenResponse> CreateUserAsync(User user);
        Task<TokenResponse> LoginUserAsync(User user);
        Task<UserDto> UpdateUserAsync(User user);
        Task<UserDto> GetUserByUsernameAsync(string username);
        Task<bool> GetUserHasDataAsync(int userId);
    }
}