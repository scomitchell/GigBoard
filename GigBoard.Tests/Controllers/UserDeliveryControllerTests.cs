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
using Microsoft.AspNetCore.Http.HttpResults;

namespace GigBoard.Tests.Controllers 
{
    public class UserDeliveryControllerTests : IDisposable
    {
        private readonly UserDeliveryController _controller;
        private readonly ApplicationDbContext _context;

        public UserDeliveryControllerTests() 
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
            _controller = new UserDeliveryController(_context, mockHubContext.Object, mockStatsService.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] 
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() {User = user}
            };
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
                    Password="password123"});

                _context.SaveChanges();
            }

            if (!_context.Deliveries.Any())
            {
                var delivery1 = new Delivery 
                {
                    Id = 1,
                    App = DeliveryApp.UberEats,
                    DeliveryTime = DateTime.Now,
                    BasePay = 3.0,
                    TipPay = 2.50,
                    TotalPay = 5.50,
                    Mileage = 1.2,
                    Restaurant = "Love Art Sushi",
                    CustomerNeighborhood= "Back Bay",
                    Notes = "test 1"
                };

                _context.Deliveries.Add(delivery1);
                _context.SaveChanges();

                _context.UserDeliveries.Add(new UserDelivery 
                {
                    UserId = 1,
                    DeliveryId = 1
                });

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
            var results = await _controller.GetDeliveries();
            var okResult = Assert.IsType<OkObjectResult>(results);
            var userDeliveries = Assert.IsAssignableFrom<IEnumerable<DeliveryDto>>(okResult.Value);

            var userDelivery = Assert.Single(userDeliveries);
            Assert.Equal("Love Art Sushi", userDelivery.Restaurant);
        }

        [Fact]
        public async Task AddDelivery_AddsTo_UserList()
        {
            var delivery2 = new Delivery
            {
                Id = 2,
                App = DeliveryApp.Doordash,
                DeliveryTime = DateTime.Now,
                BasePay = 2.43,
                TipPay = 2.00,
                TotalPay = 4.43,
                Mileage = 1.4,
                Restaurant = "Serafina",
                CustomerNeighborhood = "Fenway",
                Notes = "test2"
            };

            var result = await _controller.AddDelivery(delivery2);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var userDelivery = await _context.UserDeliveries
                .FirstOrDefaultAsync(ud => ud.UserId == 1 && ud.DeliveryId == delivery2.Id);

            Assert.NotNull(userDelivery);

            var badDelivery = new Delivery
            {
                Id = 3,
                App = DeliveryApp.UberEats,
                DeliveryTime = DateTime.Now.AddHours(1),
                BasePay = 3.0,
                TipPay = 4.0,
                Mileage = 1.3,
                Restaurant = "Test Restaurant 1",
                CustomerNeighborhood = "Allston",
                Notes = "Test 3"
            };

            var response = await _controller.AddDelivery(badDelivery);

            var badResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal("Delivery time cannot be in the future", badResult.Value);
        }

        [Fact]
        public async Task GetFilteredDeliveries_ReturnsFilteredDeliveries()
        {
            var delivery3 = new Delivery
            {
                Id = 3,
                App = DeliveryApp.Grubhub,
                DeliveryTime = DateTime.Now,
                BasePay = 3.50,
                TipPay = 2.50,
                TotalPay = 6.00,
                Mileage = 0.9,
                Restaurant = "YGF Malatang",
                CustomerNeighborhood= "Mission Hill",
                Notes = "test 13"
            };

            _context.Deliveries.Add(delivery3);
            _context.UserDeliveries.Add(new UserDelivery
            {
                UserId = 1,
                DeliveryId = delivery3.Id
            });
            await _context.SaveChangesAsync();

            var result = await _controller.GetDeliveriesByApp(app: DeliveryApp.Grubhub,
                basePay: null,
                tipPay: null,
                totalPay: null,
                mileage: null,
                restaurant: null,
                customerNeighborhood: null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var deliveries = Assert.IsAssignableFrom<IEnumerable<dynamic>>(okResult.Value);
            Assert.Contains(deliveries, d => (string)d.Restaurant == "YGF Malatang");
            Assert.DoesNotContain(deliveries, d => d.Id == 1);
            Assert.DoesNotContain(deliveries, d => (string)d.Restaurant == "Love Art Sushi");
        }

        [Fact]
        public async Task GetDeliveryById_ReturnsDelivery() {
            var result = await _controller.GetDeliveryById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var userDelivery = Assert.IsAssignableFrom<UserDelivery>(okResult.Value);
            Assert.NotNull(userDelivery.Delivery);
            Assert.Equal("Love Art Sushi", userDelivery.Delivery.Restaurant);

            var result2 = await _controller.GetDeliveryById(4);
            Assert.IsType<NotFoundObjectResult>(result2);
        }

        [Fact]
        public async Task DeleteDelivery_DeletesDelivery()
        {
            Assert.True(_context.Deliveries.Any(d => d.Id == 1));
            var result = await _controller.DeleteDelivery(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False(_context.Deliveries.Any());
        }

        [Fact]
        public async Task GetNeighborhoods_ReturnsNeighborhoods()
        {
            var delivery3 = new Delivery
            {
                Id = 3,
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

            var userDelivery = new UserDelivery
            {
                UserId = 1,
                DeliveryId = delivery3.Id
            };
            _context.UserDeliveries.Add(userDelivery);

            await _context.SaveChangesAsync();

            var result = await _controller.GetUserDeliveryNeighborhoods();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var neighborhoods = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);

            Assert.Contains("Mission Hill", neighborhoods);
            Assert.Contains("Back Bay", neighborhoods);
        }

        [Fact]
        public async Task GetApps_ReturnsApps()
        {
            var delivery4 = new Delivery
            {
                Id = 4,
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

            var userDelivery = new UserDelivery
            {
                UserId = 1,
                DeliveryId = delivery4.Id
            };
            _context.UserDeliveries.Add(userDelivery);

            await _context.SaveChangesAsync();

            var result = await _controller.GetUserDeliveryApps();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var apps = Assert.IsAssignableFrom<IEnumerable<DeliveryApp>>(okResult.Value);

            Assert.Contains(DeliveryApp.UberEats, apps);
            Assert.Contains(DeliveryApp.Doordash, apps);
        }

        [Fact]
        public async Task UpdateDelivery_PutsDelivery()
        {
            var delivery1 = new Delivery 
            {
                Id = 1,
                App = DeliveryApp.UberEats,
                DeliveryTime = DateTime.Now,
                BasePay = 5.0,
                TipPay = 2.50,
                TotalPay = 7.50,
                Mileage = 1.2,
                Restaurant = "Love Art Sushi",
                CustomerNeighborhood= "Back Bay",
                Notes = "test 1"
            };

            var result = await _controller.UpdateDelivery(delivery1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var delivery = Assert.IsAssignableFrom<DeliveryDto>(okResult.Value);

            Assert.Equal(5.0, delivery.BasePay);
            Assert.Equal(7.50, delivery.TotalPay);

            var badDelivery1 = new Delivery
            {
                Id = 1,
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

            var response = await _controller.UpdateDelivery(badDelivery1);

            var badResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal("Delivery time cannot be in the future", badResult.Value);
        }
    }
}