using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using GigBoardBackend.Data;
using GigBoardBackend.Models;
using Microsoft.AspNetCore.SignalR;
using GigBoardBackend.Hubs;
using GigBoardBackend.Services;

namespace GigBoardBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserShiftController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<StatisticsHub> _hub;
        private readonly IStatisticsService _statsService;

        public UserShiftController(ApplicationDbContext context, 
            IHubContext<StatisticsHub> hub, IStatisticsService statsService)
        {
            _context = context;
            _hub = hub;
            _statsService = statsService;
        }

        [HttpPost]
        public async Task<IActionResult> AddShift([FromBody] Shift shift)
        {
            if (shift.StartTime > DateTime.Now)
            {
                return BadRequest("Shift start time cannot be in the future");
            }

            if (shift.StartTime >= shift.EndTime)
            {
                return BadRequest("Shift end time must come after shift start time");
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);

                if (!userExists)
                {
                    return BadRequest("User does not exist");
                }

                _context.Shifts.Add(shift);
                await _context.SaveChangesAsync();

                var userDeliveries = await _context.UserDeliveries
                    .Where(ud => ud.UserId == userId
                        && ud.Delivery != null
                        && ud.Delivery.DeliveryTime >= shift.StartTime
                        && ud.Delivery.DeliveryTime <= shift.EndTime)
                    .Include(ud => ud.Delivery)
                    .ToListAsync();

                var userShift = new UserShift
                {
                    UserId = userId,
                    ShiftId = shift.Id,
                    DateAdded = DateTime.UtcNow
                };

                _context.UserShifts.Add(userShift);

                if (userDeliveries.Count != 0)
                {
                    var shiftDeliveries = new List<ShiftDelivery>();

                    foreach (var ud in userDeliveries)
                    {
                        var shiftDelivery = new ShiftDelivery
                        {
                            UserId = userId,
                            ShiftId = shift.Id,
                            DeliveryId = ud.DeliveryId
                        };

                        shiftDeliveries.Add(shiftDelivery);
                    }

                    _context.ShiftDeliveries.AddRange(shiftDeliveries);
                }

                await _context.SaveChangesAsync();

                var shiftStats = await _statsService.CalculateShiftStatistics(userId);
                await _hub.Clients.User(userId.ToString()).SendAsync("ShiftStatisticsUpdated", shiftStats);

                return Ok(new
                {
                    id = shift.Id,
                    startTime = shift.StartTime,
                    endTime = shift.EndTime,
                    app = shift.App
                });
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("my-shifts")]
        public async Task<IActionResult> GetShifts()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var userShifts = await _context.UserShifts
                .Where(us => us.UserId == userId && us.Shift != null)
                .Include(us => us.Shift)
                .Select(us => new ShiftDto
                {
                    Id = us.Shift!.Id,
                    StartTime = us.Shift.StartTime,
                    EndTime = us.Shift.EndTime,
                    App = us.Shift.App
                })
                .OrderByDescending(x => x.EndTime)
                .ToListAsync();

                return Ok(userShifts);
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("apps")]
        public async Task<IActionResult> GetUserShiftApps()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var result = await _context.UserShifts
                .Where(us => us.UserId == userId && us.Shift != null)
                .Select(us => us.Shift!.App)
                .Distinct()
                .ToListAsync();

                return Ok(result);
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("filtered-shifts")]
        public async Task<IActionResult> GetFilteredShifts([FromQuery] DateTime? startTime,
            [FromQuery] DateTime? endTime,
            [FromQuery] DeliveryApp? app)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var userShiftsQuery = _context.UserShifts
                .Where(us => us.UserId == userId && us.Shift != null)
                .Include(us => us.Shift)
                .AsQueryable();

                if (startTime.HasValue)
                {
                    userShiftsQuery = userShiftsQuery.Where(us => us.Shift!.StartTime >= startTime);
                }

                if (endTime.HasValue)
                {
                    userShiftsQuery = userShiftsQuery.Where(us => us.Shift!.EndTime <= endTime);
                }

                if (app.HasValue)
                {
                    userShiftsQuery = userShiftsQuery.Where(us => us.Shift!.App == app);
                }

                var userShifts = await userShiftsQuery
                    .Select(us => new ShiftDto
                    {
                        Id = us.Shift!.Id,
                        StartTime = us.Shift.StartTime,
                        EndTime = us.Shift.EndTime,
                        App = us.Shift.App
                    })
                    .OrderByDescending(x => x.EndTime)
                    .ToListAsync();

                return Ok(userShifts);
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpGet("{shiftId:int}")]
        public async Task<IActionResult> GetShiftById(int shiftId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var userShift = await _context.UserShifts
                .Include(us => us.Shift)
                .FirstOrDefaultAsync(us => us.UserId == userId && us.Shift != null && us.ShiftId == shiftId);

                if (userShift == null)
                {
                    return NotFound("Shift not found");
                }

                var shift = new ShiftDto
                {
                    Id = userShift.Shift!.Id,
                    App = userShift.Shift.App,
                    StartTime = userShift.Shift.StartTime,
                    EndTime = userShift.Shift.EndTime
                };

                return Ok(shift);
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpDelete("{shiftId}")]
        public async Task<IActionResult> DeleteShift(int shiftId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var userShift = await _context.UserShifts
                .FirstOrDefaultAsync(us => us.UserId == userId && us.ShiftId == shiftId);

                if (userShift == null)
                {
                    return NotFound("Shift not found");
                }

                _context.UserShifts.Remove(userShift);
                await _context.SaveChangesAsync();

                var shift = await _context.Shifts.FindAsync(shiftId);
                if (shift != null)
                {
                    _context.Shifts.Remove(shift);
                    await _context.SaveChangesAsync();
                }

                var shiftStats = await _statsService.CalculateShiftStatistics(userId);
                await _hub.Clients.User(userId.ToString()).SendAsync("ShiftStatisticsUpdated", shiftStats);

                return Ok("Shift Deleted");
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateShift([FromBody] Shift shift)
        {
            if (shift.StartTime > DateTime.Now)
            {
                return BadRequest("Shift start time cannot be in the future");
            }

            if (shift.StartTime >= shift.EndTime)
            {
                return BadRequest("Shift end time must come after shift start time");
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out int userId))
            {
                var existingShift = await _context.UserShifts
                .Where(us => us.UserId == userId && us.ShiftId == shift.Id && us.Shift != null)
                .Include(us => us.Shift)
                .FirstOrDefaultAsync();

                if (existingShift == null)
                {
                    return BadRequest("Shift does not exist");
                }

                var targetShift = existingShift.Shift;

                targetShift!.StartTime = shift.StartTime;
                targetShift.EndTime = shift.EndTime;
                targetShift.App = shift.App;

                _context.Shifts.Update(targetShift);
                await _context.SaveChangesAsync();


                // Remove deliveries if app or time no longer matches after update
                var deliveriesToRemove = await _context.ShiftDeliveries
                    .Where(sd =>
                        sd.UserId == userId && sd.ShiftId == targetShift.Id && sd.Delivery != null &&
                        (sd.Delivery.DeliveryTime < targetShift.StartTime || sd.Delivery.DeliveryTime > targetShift.EndTime
                            || sd.Delivery.App != targetShift.App)
                    )
                    .ToListAsync();

                _context.ShiftDeliveries.RemoveRange(deliveriesToRemove);
                await _context.SaveChangesAsync();


                // Add deliveries if updated time or app matches an existing delivery
                var matchingUserDeliveries = await _context.UserDeliveries
                    .Include(ud => ud.Delivery)
                    .Where(ud =>
                        ud.UserId == userId && ud.Delivery != null &&
                        ud.Delivery.App == targetShift.App &&
                        ud.Delivery.DeliveryTime >= targetShift.StartTime &&
                        ud.Delivery.DeliveryTime <= targetShift.EndTime
                    )
                    .ToListAsync();

                var existingDeliveryIds = await _context.ShiftDeliveries
                    .Where(sd => sd.UserId == userId && sd.ShiftId == targetShift.Id)
                    .Select(sd => sd.DeliveryId)
                    .ToListAsync();

                var newShiftDeliveries = matchingUserDeliveries
                    .Where(ud => !existingDeliveryIds.Contains(ud.DeliveryId))
                    .Select(ud => new ShiftDelivery
                    {
                        UserId = userId,
                        ShiftId = targetShift.Id,
                        DeliveryId = ud.DeliveryId
                    })
                    .ToList();

                _context.ShiftDeliveries.AddRange(newShiftDeliveries);
                await _context.SaveChangesAsync();

                var shiftStats = await _statsService.CalculateShiftStatistics(userId);
                await _hub.Clients.User(userId.ToString()).SendAsync("ShiftStatisticsUpdated", shiftStats);

                // Response
                var responseShift = new ShiftDto
                {
                    Id = targetShift.Id,
                    StartTime = targetShift.StartTime,
                    EndTime = targetShift.EndTime,
                    App = targetShift.App
                };

                return Ok(responseShift);
            }
            else
            {
                return BadRequest("User claim is invalid");
            }
        }
    }
}