using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FoodDeliveryBackend.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public async Task JoinOrderChat(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Order-{orderId}");
        }

        public async Task LeaveOrderChat(string orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Order-{orderId}");
        }
    }
}
