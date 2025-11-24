using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using GigBoardBackend.Controllers;
using GigBoardBackend.Models;
using GigBoardBackend.Data;
using Moq;
using Microsoft.AspNetCore.SignalR;
using GigBoard.Hubs;
using GigBoardBackend.Services;

namespace GigBoard.Tests.Controllers
{
    public class UserShiftControllerTests : IDisposable
    {
        private readonly UserShiftController _controller;
        private readonly ApplicationDbContext _context;

        public UserShiftControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            SeedDatabase();

            // Mock SignalR
            var mockHubClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();

            mockHubClients.Setup(clients => clients.User(It.IsAny<string>())).Returns(mockClientProxy.Object);

            var mockHubContext = new Mock<IHubContext<StatisticsHub>>();
            mockHubContext.Setup(h => h.Clients).Returns(mockHubClients.Object);

            var mockStatsService = new Mock<StatisticsService>(_context);

            _controller = new UserShiftController(_context, mockHubContext.Object, mockStatsService.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext {User = user}
            };
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
                    Email = "example@email.com",
                    Username = "jsmith",
                    Password = "password123"
                });

                _context.SaveChanges();
            }

            if (!_context.Shifts.Any())
            {
                var shift1 = new Shift
                {
                    Id = 1,
                    StartTime = DateTime.Now.AddHours(-1),
                    EndTime = DateTime.Now,
                    App = DeliveryApp.UberEats
                };

                _context.Shifts.Add(shift1);
                _context.SaveChanges();

                _context.UserShifts.Add(new UserShift
                {
                    UserId = 1,
                    ShiftId = 1
                });

                _context.SaveChanges();
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetShifts_ReturnsShifts()
        {
            var results = await _controller.GetShifts();
            var okResult = Assert.IsType<OkObjectResult>(results);
            var userShifts = Assert.IsAssignableFrom<IEnumerable<ShiftDto>>(okResult.Value);

            var userShift = Assert.Single(userShifts);
            Assert.Equal(DeliveryApp.UberEats, userShift.App);
        }

        [Fact]
        public async Task GetFilteredShifts_FiltersShifts()
        {
            var shift2 = new Shift
            {
                Id = 2,
                StartTime = DateTime.Now.AddHours(-1),
                EndTime = DateTime.Now,
                App = DeliveryApp.Doordash
            };
            _context.Shifts.Add(shift2);

            var userShift = new UserShift
            {
                UserId = 1,
                ShiftId = shift2.Id
            };
            _context.UserShifts.Add(userShift);

            await _context.SaveChangesAsync();

            var result = await _controller.GetFilteredShifts(startTime: null,
                endTime: null,
                app: DeliveryApp.Doordash);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var shifts = Assert.IsAssignableFrom<IEnumerable<ShiftDto>>(okResult.Value);

            Assert.Contains(shifts, s => s.Id == 2);
            Assert.DoesNotContain(shifts, s => s.Id == 1);
        }

        [Fact]
        public async Task DeleteShift_RemovesShift()
        {
            Assert.True(_context.Shifts.Any(d => d.Id == 1));
            var result = await _controller.DeleteShift(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False(_context.Shifts.Any());
        }

        [Fact]
        public async Task AddShifts_PostsShift()
        {
            var shift3 = new Shift
            {
                Id = 3,
                StartTime = DateTime.Now.AddHours(-1),
                EndTime = DateTime.Now,
                App = DeliveryApp.Grubhub
            };

            var result = await _controller.AddShift(shift3);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var userShift = await _context.UserShifts
                .FirstOrDefaultAsync(us => us.UserId == 1 && us.ShiftId == shift3.Id);

            Assert.NotNull(userShift);
        }

        [Fact]
        public async Task GetShiftById_ReturnsShift()
        {
            var result = await _controller.GetShiftById(1);
            
            var okResult = Assert.IsType<OkObjectResult>(result);
            var userShift = Assert.IsAssignableFrom<ShiftDto>(okResult.Value);
            Assert.Equal(DeliveryApp.UberEats, userShift.App);

            var result2 = await _controller.GetShiftById(2);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result2);
        }

        [Fact]
        public async Task GetShiftApps_ReturnsApps()
        {
            var shift4 = new Shift
            {
                Id = 4,
                StartTime = DateTime.Now.AddHours(-1),
                EndTime = DateTime.Now,
                App = DeliveryApp.Grubhub
            };
            _context.Shifts.Add(shift4);

            var userShift = new UserShift
            {
                UserId = 1,
                ShiftId = shift4.Id
            };
            _context.UserShifts.Add(userShift);

            await _context.SaveChangesAsync();

            var result = await _controller.GetUserShiftApps();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var apps = Assert.IsAssignableFrom<IEnumerable<DeliveryApp>>(okResult.Value);

            Assert.Contains(DeliveryApp.Grubhub, apps);
            Assert.Contains(DeliveryApp.UberEats, apps);
            Assert.DoesNotContain(DeliveryApp.Doordash, apps);
        }

        [Fact]
        public async Task UpdateShift_PutsShift()
        {
            var shift1 = new Shift
            {
                Id = 1,
                StartTime = DateTime.Now.AddHours(-1),
                EndTime = DateTime.Now,
                App = DeliveryApp.Doordash
            };

            var result = await _controller.UpdateShift(shift1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var shift = Assert.IsAssignableFrom<ShiftDto>(okResult.Value);

            Assert.Equal(DeliveryApp.Doordash, shift.App);
        }
    }
}