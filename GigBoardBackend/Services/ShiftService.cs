using GigBoardBackend.Data;
using GigBoardBackend.Hubs;
using GigBoardBackend.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GigBoardBackend.Services
{
    public class ShiftService : IShiftService
    {
        private readonly ApplicationDbContext _context;
        private readonly IStatisticsService _statsService;
        private readonly IHubContext<StatisticsHub> _hub;

        public ShiftService(ApplicationDbContext context, IStatisticsService statsService,
            IHubContext<StatisticsHub> hub)
        {
            _context = context;
            _statsService = statsService;
            _hub = hub;
        }

        public async Task<ShiftDto> AddShiftAsync(int userId, Shift shift)
        {
            if (shift.StartTime > DateTime.Now)
            {
                throw new ArgumentException("Shift start time cannot be in the future");
            }

            if (shift.StartTime >= shift.EndTime)
            {
                throw new ArgumentException("Shift end time must come after shift start time");
            }

            shift.UserId = userId;

            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();

            var existingDeliveries = await _context.Deliveries
                .Where(d => d.UserId == userId
                    && d.DeliveryTime >= shift.StartTime
                    && d.DeliveryTime <= shift.EndTime
                    && d.App == shift.App)
                .ToListAsync();

            foreach (var delivery in existingDeliveries)
            {
                delivery.ShiftId = shift.Id;
            }

            if (existingDeliveries.Any())
            {
                await _context.SaveChangesAsync();
            }

            await UpdateStatisticsAndNotifyAsync(userId);
            return MapToDto(shift);
        }

        public async Task<IEnumerable<ShiftDto>> GetShiftsAsync(int userId)
        {
            var shifts = await _context.Shifts
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.EndTime)
                .ToListAsync();

            return shifts.Select(MapToDto);
        }

        public async Task<IEnumerable<DeliveryApp>> GetUserShiftAppsAsync(int userId)
        {
            return await _context.Shifts
                .Where(s => s.UserId == userId)
                .Select(s => s.App)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<ShiftDto>> GetFilteredShiftsAsync(int userId, DateTime? startTime,
            DateTime? endTime, DeliveryApp? app)
        {
            var shiftsQuery = _context.Shifts
                .Where(s => s.UserId == userId)
                .AsQueryable();

            if (startTime.HasValue)
            {
                shiftsQuery = shiftsQuery.Where(s => s.StartTime >= startTime);
            }

            if (endTime.HasValue)
            {
                shiftsQuery = shiftsQuery.Where(s => s.EndTime <= endTime);
            }

            if (app.HasValue)
            {
                shiftsQuery = shiftsQuery.Where(s => s.App == app);
            }

            var shifts = await shiftsQuery
                .OrderByDescending(s => s.EndTime)
                .ToListAsync();

            return shifts.Select(MapToDto);
        }

        public async Task<Shift?> GetShiftByIdAsync(int userId, int shiftId)
        {
            return await _context.Shifts
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Id == shiftId);
        }

        public async Task DeleteShiftAsync(int userId, int shiftId)
        {
            var shift = await _context.Shifts
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Id == shiftId);

            if (shift == null) throw new KeyNotFoundException("Shift not found");

            _context.Shifts.Remove(shift);

            var deliveriesToUnassign = await _context.Deliveries
                .Where(d => d.UserId == userId && d.ShiftId == shift.Id)
                .ToListAsync();

            foreach (var delivery in deliveriesToUnassign)
            {
                delivery.ShiftId = null;
            }

            await _context.SaveChangesAsync();
            await UpdateStatisticsAndNotifyAsync(userId);
        }

        public async Task<ShiftDto> UpdateShiftAsync(int userId, Shift shift)
        {
            if (shift.StartTime > DateTime.Now)
            {
                throw new ArgumentException("Shift start time cannot be in the future");
            }

            if (shift.StartTime >= shift.EndTime)
            {
                throw new ArgumentException("Shift start time must be before end time");
            }

            var targetShift = await _context.Shifts
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Id == shift.Id);

            if (targetShift == null) throw new KeyNotFoundException("Shift does not exist");

            targetShift.StartTime = shift.StartTime;
            targetShift.EndTime = shift.EndTime;
            targetShift.App = shift.App;

            // Remove deliveries that no longer match.
            var currentAssignments = await _context.Deliveries
                .Where(d => d.UserId == userId && d.ShiftId == shift.Id)
                .ToListAsync();

            foreach (var delivery in currentAssignments)
            {
                if (delivery.DeliveryTime < shift.StartTime
                    || delivery.DeliveryTime > shift.EndTime
                    || delivery.App != shift.App)
                {
                    delivery.ShiftId = null;
                }
            }

            var existingUnassignedDeliveries = await _context.Deliveries
                .Where(d => d.UserId == userId
                    && d.ShiftId != shift.Id
                    && d.DeliveryTime >= shift.StartTime
                    && d.DeliveryTime <= shift.EndTime
                    && d.App == shift.App)
                .ToListAsync();

            foreach (var delivery in existingUnassignedDeliveries)
            {
                delivery.ShiftId = shift.Id;
            }

            await _context.SaveChangesAsync();
            await UpdateStatisticsAndNotifyAsync(userId);
            return MapToDto(targetShift);
        }

        public async Task<IEnumerable<DeliveryDto>> GetDeliveriesForShiftAsync(int userId, int shiftId)
        {
            var deliveries = await _context.Deliveries
                .Where(d => d.UserId == userId && d.ShiftId == shiftId)
                .OrderBy(d => d.DeliveryTime)
                .ToListAsync();

            return deliveries.Select(MapToDto);
        }

        private async Task UpdateStatisticsAndNotifyAsync(int userId)
        {
            var shiftStats = await _statsService.CalculateShiftStatistics(userId);
            await _hub.Clients.User(userId.ToString()).SendAsync("ShiftStatisticsUpdated", shiftStats);

            var stats = await _statsService.CalculateDeliveryStatistics(userId);
            await _hub.Clients.User(userId.ToString()).SendAsync("StatisticsUpdated", stats);
        }

        private static ShiftDto MapToDto(Shift s)
        {
            return new ShiftDto
            {
                Id = s.Id,
                App = s.App,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            };
        }

        private static DeliveryDto MapToDto(Delivery d)
        {
            return new DeliveryDto
            {
                Id = d.Id,
                App = d.App,
                DeliveryTime = d.DeliveryTime,
                BasePay = d.BasePay,
                TipPay = d.TipPay,
                TotalPay = d.TotalPay,
                Restaurant = d.Restaurant,
                CustomerNeighborhood = d.CustomerNeighborhood,
                Mileage = d.Mileage,
                Notes = d.Notes
            };
        }
    }
}