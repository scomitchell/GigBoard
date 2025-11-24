using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using GigBoardBackend.Controllers;
using GigBoardBackend.Models;
using GigBoardBackend.Data;
using Moq;
using Microsoft.AspNetCore.SignalR;
using GigBoard.Hubs;
using GigBoardBackend.Services;

namespace GigBoard.Tests.Controllers 
{
    public class UserExpenseControllerTests : IDisposable
    {
        private readonly UserExpenseController _controller;
        private readonly ApplicationDbContext _context;

        public UserExpenseControllerTests()
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

            _controller = new UserExpenseController(_context, mockHubContext.Object, mockStatsService.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext {User = user}
            };
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
                    Amount = 30,
                    Date = DateTime.Now,
                    Type = "Gas",
                    Notes = "Test1"
                };

                _context.Expenses.Add(expense1);
                _context.SaveChanges();

                _context.UserExpenses.Add(new UserExpense
                {
                    UserId = 1,
                    ExpenseId = 1
                });

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
            var result = await _controller.GetExpenses();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var userExpenses = Assert.IsAssignableFrom<IEnumerable<ExpenseDto>>(okResult.Value);
            
            var userExpense = Assert.Single(userExpenses);
            Assert.Equal(30, userExpense.Amount);
        }

        [Fact]
        public async Task DeleteExpense_RemovesExpense()
        {
            Assert.True(_context.Expenses.Any(e => e.Id == 1));
            var result = await _controller.DeleteExpense(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False(_context.Expenses.Any());
        }

        [Fact]
        public async Task GetExpenseById_ReturnsExpense()
        {
            var result = await _controller.GetExpenseById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var userExpense = Assert.IsAssignableFrom<UserExpense>(okResult.Value);
            Assert.NotNull(userExpense.Expense);
            Assert.Equal(30, userExpense.Expense.Amount);

            var result2 = await _controller.GetExpenseById(2);
            Assert.IsType<NotFoundObjectResult>(result2);
        }

        [Fact]
        public async Task GetFilteredExpenses_FiltersExpenses()
        {
            Assert.Contains(_context.UserExpenses, ue => ue.ExpenseId == 1);

            var expense2 = new Expense
            {
                Id = 2,
                Amount = 20.50,
                Date = DateTime.Now,
                Type = "Car Maintenance",
                Notes = "Test 2"
            };
            _context.Expenses.Add(expense2);
            
            var userExpense = new UserExpense
            {
                UserId = 1,
                ExpenseId = expense2.Id
            };
            _context.UserExpenses.Add(userExpense);

            await _context.SaveChangesAsync();

            var result = await _controller.GetFilteredExpenses(amount: null,
                date: null,
                type: "Car Maintenance");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var expenses = Assert.IsAssignableFrom<IEnumerable<ExpenseDto>>(okResult.Value);

            Assert.Contains(expenses, e => e.Id == 2);
            Assert.DoesNotContain(expenses, e => e.Id == 1);
        }

        [Fact]
        public async Task AddExpense_PostsExpense()
        {
            var expense3 = new Expense
            {
                Id = 3,
                Amount = 25,
                Date = DateTime.Now,
                Type = "Gas",
                Notes = "Test 3"
            };

            var result = await _controller.AddExpense(expense3);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var userExpense = await _context.UserExpenses
                .FirstOrDefaultAsync(ue => ue.UserId == 1 && ue.ExpenseId == expense3.Id);

            Assert.NotNull(userExpense);
        }

        [Fact]
        public async Task GetExpenseTypes_ReturnsTypes()
        {
            var expense4 = new Expense
            {
                Id = 4,
                Amount = 34,
                Date = DateTime.Now,
                Type = "Car Maintenance",
                Notes = "Test 4"
            };
            _context.Expenses.Add(expense4);

            var userExpense = new UserExpense
            {
                UserId = 1,
                ExpenseId = expense4.Id
            };
            _context.UserExpenses.Add(userExpense);

            await _context.SaveChangesAsync();

            var result = await _controller.GetExpenseTypes();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var types = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);

            Assert.Contains("Car Maintenance", types);
            Assert.Contains("Gas", types);
        }

        [Fact]
        public async Task UpdateExpense_PutsExpense()
        {
            var expense1 = new Expense
            {
                Id = 1,
                Amount = 33,
                Date = DateTime.Now,
                Type = "Gas",
                Notes = "Test1"
            };

            var result = await _controller.UpdateExpense(expense1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var expense = Assert.IsAssignableFrom<ExpenseDto>(okResult.Value);

            Assert.Equal(33, expense.Amount);
        }
    }
}