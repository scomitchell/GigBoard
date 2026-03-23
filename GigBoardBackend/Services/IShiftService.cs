using GigBoardBackend.Models;

namespace GigBoardBackend.Services
{
    public interface IShiftService
    {
        Task<ShiftDto> AddShiftAsync(int userId, Shift shift);
        Task<IEnumerable<ShiftDto>> GetShiftsAsync(int userId);
        Task<IEnumerable<DeliveryApp>> GetUserShiftAppsAsync(int userId);
        Task<IEnumerable<ShiftDto>> GetFilteredShiftsAsync(int userId, DateTime? startTime,
            DateTime? endTime, DeliveryApp? app);
        Task<Shift?> GetShiftByIdAsync(int userId, int shiftId);
        Task DeleteShiftAsync(int userId, int shiftId);
        Task<ShiftDto> UpdateShiftAsync(int userId, Shift shift);
        Task<IEnumerable<DeliveryDto>> GetDeliveriesForShiftAsync(int userId, int shiftId);
    }
}