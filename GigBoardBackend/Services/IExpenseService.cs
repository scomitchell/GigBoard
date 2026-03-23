using GigBoardBackend.Models;

namespace GigBoardBackend.Services
{
    public interface IExpenseService
    {
        Task<ExpenseDto> AddExpenseAsync(int userId, Expense expense);
        Task<IEnumerable<ExpenseDto>> GetExpensesAsync(int userId);
        Task<IEnumerable<ExpenseDto>> GetFilteredExpensesAsync(int userId, double? amount,
            DateTime? date, string? type);
        Task<IEnumerable<string>> GetExpenseTypesAsync(int userId);
        Task<Expense?> GetExpenseByIdAsync(int userId, int expenseId);
        Task DeleteExpenseAsync(int userId, int expenseId);
        Task<ExpenseDto> UpdateExpenseAsync(int userId, Expense expense);
    }
}