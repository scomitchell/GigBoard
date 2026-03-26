using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GigBoardBackend.Services;
using GigBoardBackend.Models;
using GigBoardBackend.Data;
using Moq;
using Microsoft.AspNetCore.SignalR;
using GigBoardBackend.Hubs;

namespace GigBoard.Tests.Services
{
    public class ExpenseServiceTests : IDisposable
    {
        private readonly ExpenseService _service;
        private readonly ApplicationDbContext _context;

        public ExpenseServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            SeedDatabase();

            // Mock SignalR
            var mockHubClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();

            mockHubClients.Setup(clients => clients.User(It.IsAny<string>())).Returns(mockClientProxy.Object);

            var mockHubContext = new Mock<IHubContext<StatisticsHub>>();
            mockHubContext
                .Setup(h => h.Clients)
                .Returns(mockHubClients.Object);

            var mockStatsService = new Mock<StatisticsService>(_context);

            _service = new ExpenseService(_context, mockStatsService.Object, mockHubContext.Object);
        }

        private void SeedDatabase()
        {
            if (!_context.Users.Any())
            {
                _context.Users.Add(new User
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Smith",
                    Email = "example@email.com",
                    Username = "jsmith",
                    Password = "password123"
                });

                _context.SaveChanges();
            }

            if (!_context.Expenses.Any())
            {
                var expense1 = new Expense
                {
                    Id = 1,
                    UserId = 1,
                    Amount = 30,
                    Date = DateTime.Now,
                    Type = "Gas",
                    Notes = "Test1"
                };

                _context.Expenses.Add(expense1);
                _context.SaveChanges();
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetExpenses_ReturnsExpenses()
        {
            var result = await _service.GetExpensesAsync(1);

            var userExpenses = result.ToList();

            var userExpense = Assert.Single(userExpenses);
            Assert.Equal(30, userExpense.Amount);
        }

        [Fact]
        public async Task DeleteExpense_RemovesExpense()
        {
            Assert.True(_context.Expenses.Any(e => e.Id == 1));

            await _service.DeleteExpenseAsync(1, 1);
            Assert.False(_context.Expenses.Any());
        }

        [Fact]
        public async Task GetExpenseById_ReturnsExpense()
        {
            var result = await _service.GetExpenseByIdAsync(1, 1);

            Assert.NotNull(result);
            Assert.Equal(30, result.Amount);

            var result2 = await _service.GetExpenseByIdAsync(1, 2);
            Assert.Null(result2);
        }

        [Fact]
        public async Task GetFilteredExpenses_FiltersExpenses()
        {
            Assert.Contains(_context.Expenses, e => e.Id == 1);

            var expense2 = new Expense
            {
                Id = 2,
                UserId = 1,
                Amount = 20.50,
                Date = DateTime.Now,
                Type = "Car Maintenance",
                Notes = "Test 2"
            };
            _context.Expenses.Add(expense2);
            await _context.SaveChangesAsync();

            var result = await _service.GetFilteredExpensesAsync(1, amount: null,
                date: null,
                type: "Car Maintenance");

            var expenses = result.ToList();

            Assert.Contains(expenses, e => e.Id == 2);
            Assert.DoesNotContain(expenses, e => e.Id == 1);
        }

        [Fact]
        public async Task AddExpense_PostsExpense()
        {
            var expense3 = new Expense
            {
                Id = 3,
                UserId = 1,
                Amount = 25,
                Date = DateTime.Now,
                Type = "Gas",
                Notes = "Test 3"
            };

            var result = await _service.AddExpenseAsync(1, expense3);

            Assert.IsType<ExpenseDto>(result);
            var userExpense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.UserId == 1 && e.Id == expense3.Id);

            Assert.NotNull(userExpense);
        }

        [Fact]
        public async Task GetExpenseTypes_ReturnsTypes()
        {
            var expense4 = new Expense
            {
                Id = 4,
                UserId = 1,
                Amount = 34,
                Date = DateTime.Now,
                Type = "Car Maintenance",
                Notes = "Test 4"
            };
            _context.Expenses.Add(expense4);
            await _context.SaveChangesAsync();

            var result = await _service.GetExpenseTypesAsync(1);

            var types = result.ToList();

            Assert.Contains("Car Maintenance", types);
            Assert.Contains("Gas", types);
        }

        [Fact]
        public async Task UpdateExpense_PutsExpense()
        {
            var expense1 = new Expense
            {
                Id = 1,
                UserId = 1,
                Amount = 33,
                Date = DateTime.Now,
                Type = "Gas",
                Notes = "Test1"
            };

            var result = await _service.UpdateExpenseAsync(1, expense1);

            Assert.IsType<ExpenseDto>(result);
            Assert.Equal(33, result.Amount);
        }
    }
}