using GigBoardBackend.Data;
using GigBoardBackend.Hubs;
using GigBoardBackend.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GigBoardBackend.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IStatisticsService _statsService;
        private readonly IHubContext<StatisticsHub> _hub;

        public ExpenseService(ApplicationDbContext context, IStatisticsService statsService,
            IHubContext<StatisticsHub> hub)
        {
            _context = context;
            _statsService = statsService;
            _hub = hub;
        }

        public async Task<ExpenseDto> AddExpenseAsync(int userId, Expense expense)
        {
            expense.UserId = userId;
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            await UpdateStatisticsAndNotifyAsync(userId);
            return MapToDto(expense);
        }

        public async Task<IEnumerable<ExpenseDto>> GetExpensesAsync(int userId)
        {
            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return expenses.Select(MapToDto);
        }

        public async Task<IEnumerable<ExpenseDto>> GetFilteredExpensesAsync(int userId, double? amount,
            DateTime? date, string? type)
        {
            var expensesQuery = _context.Expenses
                .Where(e => e.UserId == userId)
                .AsQueryable();

            if (amount.HasValue)
            {
                expensesQuery = expensesQuery.Where(e => e.Amount >= amount);
            }

            if (date.HasValue)
            {
                expensesQuery = expensesQuery.Where(e => e.Date >= date);
            }

            if (!string.IsNullOrEmpty(type))
            {
                expensesQuery = expensesQuery.Where(e => e.Type == type);
            }

            var expenses = await expensesQuery
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return expenses.Select(MapToDto);
        }

        public async Task<IEnumerable<string>> GetExpenseTypesAsync(int userId)
        {
            return await _context.Expenses
                .Where(e => e.UserId == userId)
                .Select(e => e.Type)
                .Distinct()
                .ToListAsync();
        }

        public async Task<Expense?> GetExpenseByIdAsync(int userId, int expenseId)
        {
            return await _context.Expenses
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Id == expenseId);
        }

        public async Task DeleteExpenseAsync(int userId, int expenseId)
        {
            var existingExpense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Id == expenseId);

            if (existingExpense == null) throw new KeyNotFoundException("Expense not found");

            _context.Expenses.Remove(existingExpense);
            await _context.SaveChangesAsync();
            await UpdateStatisticsAndNotifyAsync(userId);
        }

        public async Task<ExpenseDto> UpdateExpenseAsync(int userId, Expense expense)
        {
            var targetExpense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Id == expense.Id);

            if (targetExpense == null) throw new KeyNotFoundException("Expense not found");

            targetExpense.Amount = expense.Amount;
            targetExpense.Date = expense.Date;
            targetExpense.Type = expense.Type;
            targetExpense.Notes = expense.Notes;

            await _context.SaveChangesAsync();
            await UpdateStatisticsAndNotifyAsync(userId);
            return MapToDto(targetExpense);
        }

        private async Task UpdateStatisticsAndNotifyAsync(int userId)
        {
            var expenseStats = await _statsService.CalculateExpenseStatistics(userId);
            await _hub.Clients.User(userId.ToString()).SendAsync("ExpenseStatisticsUpdated", expenseStats);
        }

        private static ExpenseDto MapToDto(Expense e)
        {
            return new ExpenseDto
            {
                Id = e.Id,
                Amount = e.Amount,
                Date = e.Date,
                Type = e.Type,
                Notes = e.Notes
            };
        }
    }
}