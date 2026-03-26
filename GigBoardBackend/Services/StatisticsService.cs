using GigBoardBackend.Data;
using GigBoardBackend.Models;
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

        public async Task<DeliveryStatisticsDto> CalculateDeliveryStatistics(int userId)
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

            return new DeliveryStatisticsDto
            {
                AvgPay = totals?.AvgPay ?? 0,
                AvgBase = totals?.AvgBase ?? 0,
                AvgTip = totals?.AvgTip ?? 0,
                DollarPerMile = totals?.SumMiles > 0 ? totals.SumTotal / totals.SumMiles : 0,
                TipPerMile = totals?.SumMiles > 0 ? totals.SumTip / totals.SumMiles : 0,
                HighestPayingRestaurant = new HighestPayingRestaurantDto
                {
                    Restaurant = highestRestaurant?.Name ?? "N/A",
                    AvgTotalPay = highestRestaurant?.Avg ?? 0
                },
                RestaurantWithMost = new RestaurantWithMostDto
                {
                    RestaurantWithMost = mostRestaurant?.Name ?? "N/A",
                    OrderCount = mostRestaurant?.Count ?? 0
                },
                PlotlyEarningsData = new PlotlyEarningsDataDto
                {
                    Dates = plotlyEarnings.Select(x => x.Date.ToString("yyyy-MM-dd")).ToList(),
                    Earnings = plotlyEarnings.Select(x => x.Total).ToList()
                },
                PlotlyNeighborhoodsData = new PlotlyNeighborhoodsDataDto
                {
                    Neighborhoods = neighborhoodData.Select(x => x.Name).ToList(),
                    TipPays = neighborhoodData.Select(x => x.AvgTip).ToList()
                },
                AppsByBaseData = new AppsByBaseDataDto
                {
                    Apps = appData.Select(x => x.App).ToList(),
                    BasePays = appData.Select(x => x.AvgBase).ToList()
                },
                TipsByAppData = new TipsByAppDataDto
                {
                    TipApps = appData.Select(x => x.App).ToList(),
                    TipPays = appData.Select(x => x.AvgTip).ToList()
                },
                HourlyEarningsData = new PlotlyHourlyDataDto
                {
                    Hours = hourlyProcessed.Select(x => x.Hour).ToList(),
                    Earnings = hourlyProcessed.Select(x => x.Avg).ToList()
                },
                DonutChartData = new DonutChartDataDto
                {
                    TotalPay = totals?.SumTotal ?? 0,
                    TotalBasePay = totals?.SumBase ?? 0,
                    TotalTipPay = totals?.SumTip ?? 0
                }
            };
        }

        public async Task<ShiftStatisticsDto> CalculateShiftStatistics(int userId)
        {
            var shiftsQuery = _context.Shifts.Where(s => s.UserId == userId);

            if (!await shiftsQuery.AnyAsync())
            {
                return new ShiftStatisticsDto
                {
                    AverageShiftLength = 0,
                    AppWithMostShifts = "N/A",
                    AverageDeliveriesForShift = 0
                };
            }

            var shifts = await shiftsQuery.ToListAsync();
            var deliveryCount = await _context.Deliveries.CountAsync(d => d.UserId == userId && d.ShiftId != null);

            var avgLength = shifts.Average(s => (s.EndTime - s.StartTime).TotalMinutes);

            var appWithMost = shifts
                .GroupBy(s => s.App)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key.ToString())
                .FirstOrDefault() ?? "N/A";

            return new ShiftStatisticsDto
            {
                AverageShiftLength = avgLength,
                AppWithMostShifts = appWithMost,
                AverageDeliveriesForShift = (double)deliveryCount / shifts.Count
            };
        }

        public async Task<ExpenseStatisticsDto> CalculateExpenseStatistics(int userId)
        {
            var expenseQuery = _context.Expenses.Where(e => e.UserId == userId);

            if (!await expenseQuery.AnyAsync())
            {
                return new ExpenseStatisticsDto
                {
                    AverageMonthlySpending = 0.0,
                    AverageSpendingByType = new List<SpendingByTypeDto>()
                };
            }

            var expenses = await expenseQuery.ToListAsync();

            var monthlyAvg = expenses
                .GroupBy(e => new { e.Date.Year, e.Date.Month })
                .Average(g => g.Sum(e => e.Amount));

            var totalMonths = expenses.Select(e => new { e.Date.Year, e.Date.Month }).Distinct().Count();

            var spendingByType = expenses
                .GroupBy(e => e.Type)
                .Select(g => new SpendingByTypeDto
                {
                    Type = g.Key,
                    AvgExpense = totalMonths > 0 ? g.Sum(x => x.Amount) / totalMonths : 0
                }).ToList();

            return new ExpenseStatisticsDto
            {
                AverageMonthlySpending = monthlyAvg,
                AverageSpendingByType = spendingByType
            };
        }

        private DeliveryStatisticsDto GetDefaultDeliveryStats()
        {
            return new DeliveryStatisticsDto
            {
                AvgPay = 0.0,
                AvgBase = 0.0,
                AvgTip = 0.0,
                DollarPerMile = 0.0,
                TipPerMile = 0.0,
                HighestPayingRestaurant = new HighestPayingRestaurantDto { Restaurant = "N/A", AvgTotalPay = 0.0 },
                RestaurantWithMost = new RestaurantWithMostDto { RestaurantWithMost = "N/A", OrderCount = 0 },
                PlotlyEarningsData = new PlotlyEarningsDataDto { Dates = new List<string>(), Earnings = new List<double>() },
                PlotlyNeighborhoodsData = new PlotlyNeighborhoodsDataDto { Neighborhoods = new List<string>(), TipPays = new List<double>() },
                AppsByBaseData = new AppsByBaseDataDto { Apps = new List<string>(), BasePays = new List<double>() },
                TipsByAppData = new TipsByAppDataDto { TipApps = new List<string>(), TipPays = new List<double>() },
                HourlyEarningsData = new PlotlyHourlyDataDto { Hours = new List<string>(), Earnings = new List<double>() },
                DonutChartData = new DonutChartDataDto { TotalPay = 0.0, TotalBasePay = 0.0, TotalTipPay = 0.0 }
            };
        }
    }
}