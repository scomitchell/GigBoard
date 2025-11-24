using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Protected;
using System.Net;
using GigBoardBackend.Controllers;
using GigBoardBackend.Models;
using GigBoardBackend.Data;

namespace GigBoard.Tests.Controllers
{
    public class StatisticsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly StatisticsController _controller;

        public StatisticsControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            SeedDatabase();

            // Mock the HttpMessageHandler to control responses
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"image\":\"dummyBase64ImageString\"}")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            // Mock IHttpClientFactory to return the mocked HttpClient
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            _controller = new StatisticsController(_context, mockHttpClientFactory.Object);

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
                    DeliveryTime = DateTime.Today.AddHours(12),
                    BasePay = 3.0,
                    TipPay = 2.50,
                    TotalPay = 5.50,
                    Mileage = 1.2,
                    Restaurant = "Love Art Sushi",
                    CustomerNeighborhood= "Back Bay",
                    Notes = "test 1"
                };

                var delivery2 = new Delivery
                {
                    Id = 2,
                    App = DeliveryApp.Doordash,
                    DeliveryTime = DateTime.Today.AddHours(17),
                    BasePay = 4.50,
                    TipPay = 2.00,
                    TotalPay = 6.50,
                    Mileage = 1.5,
                    Restaurant = "Wendy's",
                    CustomerNeighborhood = "Mission Hill",
                    Notes = "test 2"
                };

                _context.Deliveries.AddRange(delivery1, delivery2);
                _context.SaveChanges();

                _context.UserDeliveries.Add(new UserDelivery 
                {
                    UserId = 1,
                    DeliveryId = 1
                });

                _context.UserDeliveries.Add(new UserDelivery
                {
                    UserId = 1,
                    DeliveryId = 2
                });

                _context.SaveChanges();
            }

            if (!_context.Expenses.Any())
            {
                var expense1 = new Expense
                {
                    Id = 1,
                    Amount = 100,
                    Date = DateTime.Now,
                    Type = "Gas",
                    Notes = "Test1"
                };

                var expense2 = new Expense
                {
                    Id = 2,
                    Amount = 40,
                    Date = DateTime.Now,
                    Type = "Car Maintenance",
                    Notes = "Test2"
                };

                _context.Expenses.AddRange(expense1, expense2);
                _context.SaveChanges();

                _context.UserExpenses.Add(new UserExpense
                {
                    UserId = 1,
                    ExpenseId = 1
                });

                _context.UserExpenses.Add(new UserExpense
                {
                    UserId = 1,
                    ExpenseId = 2
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

                var shift2 = new Shift
                {
                    Id = 2,
                    StartTime = DateTime.Now.AddHours(-2),
                    EndTime = DateTime.Now,
                    App = DeliveryApp.Doordash
                };

                var shift3 = new Shift
                {
                    Id = 3,
                    StartTime = DateTime.Now.AddHours(-1),
                    EndTime = DateTime.Now,
                    App = DeliveryApp.UberEats
                };

                _context.Shifts.AddRange(shift1, shift2, shift3);
                _context.SaveChanges();

                _context.UserShifts.Add(new UserShift
                {
                    UserId = 1,
                    ShiftId = 1
                });

                _context.UserShifts.Add(new UserShift
                {
                    UserId = 1,
                    ShiftId = 2
                });

                _context.UserShifts.Add(new UserShift
                {
                    UserId = 1,
                    ShiftId = 3
                });

                _context.ShiftDeliveries.Add(new ShiftDelivery
                {
                    UserId = 1,
                    ShiftId = 1,
                    DeliveryId = 1
                });

                _context.ShiftDeliveries.Add(new ShiftDelivery
                {
                    UserId = 1,
                    ShiftId = 2,
                    DeliveryId = 2
                });

                _context.SaveChanges();
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetAveragePay_CalculatesPay()
        {
            var result = await _controller.GetAvgDeliveryPay();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var avgPay = Assert.IsAssignableFrom<double>(okResult.Value);

            Assert.Equal(6, avgPay);
        }

        [Fact]
        public async Task GetAverageBase_CalculatesBasePay()
        {
            var result = await _controller.GetAvgBasePay();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var avgBase = Assert.IsAssignableFrom<double>(okResult.Value);

            Assert.Equal(3.75, avgBase);
        }

        [Fact]
        public async Task GetAverageTip_CalculatesTipPay()
        {
            var result = await _controller.GetAvgTipPay();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var avgTip = Assert.IsAssignableFrom<double>(okResult.Value);

            Assert.Equal(2.25, avgTip);
        }

        [Fact]
        public async Task GetBestNeighborhood_ReturnsNeighborhood()
        {
            var result = await _controller.GetHighestPayingNeighborhood();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var value = okResult.Value;
            Assert.NotNull(value);
            var dict = value.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(value));

            Assert.Equal("Back Bay", dict["neighborhood"] as string);
            Assert.Equal(2.50, dict["averageTipPay"]);
        }

        [Fact]
        public async Task GetBestRestaurant_ReturnsRestaurant()
        {
            var result = await _controller.GetHighestPayingRestaurant();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var value = okResult.Value;
            Assert.NotNull(value);
            var dict = value.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(value));

            Assert.Equal("Wendy's", dict["restaurant"] as string);
            Assert.Equal(6.50, dict["avgTotalPay"]);
        }

        [Fact]
        public async Task GetBestBaseApp_ReturnsApp()
        {
            var result = await _controller.GetHighestPayingBaseApp();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var value = okResult.Value;
            Assert.NotNull(value);
            var dict = value.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(value));

            Assert.Equal(DeliveryApp.Doordash, dict["app"]);
            Assert.Equal(4.50, dict["avgBase"]);
        }

        [Fact]
        public async Task GetBestTipApp_ReturnsApp()
        {
            var result = await _controller.GetHighestPayingTipApp();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var value = okResult.Value;
            Assert.NotNull(value);
            var dict = value.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(value));

            Assert.Equal(DeliveryApp.UberEats, dict["app"]);
            Assert.Equal(2.50, dict["avgTip"]);
        }

        [Fact]
        public async Task GetDollarPerMile_ReturnsAverage()
        {
            var result = await _controller.GetDollarPerMile();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var dollarPerMile = Assert.IsAssignableFrom<double>(okResult.Value);

            Assert.Equal(4.44, dollarPerMile, precision: 2);
        }

        [Fact]
        public async Task GetRestaurant_WithMostDeliveries_ReturnsDeliveries()
        {
            var delivery3 = new Delivery
            {
                Id = 3,
                App = DeliveryApp.Grubhub,
                DeliveryTime = DateTime.Now,
                BasePay = 3.2,
                TipPay = 2.70,
                TotalPay = 5.90,
                Mileage = 1.2,
                Restaurant = "Love Art Sushi",
                CustomerNeighborhood= "Fenway",
                Notes = "test 1"
            };
            _context.Deliveries.Add(delivery3);

            _context.UserDeliveries.Add(new UserDelivery
            {
                UserId = 1,
                DeliveryId = 3
            });
            await _context.SaveChangesAsync();

            var result = await _controller.GetRestaurantWithMostDeliveries();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            Assert.NotNull(value);
            var dict = value.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(value));

            Assert.Equal("Love Art Sushi", dict["restaurantWithMost"] as string);
            Assert.Equal(2, dict["orderCount"]);
        }

        [Fact]
        public async Task GetTipPerMile_ReturnsTipRate()
        {
            var result = await _controller.GetTipPerMile();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var tipPerMile = Assert.IsAssignableFrom<double>(okResult.Value);

            Assert.Equal(1.67, tipPerMile, precision: 2);
        }

        [Fact]
        public async Task GetMonthlySpending_ReturnsSpending()
        {
            var result = await _controller.GetAverageMonthlySpending();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var avgMonthlySpending = Assert.IsAssignableFrom<double>(okResult.Value);

            Assert.Equal(140, avgMonthlySpending);
        }

        [Fact]
        public async Task GetSpendingByType_ReturnsSpending()
        {
            var result = await _controller.GetAverageSpendingByType();

            var okResult = Assert.IsType<OkObjectResult>(result);
            
            var value = okResult.Value;
            var resultList = value as IEnumerable<object>;
            Assert.NotNull(resultList);

            var dictList = resultList.Select(item => 
            {
                var props = item.GetType().GetProperties();
                return props.ToDictionary(p => p.Name, p => p.GetValue(item));
            }).ToList();

            var gas = dictList.FirstOrDefault(d => d["Type"] as string == "Gas");
            var maintenance = dictList.FirstOrDefault(d => d["Type"] as string == "Car Maintenance");

            Assert.NotNull(gas);
            Assert.NotNull(maintenance);

            if (gas["AvgExpense"] is double gasExpense &&
                maintenance["AvgExpense"] is double maintenanceExpense)
            {
                Assert.Equal(100.00, gasExpense, precision: 2);
                Assert.Equal(40.00, maintenanceExpense, precision: 2);
            }
            else
            {
                Assert.Fail("AvgExpense was null or not a double");
            }
        }

        [Fact]
        public async Task GetAvgShiftLength_CalculatesShiftLength()
        {
            var result = await _controller.getAverageShiftLength();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var avgShift = Assert.IsAssignableFrom<double>(okResult.Value);

            Assert.Equal(80, avgShift, precision: 0);
        }

        [Fact]
        public async Task GetAppWithMostShifts_ReturnsApp()
        {
            var result = await _controller.getAppWithMostShifts();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var app = Assert.IsAssignableFrom<DeliveryApp>(okResult.Value);

            Assert.Equal(DeliveryApp.UberEats, app);
        }

        [Fact]
        public async Task GetAverageDeliveries_ReturnsAvgNum()
        {
            var result = await _controller.GetAverageDeliveriesPerShift();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var average = Assert.IsAssignableFrom<double>(okResult.Value);

            Assert.Equal(1, average, precision: 0);
        }

        [Fact]
        public async Task PlotlyEarningsData_ReturnsData()
        {
            // Add test delivery
            var delivery3 = new Delivery
            {
                Id = 3,
                App = DeliveryApp.UberEats,
                DeliveryTime = new DateTime(2025, 1, 1, 12, 0, 0),
                BasePay = 3.50,
                TipPay = 4.00,
                TotalPay = 7.50,
                Mileage = 1.2,
                Restaurant = "Back Bay Social",
                CustomerNeighborhood = "Fenway",
                Notes = "test 3"
            };
            _context.Deliveries.Add(delivery3);

            var userDelivery3 = new UserDelivery
            {
                UserId = 1,
                DeliveryId = 3
            };
            _context.UserDeliveries.Add(userDelivery3);
            await _context.SaveChangesAsync();

            // Results
            var result = await _controller.PlotlyEarningsData();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            Assert.NotNull(value);

            var dict = value.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(value));
            Assert.True(dict.ContainsKey("dates"));
            Assert.True(dict.ContainsKey("earnings"));

            var dates = Assert.IsAssignableFrom<IEnumerable<object>>(dict["dates"]);
            var earnings = Assert.IsAssignableFrom<List<double>>(dict["earnings"]);
            Assert.NotEmpty(dates);
            Assert.NotEmpty(earnings);

            Assert.Contains("2025-01-01", dates);
            Assert.Contains(7.50, earnings);
        }

        [Fact]
        public async Task PlotlyNeighborhoodsData_ReturnsData()
        {
            // Add delivery
            var delivery3 = new Delivery
            {
                Id = 3,
                App = DeliveryApp.UberEats,
                DeliveryTime = new DateTime(2025, 1, 1, 12, 0, 0),
                BasePay = 3.50,
                TipPay = 4.00,
                TotalPay = 7.50,
                Mileage = 1.5,
                Restaurant = "Back Bay Social",
                CustomerNeighborhood = "Roxbury",
                Notes = "test 3"
            };
            _context.Deliveries.Add(delivery3);

            var userDelivery3 = new UserDelivery
            {
                UserId = 1,
                DeliveryId = 3
            };
            _context.UserDeliveries.Add(userDelivery3);
            await _context.SaveChangesAsync();

            var result = await _controller.GetPlotlyNeighborhoodsData();

            var okResult = Assert.IsAssignableFrom<OkObjectResult>(result);
            var value = okResult.Value;
            Assert.NotNull(value);

            var dict = value.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(value));
            Assert.True(dict.ContainsKey("neighborhoods"));
            Assert.True(dict.ContainsKey("tipPays"));

            var neighborhoods = Assert.IsAssignableFrom<List<string>>(dict["neighborhoods"]);
            var tipPays = Assert.IsAssignableFrom<List<double>>(dict["tipPays"]);
            Assert.NotEmpty(neighborhoods);
            Assert.NotEmpty(tipPays);

            Assert.Contains("Roxbury", neighborhoods);
            Assert.Contains("Back Bay", neighborhoods);
            Assert.Contains(4.00, tipPays);
            Assert.Contains(2.50, tipPays);
        }

        [Fact]
        public async Task PlotlyAppsByBase_ReturnsData()
        {
            // Add delivery
            var delivery3 = new Delivery
            {
                Id = 3,
                App = DeliveryApp.Grubhub,
                DeliveryTime = new DateTime(2025, 1, 1, 12, 0, 0),
                BasePay = 3.50,
                TipPay = 4.00,
                TotalPay = 7.50,
                Mileage = 1.5,
                Restaurant = "Back Bay Social",
                CustomerNeighborhood = "Roxbury",
                Notes = "test 3"
            };
            _context.Deliveries.Add(delivery3);

            var userDelivery3 = new UserDelivery
            {
                UserId = 1,
                DeliveryId = 3
            };
            _context.UserDeliveries.Add(userDelivery3);
            await _context.SaveChangesAsync();

            var result = await _controller.GetPlotlyAppsByBase();

            var okResult = Assert.IsAssignableFrom<OkObjectResult>(result);
            var value = okResult.Value;
            Assert.NotNull(value);

            var dict = value.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(value));
            Assert.True(dict.ContainsKey("apps"));
            Assert.True(dict.ContainsKey("basePays"));

            var apps = Assert.IsAssignableFrom<List<string>>(dict["apps"]);
            var basePays = Assert.IsAssignableFrom<List<double>>(dict["basePays"]);
            Assert.NotEmpty(apps);
            Assert.NotEmpty(basePays);

            Assert.Contains("Grubhub", apps);
            Assert.Contains("UberEats", apps);
            Assert.Contains(3.50, basePays);
            Assert.Contains(3.00, basePays);
        }

        [Fact]
        public async Task PlotlyHourlyEarnings_ReturnsHourlyEarnings()
        {
            var result = await _controller.GetPlotlyHourlyEarnings();

            var okResult = Assert.IsAssignableFrom<OkObjectResult>(result);
            var value = okResult.Value;
            Assert.NotNull(value);

            var dict = value.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(value));
            Assert.True(dict.ContainsKey("hours"));
            Assert.True(dict.ContainsKey("earnings"));

            var hoursEnumerable = dict["hours"] as IEnumerable<string>;
            Assert.NotNull(hoursEnumerable);
            var hoursList = hoursEnumerable.ToList();

            var earningsEnumerable = dict["earnings"] as IEnumerable<double>;
            Assert.NotNull(earningsEnumerable);
            var earningsList = earningsEnumerable.ToList();

            int indexTwelve = hoursList.IndexOf("12");
            Assert.True(indexTwelve >= 0, "Hour '12' is not present in hours");
            Assert.Equal(5.50, earningsList[indexTwelve], 2);

            int indexSeventeen = hoursList.IndexOf("17");
            Assert.True(indexSeventeen >= 0, "Hour '17' is not present in hours");
            Assert.Equal(6.50, earningsList[indexSeventeen], 2);
        }

        [Fact]
        public async Task GetDonutChartData_ReturnsData()
        {
            var result = await _controller.GetDataForDonutChart();

            var okResult = Assert.IsAssignableFrom<OkObjectResult>(result);
            var value = okResult.Value;
            Assert.NotNull(value);

            var dict = value.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(value));
            Assert.True(dict.ContainsKey("totalPay"));
            Assert.True(dict.ContainsKey("totalBasePay"));
            Assert.True(dict.ContainsKey("totalTipPay"));

            var totalPay = Assert.IsAssignableFrom<double>(dict["totalPay"]);
            var totalBasePay = Assert.IsAssignableFrom<double>(dict["totalBasePay"]);
            var totalTipPay = Assert.IsAssignableFrom<double>(dict["totalTipPay"]);

            Assert.Equal(12, totalPay, 2);
            Assert.Equal(7.50, totalBasePay, 2);
            Assert.Equal(4.50, totalTipPay, 2);
        }

        [Fact]
        public async Task GetTipsByApp_ReturnsData()
        {
            // Add delivery
            var delivery3 = new Delivery
            {
                Id = 3,
                App = DeliveryApp.Doordash,
                DeliveryTime = new DateTime(2025, 1, 1, 12, 0, 0),
                BasePay = 3.50,
                TipPay = 4.00,
                TotalPay = 7.50,
                Mileage = 1.5,
                Restaurant = "Back Bay Social",
                CustomerNeighborhood = "Roxbury",
                Notes = "test 3"
            };
            _context.Deliveries.Add(delivery3);

            var userDelivery3 = new UserDelivery
            {
                UserId = 1,
                DeliveryId = 3
            };
            _context.UserDeliveries.Add(userDelivery3);
            await _context.SaveChangesAsync();

            var result = await _controller.GetTipsByAppData();

            var okResult = Assert.IsAssignableFrom<OkObjectResult>(result);
            var value = okResult.Value;
            Assert.NotNull(value);

            var dict = value.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(value));
            Assert.True(dict.ContainsKey("tipApps"));
            Assert.True(dict.ContainsKey("appTipPays"));

            var apps = Assert.IsAssignableFrom<List<string>>(dict["tipApps"]);
            var tipPays = Assert.IsAssignableFrom<List<double>>(dict["appTipPays"]);
            Assert.NotEmpty(apps);
            Assert.NotEmpty(tipPays);

            Assert.Contains("UberEats", apps);
            Assert.Contains("Doordash", apps);
            Assert.Contains(2.50, tipPays);
            Assert.Contains(3.00, tipPays);
        }
    }
}