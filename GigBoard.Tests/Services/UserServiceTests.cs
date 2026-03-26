using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using GigBoardBackend.Models;
using GigBoardBackend.Data;
using GigBoardBackend.Services;

namespace GigBoard.Tests.Service
{
    public class UserServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly UserService _service;

        public UserServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _authService = new AuthService();
            _service = new UserService(_context, _authService);

            SeedDatabase();

            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "this_is_a_test_secret_key_32_bytes");
        }

        private void SeedDatabase()
        {
            if (!_context.Users.Any())
            {
                _context.Users.Add(new User
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Smith",
                    Email = "test@example.com",
                    Username = "jsmith",
                    Password = BCrypt.Net.BCrypt.HashPassword("password123")
                });

                _context.Users.Add(new User
                {
                    Id = 2,
                    FirstName = "Harry",
                    LastName = "Williams",
                    Email = "example2@example.com",
                    Username = "hwilliams",
                    Password = BCrypt.Net.BCrypt.HashPassword("password1234")
                });

                _context.SaveChanges();
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task CreateUser_WithValidUser_ReturnsTokenAndUser()
        {
            var newUser = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "janesmith@example.com",
                Username = "janesmith",
                Password = "password123"
            };

            var result = await _service.CreateUserAsync(newUser);

            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.NotEmpty(result.Token);
            Assert.Equal("Jane", result.User.FirstName);
            Assert.Equal("janesmith", result.User.Username);
        }

        [Fact]
        public async Task CreateUser_WithDuplicateUsername_ThrowsArgumentException()
        {
            var duplicateUser = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "newjane@example.com",
                Username = "jsmith",
                Password = "password123"
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateUserAsync(duplicateUser));

            Assert.Equal("Username is already taken", exception.Message);
        }

        [Fact]
        public async Task CreateUser_WithDuplicateEmail_ThrowsArgumentException()
        {
            var duplicateEmailUser = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "test@example.com",
                Username = "janesmith",
                Password = "password123"
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateUserAsync(duplicateEmailUser));

            Assert.Equal("Email address is already taken", exception.Message);
        }

        [Fact]
        public async Task LoginUser_WithValidCredentials_ReturnsTokenAndUser()
        {
            var loginUser = new User
            {
                Username = "jsmith",
                Password = "password123"
            };

            var result = await _service.LoginUserAsync(loginUser);

            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.NotEmpty(result.Token);
            Assert.Equal("jsmith", result.User.Username);
            Assert.Equal("John", result.User.FirstName);
        }

        [Fact]
        public async Task LoginUser_WithInvalidPassword_ThrowsUnauthorizedAccessException()
        {
            var loginUser = new User
            {
                Username = "jsmith",
                Password = "wrongpassword"
            };

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.LoginUserAsync(loginUser));

            Assert.Equal("Wrong username or password", exception.Message);
        }

        [Fact]
        public async Task LoginUser_WithNonexistentUsername_ThrowsUnauthorizedAccessException()
        {
            var loginUser = new User
            {
                Username = "nonexistent",
                Password = "password123"
            };

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.LoginUserAsync(loginUser));

            Assert.Equal("Wrong username or password", exception.Message);
        }

        [Fact]
        public async Task UpdateUser_WithValidData_UpdatesAndReturnsUser()
        {
            var updatedUser = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith",
                Email = "newemail@example.com",
                Username = "jsmith"
            };

            var result = await _service.UpdateUserAsync(updatedUser);

            Assert.NotNull(result);
            Assert.Equal("newemail@example.com", result.Email);
            Assert.Equal("jsmith", result.Username);
        }

        [Fact]
        public async Task UpdateUser_WithDuplicateUsername_ThrowsArgumentException()
        {
            var updatedUser = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith",
                Email = "test@example.com",
                Username = "hwilliams"
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateUserAsync(updatedUser));

            Assert.Equal("Username is already taken", exception.Message);
        }

        [Fact]
        public async Task UpdateUser_WithDuplicateEmail_ThrowsArgumentException()
        {
            var updatedUser = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith",
                Email = "example2@example.com",
                Username = "jsmith"
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateUserAsync(updatedUser));

            Assert.Equal("Email is already taken", exception.Message);
        }

        [Fact]
        public async Task UpdateUser_WithNonexistentUser_ThrowsKeyNotFoundException()
        {
            var updatedUser = new User
            {
                Id = 999,
                FirstName = "Nonexistent",
                LastName = "User",
                Email = "fake@example.com",
                Username = "fakeuser"
            };

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateUserAsync(updatedUser));

            Assert.Equal("User not found", exception.Message);
        }

        [Fact]
        public async Task GetUserByUsername_WithValidUsername_ReturnsUser()
        {
            var result = await _service.GetUserByUsernameAsync("jsmith");

            Assert.NotNull(result);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("jsmith", result.Username);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetUserByUsername_WithNonexistentUsername_ThrowsKeyNotFoundException()
        {
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.GetUserByUsernameAsync("nonexistent"));

            Assert.Equal("User not found with this username", exception.Message);
        }

        [Fact]
        public async Task GetUserHasData_WithUserHavingDeliveries_ReturnsTrue()
        {
            // Add a delivery for user 1
            _context.Deliveries.Add(new Delivery
            {
                UserId = 1,
                App = DeliveryApp.UberEats,
                DeliveryTime = DateTime.Now,
                BasePay = 5.0,
                TipPay = 2.0,
                TotalPay = 7.0,
                Mileage = 2.5,
                Restaurant = "Test Restaurant",
                CustomerNeighborhood = "Test Area",
                Notes = "Test"
            });
            await _context.SaveChangesAsync();

            var result = await _service.GetUserHasDataAsync(1);

            Assert.True(result);
        }

        [Fact]
        public async Task GetUserHasData_WithUserHavingNoDeliveries_ReturnsFalse()
        {
            var result = await _service.GetUserHasDataAsync(2);

            Assert.False(result);
        }
    }
}