using System.Globalization;
using GigBoardBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace GigBoardBackend.Services
{
    public class StatisticsService
    {
        private readonly ApplicationDbContext _context;

        public StatisticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object> CalculateStatistics(int userId)
        {
            var deliveries = await _context.UserDeliveries
                .Where(ud => ud.UserId == userId && ud.Delivery != null)
                .Select(ud => ud.Delivery)
                .ToListAsync();

            // If no deliveries default to 0, empty list, or ""
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
                    tipsByAppData = new {tipApps = new List<string>(), tipPays = new List<double>()}
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
                tipsByAppData
            };
        }
    }
}