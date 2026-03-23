using System.Text.Json.Serialization;

namespace GigBoardBackend.Models
{
    public class Expense
    {
        // Primary Key
        public int Id { get; set; }

        // Foeign Key
        public int UserId { get; set; }

        [JsonIgnore]
        public User? User { get; set; }

        // Data
        public double Amount { get; set; }

        public DateTime Date { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
    }
}