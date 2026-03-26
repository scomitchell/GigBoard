
namespace GigBoardBackend.Models
{
    public class DeliveryStatisticsDto
    {
        public double AvgPay { get; set; }
        public double AvgBase { get; set; }
        public double AvgTip { get; set; }
        public double DollarPerMile { get; set; }
        public double TipPerMile { get; set; }
        public HighestPayingRestaurantDto? HighestPayingRestaurant { get; set; }
        public RestaurantWithMostDto? RestaurantWithMost { get; set; }
        public PlotlyEarningsDataDto? PlotlyEarningsData { get; set; }
        public PlotlyNeighborhoodsDataDto? PlotlyNeighborhoodsData { get; set; }
        public AppsByBaseDataDto? AppsByBaseData { get; set; }
        public TipsByAppDataDto? TipsByAppData { get; set; }
        public PlotlyHourlyDataDto? HourlyEarningsData { get; set; }
        public DonutChartDataDto? DonutChartData { get; set; }
    }

    public class HighestPayingRestaurantDto
    {
        public string? Restaurant { get; set; }
        public double AvgTotalPay { get; set; }
    }

    public class RestaurantWithMostDto
    {
        public string? RestaurantWithMost { get; set; }
        public int OrderCount { get; set; }
    }

    public class PlotlyEarningsDataDto
    {
        public IEnumerable<string>? Dates { get; set; }
        public IEnumerable<double>? Earnings { get; set; }
    }

    public class PlotlyNeighborhoodsDataDto
    {
        public IEnumerable<string>? Neighborhoods { get; set; }
        public IEnumerable<double>? TipPays { get; set; }
    }

    public class AppsByBaseDataDto
    {
        public IEnumerable<string>? Apps { get; set; }
        public IEnumerable<double>? BasePays { get; set; }
    }

    public class TipsByAppDataDto
    {
        public IEnumerable<string>? TipApps { get; set; }
        public IEnumerable<double>? TipPays { get; set; }
    }

    public class PlotlyHourlyDataDto
    {
        public IEnumerable<string>? Hours { get; set; }
        public IEnumerable<double>? Earnings { get; set; }
    }

    public class DonutChartDataDto
    {
        public double TotalPay { get; set; }
        public double TotalBasePay { get; set; }
        public double TotalTipPay { get; set; }
    }
}