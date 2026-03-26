using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GigBoardBackend.Models;
using GigBoardBackend.Data;
using GigBoardBackend.Services;

namespace GigBoard.Tests.Services
{
    public class StatisticsServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly StatisticsService _service;

        public StatisticsServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            SeedDatabase();

            _service = new StatisticsService(_context);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));
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
                    ShiftId = 1,
                    App = DeliveryApp.UberEats,
                    DeliveryTime = DateTime.Today.AddHours(12),
                    BasePay = 3.0,
                    TipPay = 2.50,
                    TotalPay = 5.50,
                    Mileage = 1.2,
                    Restaurant = "Love Art Sushi",
                    CustomerNeighborhood = "Back Bay",
                    Notes = "test 1"
                };

                var delivery2 = new Delivery
                {
                    Id = 2,
                    UserId = 1,
                    ShiftId = 2,
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
            }

            if (!_context.Expenses.Any())
            {
                var expense1 = new Expense
                {
                    Id = 1,
                    UserId = 1,
                    Amount = 100,
                    Date = DateTime.Now,
                    Type = "Gas",
                    Notes = "Test1"
                };

                var expense2 = new Expense
                {
                    Id = 2,
                    UserId = 1,
                    Amount = 40,
                    Date = DateTime.Now,
                    Type = "Car Maintenance",
                    Notes = "Test2"
                };

                _context.Expenses.AddRange(expense1, expense2);
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

                var shift2 = new Shift
                {
                    Id = 2,
                    UserId = 1,
                    StartTime = DateTime.Now.AddHours(-2),
                    EndTime = DateTime.Now,
                    App = DeliveryApp.Doordash
                };

                var shift3 = new Shift
                {
                    Id = 3,
                    UserId = 1,
                    StartTime = DateTime.Now.AddHours(-1),
                    EndTime = DateTime.Now,
                    App = DeliveryApp.UberEats
                };

                _context.Shifts.AddRange(shift1, shift2, shift3);
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
            var result = await _service.CalculateDeliveryStatistics(1);

            Assert.IsType<DeliveryStatisticsDto>(result);
            var stats = result;

            Assert.Equal(6, stats.AvgPay);
            Assert.Equal(3.75, stats.AvgBase);
            Assert.Equal(2.25, stats.AvgTip);
            Assert.Equal(4.44, stats.DollarPerMile, precision: 2);
            Assert.Equal(1.67, stats.TipPerMile, precision: 2);

            var highestPayingRestaurant = stats.HighestPayingRestaurant;
            Assert.NotNull(highestPayingRestaurant);
            var dict = highestPayingRestaurant.GetType().GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(highestPayingRestaurant));

            Assert.Equal("Wendy's", dict["Restaurant"] as string);
            Assert.Equal(6.50, dict["AvgTotalPay"]);
        }

        [Fact]
        public async Task GetRestaurantWithMost_IdentifiesRestaurant()
        {
            var delivery3 = new Delivery
            {
                Id = 3,
                UserId = 1,
                App = DeliveryApp.UberEats,
                DeliveryTime = new DateTime(2025, 1, 1, 12, 0, 0),
                BasePay = 3.50,
                TipPay = 4.00,
                TotalPay = 7.50,
                Mileage = 1.5,
                Restaurant = "Love Art Sushi",
                CustomerNeighborhood = "Roxbury",
                Notes = "test 3"
            };

            _context.Deliveries.Add(delivery3);
            await _context.SaveChangesAsync();

            var result = await _service.CalculateDeliveryStatistics(1);

            Assert.IsType<DeliveryStatisticsDto>(result);
            var stats = result;

            var restaurantWithMost = stats.RestaurantWithMost;
            Assert.NotNull(restaurantWithMost);
            var restDict = restaurantWithMost.GetType().GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(restaurantWithMost));

            Assert.Equal("Love Art Sushi", restDict["RestaurantWithMost"] as string);
            Assert.Equal(2, restDict["OrderCount"]);
        }

        [Fact]
        public async Task GetMonthlySpending_ReturnsSpending()
        {
            var result = await _service.CalculateExpenseStatistics(1);

            Assert.IsType<ExpenseStatisticsDto>(result);
            var stats = result;

            Assert.Equal(140, stats.AverageMonthlySpending);
        }

        [Fact]
        public async Task GetSpendingByType_ReturnsSpending()
        {
            var result = await _service.CalculateExpenseStatistics(1);

            Assert.IsType<ExpenseStatisticsDto>(result);
            var stats = result;
            var value = stats.AverageSpendingByType;
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
        public async Task GetAppWithMostShifts_ReturnsApp()
        {
            var result = await _service.CalculateShiftStatistics(1);

            Assert.IsType<ShiftStatisticsDto>(result);
            var stats = result;

            Assert.Equal("UberEats", stats.AppWithMostShifts);
            Assert.Equal(80, stats.AverageShiftLength, precision: 0);
            Assert.Equal(1, stats.AverageDeliveriesForShift, precision: 0);
        }

        [Fact]
        public async Task PlotlyEarningsData_ReturnsData()
        {
            // Add test delivery
            var delivery3 = new Delivery
            {
                Id = 3,
                UserId = 1,
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
            await _context.SaveChangesAsync();

            // Results
            var result = await _service.CalculateDeliveryStatistics(1);

            Assert.IsType<DeliveryStatisticsDto>(result);
            var stats = result;
            var value = stats.PlotlyEarningsData;
            Assert.NotNull(value);

            var dict = value.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(value));
            Assert.True(dict.ContainsKey("Dates"));
            Assert.True(dict.ContainsKey("Earnings"));

            var dates = dict["Dates"] as IEnumerable<string>;
            var earnings = dict["Earnings"] as List<double>;
            Assert.NotNull(dates);
            Assert.NotNull(earnings);
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
                UserId = 1,
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
            await _context.SaveChangesAsync();

            var result = await _service.CalculateDeliveryStatistics(1);

            Assert.IsType<DeliveryStatisticsDto>(result);
            var stats = result;
            var value = stats.PlotlyNeighborhoodsData;
            Assert.NotNull(value);

            var dict = value.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(value));
            Assert.True(dict.ContainsKey("Neighborhoods"));
            Assert.True(dict.ContainsKey("TipPays"));

            var neighborhoods = dict["Neighborhoods"] as List<string>;
            var tipPays = dict["TipPays"] as List<double>;
            Assert.NotNull(neighborhoods);
            Assert.NotNull(tipPays);
            Assert.NotEmpty(neighborhoods!);
            Assert.NotEmpty(tipPays!);

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
                UserId = 1,
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
            await _context.SaveChangesAsync();

            var result = await _service.CalculateDeliveryStatistics(1);

            Assert.IsType<DeliveryStatisticsDto>(result);
            var stats = result;

            var value = stats.AppsByBaseData;
            Assert.NotNull(value);

            var dict = value.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(value));
            Assert.True(dict.ContainsKey("Apps"));
            Assert.True(dict.ContainsKey("BasePays"));

            var apps = dict["Apps"] as List<string>;
            var basePays = dict["BasePays"] as List<double>;
            Assert.NotNull(apps);
            Assert.NotNull(basePays);
            Assert.NotEmpty(apps!);
            Assert.NotEmpty(basePays!);

            Assert.Contains("Grubhub", apps);
            Assert.Contains("UberEats", apps);
            Assert.Contains(3.50, basePays);
            Assert.Contains(3.00, basePays);
        }

        [Fact]
        public async Task PlotlyHourlyEarnings_ReturnsHourlyEarnings()
        {
            var result = await _service.CalculateDeliveryStatistics(1);

            Assert.IsType<DeliveryStatisticsDto>(result);
            var stats = result;

            var value = stats.HourlyEarningsData;
            Assert.NotNull(value);

            var dict = value.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(value));
            Assert.True(dict.ContainsKey("Hours"));
            Assert.True(dict.ContainsKey("Earnings"));

            var hoursEnumerable = dict["Hours"] as IEnumerable<string>;
            Assert.NotNull(hoursEnumerable);
            var hoursList = hoursEnumerable.ToList();

            var earningsEnumerable = dict["Earnings"] as IEnumerable<double>;
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
            var result = await _service.CalculateDeliveryStatistics(1);

            Assert.IsType<DeliveryStatisticsDto>(result);
            var stats = result;

            var value = stats.DonutChartData;
            Assert.NotNull(value);

            var dict = value.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(value));
            Assert.True(dict.ContainsKey("TotalPay"));
            Assert.True(dict.ContainsKey("TotalBasePay"));
            Assert.True(dict.ContainsKey("TotalTipPay"));

            var totalPay = Convert.ToDouble(dict["TotalPay"]);
            var totalBasePay = Convert.ToDouble(dict["TotalBasePay"]);
            var totalTipPay = Convert.ToDouble(dict["TotalTipPay"]);

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
                UserId = 1,
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
            await _context.SaveChangesAsync();

            var result = await _service.CalculateDeliveryStatistics(1);

            Assert.IsType<DeliveryStatisticsDto>(result);
            var stats = result;

            var value = stats.TipsByAppData;
            Assert.NotNull(value);

            var dict = value.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(value));
            Assert.True(dict.ContainsKey("TipApps"));
            Assert.True(dict.ContainsKey("TipPays"));

            var apps = dict["TipApps"] as List<string>;
            var tipPays = dict["TipPays"] as List<double>;
            Assert.NotNull(apps);
            Assert.NotNull(tipPays);
            Assert.NotEmpty(apps!);
            Assert.NotEmpty(tipPays!);

            Assert.Contains("UberEats", apps);
            Assert.Contains("Doordash", apps);
            Assert.Contains(2.50, tipPays);
            Assert.Contains(3.00, tipPays);
        }
    }
}