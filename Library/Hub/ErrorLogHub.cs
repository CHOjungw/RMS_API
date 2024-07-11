using Library.Models;
using Microsoft.AspNetCore.SignalR;

using Library.Interfaces;

namespace Library.Hub
{
    public class ErrorLogHub : Microsoft.AspNetCore.SignalR.Hub
    { 
        public async Task SendErrorLogAsync(ErrorLog errorLog)
        {
            try
            {
            
                await Clients.All.SendAsync("ReceiveErrorLog", errorLog);              
                Console.WriteLine("Sent error log to clients");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending error log: {ex.Message}");
                throw; // 예외를 다시 던져서 호출자에게 전달
            }
        }
        public async Task SendDeviceConnect(string device)
        {
            try
            {
                await Clients.All.SendAsync("ReceiveConnect", device);
                Console.WriteLine("Sent Device Connect to Client");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending error log: {ex.Message}");
                throw; // 예외를 다시 던져서 호출자에게 전달
            }
        }
        public async Task SendDevice(Device device)
        {
            try
            {
                await Clients.All.SendAsync("ReceiveDevice", device);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            // 클라이언트가 연결되었음을 로깅
            Console.WriteLine("Client connected: " + Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
            // 클라이언트가 연결 해제되었음을 로깅
            Console.WriteLine("Client disconnected: " + Context.ConnectionId);
        }
    }
}
