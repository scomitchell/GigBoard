using System.Text.Json.Serialization;

namespace GigBoardBackend.Models
{
    public enum DeliveryApp
    {
        Doordash,
        UberEats,
        InstaCart,
        Grubhub
    }
    public class Delivery
    {
        // Primary Key
        public int Id { get; set; }

        // Foreign Keys
        public int UserId { get; set; }
        [JsonIgnore]
        public User? User { get; set; }

        public int? ShiftId { get; set; }
        public Shift? Shift { get; set; }

        // Data
        public DeliveryApp App { get; set; }

        public DateTime DeliveryTime { get; set; }

        public double BasePay { get; set; }

        public double TipPay { get; set; }

        public double TotalPay { get; set; }

        public double Mileage { get; set; }

        public string Restaurant { get; set; } = string.Empty;

        public string CustomerNeighborhood { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
    }
}