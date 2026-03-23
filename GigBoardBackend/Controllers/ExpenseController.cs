using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using GigBoardBackend.Models;
using GigBoardBackend.Services;

namespace GigBoardBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expenseService;

        public ExpenseController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        [HttpPost]
        public async Task<IActionResult> AddExpense([FromBody] Expense expense)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            var result = await _expenseService.AddExpenseAsync(userId.Value, expense);
            return Ok(result);
        }

        [HttpGet("my-expenses")]
        public async Task<IActionResult> GetExpenses()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            var result = await _expenseService.GetExpensesAsync(userId.Value);
            return Ok(result);
        }

        [HttpGet("filtered-expenses")]
        public async Task<IActionResult> GetFilteredExpenses([FromQuery] double? amount,
            [FromQuery] DateTime? date,
            [FromQuery] string? type)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            var result = await _expenseService.GetFilteredExpensesAsync(userId.Value, amount, date, type);
            return Ok(result);
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetExpenseTypes()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            var result = await _expenseService.GetExpenseTypesAsync(userId.Value);
            return Ok(result);
        }

        [HttpGet("{expenseId:int}")]
        public async Task<IActionResult> GetExpenseById(int expenseId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            var result = await _expenseService.GetExpenseByIdAsync(userId.Value, expenseId);
            if (result == null) return NotFound("Expense not found");
            return Ok(result);
        }

        [HttpDelete("{expenseId}")]
        public async Task<IActionResult> DeleteExpense(int expenseId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            try
            {
                await _expenseService.DeleteExpenseAsync(userId.Value, expenseId);
                return Ok("Expense removed");
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateExpense([FromBody] Expense expense)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            try
            {
                await _expenseService.UpdateExpenseAsync(userId.Value, expense);
                return Ok("Expense updated");
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId)) return userId;
            return null;
        }
    }
}