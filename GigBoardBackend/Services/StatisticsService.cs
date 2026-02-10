using System.Globalization;
using System.Net.Sockets;
using GigBoardBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace GigBoardBackend.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly ApplicationDbContext _context;

        public StatisticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object> CalculateDeliveryStatistics(int userId)
        {
            var deliveries = await _context.UserDeliveries
                .Where(ud => ud.UserId == userId && ud.Delivery != null)
                .Select(ud => ud.Delivery)
                .ToListAsync();

            // If no deliveries default to 0, empty list, or "N/A"
            if (!deliveries.Any())
            {
                return new
                {
                    avgPay = 0,
                    avgBase = 0,
                    avgTip = 0,
                    dollarPerMile = 0,
                    tipPerMile = 0,
                    highestPayingRestaurant = new {restaurant = "N/A", avgTotalPay = 0},
                    restaurantWithMost = new {restaurantWithMost = "N/A", orderCount = 0},
                    plotlyEarningsData = new {dates = new List<string>(), earnings = new List<double>()},
                    plotlyNeighborhoodsDaata = new {neighborhoods = new List<string>(), tipPays = new List<double>()},
                    appsByBaseData = new {apps = new List<string>(), basePays = new List<double>()},
                    tipsByAppData = new {tipApps = new List<string>(), tipPays = new List<double>()},
                    hourlyEarningsData = new {hours = new List<string>(), earnings = new List<string>()},
                    donutChartData = new {totalPay = 0, totalBasePay = 0, totalTipPay = 0}
                };
            }
            
            // Average total, base, and tip
            var avgPay = deliveries.Average(x => x!.TotalPay);
            var avgBase = deliveries.Average(x => x!.BasePay);
            var avgTip = deliveries.Average(x => x!.TipPay);

            // Highest paying restaurant with total
            var highestPayingRestaurant = deliveries
                .GroupBy(d => d!.Restaurant)
                .Select(g => new { Restaurant = g.Key, AvgTotalPay = g.Average(x => x!.TotalPay) })
                .OrderByDescending(x => x.AvgTotalPay)
                .FirstOrDefault();

            // Restaurant with most orders
            var restaurantWithMost = deliveries
                .GroupBy(d => d!.Restaurant)
                .Select(g => new
                {
                    RestaurantWithMost = g.Key,
                    OrderCount = g.Count()
                })
                .OrderByDescending(g => g.OrderCount)
                .FirstOrDefault();

            // Dollar-per-mile and tip-per-mile
            var miles = deliveries.Sum(x => x!.Mileage);
            var dollarsTotal = deliveries.Sum(x => x!.TotalPay);
            var dollarsTip = deliveries.Sum(x => x!.TipPay);

            var dollarPerMile = dollarsTotal / miles;
            var tipPerMile = dollarsTip / miles;

            // Plotly Earnings Data
            var plotlyEarnings = deliveries
                .GroupBy(d => d!.DeliveryTime.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalEarnings = g.Sum(x => x!.TotalPay)
                })
                .ToList();

            var dates = plotlyEarnings.Select(d => d.Date.ToString("yyyy-MM-dd")).ToList();
            var earnings = plotlyEarnings.Select(d => (double)d.TotalEarnings).ToList();

            var plotlyEarningsData = new {dates, earnings};

            // Plotly neighborhoods data
            var plotlyNeighborhoods = deliveries
            .GroupBy(d => d!.CustomerNeighborhood)
            .Select(g => new
            {
                Neighborhood = g.Key,
                AverageTipPay = g.Average(x => x!.TipPay)
            })
            .OrderBy(x => x.Neighborhood)
            .ToList();

            var neighborhoods = plotlyNeighborhoods.Select(d => d.Neighborhood.ToString()).ToList();
            var tipPays = plotlyNeighborhoods.Select(d => (double)d.AverageTipPay).ToList();

            var plotlyNeighborhoodsData = new {neighborhoods, tipPays};

            // Apps by base data
            var appsByBase = deliveries
            .GroupBy(d => d!.App)
            .Select(g => new
            {
                App = g.Key,
                AverageBasePay = g.Average(x => x!.BasePay)
            })
            .OrderBy(x => x.App)
            .ToList();

            var apps = appsByBase.Select(d => d.App.ToString()).ToList();
            var basePays = appsByBase.Select(d => (double)d.AverageBasePay).ToList();

            var appsByBaseData = new {apps, basePays};

            // Tips by App data
            var tipsByApp = deliveries
            .GroupBy(d => d!.App)
            .Select(g => new
            {
                App = g.Key,
                AverageTipPay = g.Average(x => x!.TipPay)
            })
            .OrderBy(x => x!.App)
            .ToList();

            var tipApps = tipsByApp.Select(d => d.App.ToString()).ToList();
            var appTipPays = tipsByApp.Select(d => (double)d.AverageTipPay).ToList();

            var tipsByAppData = new {tipApps, appTipPays};

            // Hourly earnings data
            var oneWeekAgo = DateTime.Now.AddDays(-7);

            var hourlyEarnings = deliveries
            .Where(d => d!.DeliveryTime >= oneWeekAgo)
            .Select(d => new
            {
                d!.DeliveryTime.Hour,
                Earnings = d.TotalPay
            })
            .GroupBy(x => x.Hour)
            .Select(g => new
            {
                Hour = g.Key,
                AverageEarnings = g.Average(x => x.Earnings)
            })
            .OrderBy(x => x.Hour)
            .ToList();

            var allHours = Enumerable.Range(0, 24);

            var earningsByHour = allHours
            .Select(h => new
            {
                Hour = h,
                AverageEarnings = hourlyEarnings.FirstOrDefault(x => x.Hour == h)?.AverageEarnings ?? 0
            })
            .ToList();

            var hoursStrings = earningsByHour.Select(x => x.Hour.ToString("D2")).ToList();
            var hourlyEarningsAll = earningsByHour.Select(x => x.AverageEarnings).ToList();

            var hourlyEarningsData = new {hours = hoursStrings, earnings = hourlyEarningsAll};

            // Donut chart data
            var totalPay = deliveries
            .Sum(d => d!.TotalPay);

            var totalBasePay = deliveries
            .Sum(d => d!.BasePay);

            var totalTipPay = deliveries
            .Sum(d => d!.TipPay);

            var donutChartData = new {totalPay, totalBasePay, totalTipPay};

            // Return all stats
            return new
            {
                avgPay,
                avgBase,
                avgTip,
                highestPayingRestaurant,
                restaurantWithMost,
                dollarPerMile,
                tipPerMile,
                plotlyEarningsData,
                plotlyNeighborhoodsData,
                appsByBaseData,
                tipsByAppData,
                hourlyEarningsData,
                donutChartData
            };
        }

        public async Task<object> CalculateShiftStatistics(int userId)
        {
            var shifts = await _context.UserShifts
                .Where(us => us.UserId == userId && us.Shift != null)
                .Select(us => us.Shift)
                .ToListAsync();

            var shiftDeliveries = await _context.ShiftDeliveries
                .Where(sd => sd.UserId == userId && sd.Delivery != null && sd.Shift != null)
                .Select(sd => new
                {
                    sd.Shift,
                    sd.Delivery
                })
                .ToListAsync();

            if (!shifts.Any())
            {
                return new
                {
                    averageShiftLength = 0,
                    appWithMostShifts = "N/A",
                    averageDeliveriesForShift = 0
                };
            }

            // Average shift length in minutes
            var durations = shifts
            .Select(s => s!.EndTime - s!.StartTime)
            .ToList();

            var averageShiftLength = durations.Average(x => x.TotalMinutes);

            // App with most shifts
            var appWithMost = shifts
            .GroupBy(s => s!.App)
            .Select(g => new
            {
                App = g.Key,
                ShiftCount = g.Count()
            })
            .OrderByDescending(g => g.ShiftCount)
            .FirstOrDefault();

            var appWithMostShifts = appWithMost == null ? "N/A" : appWithMost.App.ToString();

            // Average deliveries per shift
            var averageDeliveriesForShift = shifts.Count == 0 ? 0 : (double)shiftDeliveries.Count / shifts.Count;

            return new
            {
                averageShiftLength,
                appWithMostShifts,
                averageDeliveriesForShift
            };
        }

        public async Task<object> CalculateExpenseStatistics(int userId)
        {
            var expenses = await _context.UserExpenses
                .Where(ue => ue.UserId == userId && ue.Expense != null)
                .Select(ue => ue.Expense)
                .ToListAsync();

            if (!expenses.Any())
            {
                return new
                {
                    averageMonthlySpending = 0,
                    averageSpendingByType = new List<object>()
                };
            }

            // Average Monthly Spending
            var averageMonthlySpending = expenses
            .GroupBy(e => new {e!.Date.Year, e!.Date.Month})
            .Average(g => g.Sum(e => e!.Amount));

            // Average Monthly Spending By Type
            var totalMonths = expenses
            .Select(e => new {e!.Date.Year, e!.Date.Month})
            .Distinct()
            .Count();

            var averageSpendingByType = expenses
            .GroupBy(e => e!.Type)
            .Select(g => new
            {
                Type = g.Key,
                AvgExpense = totalMonths > 0 ? g.Sum(x => x!.Amount) / totalMonths : 0
            })
            .ToList();

            return new
            {
                averageMonthlySpending,
                averageSpendingByType
            };
        }
    }
}