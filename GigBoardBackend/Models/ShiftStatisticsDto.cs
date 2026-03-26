namespace GigBoardBackend.Models
{
    public class ShiftStatisticsDto
    {
        public double AverageShiftLength { get; set; }
        public string? AppWithMostShifts { get; set; }
        public double AverageDeliveriesForShift { get; set; }
    }
}