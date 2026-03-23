namespace GigBoardBackend.Models
{
    public class CreateShiftDto
    {
        public DeliveryApp App { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}