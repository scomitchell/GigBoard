using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GigBoardBackend.Data;
using GigBoardBackend.Models;
using GigBoardBackend.Services;

namespace GigBoardBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statsService;
        private readonly HttpClient _httpClient;

        private readonly string _pythonServiceUrl;

        public StatisticsController(IStatisticsService statsService, IHttpClientFactory httpClientFactory)
        {
            _statsService = statsService;
            _httpClient = httpClientFactory.CreateClient();
            _pythonServiceUrl = Environment.GetEnvironmentVariable("PYTHON_SERVICE_URL")
                ?? "http://localhost:8001";
        }

        [HttpGet("deliveries")]
        public async Task<IActionResult> GetDeliveryStats()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            var stats = await _statsService.CalculateDeliveryStatistics(userId.Value);
            return Ok(stats);
        }

        [HttpGet("shifts")]
        public async Task<IActionResult> GetShiftStats()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            var stats = await _statsService.CalculateShiftStatistics(userId.Value);
            return Ok(stats);
        }

        [HttpGet("expenses")]
        public async Task<IActionResult> GetExpenseStats()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User claim is invalid");

            var stats = await _statsService.CalculateExpenseStatistics(userId.Value);
            return Ok(stats);
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

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId)) return userId;
            return null;
        }
    }
}