using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GigBoardBackend.Data;
using GigBoardBackend.Models;

namespace GigBoardBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StatisticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        private readonly string _pythonServiceUrl;

        public StatisticsController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _pythonServiceUrl = Environment.GetEnvironmentVariable("PYTHON_SERVICE_URL") 
                ?? "http://localhost:8001";
        }

        [HttpGet("deliveries/avg-delivery-pay")]
        public async Task<IActionResult> GetAvgDeliveryPay()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                // Get all the total pays for deliveries of this user
                var totalPays = await (
                    from ud in _context.UserDeliveries
                    join d in _context.Deliveries on ud.DeliveryId equals d.Id
                    where ud.UserId == userId
                    select d.TotalPay
                ).ToListAsync();

                // If no deliveries, averaeg is zero
                if (totalPays.Count == 0)
                {
                    return Ok(0);
                }

                // Compute average
                var avgPay = totalPays.Average();

                return Ok(avgPay);
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("deliveries/average-base-pay")]
        public async Task<IActionResult> GetAvgBasePay()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                // Get total base pays as list
                var totalBases = await (
                    from ud in _context.UserDeliveries
                    join d in _context.Deliveries on ud.DeliveryId equals d.Id
                    where ud.UserId == userId
                    select d.BasePay
                ).ToListAsync();

                // If none return 0
                if (totalBases.Count == 0)
                {
                    return Ok(0);
                }

                // Average base pays and return
                var avgBase = totalBases.Average();

                return Ok(avgBase);
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("deliveries/average-tip")]
        public async Task<IActionResult> GetAvgTipPay()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                // Get total tips as list
                var totalTips = await (
                    from ud in _context.UserDeliveries
                    join d in _context.Deliveries on ud.DeliveryId equals d.Id
                    where ud.UserId == userId
                    select d.TipPay
                ).ToListAsync();

                // If none, average is zero
                if (totalTips.Count == 0)
                {
                    return Ok(0);
                }

                // Average tip pays
                var avgTip = totalTips.Average();

                return Ok(avgTip);
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("deliveries/highest-paying-neighborhood")]
        public async Task<IActionResult> GetHighestPayingNeighborhood()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var result = await _context.UserDeliveries
                    .Where(ud => ud.UserId == userId && ud.Delivery != null)
                    .GroupBy(ud => ud.Delivery!.CustomerNeighborhood)
                    // Get neighborhood and its average tip pay across all associated deliveries
                    .Select(g => new
                    {
                        Neighborhood = g.Key,
                        AvgTipPay = g.Average(ud => ud.Delivery!.TipPay)
                    })
                    // Sort by tip pay
                    .OrderByDescending(x => x.AvgTipPay)
                    // Return highest average
                    .FirstOrDefaultAsync();

                if (result == null)
                {
                    return NotFound("No deliveries found for user");
                }

                return Ok(new
                {
                    neighborhood = result.Neighborhood,
                    averageTipPay = result.AvgTipPay
                });
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("deliveries/highest-paying-restaurant")]
        public async Task<IActionResult> GetHighestPayingRestaurant()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var result = await _context.UserDeliveries
                    .Where(ud => ud.UserId == userId && ud.Delivery != null)
                    .GroupBy(ud => ud.Delivery!.Restaurant)
                    // Get restaurant and its average total pay across all associated deliveries
                    .Select(g => new
                    {
                        Restaurant = g.Key,
                        AvgTotalPay = g.Average(ud => ud.Delivery!.TotalPay)
                    })
                    // Sort by average total pay
                    .OrderByDescending(x => x.AvgTotalPay)
                    // Return highest
                    .FirstOrDefaultAsync();

                if (result == null)
                {
                    return Ok(new
                    {
                        restaurant = "N/A",
                        avgTotalPay = 0
                    });
                }

                return Ok(new
                {
                    restaurant = result.Restaurant,
                    avgTotalPay = result.AvgTotalPay
                });
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("deliveries/highest-paying-base-app")]
        public async Task<IActionResult> GetHighestPayingBaseApp()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var result = await _context.UserDeliveries
                    .Where(ud => ud.UserId == userId && ud.Delivery != null)
                    .GroupBy(ud => ud.Delivery!.App)
                    // Get app and average base pay across all associated deliveries
                    .Select(g => new
                    {
                        App = g.Key,
                        AverageBase = g.Average(ud => ud.Delivery!.BasePay)
                    })
                    // Sort by average base pay
                    .OrderByDescending(x => x.AverageBase)
                    // Return highest
                    .FirstOrDefaultAsync();

                if (result == null)
                {
                    return NotFound("No deliveries found");
                }

                return Ok(new
                {
                    app = result.App,
                    avgBase = result.AverageBase
                });
            }
            else
            {
                return BadRequest("User claim does not exist");
            }
        }

        [HttpGet("deliveries/highest-paying-tip-app")]
        public async Task<IActionResult> GetHighestPayingTipApp()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var result = await _context.UserDeliveries
                    .Where(ud => ud.UserId == userId && ud.Delivery != null)
                    .GroupBy(ud => ud.Delivery!.App)
                    .Select(g => new
                    {
                        App = g.Key,
                        AverageTip = g.Average(ud => ud.Delivery!.TipPay)
                    })
                    .OrderByDescending(x => x.AverageTip)
                    .FirstOrDefaultAsync();

                if (result == null)
                {
                    return NotFound("No deliveries found");
                }

                return Ok(new
                {
                    app = result.App,
                    avgTip = result.AverageTip
                });
            }
            else
            {
                return BadRequest("User claim does not exist");
            }
        }

        [HttpGet("deliveries/dollar-per-mile")]
        public async Task<IActionResult> GetDollarPerMile()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                // Get all user deliveries and sum miles
                var miles = await _context.UserDeliveries
                    .Where(ud => ud.UserId == userId && ud.Delivery != null)
                    .SumAsync(ud => ud.Delivery!.Mileage);

                // Get all user deliveries and sum total pays
                var totalPay = await _context.UserDeliveries
                    .Where(ud => ud.UserId == userId)
                    .SumAsync(ud => ud.Delivery!.TotalPay);

                // If either are zero, return zero
                if (miles == 0 || totalPay == 0)
                {
                    return Ok(0);
                }

                // Get dollar-per-mile
                var result = totalPay / miles;

                return Ok(result);
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("expenses/average-monthly-spending")]
        public async Task<IActionResult> GetAverageMonthlySpending()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var monthlyTotals = await _context.UserExpenses
                    .Where(ue => ue.UserId == userId && ue.Expense != null)
                    // Group exenses by year and month (ex. May 2025 and May 2024 are separate months)
                    .GroupBy(ue => new { ue.Expense!.Date.Year, ue.Expense.Date.Month })
                    // Get sum of expenses for each month and return as list
                    .Select(g => g.Sum(ue => ue.Expense!.Amount))
                    .ToListAsync();

                // If none, return 0
                if (!monthlyTotals.Any())
                {
                    return Ok(0);
                }

                // Average totals
                var result = monthlyTotals.Average();
                return Ok(result);
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("expenses/average-spending-by-type")]
        public async Task<IActionResult> GetAverageSpendingByType()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                // Get expenses as list
                var userExpenses = await _context.UserExpenses
                .Where(ue => ue.UserId == userId && ue.Expense != null)
                .Select(ue => new
                {
                    ue.Expense!.Type,
                    ue.Expense.Amount,
                    ue.Expense.Date.Month,
                    ue.Expense.Date.Year
                })
                .ToListAsync();
                
                // Get number of months, months in different years are unique
                var totalMonths = userExpenses
                    .Select(e => new { e.Year, e.Month })
                    .Distinct()
                    .Count();

                // Get list of expense types with associated average monthly cost
                var result = userExpenses
                    .GroupBy(e => e.Type)
                    .Select(g => new
                    {
                        Type = g.Key,
                        AvgExpense = totalMonths > 0 ? g.Sum(x => x.Amount) / totalMonths : 0
                    })
                    .ToList();

                return Ok(result);
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
            
        }

        [HttpGet("shifts/average-shift-length")]
        public async Task<IActionResult> getAverageShiftLength()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                // Get list of shift durations in minutes by subtracting start time from end time
                var durations = await _context.UserShifts
                .Where(us => us.UserId == userId && us.Shift != null)
                .Select(us => us.Shift!.EndTime - us.Shift.StartTime)
                .ToListAsync();

                // If none return 0
                if (durations.Count == 0)
                {
                    return Ok(0);
                }

                // Average shift minutes
                var averageMinutes = durations.Average(d => d.TotalMinutes);

                return Ok(averageMinutes);
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("shifts/app-with-most-shifts")]
        public async Task<IActionResult> getAppWithMostShifts()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var result = await _context.UserShifts
                .Where(us => us.UserId == userId && us.Shift != null)
                // Group shifts by app
                .GroupBy(us => us.Shift!.App)
                // Get app and number of shifts associated
                .Select(g => new
                {
                    App = g.Key,
                    ShiftCount = g.Count()
                })
                // Sort in order of most shifts and return highest
                .OrderByDescending(g => g.ShiftCount)
                .FirstOrDefaultAsync();

                if (result == null)
                {
                    return Ok(null);
                }

                return Ok(result.App);
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("deliveries/restaurant-with-most-deliveries")]
        public async Task<IActionResult> GetRestaurantWithMostDeliveries()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var result = await _context.UserDeliveries
                .Where(ud => ud.UserId == userId && ud.Delivery != null)
                .GroupBy(ud => ud.Delivery!.Restaurant)
                .Select(g => new
                {
                    Restaurant = g.Key,
                    OrderCount = g.Count()
                })
                .OrderByDescending(g => g.OrderCount)
                .FirstOrDefaultAsync();

                if (result == null)
                {
                    return Ok(new
                    {
                        restaurantWithMost = "N/A",
                        orderCount = 0
                    });
                }

                return Ok(new
                {
                    restaurantWithMost = result.Restaurant,
                    orderCount = result.OrderCount
                });
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("deliveries/tip-per-mile")]
        public async Task<IActionResult> GetTipPerMile()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var miles = await _context.UserDeliveries
                .Where(ud => ud.UserId == userId && ud.Delivery != null)
                .SumAsync(ud => ud.Delivery!.Mileage);

                var tipPay = await _context.UserDeliveries
                .Where(ud => ud.UserId == userId)
                .SumAsync(ud => ud.Delivery!.TipPay);

                if (miles == 0 || tipPay == 0)
                {
                    return Ok(0);
                }

                var result = tipPay / miles;

                return Ok(result);
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
            
        }

        [HttpGet("shifts/average-num-deliveries")]
        public async Task<IActionResult> GetAverageDeliveriesPerShift()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var userShiftIds = await _context.UserShifts
                .Where(us => us.UserId == userId && us.Shift != null)
                .Select(sd => sd.ShiftId)
                .ToListAsync();

                var shiftDeliveries = await _context.ShiftDeliveries
                .Where(sd => sd.UserId == userId)
                .GroupBy(sd => sd.ShiftId)
                .Select(g => new
                {
                    ShiftId = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

                if (!userShiftIds.Any())
                {
                    return Ok(0);
                }

                var deliveriesDict = shiftDeliveries.ToDictionary(d => d.ShiftId, d => d.Count);

                var deliveryCountsIncludingZeros = userShiftIds
                    .Select(shiftId => deliveriesDict.ContainsKey(shiftId) ? deliveriesDict[shiftId] : 0)
                    .ToList();

                var average = deliveryCountsIncludingZeros.Average();

                return Ok(average);
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("plotly-charts/earnings-over-time")]
        public async Task<IActionResult> PlotlyEarningsData()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var deliveries = await (
                from ud in _context.UserDeliveries
                join d in _context.Deliveries on ud.DeliveryId equals d.Id
                where ud.UserId == userId
                group d by d.DeliveryTime.Date into g
                orderby g.Key
                select new
                {
                    Date = g.Key,
                    TotalEarnings = g.Sum(x => x.TotalPay)
                }
                ).ToListAsync();

                if (deliveries.Count == 0)
                {
                    return NotFound("No deliveries found for this user");
                }

                var dates = deliveries.Select(d => d.Date.ToString("yyyy-MM-dd")).ToList();
                var earnings = deliveries.Select(d => (double)d.TotalEarnings).ToList();

                return Ok(new
                {
                    dates,
                    earnings
                });
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("plotly-charts/tip-neighborhoods")]
        public async Task<IActionResult> GetPlotlyNeighborhoodsData()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var deliveries = await _context.UserDeliveries
                .Where(ud => ud.UserId == userId && ud.Delivery != null)
                .Select(ud => new
                {
                    Neighborhood = ud.Delivery!.CustomerNeighborhood.Trim(),
                    ud.Delivery.TipPay
                })
                .GroupBy(x => x.Neighborhood)
                .Select(g => new
                {
                    CustomerNeighborhood = g.Key,
                    AverageTipPay = g.Average(x => x.TipPay)
                })
                .OrderBy(x => x.CustomerNeighborhood)
                .ToListAsync();

                if (deliveries.Count == 0)
                {
                    return NotFound("No deliveries found for this user");
                }

                var neighborhoods = deliveries.Select(d => d.CustomerNeighborhood).ToList();
                var tipPays = deliveries.Select(d => (double)d.AverageTipPay).ToList();

                return Ok(new
                {
                    neighborhoods,
                    tipPays
                });
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("plotly-charts/apps-by-base")]
        public async Task<IActionResult> GetPlotlyAppsByBase()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var deliveries = await (
                from ud in _context.UserDeliveries
                join d in _context.Deliveries on ud.DeliveryId equals d.Id
                where ud.UserId == userId
                group d by d.App into g
                orderby g.Key
                select new
                {
                    App = g.Key,
                    BasePay = g.Average(x => x.BasePay)
                }
                ).ToListAsync();

                if (deliveries.Count == 0)
                {
                    return NotFound("No deliveries found for this user");
                }

                var apps = deliveries.Select(d => d.App.ToString()).ToList();
                var basePays = deliveries.Select(d => (double)d.BasePay).ToList();

                return Ok(new
                {
                    apps,
                    basePays
                });
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("tips-by-app")]
        public async Task<IActionResult> GetTipsByAppData()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var deliveries = await (
                    from ud in _context.UserDeliveries
                    join d in _context.Deliveries on ud.DeliveryId equals d.Id
                    where ud.UserId == userId
                    group d by d.App into g
                    select new
                    {
                        App = g.Key,
                        TipPay = g.Average(x => x.TipPay)
                    }
                ).ToListAsync();

                if (deliveries.Count == 0)
                {
                    return NotFound("No deliveries found for this user");
                }

                var tipApps = deliveries.Select(d => d.App.ToString()).ToList();
                var appTipPays = deliveries.Select(d => (double)d.TipPay).ToList();

                return Ok(new
                {
                    tipApps,
                    appTipPays
                });
            } 
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("plotly-charts/hourly-earnings")]
        public async Task<IActionResult> GetPlotlyHourlyEarnings()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var oneWeekAgo = DateTime.UtcNow.AddDays(-7);

                var hourlyEarnings = await _context.UserDeliveries
                .Where(ud => ud.UserId == userId && ud.Delivery != null && ud.Delivery.DeliveryTime >= oneWeekAgo)
                .Select(ud => new
                {
                    ud.Delivery!.DeliveryTime.Hour,
                    Earnings = ud.Delivery.TotalPay
                })
                .GroupBy(x => x.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    AverageEarnings = g.Average(x => x.Earnings)
                })
                .OrderBy(x => x.Hour)
                .ToListAsync();

                var allHours = Enumerable.Range(0, 24).ToList();

                var earningsByHour = allHours
                .Select(h => new
                {
                    Hour = h,
                    AverageEarnings = hourlyEarnings.FirstOrDefault(x => x.Hour == h)?.AverageEarnings ?? 0
                })
                .ToList();

                var hoursStrings = earningsByHour.Select(x => x.Hour.ToString("D2")).ToList();
                var earnings = earningsByHour.Select(x => x.AverageEarnings).ToList();

                return Ok(new
                {
                    hours = hoursStrings,
                    earnings
                });
            }
            else
            {
                return BadRequest("User Claim is invalid");
            }
        }

        [HttpGet("donut-chart-data")]
        public async Task<IActionResult> GetDataForDonutChart()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var totalPay = await _context.UserDeliveries
                .Where(ud => ud.UserId == userId && ud.Delivery != null)
                .Select(ud => ud.Delivery!.TotalPay)
                .SumAsync();

                var totalBasePay = await _context.UserDeliveries
                .Where(ud => ud.UserId == userId && ud.Delivery != null)
                .Select(ud => ud.Delivery!.BasePay)
                .SumAsync();

                var totalTipPay = await _context.UserDeliveries
                .Where(ud => ud.UserId == userId && ud.Delivery != null)
                .Select(ud => ud.Delivery!.TipPay)
                .SumAsync();

                return Ok (new
                {
                    totalPay,
                    totalBasePay,
                    totalTipPay
                });
            } 
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpPost("predict/shift-earnings")]
        public async Task<IActionResult> PredictShiftEarnings([FromBody] ShiftPredictionRequest request)
        {
            var payload = new
            {
                start_time = request.StartTime.ToString("HH:mm"),
                end_time = request.EndTime.ToString("HH:mm"),
                app = request.App,
                neighborhood = request.Neighborhood
            };

            var response = await _httpClient.PostAsJsonAsync($"{_pythonServiceUrl}/predict/shift-earnings", payload);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(500, "Python prediction API error");
            }

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

            return Ok(result);
        }
    }
}