namespace GigBoardBackend.Services
{
    public interface IStatisticsService
    {
        Task<object> CalculateDeliveryStatistics(int userId);
        Task<object> CalculateShiftStatistics(int userId);
        Task<object> CalculateExpenseStatistics(int userId);
    }
}