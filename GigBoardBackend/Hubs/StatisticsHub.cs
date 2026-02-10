using Microsoft.AspNetCore.SignalR;

namespace GigBoardBackend.Hubs
{
    public class StatisticsHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }
}