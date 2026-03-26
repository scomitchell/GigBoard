namespace GigBoardBackend.Models
{
    public class ExpenseStatisticsDto
    {
        public double AverageMonthlySpending { get; set; }
        public IEnumerable<SpendingByTypeDto>? AverageSpendingByType { get; set; }
    }

    public class SpendingByTypeDto
    {
        public string? Type { get; set; }
        public double AvgExpense { get; set; }
    }
}