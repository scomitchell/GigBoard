using GigBoardBackend.Models;
using GigBoardBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace GigBoardBackend.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;

        public UserService(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<TokenResponse> CreateUserAsync(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                throw new ArgumentException("Username is already taken");
            }

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                throw new ArgumentException("Email address is already taken");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _authService.GenerateToken(user);

            var response = new TokenResponse
            {
                Token = token,
                User = MapToDto(user)
            };

            return response;
        }

        public async Task<TokenResponse> LoginUserAsync(User user)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == user.Username);

            if (existingUser == null || !BCrypt.Net.BCrypt.Verify(user.Password, existingUser.Password))
            {
                throw new UnauthorizedAccessException("Wrong username or password");
            }

            var token = _authService.GenerateToken(existingUser);

            var response = new TokenResponse
            {
                Token = token,
                User = MapToDto(existingUser)
            };

            return response;
        }

        public async Task<UserDto> UpdateUserAsync(User user)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

            if (existingUser == null) throw new KeyNotFoundException("User not found");

            if (existingUser.Username != user.Username)
            {
                bool usernameExists = await _context.Users.AnyAsync(u => u.Username == user.Username);
                if (usernameExists)
                {
                    throw new ArgumentException("Username is already taken");
                }
            }

            if (existingUser.Email != user.Email)
            {
                bool emailExists = await _context.Users.AnyAsync(u => u.Email == user.Email);
                if (emailExists)
                {
                    throw new ArgumentException("Email is already taken");
                }
            }

            existingUser.Username = user.Username;
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Email = user.Email;

            if (!string.IsNullOrEmpty(user.Password))
            {
                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            }

            await _context.SaveChangesAsync();
            return MapToDto(existingUser);
        }

        public async Task<UserDto> GetUserByUsernameAsync(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) throw new KeyNotFoundException("User not found with this username");
            return MapToDto(user);
        }

        public async Task<bool> GetUserHasDataAsync(int userId)
        {
            return await _context.Deliveries.AnyAsync(d => d.UserId == userId);
        }

        private static UserDto MapToDto(User u)
        {
            return new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
            };
        }
    }
}