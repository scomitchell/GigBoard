using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using GigBoardBackend.Controllers;
using GigBoardBackend.Models;
using GigBoardBackend.Data;
using GigBoardBackend.Services;

namespace GigBoard.Tests.Controllers
{
    public class UserControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _service;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _service = new AuthService();

            SeedDatabase();

            _controller = new UserController(_context, _service);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() {User = user}
            };

            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "this_is_a_test_secret_key_32_bytes");
        }

        private void SeedDatabase()
        {
            if (!_context.Users.Any())
            {
                _context.Users.Add(new User { 
                    Id = 1, 
                    FirstName="John",
                    LastName="Smith",
                    Email = "test@example.com",
                    Username="jsmith",
                    Password = BCrypt.Net.BCrypt.HashPassword("password123")
                });

                _context.Users.Add(new User { 
                    Id = 2, 
                    FirstName="Harry",
                    LastName="Williams",
                    Email = "example2@example.com",
                    Username="hwilliams",
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
        public async Task CreateUser()
        {
            var user1 = new User {
                FirstName="Jane",
                LastName="Smith",
                Email = "test@example.com",
                Username="jsmith",
                Password="password123"
            };

            var user2 = new User {
                FirstName="Paula",
                LastName="Jones",
                Email = "example@example.com",
                Username="pjones",
                Password="password1234"
            };

            var badResult = await _controller.CreateUser(user1);
            var badObjectResult = Assert.IsType<BadRequestObjectResult>(badResult);

            var goodResult = await _controller.CreateUser(user2);
            var okResult = Assert.IsType<OkObjectResult>(goodResult);
            var response = Assert.IsAssignableFrom<TokenResponse>(okResult.Value);

            Assert.Equal("Paula", response.User.FirstName);
        }

        [Fact]
        public async Task LoggingIn_SignsUserIn()
        {
            var user1 = new User
            {
                Username = "jsmith",
                Password = "password123"
            };

            var user2 = new User
            {
                Username = "jsmith",
                Password = "password1234"
            };

            var goodResult = await _controller.Login(user1);
            var okResult = Assert.IsType<OkObjectResult>(goodResult);
            var response = Assert.IsAssignableFrom<TokenResponse>(okResult.Value);

            Assert.False(string.IsNullOrEmpty(response.Token));
            Assert.Equal("jsmith", response.User.Username);

            var badResult = await _controller.Login(user2);
            Assert.IsType<UnauthorizedObjectResult>(badResult);
        }

        [Fact]
        public async Task UpdateUser_PutsUser()
        {
            var user1 = new User 
            { 
                Id = 1, 
                FirstName="John",
                LastName="Smith",
                Email = "example@example.com",
                Username="jsmith"
            };

            var user2 = new User
            {
                Id = 1,
                FirstName="John",
                LastName="Smith",
                Email = "test@example.com",
                Username = "hwilliams"
            };

            var goodResult = await _controller.UpdateUser(user1);
            var okResult = Assert.IsType<OkObjectResult>(goodResult);
            var user = Assert.IsAssignableFrom<UserDto>(okResult.Value);

            Assert.Equal("example@example.com", user.Email);

            var badResult = await _controller.UpdateUser(user2);
            Assert.IsType<BadRequestObjectResult>(badResult);
        }

        [Fact]
        public async Task GetUserByUsername_ReturnsUser()
        {
            var result = await _controller.GetUserByUsername("jsmith");
            var badResult = await _controller.GetUserByUsername("jsmith2");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var user = Assert.IsAssignableFrom<UserDto>(okResult.Value);

            Assert.IsType<NotFoundResult>(badResult);

            Assert.Equal(1, user.Id);
        }
    }
}