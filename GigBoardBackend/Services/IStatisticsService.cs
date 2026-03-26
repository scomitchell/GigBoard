using GigBoardBackend.Models;

namespace GigBoardBackend.Services
{
    public interface IStatisticsService
    {
        Task<DeliveryStatisticsDto> CalculateDeliveryStatistics(int userId);
        Task<ShiftStatisticsDto> CalculateShiftStatistics(int userId);
        Task<ExpenseStatisticsDto> CalculateExpenseStatistics(int userId);
    }
}