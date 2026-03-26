using Microsoft.EntityFrameworkCore;
using GigBoardBackend.Models;
using GigBoardBackend.Data;
using Moq;
using Microsoft.AspNetCore.SignalR;
using GigBoardBackend.Services;
using GigBoardBackend.Hubs;

namespace GigBoard.Tests.Controllers
{
    public class ShiftServiceTests : IDisposable
    {
        private readonly ShiftService _service;
        private readonly ApplicationDbContext _context;

        public ShiftServiceTests()
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

            _service = new ShiftService(_context, mockStatsService.Object, mockHubContext.Object);
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
                    UserId = 1,
                    StartTime = DateTime.Now.AddHours(-1),
                    EndTime = DateTime.Now,
                    App = DeliveryApp.UberEats
                };

                _context.Shifts.Add(shift1);
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
            var results = await _service.GetShiftsAsync(1);
            var userShifts = results.ToList();

            var userShift = Assert.Single(userShifts);
            Assert.Equal(DeliveryApp.UberEats, userShift.App);
        }

        [Fact]
        public async Task GetFilteredShifts_FiltersShifts()
        {
            var shift2 = new Shift
            {
                Id = 2,
                UserId = 1,
                StartTime = DateTime.Now.AddHours(-1),
                EndTime = DateTime.Now,
                App = DeliveryApp.Doordash
            };
            _context.Shifts.Add(shift2);
            await _context.SaveChangesAsync();

            var result = await _service.GetFilteredShiftsAsync(1, startTime: null,
                endTime: null,
                app: DeliveryApp.Doordash);

            var shifts = result.ToList();

            Assert.Contains(shifts, s => s.Id == 2);
            Assert.DoesNotContain(shifts, s => s.Id == 1);
        }

        [Fact]
        public async Task DeleteShift_RemovesShift()
        {
            Assert.True(_context.Shifts.Any(d => d.Id == 1));

            await _service.DeleteShiftAsync(1, 1);
            Assert.False(_context.Shifts.Any());
        }

        [Fact]
        public async Task AddShifts_PostsShift()
        {
            var shift3 = new Shift
            {
                Id = 3,
                UserId = 1,
                StartTime = DateTime.Now.AddHours(-1),
                EndTime = DateTime.Now,
                App = DeliveryApp.Grubhub
            };

            var result = await _service.AddShiftAsync(1, shift3);

            Assert.IsType<ShiftDto>(result);
            var userShift = await _context.Shifts
                .FirstOrDefaultAsync(s => s.UserId == 1 && s.Id == shift3.Id);

            Assert.NotNull(userShift);

            var startAfterEndShift = new Shift
            {
                Id = 4,
                UserId = 1,
                StartTime = new DateTime(2025, 1, 1, 3, 0, 0),
                EndTime = new DateTime(2025, 1, 1, 0, 0, 0),
                App = DeliveryApp.UberEats
            };

            var startAfterEndResult = await Assert.ThrowsAsync<ArgumentException>(() => _service.AddShiftAsync(1, startAfterEndShift));
            Assert.Equal("Shift end time must come after shift start time", startAfterEndResult.Message);

            var startEqualEndShift = new Shift
            {
                Id = 5,
                UserId = 1,
                StartTime = new DateTime(2025, 1, 1, 0, 0, 0),
                EndTime = new DateTime(2025, 1, 1, 0, 0, 0),
                App = DeliveryApp.Doordash
            };

            var startEqualEndResult = await Assert.ThrowsAsync<ArgumentException>(() => _service.AddShiftAsync(1, startEqualEndShift));
            Assert.Equal("Shift end time must come after shift start time", startEqualEndResult.Message);

            var futureStartShift = new Shift
            {
                Id = 6,
                UserId = 1,
                StartTime = DateTime.Now.AddHours(1),
                EndTime = DateTime.Now.AddHours(2),
                App = DeliveryApp.Grubhub
            };

            var futureStartShiftResult = await Assert.ThrowsAsync<ArgumentException>(() => _service.AddShiftAsync(1, futureStartShift));
            Assert.Equal("Shift start time cannot be in the future", futureStartShiftResult.Message);
        }

        [Fact]
        public async Task GetShiftById_ReturnsShift()
        {
            var result = await _service.GetShiftByIdAsync(1, 1);

            Assert.NotNull(result);
            Assert.Equal(DeliveryApp.UberEats, result.App);

            var result2 = await _service.GetShiftByIdAsync(1, 2);
            Assert.Null(result2);
        }

        [Fact]
        public async Task GetShiftApps_ReturnsApps()
        {
            var shift4 = new Shift
            {
                Id = 4,
                UserId = 1,
                StartTime = DateTime.Now.AddHours(-1),
                EndTime = DateTime.Now,
                App = DeliveryApp.Grubhub
            };
            _context.Shifts.Add(shift4);
            await _context.SaveChangesAsync();

            var result = await _service.GetUserShiftAppsAsync(1);

            var apps = result.ToList();

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
                UserId = 1,
                StartTime = DateTime.Now.AddHours(-1),
                EndTime = DateTime.Now,
                App = DeliveryApp.Doordash
            };

            var result = await _service.UpdateShiftAsync(1, shift1);

            Assert.Equal(DeliveryApp.Doordash, result.App);

            var startAfterEndShift = new Shift
            {
                Id = 1,
                UserId = 1,
                StartTime = new DateTime(2025, 1, 1, 3, 0, 0),
                EndTime = new DateTime(2025, 1, 1, 0, 0, 0),
                App = DeliveryApp.UberEats
            };

            var startAfterEndResult = await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateShiftAsync(1, startAfterEndShift));
            Assert.Equal("Shift start time must be before end time", startAfterEndResult.Message);

            var startEqualEndShift = new Shift
            {
                Id = 1,
                UserId = 1,
                StartTime = new DateTime(2025, 1, 1, 0, 0, 0),
                EndTime = new DateTime(2025, 1, 1, 0, 0, 0),
                App = DeliveryApp.Doordash
            };

            var startEqualEndResult = await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateShiftAsync(1, startEqualEndShift));
            Assert.Equal("Shift start time must be before end time", startEqualEndResult.Message);

            var futureStartShift = new Shift
            {
                Id = 1,
                UserId = 1,
                StartTime = DateTime.Now.AddHours(1),
                EndTime = DateTime.Now.AddHours(2),
                App = DeliveryApp.Grubhub
            };

            var futureStartShiftResult = await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateShiftAsync(1, futureStartShift));
            Assert.Equal("Shift start time cannot be in the future", futureStartShiftResult.Message);
        }
    }
}