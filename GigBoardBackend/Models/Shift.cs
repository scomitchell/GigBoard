using System.Text.Json.Serialization;

namespace GigBoardBackend.Models
{
    public class Shift
    {
        // Primary Key
        public int Id { get; set; }

        // Foreign Key
        public int UserId { get; set; }

        [JsonIgnore]
        public User? User { get; set; }

        [JsonIgnore]
        public ICollection<Delivery>? Deliveries { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public DeliveryApp App { get; set; }
    }
}