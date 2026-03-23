using GigBoardBackend.Data;
using GigBoardBackend.Hubs;
using GigBoardBackend.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GigBoardBackend.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IStatisticsService _statisticsService;
        private readonly IHubContext<StatisticsHub> _hub;

        public DeliveryService(ApplicationDbContext context, IStatisticsService statisticsService,
            IHubContext<StatisticsHub> hub)
        {
            _context = context;
            _statisticsService = statisticsService;
            _hub = hub;
        }

        public async Task<DeliveryDto> AddDeliveryAsync(int userId, Delivery delivery)
        {
            if (delivery.DeliveryTime > DateTime.Now)
            {
                throw new ArgumentException("Delivery time cannot be in the future");
            }

            delivery.UserId = userId;
            delivery.TotalPay = delivery.BasePay + delivery.TipPay;

            var activeShift = await _context.Shifts
                .FirstOrDefaultAsync(s => s.UserId == userId
                    && s.App == delivery.App
                    && s.StartTime <= delivery.DeliveryTime
                    && s.EndTime >= delivery.DeliveryTime);

            delivery.ShiftId = activeShift?.Id;

            _context.Deliveries.Add(delivery);
            await _context.SaveChangesAsync();

            await UpdateStatisticsAndNotifyAsync(userId);

            return MapToDto(delivery);
        }

        public async Task<IEnumerable<DeliveryDto>> GetDeliveriesAsync(int userId)
        {
            var deliveries = await _context.Deliveries
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.DeliveryTime)
                .ToListAsync();

            return deliveries.Select(MapToDto);
        }

        public async Task<IEnumerable<DeliveryDto>> GetUnassignedDeliveriesAsync(int userId)
        {
            return await _context.Deliveries
                .Where(d => d.UserId == userId && d.ShiftId == null)
                .Select(d => MapToDto(d))
                .ToListAsync();
        }

        public async Task<IEnumerable<DeliveryDto>> GetFilteredDeliveriesAsync(int userId, DeliveryApp? app, double? basePay, double? tipPay, double? totalPay, double? mileage, string? restaurant, string? customerNeighborhood)
        {
            var query = _context.Deliveries.Where(d => d.UserId == userId).AsQueryable();

            if (app.HasValue) query = query.Where(d => d.App == app.Value);
            if (basePay.HasValue) query = query.Where(d => d.BasePay >= basePay.Value);
            if (tipPay.HasValue) query = query.Where(d => d.TipPay >= tipPay.Value);
            if (totalPay.HasValue) query = query.Where(d => d.TotalPay >= totalPay.Value);
            if (mileage.HasValue) query = query.Where(d => d.Mileage >= mileage.Value);
            if (!string.IsNullOrEmpty(restaurant)) query = query.Where(d => d.Restaurant.ToLower().Contains(restaurant.ToLower()));
            if (!string.IsNullOrEmpty(customerNeighborhood)) query = query.Where(d => d.CustomerNeighborhood.ToLower().Contains(customerNeighborhood.ToLower()));

            var deliveries = await query
                .OrderByDescending(d => d.DeliveryTime)
                .ToListAsync();

            return deliveries.Select(MapToDto);
        }

        public async Task<IEnumerable<string>> GetUserDeliveryNeighborhoodsAsync(int userId)
        {
            return await _context.Deliveries
                .Where(d => d.UserId == userId && !string.IsNullOrEmpty(d.CustomerNeighborhood))
                .Select(d => d.CustomerNeighborhood)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<DeliveryApp>> GetUserDeliveryAppsAsync(int userId)
        {
            return await _context.Deliveries
                .Where(d => d.UserId == userId)
                .Select(d => d.App)
                .Distinct()
                .ToListAsync();
        }

        public async Task<Delivery?> GetDeliveryByIdAsync(int userId, int deliveryId)
        {
            return await _context.Deliveries
                .FirstOrDefaultAsync(d => d.UserId == userId && d.Id == deliveryId);
        }

        public async Task DeleteDeliveryAsync(int userId, int deliveryId)
        {
            var delivery = await _context.Deliveries
                .FirstOrDefaultAsync(d => d.UserId == userId && d.Id == deliveryId);

            if (delivery == null) throw new KeyNotFoundException("Delivery not found");

            _context.Deliveries.Remove(delivery);
            await _context.SaveChangesAsync();

            await UpdateStatisticsAndNotifyAsync(userId);
        }

        public async Task<DeliveryDto> UpdateDeliveryAsync(int userId, Delivery delivery)
        {
            if (delivery.DeliveryTime > DateTime.Now)
                throw new ArgumentException("Delivery time cannot be in the future");

            var targetDelivery = await _context.Deliveries
                .FirstOrDefaultAsync(d => d.UserId == userId && d.Id == delivery.Id);

            if (targetDelivery == null) throw new KeyNotFoundException("Delivery does not exist");

            targetDelivery.App = delivery.App;
            targetDelivery.TotalPay = delivery.BasePay + delivery.TipPay;
            targetDelivery.TipPay = delivery.TipPay;
            targetDelivery.BasePay = delivery.BasePay;
            targetDelivery.Mileage = delivery.Mileage;
            targetDelivery.Restaurant = delivery.Restaurant;
            targetDelivery.CustomerNeighborhood = delivery.CustomerNeighborhood;
            targetDelivery.DeliveryTime = delivery.DeliveryTime;
            targetDelivery.Notes = delivery.Notes;

            var validShift = await _context.Shifts
                .FirstOrDefaultAsync(s => s.UserId == userId
                                       && s.App == delivery.App
                                       && s.StartTime <= delivery.DeliveryTime
                                       && s.EndTime >= delivery.DeliveryTime);

            targetDelivery.ShiftId = validShift?.Id;

            await _context.SaveChangesAsync();

            await UpdateStatisticsAndNotifyAsync(userId);

            return MapToDto(targetDelivery);
        }

        private async Task UpdateStatisticsAndNotifyAsync(int userId)
        {
            var stats = await _statisticsService.CalculateDeliveryStatistics(userId);
            await _hub.Clients.User(userId.ToString()).SendAsync("StatisticsUpdated", stats);

            var shiftStats = await _statisticsService.CalculateShiftStatistics(userId);
            await _hub.Clients.User(userId.ToString()).SendAsync("ShiftStatisticsUpdated", shiftStats);
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