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
    public class ShiftController : ControllerBase
    {
        private readonly IShiftService _shiftService;

        public ShiftController(IShiftService shiftService)
        {
            _shiftService = shiftService;
        }

        [HttpPost]
        public async Task<IActionResult> AddShift([FromBody] Shift shift)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            try
            {
                var result = await _shiftService.AddShiftAsync(userId.Value, shift);
                return Ok(result);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("my-shifts")]
        public async Task<IActionResult> GetShifts()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            var shifts = await _shiftService.GetShiftsAsync(userId.Value);
            return Ok(shifts);
        }

        [HttpGet("apps")]
        public async Task<IActionResult> GetUserShiftApps()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            var apps = await _shiftService.GetUserShiftAppsAsync(userId.Value);
            return Ok(apps);
        }

        [HttpGet("filtered-shifts")]
        public async Task<IActionResult> GetFilteredShifts([FromQuery] DateTime? startTime,
            [FromQuery] DateTime? endTime,
            [FromQuery] DeliveryApp? app)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            var filteredShifts = await _shiftService.GetFilteredShiftsAsync(userId.Value, startTime, endTime, app);
            return Ok(filteredShifts);
        }

        [HttpGet("{shiftId:int}")]
        public async Task<IActionResult> GetShiftById(int shiftId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            var shift = await _shiftService.GetShiftByIdAsync(userId.Value, shiftId);
            if (shift == null) return NotFound("Shift not found");
            return Ok(shift);
        }

        [HttpDelete("{shiftId}")]
        public async Task<IActionResult> DeleteShift(int shiftId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            try
            {
                await _shiftService.DeleteShiftAsync(userId.Value, shiftId);
                return Ok("Shift removed");
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateShift([FromBody] Shift shift)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            try
            {
                var result = await _shiftService.UpdateShiftAsync(userId.Value, shift);
                return Ok(result);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpGet("{shiftId:int}/deliveries")]
        public async Task<IActionResult> GetDeliveriesForShift(int shiftId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            var deliveries = await _shiftService.GetDeliveriesForShiftAsync(userId.Value, shiftId);
            return Ok(deliveries);
        }

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId)) return userId;
            return null;
        }
    }
}