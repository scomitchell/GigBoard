using GigBoardBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace GigBoardBackend.Services
{
    public class ShiftTrainingJob
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public ShiftTrainingJob(ApplicationDbContext context, HttpClient client, IConfiguration config)
        {
            _context = context;
            _httpClient = client;
            _config = config;
        }

        public async Task TrainShiftModelJob()
        {
            try
            {
                var shiftData = await _context.Shifts
                    .Where(s => _context.Deliveries.Any(d => d.ShiftId == s.Id))
                    .Select(s => new
                    {
                        s.StartTime,
                        s.EndTime,
                        s.App,
                        Neighborhoods = _context.Deliveries
                            .Where(d => d.ShiftId == s.Id && d.CustomerNeighborhood != null)
                            .Select(d => d.CustomerNeighborhood)
                            .Distinct()
                            .ToList(),
                        TotalEarnings = _context.Deliveries
                            .Where(d => d.ShiftId == s.Id)
                            .Sum(d => d.TotalPay)
                    })
                    .ToListAsync();

                if (!shiftData.Any()) return;

                var samples = shiftData.Select(d => new
                {
                    start_time = d.StartTime.ToString("HH:mm"),
                    end_time = d.EndTime.ToString("HH:mm"),
                    app = d.App.ToString(),
                    neighborhoods = d.Neighborhoods,
                    earnings = d.TotalEarnings
                });

                var payload = new { samples };
                var pythonServiceUrl = _config["PYTHON_SERVICE_URL"] ?? "http://localhost:8001";

                await _httpClient.PostAsJsonAsync($"{pythonServiceUrl}/train/shift-model", payload);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}