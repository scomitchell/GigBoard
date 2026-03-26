using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GigBoardBackend.Models;
using GigBoardBackend.Data;
using Moq;
using Microsoft.AspNetCore.SignalR;
using GigBoardBackend.Services;
using GigBoardBackend.Hubs;

namespace GigBoard.Tests.Services
{
    public class DeliveryServiceTests : IDisposable
    {
        private readonly DeliveryService _service;
        private readonly ApplicationDbContext _context;

        public DeliveryServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            SeedDatabase();

            // Mock SignalR setup
            var mockHubClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();

            mockHubClients.Setup(clients => clients.User(It.IsAny<string>())).Returns(mockClientProxy.Object);

            var mockHubContext = new Mock<IHubContext<StatisticsHub>>();
            mockHubContext
                .Setup(h => h.Clients)
                .Returns(mockHubClients.Object);

            var mockStatsService = new Mock<StatisticsService>(_context);

            // Controller
            _service = new DeliveryService(_context, mockStatsService.Object, mockHubContext.Object);
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
                    Password = "password123"
                });

                _context.SaveChanges();
            }

            if (!_context.Deliveries.Any())
            {
                var delivery1 = new Delivery
                {
                    Id = 1,
                    UserId = 1,
                    App = DeliveryApp.UberEats,
                    DeliveryTime = DateTime.Now,
                    BasePay = 3.0,
                    TipPay = 2.50,
                    TotalPay = 5.50,
                    Mileage = 1.2,
                    Restaurant = "Love Art Sushi",
                    CustomerNeighborhood = "Back Bay",
                    Notes = "test 1"
                };

                _context.Deliveries.Add(delivery1);
                _context.SaveChanges();
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetDeliveries_ReturnsUserDeliveries()
        {
            var results = await _service.GetDeliveriesAsync(1);
            var userDeliveries = Assert.IsAssignableFrom<IEnumerable<DeliveryDto>>(results);

            var userDelivery = Assert.Single(userDeliveries);
            Assert.Equal("Love Art Sushi", userDelivery.Restaurant);
        }

        [Fact]
        public async Task AddDelivery_AddsTo_UserList()
        {
            var delivery2 = new Delivery
            {
                Id = 2,
                UserId = 1,
                App = DeliveryApp.Doordash,
                DeliveryTime = DateTime.Now.AddHours(-1),
                BasePay = 2.43,
                TipPay = 2.00,
                TotalPay = 4.43,
                Mileage = 1.4,
                Restaurant = "Serafina",
                CustomerNeighborhood = "Fenway",
                Notes = "test2"
            };

            var result = await _service.AddDeliveryAsync(1, delivery2);

            Assert.IsType<DeliveryDto>(result);
            var userDelivery = await _context.Deliveries
                .FirstOrDefaultAsync(d => d.UserId == 1 && d.Id == delivery2.Id);

            Assert.NotNull(userDelivery);

            var badDelivery = new Delivery
            {
                Id = 3,
                UserId = 1,
                App = DeliveryApp.UberEats,
                DeliveryTime = DateTime.Now.AddHours(1),
                BasePay = 3.0,
                TipPay = 4.0,
                Mileage = 1.3,
                Restaurant = "Test Restaurant 1",
                CustomerNeighborhood = "Allston",
                Notes = "Test 3"
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.AddDeliveryAsync(1, badDelivery));
            Assert.Equal("Delivery time cannot be in the future", exception.Message);
        }

        [Fact]
        public async Task GetFilteredDeliveries_ReturnsFilteredDeliveries()
        {
            var delivery3 = new Delivery
            {
                Id = 3,
                UserId = 1,
                App = DeliveryApp.Grubhub,
                DeliveryTime = DateTime.Now,
                BasePay = 3.50,
                TipPay = 2.50,
                TotalPay = 6.00,
                Mileage = 0.9,
                Restaurant = "YGF Malatang",
                CustomerNeighborhood = "Mission Hill",
                Notes = "test 13"
            };

            _context.Deliveries.Add(delivery3);
            await _context.SaveChangesAsync();

            var result = await _service.GetFilteredDeliveriesAsync(1, app: DeliveryApp.Grubhub,
                basePay: null,
                tipPay: null,
                totalPay: null,
                mileage: null,
                restaurant: null,
                customerNeighborhood: null);

            var deliveries = Assert.IsAssignableFrom<IEnumerable<DeliveryDto>>(result);
            Assert.Contains(deliveries, d => d.Restaurant == "YGF Malatang");
            Assert.DoesNotContain(deliveries, d => d.Id == 1);
            Assert.DoesNotContain(deliveries, d => d.Restaurant == "Love Art Sushi");
        }

        [Fact]
        public async Task GetDeliveryById_ReturnsDelivery()
        {
            var result = await _service.GetDeliveryByIdAsync(1, 1);

            var userDelivery = Assert.IsAssignableFrom<Delivery>(result);
            Assert.Equal("Love Art Sushi", userDelivery.Restaurant);

            var result2 = await _service.GetDeliveryByIdAsync(1, 4);
            Assert.Null(result2);
        }

        [Fact]
        public async Task DeleteDelivery_DeletesDelivery()
        {
            Assert.True(_context.Deliveries.Any(d => d.Id == 1));

            await _service.DeleteDeliveryAsync(1, 1);
            Assert.False(_context.Deliveries.Any());
        }

        [Fact]
        public async Task GetNeighborhoods_ReturnsNeighborhoods()
        {
            var delivery3 = new Delivery
            {
                Id = 3,
                UserId = 1,
                App = DeliveryApp.Grubhub,
                DeliveryTime = DateTime.Now,
                BasePay = 2.43,
                TipPay = 2.00,
                TotalPay = 4.43,
                Mileage = 1.4,
                Restaurant = "Serafina",
                CustomerNeighborhood = "Mission Hill",
                Notes = "test3"
            };

            _context.Deliveries.Add(delivery3);
            await _context.SaveChangesAsync();

            var result = await _service.GetUserDeliveryNeighborhoodsAsync(1);
            var neighborhoods = Assert.IsAssignableFrom<IEnumerable<string>>(result);

            Assert.Contains("Mission Hill", neighborhoods);
            Assert.Contains("Back Bay", neighborhoods);
        }

        [Fact]
        public async Task GetApps_ReturnsApps()
        {
            var delivery4 = new Delivery
            {
                Id = 4,
                UserId = 1,
                App = DeliveryApp.Doordash,
                DeliveryTime = DateTime.Now,
                BasePay = 2.43,
                TipPay = 2.00,
                TotalPay = 4.43,
                Mileage = 1.4,
                Restaurant = "El Jefe's",
                CustomerNeighborhood = "Fenway",
                Notes = "test4"
            };
            _context.Deliveries.Add(delivery4);
            await _context.SaveChangesAsync();

            var result = await _service.GetUserDeliveryAppsAsync(1);
            var apps = Assert.IsAssignableFrom<IEnumerable<DeliveryApp>>(result);

            Assert.Contains(DeliveryApp.UberEats, apps);
            Assert.Contains(DeliveryApp.Doordash, apps);
        }

        [Fact]
        public async Task UpdateDelivery_PutsDelivery()
        {
            var delivery1 = new Delivery
            {
                Id = 1,
                UserId = 1,
                App = DeliveryApp.UberEats,
                DeliveryTime = DateTime.Now,
                BasePay = 5.0,
                TipPay = 2.50,
                TotalPay = 7.50,
                Mileage = 1.2,
                Restaurant = "Love Art Sushi",
                CustomerNeighborhood = "Back Bay",
                Notes = "test 1"
            };

            var result = await _service.UpdateDeliveryAsync(1, delivery1);

            Assert.Equal(5.0, result.BasePay);
            Assert.Equal(7.50, result.TotalPay);

            var badDelivery1 = new Delivery
            {
                Id = 1,
                UserId = 1,
                App = DeliveryApp.UberEats,
                DeliveryTime = DateTime.Now.AddHours(1),
                BasePay = 5.0,
                TipPay = 2.50,
                TotalPay = 7.50,
                Mileage = 1.2,
                Restaurant = "Love Art Sushi",
                CustomerNeighborhood = "Back Bay",
                Notes = "test 1"
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateDeliveryAsync(1, badDelivery1));
            Assert.Equal("Delivery time cannot be in the future", exception.Message);
        }
    }
}