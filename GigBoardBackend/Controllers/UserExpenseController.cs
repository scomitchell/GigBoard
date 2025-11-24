using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using GigBoardBackend.Data;
using GigBoardBackend.Models;
using Microsoft.AspNetCore.SignalR;
using GigBoard.Hubs;
using GigBoardBackend.Services;

namespace GigBoardBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserExpenseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<StatisticsHub> _hub;
        private readonly StatisticsService _statsService;

        public UserExpenseController(ApplicationDbContext context,
            IHubContext<StatisticsHub> hub, StatisticsService statsService)
        {
            _context = context;
            _hub = hub;
            _statsService = statsService;
        }

        [HttpPost]
        public async Task<IActionResult> AddExpense([FromBody] Expense expense)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                _context.Expenses.Add(expense);
                await _context.SaveChangesAsync();

                var userExpense = new UserExpense
                {
                    UserId = userId,
                    ExpenseId = expense.Id,
                    DateAdded = DateTime.UtcNow
                };

                _context.UserExpenses.Add(userExpense);
                await _context.SaveChangesAsync();

                var expenseStats = await _statsService.CalculateExpenseStatistics(userId);
                await _hub.Clients.User(userId.ToString()).SendAsync("ExpenseStatisticsUpdated", expenseStats);

                return Ok(new ExpenseDto
                {
                    Id = expense.Id,
                    Amount = expense.Amount,
                    Date = expense.Date,
                    Type = expense.Type,
                    Notes = expense.Notes
                });
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("my-expenses")]
        public async Task<IActionResult> GetExpenses()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var userExpenses = await _context.UserExpenses
                .Where(ue => ue.UserId == userId && ue.Expense != null)
                .Include(ue => ue.Expense)
                .Select(ue => new ExpenseDto
                {
                    Id = ue.Expense!.Id,
                    Amount = ue.Expense.Amount,
                    Date = ue.Expense.Date,
                    Type = ue.Expense.Type,
                    Notes = ue.Expense.Notes
                })
                .OrderByDescending(x => x.Date)
                .ToListAsync();

                return Ok(userExpenses);
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("filtered-expenses")]
        public async Task<IActionResult> GetFilteredExpenses([FromQuery] double? amount,
            [FromQuery] DateTime? date,
            [FromQuery] string? type) 
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var userExpensesQuery = _context.UserExpenses
                .Where(ue => ue.UserId == userId && ue.Expense != null)
                .Include(ue => ue.Expense)
                .AsQueryable();

                if (amount.HasValue)
                {
                    userExpensesQuery = userExpensesQuery.Where(ue => ue.Expense!.Amount >= amount);
                }

                if (date.HasValue)
                {
                    userExpensesQuery = userExpensesQuery.Where(ue => ue.Expense!.Date >= date);
                }

                if (!string.IsNullOrEmpty(type))
                {
                    userExpensesQuery = userExpensesQuery.Where(ue => ue.Expense!.Type == type);
                }

                var userExpenses = await userExpensesQuery
                    .Select(ue => new ExpenseDto
                    {
                        Id = ue.Expense!.Id,
                        Amount = ue.Expense.Amount,
                        Date = ue.Expense.Date,
                        Type = ue.Expense.Type
                    })
                    .OrderByDescending(x => x.Date)
                    .ToListAsync();

                return Ok(userExpenses);
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetExpenseTypes() {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var result = await _context.UserExpenses
                .Where(ue => ue.UserId == userId && ue.Expense != null)
                .Select(ue => ue.Expense!.Type)
                .Distinct()
                .ToListAsync();

                return Ok(result);
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("{expenseId:int}")]
        public async Task<IActionResult> GetExpenseById(int expenseId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var userExpense = await _context.UserExpenses
                .FirstOrDefaultAsync(us => us.UserId == userId && us.ExpenseId == expenseId);

                if (userExpense == null)
                {
                    return NotFound("Expense not found");
                }

                return Ok(userExpense);
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpDelete("{expenseId}")]
        public async Task<IActionResult> DeleteExpense(int expenseId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var userExpense = await _context.UserExpenses
                .FirstOrDefaultAsync(ue => ue.UserId == userId && ue.ExpenseId == expenseId);

                if (userExpense == null)
                {
                    return NotFound("Expense Not found");
                }

                _context.UserExpenses.Remove(userExpense);
                await _context.SaveChangesAsync();

                var expense = await _context.Expenses.FindAsync(expenseId);
                if (expense != null)
                {
                    _context.Expenses.Remove(expense);
                    await _context.SaveChangesAsync();
                }

                var expenseStats = await _statsService.CalculateExpenseStatistics(userId);
                await _hub.Clients.User(userId.ToString()).SendAsync("ExpenseStatisticsUpdated", expenseStats);

                return Ok("Expense Deleted");
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateExpense([FromBody] Expense expense)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var existingExpense = await _context.UserExpenses
                .Where(ue => ue.UserId == userId && ue.ExpenseId == expense.Id && ue.Expense != null)
                .Include(ue => ue.Expense)
                .FirstOrDefaultAsync();

                if (existingExpense == null)
                {
                    return BadRequest("Expense does not exist");
                }

                var targetExpense = existingExpense.Expense;

                targetExpense!.Amount = expense.Amount;
                targetExpense.Date = expense.Date;
                targetExpense.Type = expense.Type;
                targetExpense.Notes = expense.Notes;

                _context.Expenses.Update(targetExpense);
                await _context.SaveChangesAsync();

                var responseExpense = new ExpenseDto
                {
                    Id = targetExpense.Id,
                    Amount = targetExpense.Amount,
                    Date = targetExpense.Date,
                    Type = targetExpense.Type,
                    Notes = targetExpense.Notes
                };

                var expenseStats = await _statsService.CalculateExpenseStatistics(userId);
                await _hub.Clients.Users(userId.ToString()).SendAsync("ExpenseStatisticsUpdated", expenseStats);

                return Ok(responseExpense);
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }
    }
}