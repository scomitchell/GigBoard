using GigBoardBackend.Models;

namespace GigBoardBackend.Services
{
    public interface IDeliveryService
    {
        Task<DeliveryDto> AddDeliveryAsync(int UserId, Delivery delivery);
        Task<IEnumerable<DeliveryDto>> GetDeliveriesAsync(int userId);
        Task<IEnumerable<DeliveryDto>> GetUnassignedDeliveriesAsync(int userId);
        Task<IEnumerable<DeliveryDto>> GetFilteredDeliveriesAsync(int userId, DeliveryApp? app, double? basePay,
            double? tipPay, double? totalPay, double? mileage, string? restaurant, string? customerNeighborhood);
        Task<IEnumerable<string>> GetUserDeliveryNeighborhoodsAsync(int userId);
        Task<IEnumerable<DeliveryApp>> GetUserDeliveryAppsAsync(int userId);
        Task<Delivery?> GetDeliveryByIdAsync(int userId, int deliveryId);
        Task DeleteDeliveryAsync(int userId, int deliveryId);
        Task<DeliveryDto> UpdateDeliveryAsync(int userId, Delivery delivery);
    }
}