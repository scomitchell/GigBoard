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
            var query = _context.Deliveries.Where(d => d.UserId == userId);

            if (!await query.AnyAsync())
            {
                return GetDefaultDeliveryStats();
            }

            // Basic Averages & Totals
            var totals = await query
                .GroupBy(d => d.UserId)
                .Select(g => new
                {
                    AvgPay = g.Average(x => x.TotalPay),
                    AvgBase = g.Average(x => x.BasePay),
                    AvgTip = g.Average(x => x.TipPay),
                    SumTotal = g.Sum(x => x.TotalPay),
                    SumBase = g.Sum(x => x.BasePay),
                    SumTip = g.Sum(x => x.TipPay),
                    SumMiles = g.Sum(x => x.Mileage)
                }).FirstOrDefaultAsync();

            // Restaurant Stats
            var highestRestaurant = await query
                .GroupBy(d => d.Restaurant)
                .Select(g => new { Name = g.Key, Avg = g.Average(x => x.TotalPay) })
                .OrderByDescending(x => x.Avg)
                .FirstOrDefaultAsync();

            var mostRestaurant = await query
                .GroupBy(d => d.Restaurant)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            // Chart Data: Plotly Earnings
            var plotlyEarnings = await query
                .GroupBy(d => d.DeliveryTime.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { Date = g.Key, Total = g.Sum(x => x.TotalPay) })
                .ToListAsync();

            // Chart Data: Neighborhoods
            var neighborhoodData = await query
                .GroupBy(d => d.CustomerNeighborhood)
                .Select(g => new { Name = g.Key, AvgTip = g.Average(x => x.TipPay) })
                .OrderBy(x => x.Name)
                .ToListAsync();

            // Chart Data: Apps Base/Tip
            var appData = await query
                .GroupBy(d => d.App)
                .Select(g => new
                {
                    App = g.Key.ToString(),
                    AvgBase = g.Average(x => x.BasePay),
                    AvgTip = g.Average(x => x.TipPay)
                })
                .ToListAsync();

            // Hourly (Last 7 Days)
            var oneWeekAgo = DateTime.Now.AddDays(-7);
            var hourlyRaw = await query
                .Where(d => d.DeliveryTime >= oneWeekAgo)
                .Select(d => new { d.DeliveryTime.Hour, d.TotalPay })
                .ToListAsync();

            var hourlyProcessed = Enumerable.Range(0, 24).Select(h => new
            {
                Hour = h.ToString("D2"),
                Avg = hourlyRaw.Where(x => x.Hour == h).Select(x => x.TotalPay).DefaultIfEmpty(0).Average()
            }).ToList();

            return new
            {
                avgPay = totals?.AvgPay ?? 0,
                avgBase = totals?.AvgBase ?? 0,
                avgTip = totals?.AvgTip ?? 0,
                dollarPerMile = totals?.SumMiles > 0 ? totals.SumTotal / totals.SumMiles : 0,
                tipPerMile = totals?.SumMiles > 0 ? totals.SumTip / totals.SumMiles : 0,
                highestPayingRestaurant = new
                {
                    restaurant = highestRestaurant?.Name ?? "N/A",
                    avgTotalPay = highestRestaurant?.Avg ?? 0
                },
                restaurantWithMost = new
                {
                    restaurantWithMost = mostRestaurant?.Name ?? "N/A",
                    orderCount = mostRestaurant?.Count ?? 0
                },
                plotlyEarningsData = new
                {
                    dates = plotlyEarnings.Select(x => x.Date.ToString("yyyy-MM-dd")),
                    earnings = plotlyEarnings.Select(x => x.Total)
                },
                plotlyNeighborhoodsData = new
                {
                    neighborhoods = neighborhoodData.Select(x => x.Name),
                    tipPays = neighborhoodData.Select(x => x.AvgTip)
                },
                appsByBaseData = new
                {
                    apps = appData.Select(x => x.App),
                    basePays = appData.Select(x => x.AvgBase)
                },
                tipsByAppData = new
                {
                    tipApps = appData.Select(x => x.App),
                    tipPays = appData.Select(x => x.AvgTip)
                },
                hourlyEarningsData = new
                {
                    hours = hourlyProcessed.Select(x => x.Hour),
                    earnings = hourlyProcessed.Select(x => x.Avg)
                },
                donutChartData = new
                {
                    totalPay = totals?.SumTotal ?? 0,
                    totalBasePay = totals?.SumBase ?? 0,
                    totalTipPay = totals?.SumTip ?? 0
                }
            };
        }

        public async Task<object> CalculateShiftStatistics(int userId)
        {
            var shiftsQuery = _context.Shifts.Where(s => s.UserId == userId);

            if (!await shiftsQuery.AnyAsync())
            {
                return new { averageShiftLength = 0, appWithMostShifts = "N/A", averageDeliveriesForShift = 0 };
            }

            var shifts = await shiftsQuery.ToListAsync();
            var deliveryCount = await _context.Deliveries.CountAsync(d => d.UserId == userId && d.ShiftId != null);

            var avgLength = shifts.Average(s => (s.EndTime - s.StartTime).TotalMinutes);

            var appWithMost = shifts
                .GroupBy(s => s.App)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key.ToString())
                .FirstOrDefault() ?? "N/A";

            return new
            {
                averageShiftLength = avgLength,
                appWithMostShifts = appWithMost,
                averageDeliveriesForShift = (double)deliveryCount / shifts.Count
            };
        }

        public async Task<object> CalculateExpenseStatistics(int userId)
        {
            var expenseQuery = _context.Expenses.Where(e => e.UserId == userId);

            if (!await expenseQuery.AnyAsync())
            {
                return new { averageMonthlySpending = 0, averageSpendingByType = new List<object>() };
            }

            var expenses = await expenseQuery.ToListAsync();

            var monthlyAvg = expenses
                .GroupBy(e => new { e.Date.Year, e.Date.Month })
                .Average(g => g.Sum(e => e.Amount));

            var totalMonths = expenses.Select(e => new { e.Date.Year, e.Date.Month }).Distinct().Count();

            var spendingByType = expenses
                .GroupBy(e => e.Type)
                .Select(g => new
                {
                    Type = g.Key,
                    AvgExpense = totalMonths > 0 ? g.Sum(x => x.Amount) / totalMonths : 0
                }).ToList();

            return new { averageMonthlySpending = monthlyAvg, averageSpendingByType = spendingByType };
        }

        private object GetDefaultDeliveryStats()
        {
            return new
            {
                avgPay = 0,
                avgBase = 0,
                avgTip = 0,
                dollarPerMile = 0,
                tipPerMile = 0,
                highestPayingRestaurant = new { restaurant = "N/A", avgTotalPay = 0 },
                restaurantWithMost = new { restaurantWithMost = "N/A", orderCount = 0 },
                plotlyEarningsData = new { dates = new List<string>(), earnings = new List<double>() },
                plotlyNeighborhoodsData = new { neighborhoods = new List<string>(), tipPays = new List<double>() },
                appsByBaseData = new { apps = new List<string>(), basePays = new List<double>() },
                tipsByAppData = new { tipApps = new List<string>(), tipPays = new List<double>() },
                hourlyEarningsData = new { hours = new List<string>(), earnings = new List<double>() },
                donutChartData = new { totalPay = 0, totalBasePay = 0, totalTipPay = 0 }
            };
        }
    }
}