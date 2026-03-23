namespace GigBoardBackend.Models
{
    public class User
    {
        public int Id { get; set; }

        public ICollection<Delivery> Deliveries { get; set; } = new HashSet<Delivery>();
        public ICollection<Shift> Shifts { get; set; } = new HashSet<Shift>();
        public ICollection<Expense> Expenses { get; set; } = new HashSet<Expense>();

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}